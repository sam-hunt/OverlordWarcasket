# Contributing

Bug reports, fixes, and features are welcome. Open an issue or pull request.
Build instructions are in README.md; the mod builds with
`dotnet build OverlordWarcasket.sln -c Release`.

If you work in Claude Code, the repo ships a Stop hook (`.claude/hooks/sync-mod.sh`,
wired by `.claude/settings.json`) that rebuilds and redeploys the mod into your
RimWorld Mods folder after any turn that changed mod files. It does nothing when no
RimWorld install is found. Like any script in a repo you clone, read it before you
let an agent run it.

## Localization

The mod targets the languages below, chosen by RimWorld's per-language
audience size. Contributions for any other language RimWorld supports are
welcome too. No translations exist yet.

| Language             | Status           | Credit    |
| -------------------- | ---------------- | --------- |
| English              | Source           | -         |
| Simplified Chinese   | Planned          | -         |
| Russian              | Planned          | -         |
| Korean               | Planned          | -         |
| German               | Planned          | -         |
| Spanish              | Planned          | -         |
| French               | Planned          | -         |
| Brazilian Portuguese | Planned          | -         |
| Japanese             | Planned          | -         |
| Traditional Chinese  | Planned          | -         |

Statuses: **Source** (the authoritative English strings), **Machine-assisted**
(generated with terminology grounded against the official RimWorld
localization; awaiting native review), **Native** (written or reviewed by a
native speaker), **Planned** (not started, contributions welcome).

Spanish here means Castilian (RimWorld's `Spanish` language folder). RimWorld
also ships a separate Latin American Spanish (`SpanishLatin`); a translation
for it is welcome as its own folder rather than as edits to this one.

Brazilian Portuguese likewise means RimWorld's `PortugueseBrazilian` folder.
European Portuguese (`Portuguese`) is a separate language folder in RimWorld,
so a translation for it is welcome in its own right rather than as edits to
this one.

### Contributing a translation

- Files live under `1.6/Languages/<Language>/`. English is served by the def
  XML plus one Keyed file, `1.6/Languages/English/Keyed/OverlordWarcasket.xml`
  (the mod settings window and the resurrect mech's messages), translated to
  `Keyed/OverlordWarcasket.xml` with the same keys; keep every `{0}`-style
  placeholder. The DefInjected key set comes from
  `Scripts/expected-injections.json`: `ThingDef` (the three warcasket parts,
  which are `VFEPirates.WarcasketDef`s the game files under `ThingDef`),
  `VEF.Abilities.AbilityDef` (the resurrect mech) and `TraitDef` (the
  pilot's trait text).
- Every translated entry carries the current English source in a comment
  directly above it, e.g. `<!-- EN: medic warcasket -->`; this is how stale
  translations are detected when the English changes.
- The Workshop title must match the translated `OLWC_SettingsCategory` (the
  settings window's header); the checker enforces it.
- The three part descriptions share their second and third paragraphs; keep
  them identical across the three entries, and keep each part's
  `shortDescription` equal to its description's first paragraph, as the
  English does.
- The mod's VFE Pirates vocabulary for each language (how "warcasket", the
  set names and the foundry are rendered, and which community VFE Pirates
  translation it follows) is recorded in
  `.claude/skills/translate/glossary/<Language>.md`; match it, or change it
  there in the same PR.
- The Workshop page text lives in `.steamworkshop/Description/<Language>.txt`
  (see that folder's README for the format); corrections there are welcome
  too.
- Formatting: UTF-8 without BOM, LF line endings, 2-space indent.
- Validate before opening a PR:

  ```bash
  python3 Scripts/check-translations.py --strict
  ```

  It checks key coverage, placeholders, DefInjected paths, staleness, and
  file hygiene. The checker's engine lives in the `l10n/` git submodule, so
  clone with `git clone --recurse-submodules` (or run
  `git submodule update --init` in an existing clone) before validating.

- Improving a machine-assisted language? Corrections from native speakers
  are gladly merged, no matter how small.
