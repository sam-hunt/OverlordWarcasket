#!/usr/bin/env python3
"""Build the animated Workshop preview (.steamworkshop/Preview.gif) from the artist's Preview.mp4.

    python3 Scripts/preview-gif.py                      # .steamworkshop/Preview.mp4 + About/Preview.png -> .steamworkshop/Preview.gif
    python3 Scripts/preview-gif.py --work /tmp/preview  # keep the intermediates for inspection

The artist animates the hero on a 3x blow-up of About/Preview.png (1920x1080, H.264, 30 fps, 5 s): the
hero is drawn from its own high-resolution source, but everything else in the video is the 640x360 PNG
scaled up, blurred by the scale and the codec, shifted in tone (darks crushed, highlights compressed) and
noisy frame to frame. Steam wants a GIF strictly under 1 MiB, at most 640x360, that stays sharp where the
PNG is sharp. This script therefore never rescales the PNG's own pixels: the GIF is the PNG, with the
video's hero composited into the region the animation actually touches.

Steps, each with its intermediate left in --work:

1. frames/      ffmpeg extracts every frame losslessly (Krita's Windows build through wslpath when no
                `ffmpeg` is on PATH). The animation is rendered at 12 fps and padded to 30, so consecutive
                duplicates (mean change below 0.2/255) are dropped: 60 unique frames, one per 1/12 s. The
                loop is not periodic at any shorter lag, so all 60 are kept.
2. lut.json     A monotone per-channel tone curve video -> PNG, fitted by regression over frame 0 against the
                PNG (both blurred 1 px so the video's softness does not bias it; the badge corner excluded
                since its digits differ). After it the static region matches the PNG to within noise, so the
                seam between video hero and PNG background disappears.
3. mask.png     Pixels that ever change by more than 12/255 against frame 0 (at 640x360, box-filtered, right
                of x=200 to shut out codec noise on the title), opened 3 px, dilated 11 px so the moving
                edges and the electric arcs are inside it, holes filled so no PNG pixel survives inside the
                silhouette. Feathered 2 px for the composite; the binary mask tells the encoder which pixels
                may ever change.
4. c640/        The composite frames: the PNG everywhere, the tone-matched video inside the mask. The hero
                is downscaled with a Hamming filter (Lanczos rings on a 3x reduction, and that ringing is
                high-frequency detail LZW then pays for every frame, 10% of the file) and denoised in time:
                a pixel whose change to both neighbouring frames is below DENOISE is averaged with them,
                so codec noise stops flickering while anything that moves is left alone.
5. Preview.gif  One global 256-colour palette (libimagequant over three spread frames). Frame 0 is the
                composite remapped with libimagequant's own gentle dithering, except the hero region, which
                is remapped without dithering like every later frame so frame 0 -> 1 shows no texture jump.
                Later frames carry only what changed; gifsicle -O3 turns the rest into transparency.

Why the later frames need gating: the hero sways by up to 3 px over the loop, which at 12 fps is a
sub-pixel shift of nearly every hero pixel every frame, and a clean encode of that is 2.1 MiB. The budget
buys full frame rate only by letting a pixel keep its last emitted colour until the true colour has moved
more than GATE from it. Every frame therefore shows every pixel within GATE of the truth, with a faint
grain where neighbouring pixels catch up at different times, worst in the dark shadow under the gun; at
viewing size that grain is far less visible than a dropped frame, which was the judgement that set the
rate. Outside the mask nothing ever changes. Delays run 8/8/9 cs so 60 frames take exactly 5.00 s. The
badge is frame 0's PNG pixels, so the gif-patch skill can bump it in place.

Tuning record (640x360, 60 frames): GATE 4 2.1 MiB, 12 1.27 MiB, 16 1.03 MiB, 17 0.99 MiB (shipped),
20 0.89 MiB. Refreshing the hero only every 2nd, 3rd or 4th frame (arcs passing every frame) fits at
GATE 8, 6 and 4 but reads as a stutter, as does interleaving the refresh over alternating rows, which
costs more than it saves. A lossy LZW pass (gifsicle --lossy) buys 15% at 10 and streaks the flat
armour from 30 up; a luma-relative gate costs more at every setting; a 128-colour palette saves 18%;
smaller frames save in proportion to their area. None of those bought a lower gate than 17 does alone.
The build is deterministic: a rerun on the same inputs is byte-identical.

Requires Pillow built with libimagequant (Pillow 9.0's binding returns a one-colour palette for any image
that is not 640x360, so the palette is always learned at 640x360 and only remapped elsewhere), gifsicle,
and ffmpeg. Nothing here ships; the MP4 stays outside the repo like the gameplay recordings.
"""
import argparse, glob, json, os, shutil, subprocess, sys, tempfile
from PIL import Image, ImageChops, ImageDraw, ImageFilter, ImageStat

