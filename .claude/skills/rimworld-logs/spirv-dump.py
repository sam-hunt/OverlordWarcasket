#!/usr/bin/env python3
"""Read RimWorld 1.7's compiled shaders.

The 1.7 (Waylith) renderer compiles Slang shaders to SPIR-V and ships them as
`shaders/<hash>.spv-cache` entries in the install's `Assets.zip`. The entries are zstd
compressed (zip method 93, which Python's zipfile cannot read, so the raw bytes go through the
system libzstd via ctypes) and each cache file carries a 20-byte `NHSC` header before the
SPIR-V magic. Uniform names survive as OpName/OpMemberName strings, so a shader is found by the
uniform or sampler it declares, and the fragment math is short enough to read from a dump of
its loads, access chains and arithmetic.

    spirv-dump.py find _DetailScrollSpeed     # list matching shaders, save them under OUTDIR
    spirv-dump.py dump OUTDIR/<hash>.spv-cache  # print member names and the body

Resolved `%n` ids are printed by name where the shader names them, by constant value where
the id is a constant, and `u[i]` indexes the material uniform block (see the member list).
"""
import argparse
import ctypes
import ctypes.util
import os
import re
import struct
import sys
import zipfile

RIMWORLD = os.environ.get("RIMWORLD_PATH", "")
OUTDIR = os.path.join(os.environ.get("TMPDIR", "/tmp"), "rimworld-shaders")
SPIRV_MAGIC = b"\x03\x02\x23\x07"

OPS = {
    12: "ExtInst", 57: "FunctionCall", 61: "Load", 62: "Store", 65: "AccessChain",
    66: "InBoundsAccessChain", 79: "VectorShuffle", 80: "CompositeConstruct",
    81: "CompositeExtract", 86: "SampledImage", 87: "ImageSampleImplicitLod",
    88: "ImageSampleExplicitLod", 115: "FConvert", 127: "FNegate", 129: "FAdd", 131: "FSub",
    133: "FMul", 136: "FDiv", 141: "FMod", 142: "VectorTimesScalar", 143: "MatrixTimesScalar",
    144: "VectorTimesMatrix", 145: "MatrixTimesVector", 148: "Dot", 169: "Select",
    180: "FOrdLessThan", 183: "FOrdGreaterThan", 249: "Branch", 250: "BranchConditional",
    253: "Return", 254: "ReturnValue",
}
# Declarations, decorations and types: noise in a body dump.
SKIP = {1, 2, 3, 4, 5, 6, 7, 8, 10, 11, 14, 15, 16, 17, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28,
        30, 32, 33, 43, 44, 54, 56, 59, 63, 71, 72, 248}


def zstd():
    lib = ctypes.util.find_library("zstd")
    if not lib:
        sys.exit("libzstd not found")
    z = ctypes.CDLL(lib)
    z.ZSTD_getFrameContentSize.restype = ctypes.c_ulonglong
    z.ZSTD_decompress.restype = ctypes.c_size_t
    z.ZSTD_isError.restype = ctypes.c_uint
    return z


def entries(path):
    """Yield (name, bytes) for every shader entry, decompressing zstd ones by hand."""
    z = zstd()
    zf = zipfile.ZipFile(path)
    with open(path, "rb") as fp:
        for zi in zf.infolist():
            if not zi.filename.startswith("shaders/"):
                continue
            fp.seek(zi.header_offset)
            nlen, elen = struct.unpack("<HH", fp.read(30)[26:30])
            fp.seek(zi.header_offset + 30 + nlen + elen)
            raw = fp.read(zi.compress_size)
            if zi.compress_type == 0:
                yield zi.filename, raw
                continue
            size = z.ZSTD_getFrameContentSize(raw, len(raw))
            if size >= 2 ** 64 - 2:
                size = zi.file_size
            out = ctypes.create_string_buffer(size)
            n = z.ZSTD_decompress(out, size, raw, len(raw))
            if z.ZSTD_isError(n):
                sys.exit("zstd failed on " + zi.filename)
            yield zi.filename, out.raw[:n]


def find(needle):
    os.makedirs(OUTDIR, exist_ok=True)
    for name, data in entries(os.path.join(RIMWORLD, "Assets.zip")):
        if needle.lower().encode() not in data.lower():
            continue
        out = os.path.join(OUTDIR, os.path.basename(name))
        with open(out, "wb") as f:
            f.write(data)
        strings = re.findall(rb"[\x20-\x7e]{4,}", data)
        names = [s.decode() for s in strings if re.match(rb"_[A-Za-z]", s)]
        print(out)
        print("   ", " ".join(dict.fromkeys(names)))


def dump(path):
    data = open(path, "rb").read()
    data = data[data.find(SPIRV_MAGIC):]
    words = struct.unpack("<%dI" % (len(data) // 4), data[: len(data) // 4 * 4])
    insts, i = [], 5
    while i < len(words):
        count, op = words[i] >> 16, words[i] & 0xFFFF
        insts.append((op, words[i + 1:i + count]))
        i += count

    def literal(args, start):
        return b"".join(struct.pack("<I", x) for x in args[start:]).split(b"\0")[0].decode()

    names, members, types, consts = {}, {}, {}, {}
    for op, a in insts:
        if op == 5:
            names[a[0]] = literal(a, 1)
        elif op == 6:
            members[(a[0], a[1])] = literal(a, 2)
        elif op == 21:
            types[a[0]] = ("int", a[2])
        elif op == 22:
            types[a[0]] = ("float",)
        elif op == 43:
            t = types.get(a[0])
            if t and t[0] == "float":
                consts[a[1]] = struct.unpack("<f", struct.pack("<I", a[2]))[0]
            elif t:
                consts[a[1]] = a[2] - 2 ** 32 if t[1] and a[2] >= 2 ** 31 else a[2]
        elif op == 44:
            consts[a[1]] = tuple(consts.get(x, "%%%d" % x) for x in a[2:])

    def ref(x):
        if x in consts:
            return repr(consts[x])
        return names.get(x, "%%%d" % x)

    print("--- members")
    for (s, idx), n in members.items():
        print("  %s[%d] = %s" % (names.get(s, "%%%d" % s), idx, n))
    print("--- body")
    for op, a in insts:
        if op in SKIP:
            continue
        name = OPS.get(op, "Op%d" % op)
        if op == 62:
            print("  Store %s <- %s" % (ref(a[0]), ref(a[1])))
        elif op in (253,):
            print("  Return")
        elif op == 12:
            print("  %%%d = ExtInst %d %s" % (a[1], a[3], " ".join(ref(x) for x in a[4:])))
        elif op in (65, 66):
            print("  %%%d = %s %s[%s]" % (a[1], name, ref(a[2]), ",".join(ref(x) for x in a[3:])))
        elif len(a) >= 2:
            print("  %%%d = %s %s" % (a[1], name, " ".join(ref(x) for x in a[2:])))
        else:
            print("  %s %s" % (name, " ".join(ref(x) for x in a)))


if __name__ == "__main__":
    p = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    sub = p.add_subparsers(dest="cmd", required=True)
    sub.add_parser("find").add_argument("needle")
    sub.add_parser("dump").add_argument("path")
    args = p.parse_args()
    find(args.needle) if args.cmd == "find" else dump(args.path)
