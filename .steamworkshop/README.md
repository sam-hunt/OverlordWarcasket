# .steamworkshop

Publishing metadata for the mod's Steam Workshop page. Nothing in this folder
ships with the mod (the StageMod manifest never matches it) or is loaded by
RimWorld.

## Preview.gif

The animated Workshop thumbnail, optional. Steam animates a GIF preview on the
page (it must stay below 1 MB), but the game's mod list decodes only PNG and
JPG, so `About/Preview.png` stays a static image and this file is set on the
page separately: the `release` skill runs a preview-only SteamCMD update after
every in-game upload, because the uploader resets the preview to the PNG each
time.

`Scripts/preview-gif.py` builds it from the artist's animated `Preview.mp4` (the
hero animated on a blow-up of `About/Preview.png`) and the PNG itself, keeping
the PNG's own pixels everywhere the animation does not touch; its header
records the method and the size tuning. The MP4 is not tracked; keep it with
the gameplay recordings. The badge is bumped in place by the `gif-patch` skill
like the PNG's, never by re-exporting.

## Images/

The page's gallery images, uploaded by hand through the page's images and
videos UI. Steam rejects gallery images of 2 MB or more and takes no video
files, so gameplay clips are GIFs cut down to fit (lower frame rate, scale and
palette) from screen recordings kept outside the repo.

## Description/

One file per language, named after the RimWorld language folders in
`1.6/Languages/`. English is the source of truth; the others are
machine-assisted first passes pending native review. Format:

- Line 1: the Workshop title for that language
- Line 2: blank
- Rest: the BBCode description

Title convention: the English title leans on VFE Pirates' own vocabulary
("warcasket") so players browsing for more sets find the mod; every localized
title must contain the term VFE Pirates' own translation for that language
uses for "warcasket" (the `translate` skill's glossary records it). Titles are
fully localized with no English brand appended: Workshop search is
language-agnostic (any language's title matches regardless of UI language)
and the preview thumbnail already carries the English name. Each title must
equal that language's `OLWC_SettingsCategory` Keyed value (the settings
window's header); the checker enforces the coupling.

Player-facing rules from CLAUDE.md apply here too: no em dashes. Numbers on
the page must match the defs; when a tuning change moves one, update this
file in the same commit.

Steam has no API for per-language Workshop text, so updated files are pasted
manually into the Workshop page's edit UI (note Steam's own language names
differ: schinese, koreana, brazilian, latam, ...). The `release` skill diffs
`English.txt` against the last stable release tag and refreshes the translations
whenever it changed.

Every shipped language has a description file; the non-English ones are
machine-assisted first passes pending native review.
