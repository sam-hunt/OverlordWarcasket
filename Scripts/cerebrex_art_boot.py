# Forces Overlord Warcasket's cerebrex art setting on for a pinned boot, used by
# the smoke shim (not shipped, not an engine: l10n/ stays untouched).
#
# RATIONALE: the l10n engine's pinned_boot (l10n/refresh/refresh_expectations.py)
# sets every pinned mod's settings file aside so the boot runs on DEFAULT
# settings, and has no per-boot settings dimension yet (its header says so).
# The cerebrex art switch is a restart-time patch gate
# (1.6/Mods/Odyssey/Patches/CerebrexNodeArt.xml) that a default boot never
# applies, so a broken xpath or a misnamed switch there surfaces only in a
# boot with the setting on. Writing the settings file before the engine runs
# is useless (it is set aside with the rest), so this wraps pinned_boot and
# writes the file INSIDE it, after the engine has cleared the folder, then
# puts back whatever sat there before the engine hands the player's own file
# back. The smoke engine looks pinned_boot up on the refresh module at call
# time, so replacing the attribute reaches it. The file's shape is the game's
# own (SettingsBlock over the settings class), with the switch's override pair
# as BoolSetting saves it: the Set flag, then the value.
import contextlib
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent.parent / "l10n" / "refresh"))
import refresh_expectations as refresh  # noqa: E402  (import after sys.path edit)

SETTINGS_FILE = "Mod_OverlordWarcasket_OverlordWarcasketMod.xml"
CEREBREX_ART_ON = (
    '<?xml version="1.0" encoding="utf-8"?>\n'
    "<SettingsBlock>\n"
    '\t<ModSettings Class="OverlordWarcasket.OverlordWarcasketSettings">\n'
    "\t\t<CerebrexArtSet>True</CerebrexArtSet>\n"
    "\t\t<CerebrexArt>True</CerebrexArt>\n"
    "\t</ModSettings>\n"
    "</SettingsBlock>"
)


def force_cerebrex_art_on():
    engine_boot = refresh.pinned_boot

    @contextlib.contextmanager
    def pinned_boot(rw, mods):
        with engine_boot(rw, mods):
            settings = refresh.modsconfig_path().with_name(SETTINGS_FILE)
            # Normally absent here (the engine set the player's copy aside);
            # kept anyway in case the engine did not find the mod's folder.
            before = settings.read_bytes() if settings.exists() else None
            settings.write_text(CEREBREX_ART_ON, encoding="utf-8-sig")
            print("Boot forces Overlord Warcasket's cerebrex art setting on.")
            try:
                yield
            finally:
                if before is None:
                    settings.unlink(missing_ok=True)
                else:
                    settings.write_bytes(before)

    refresh.pinned_boot = pinned_boot
