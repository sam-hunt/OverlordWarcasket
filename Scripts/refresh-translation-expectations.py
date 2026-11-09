#!/usr/bin/env python3
# Overlord Warcasket's config shim over the shared sidecar-refresh engine
# (l10n/refresh/refresh_expectations.py, the rimworld-l10n submodule),
# which drives the L10nProbe dev mod (source at l10n/probe/; build/deploy it
# only from the canonical ~/dev/rimworld-l10n checkout). The engine holds all
# logic; this file holds only this repo's config and the rationale behind it.
# Usage is unchanged (game must be closed):
#   python3 Scripts/refresh-translation-expectations.py [--no-launch]
# If l10n/ is empty, run: git submodule update --init

import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent.parent / "l10n" / "refresh"))
import refresh_expectations as engine  # noqa: E402  (import after sys.path edit)

engine.REPO_ROOT = Path(__file__).resolve().parent.parent

engine.PACKAGE_ID = "shunter.overlordwarcasket"

# RATIONALE: VFE Pirates is a hard dependency (our defs parent on its
# abstract warcasket bases and would not resolve without it); Vanilla
# Expanded Framework and Harmony are VFEP's own hard deps and load before
# it. Biotech is the other hard dependency (the checker shim's REQUIRED_DLCS
# note) and Odyssey is pinned beside it for the reason given there. Ideology
# is NOT pinned even though a stat leaf carries MayRequire for it: a gated
# leaf changes a number, never a def or a key. No family sibling rides along;
# this repo's list is its own. See the engine's header for the membership
# rule, the lowercase-id warning, and the pinning rationale; order is load
# order, the probe last.
#
# Plain ids on purpose: when the Workshop copy of a mod and a local Mods/
# copy coexist, RimWorld suffixes the Workshop one "_steam" and the plain id
# names the local copy. The plain id therefore resolves to whichever single
# copy is installed and never breaks when one of them is removed.
engine.CANONICAL_ACTIVE_MODS = [
    "brrainz.harmony",
    "ludeon.rimworld",
    "ludeon.rimworld.biotech",
    "ludeon.rimworld.odyssey",
    "oskarpotocki.vanillafactionsexpanded.core",
    "oskarpotocki.vfe.pirates",
    "shunter.overlordwarcasket",
    "shunter.l10nprobe",
]

raise SystemExit(engine.main())