SIZE = (640, 360)
MOTION_THRESHOLD = 12   # /255, against frame 0 at 640x360
MASK_LEFT_EDGE = 200    # the hero never reaches further left; shuts out codec noise on the title
DENOISE = 12            # /255: a hero pixel changing less than this to both neighbouring frames is averaged with them
GATE = 17               # /255: a pixel keeps its emitted colour until the truth has moved further than this
DELAYS = (8, 8, 9)      # centiseconds, repeating: 60 frames = 500 cs
LIMIT = 1048576
KRITA_FFMPEG = '/mnt/c/Program Files (x86)/Steam/steamapps/common/Krita/krita/bin/ffmpeg.exe'


def log(msg):
    print(f'[preview-gif] {msg}', flush=True)


def max_channel_diff(a, b):
    d = ImageChops.difference(a, b)
    r, g, bl = d.split()
    return ImageChops.lighter(ImageChops.lighter(r, g), bl)


def extract_frames(mp4, work, ffmpeg):
    out = os.path.join(work, 'frames')
    if glob.glob(os.path.join(out, 'f*.png')):
        log(f'frames: reusing {out}')
    else:
        os.makedirs(out, exist_ok=True)
        if ffmpeg.lower().endswith('.exe'):
            win = lambda p: subprocess.check_output(['wslpath', '-w', p], text=True).strip()
            src, pattern = win(mp4), win(out) + '\\f%03d.png'
        else:
            src, pattern = mp4, os.path.join(out, 'f%03d.png')
        log(f'frames: {os.path.basename(ffmpeg)} -> {out}')
        subprocess.run([ffmpeg, '-v', 'error', '-y', '-i', src, '-an', '-fps_mode', 'passthrough',
                        '-pix_fmt', 'rgb24', pattern], check=True)
    uniq, prev = [], None
    for p in sorted(glob.glob(os.path.join(out, 'f*.png'))):
        im = Image.open(p).convert('RGB')
        if prev is None or ImageStat.Stat(max_channel_diff(im, prev)).mean[0] > 0.2:
            uniq.append(im)
        prev = im
    log(f'frames: {len(uniq)} unique of {len(glob.glob(os.path.join(out, "f*.png")))}')
    return uniq


