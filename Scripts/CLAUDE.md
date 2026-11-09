# Scripts/: localization and release tooling

Loaded when work touches the tooling. Each shim's comments carry this repo's config and its
rationale (the VFEP dependency chain, why Biotech and Odyssey are the DLCs pinned, why VGE1 and CE
are on the smoke list); the engines live in the `l10n/` submodule. Nothing here ships.

- **`l10n/` is the `rimworld-l10n` toolkit as a git submodule pinned to a release tag**
  (`git submodule status`; if empty, `git submodule update --init`). Never edit it in place here:
  mod-independent learnings go upstream in the canonical `~/dev/rimworld-l10n` checkout,
  mod-specific ones in the `/translate` skill's glossary. The pin moves only at release, at the
  start of a translation pass, or when a new major lands (`l10n/tools/bump-consumer.sh`).
- **`check-translations.py`** validates every language deterministically and is a CI release
  gate. Its expected set is the checked-in `expected-injections.json`, a dump of every injection
  point the live game sees for this mod. The checker refuses stale expectations, so new content
  (a new def type adds a DefInjected folder) forces a regen.
- **`refresh-translation-expectations.py`** (game closed) regenerates that sidecar by driving the
  L10nProbe dev mod through the game's own walker (source at `l10n/probe/`, built and deployed
  only from the canonical checkout; this mod is ticked in its settings,
  `Config/Mod_L10nProbe_L10nProbeMod.xml`). Regenerating is always fine; the release skill does
  it every release.
- **`integration-smoke-test.py`** (game closed, pre-release) boots the deployed mod on a pinned
  list (the VFEP chain, Biotech, Odyssey, Vanilla Gravship Expanded and Combat Extended so their
  compat roots open), then fails on any logged error attributed to this mod or a VFEP/VEF/VGE/CE
  seam. The Odyssey root's cerebrex art patch stays off on this default-settings boot and gets
  no forced boot: it is cosmetic and its xpaths are checked statically. Save Our Ship 2 is
  deliberately off the list: its patch is best-effort.
  Sibling-mod "dump WILL fail" warnings at launch are expected: the probe is ticked for every
  family mod but each boot loads only its own list.
- **`preview-gif.py`** turns the artist's animated `Preview.mp4` into the Workshop
  `.steamworkshop/Preview.gif` (under 1 MiB, 640x360) by compositing the video's hero onto
  `About/Preview.png` and gating what each frame may change; its header is the record of the
  method and the tuning. Rerun it when the artist delivers a new MP4 or the PNG changes; version
  bumps go through the `gif-patch` skill instead. `Docs/Research/PREVIEW_GIF_FROM_MP4.md` is
  the guide for the next commission: what the delivery looks like, the cost model and the
  pitfalls (a snapshot of the family primary in `../`).
- **Translation passes are deferred until shortly before release**, one language at a time on
  explicit request, through the `/translate` skill, once the English text is final. No language
  exists yet; the public roster is in `CONTRIBUTING.md`.
