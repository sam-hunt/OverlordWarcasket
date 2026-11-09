#!/usr/bin/env python3
# Pre-release integration smoke test: boots the real game once with Overlord
# Warcasket, its dependency chain and the optional mods it patches for, on a
# pinned minimal list where the baseline is a clean log, then classifies
# every logged error/warning by origin and fails on anything attributed to
# this mod or an integration seam. Thin shim over the shared engine in
# l10n/smoke/startup_smoke.py (see its header for mechanics and the
# BetterTradersGuild v1.1.0 CWTL incident this exists to catch).
#
# Run this before every release, with the game closed:
#   python3 Scripts/integration-smoke-test.py              # boot + scan
#   python3 Scripts/integration-smoke-test.py --no-launch  # rescan last log
#   python3 Scripts/integration-smoke-test.py --strict     # any error fails

import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent.parent / "l10n" / "smoke"))
import startup_smoke as engine  # noqa: E402

engine.REPO_ROOT = Path(__file__).resolve().parent.parent

engine.PACKAGE_ID = "shunter.overlordwarcasket"

# RATIONALE: the first six entries are the l10n CANONICAL_ACTIVE_MODS (the
# refresh shim explains them: VFE Pirates and Biotech are the hard deps, VEF
# and Harmony are VFEP's deps, Odyssey rides along for Vanilla Gravship
# Expanded). Then the optional mods this repo integrates with. Vanilla
# Gravship Expanded (chapter 1): LoadFolders.xml opens the
# 1.6/Mods/VanillaGravshipExpanded root on its package id, and that root's
# vacuum-resistance patch only ever runs with it active. Combat Extended:
# it opens the 1.6/Mods/CombatExtended root, whose patch rescales the set's
# stats and would fail silently against a renamed node. It loads after
# VFEP, where its own About.xml puts it. Each sits before our mod, as the
# mod a compat root complements should. Probe last (auto-quit). Plain ids
# on purpose; see the refresh shim's "_steam" note.
# Save Our Ship 2 is deliberately absent: 1.6/Patches/SOS2Patch.xml is a
# best-effort mirror of VFEP's own patch (its header), checked statically by
# Tests/1.6/CompatPatchTests.cs and never booted here.
engine.SMOKE_ACTIVE_MODS = [
    "brrainz.harmony",
    "ludeon.rimworld",
    "ludeon.rimworld.biotech",
    "ludeon.rimworld.odyssey",
    "oskarpotocki.vanillafactionsexpanded.core",
    "oskarpotocki.vfe.pirates",
    "vanillaexpanded.gravship",
    "ceteam.combatextended",
    "shunter.overlordwarcasket",
    "shunter.l10nprobe",
]

# Assembly/namespace name, the C# log prefix (also the About.xml display
# name, which the engine derives and appends itself), and the def prefix.
engine.OWN_PATTERNS = ["OverlordWarcasket", "[Overlord Warcasket]", "OLWC_"]

# The seams: VFEP's warcasket code (foundry, entombing, Apparel_Warcasket)
# walks our WarcasketDefs, VEF's ApparelExtension, abilities and
# CompShieldBubble run on them, VGE1's vacuum rules read the resistance our
# patch lowers, and CE's armor and inventory code reads the stats the
# CombatExtended root sets. An error mentioning any of these gates the test
# even when the exception fires inside their code.
engine.INTEGRATION_PATTERNS = {
    "VFEP": ["VFEPirates"],
    "VEF": ["VEF."],
    "VGE": ["VanillaGravshipExpanded"],
    "CE": ["CombatExtended"],
}

raise SystemExit(engine.main())