def fit_lut(png, frame0, work):
    """Monotone per-channel curve video -> PNG from the median PNG value at each video value."""
    P = png.filter(ImageFilter.GaussianBlur(1.0))
    V = frame0.resize(SIZE, Image.LANCZOS).filter(ImageFilter.GaussianBlur(1.0))
    W, H = SIZE
    luts = []
    for ch in range(3):
        pv, pp = V.split()[ch].tobytes(), P.split()[ch].tobytes()
        buckets = [[] for _ in range(256)]
        for i in range(len(pv)):
            if i % W < 80 and i // W < 80:
                continue                      # the badge corner: its digits differ between the two
            buckets[pv[i]].append(pp[i])
        lut = [None] * 256
        for v in range(256):
            if len(buckets[v]) >= 20:
                b = sorted(buckets[v]); lut[v] = b[len(b) // 2]
        known = [v for v in range(256) if lut[v] is not None]
        for v in range(256):
            if lut[v] is None:
                lo = max([k for k in known if k < v], default=None)
                hi = min([k for k in known if k > v], default=None)
                if lo is None: lut[v] = lut[hi] * v // max(hi, 1)
                elif hi is None: lut[v] = min(255, lut[lo] + (v - lo))
                else: lut[v] = round(lut[lo] + (lut[hi] - lut[lo]) * (v - lo) / (hi - lo))
        for v in range(1, 256): lut[v] = max(lut[v], lut[v - 1])
        sm = [round(sum(lut[max(0, min(255, v + k))] for k in range(-2, 3)) / 5) for v in range(256)]
        for v in range(1, 256): sm[v] = max(sm[v], sm[v - 1])
        luts.append([min(255, x) for x in sm])
    json.dump(luts, open(os.path.join(work, 'lut.json'), 'w'))
    box = (8, 100, 250, 352)
    before = ImageStat.Stat(max_channel_diff(frame0.resize(SIZE, Image.BOX), png).crop(box)).mean[0]
    after = ImageStat.Stat(max_channel_diff(frame0.resize(SIZE, Image.BOX).point(sum(luts, [])), png).crop(box)).mean[0]
    log(f'tone: static-region mismatch {before:.1f} -> {after:.1f} /255')
    return sum(luts, [])


def build_mask(small, work):
    mx = Image.new('L', SIZE, 0)
    for im in small[1:]:
        mx = ImageChops.lighter(mx, max_channel_diff(im, small[0]))
    m = mx.point(lambda v: 255 if v > MOTION_THRESHOLD else 0)
    m.paste(Image.new('L', (MASK_LEFT_EDGE, SIZE[1]), 0), (0, 0))
    m = m.filter(ImageFilter.MinFilter(3)).filter(ImageFilter.MaxFilter(3)).filter(ImageFilter.MaxFilter(11))
    inv = m.point(lambda v: 0 if v else 255)
    ImageDraw.floodfill(inv, (0, 0), 128)
    holes = inv.point(lambda v: 255 if v == 255 else 0)
    binary = ImageChops.lighter(m, holes)
    feathered = binary.filter(ImageFilter.GaussianBlur(2))
    binary.save(os.path.join(work, 'mask.png'))
    cov = binary.histogram()[255] / (SIZE[0] * SIZE[1])
    log(f'mask: {cov:.0%} of the frame, bbox {binary.getbbox()}, {holes.histogram()[255]} hole px filled')
    return binary, feathered


def composite(png, uniq, lut, feathered, work):
    out = os.path.join(work, 'c640'); os.makedirs(out, exist_ok=True)
    heroes = [im.resize(SIZE, Image.HAMMING).point(lut) for im in uniq]
    n, still = len(heroes), lambda a, b: max_channel_diff(a, b).point(lambda v: 255 if v < DENOISE else 0)
    denoised = []
    for i, c in enumerate(heroes):
        p, q = heroes[i - 1], heroes[(i + 1) % n]
        quiet = ImageChops.multiply(still(c, p), still(c, q))
        denoised.append(Image.composite(Image.blend(Image.blend(p, q, 0.5), c, 0.5), c, quiet))
    frames = []
    for i, h in enumerate(denoised):
        c = Image.composite(h, png, feathered)
        # Pillow keeps RGB as four bytes per pixel and composite() blends the mask into the padding byte,
        # which libimagequant then reads as alpha and builds a visibly worse palette from. A fresh buffer
        # pads with 255, exactly as the frame would reload from its PNG.
        c = Image.frombytes('RGB', c.size, c.tobytes())
        c.save(os.path.join(out, f'c{i:02d}.png')); frames.append(c)
    return frames


def encode(frames, binary, out_path, work):
    n = len(frames); W, H = SIZE
    sample = Image.new('RGB', (W, H * 3))
    for k, i in enumerate((0, n // 3, 2 * n // 3)):
        sample.paste(frames[i], (0, H * k))
    pal = sample.quantize(256, method=Image.LIBIMAGEQUANT)
    assert len(pal.getcolors(256)) > 8, 'libimagequant returned a collapsed palette'
    remap = lambda im: im.quantize(palette=pal, dither=Image.NONE)
    outside = binary.point(lambda v: 0 if v else 255)
    tmp = os.path.join(work, 'gif'); os.makedirs(tmp, exist_ok=True)
    paths, prev = [], None
    for i, f in enumerate(frames):
        if i == 0:
            fr = pal.crop((0, 0, W, H)); fr.paste(remap(f), (0, 0), binary)
        else:
            cand = remap(f)
            moved = max_channel_diff(cand.convert('RGB'), prev.convert('RGB')).point(lambda v: 255 if v > GATE else 0)
            keep = ImageChops.lighter(moved.point(lambda v: 255 - v), outside)
            fr = cand.copy(); fr.paste(prev, (0, 0), keep)
        p = os.path.join(tmp, f'f{i:03d}.gif'); fr.save(p, interlace=False); paths.append(p); prev = fr
    args = ['gifsicle', '-O3', '--no-warnings', '--loopcount=forever']
    for i, p in enumerate(paths):
        args += [f'-d{DELAYS[i % len(DELAYS)]}', p]
    subprocess.run(args + ['-o', out_path], check=True)
    return os.path.getsize(out_path)


def main():
    ap = argparse.ArgumentParser(description=__doc__.splitlines()[0])
    ap.add_argument('--mp4', default='.steamworkshop/Preview.mp4')
    ap.add_argument('--png', default='About/Preview.png')
    ap.add_argument('--out', default='.steamworkshop/Preview.gif')
    ap.add_argument('--work', help='intermediates directory (default: a fresh temp dir, removed on success)')
    ap.add_argument('--ffmpeg', default=shutil.which('ffmpeg') or KRITA_FFMPEG)
    a = ap.parse_args()
    work = a.work or tempfile.mkdtemp(prefix='preview-gif-')
    os.makedirs(work, exist_ok=True)
    png = Image.open(a.png).convert('RGB')
    assert png.size == SIZE, f'{a.png} is {png.size}, expected {SIZE}'
    uniq = extract_frames(a.mp4, work, a.ffmpeg)
    lut = fit_lut(png, uniq[0], work)
    binary, feathered = build_mask([im.resize(SIZE, Image.BOX) for im in uniq], work)
    frames = composite(png, uniq, lut, feathered, work)
    size = encode(frames, binary, a.out + '.tmp', work)
    if size >= LIMIT:
        os.remove(a.out + '.tmp')
        sys.exit(f'[preview-gif] {size} bytes is over the {LIMIT} byte Workshop limit; nothing written')
    os.replace(a.out + '.tmp', a.out)
    log(f'wrote {a.out}: {size} bytes ({size / LIMIT:.1%} of the limit), {len(frames)} frames')
    if not a.work:
        shutil.rmtree(work)


if __name__ == '__main__':
    main()
