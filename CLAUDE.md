# CLAUDE.md

Guidance for Claude Code when working in this repository. This file holds only what applies
across the whole repo. Folder guides load on their own when work touches them: `1.6/CLAUDE.md`
(defs, patches, languages, gating), `Source/1.6/CLAUDE.md` (C#), `Scripts/CLAUDE.md` (l10n and
release tooling), `.steamworkshop/CLAUDE.md` (Workshop page).

## Project

**Overlord Warcasket** is a RimWorld 1.6/1.7 mod adding one warcasket apparel set (armor, shoulder
pads, helmet) for Vanilla Factions Expanded - Pirates (VFEP): a mechanitor's command suit on
VFEP's 10th-generation spacer chassis. VFEP, Biotech and Harmony are the hard dependencies (VEF
comes with VFEP); Odyssey and other optional content load only when present. C# (.NET Framework
4.7.2), Harmony, XML defs. Def prefix `OLWC_`.

**State: scaffolded, first mechanics in, not yet released.** The resurrect mech ability (Biotech's
apocriton ability on the armor, drawing on a slow-recharging core with a red gauge), the
overlord work trait (the pilot keeps smithing and research, so mech work and hacking) and the
helmet's command range (a wearing mechanitor orders drafted mechs anywhere on the map, vanilla's
24.9 cells lifted behind a settings section) are implemented; the set's plating, shield and
costs are the tier's standard pending a tuning pass.
`TODOs.md` is the ledger. Each feature's design rationale is a `Docs/Specs/` file; the sibling
`../MedicWarcasket/` repo (same author, same VFEP base) is the worked precedent for the own-trait
work profile, the settings idiom and the infrastructure copied here, and `../ToxinWarcasket/` for
the gauge and the Biotech dependency.

## Documentation rules

- **The file's header is the record.** Per-item values, tuning numbers and decompile-verified
  call paths live in the header comment of the file they describe, with the _why_. No guide
  restates a def value or call path; a line joins a guide only when it constrains other files.
- **Comments describe the present, git describes the past.** No dates, previous values, dropped
  attempts or "seen in game" notes in comments; that history goes in the commit message.
  Comments never ship: `StageMod` strips every comment node from the staged XML, so a def header
  can carry its full rationale at no download cost.
- **Never read a `Docs/Specs/` file whole unprompted**: specs run long and the code supersedes
  them. Grep for the section you need, or delegate a spec-wide question to a subagent and keep
  its answer.
- **Dependency sources are read-only.** `../VanillaExpanded/VanillaFactionsExpanded-Pirates/`
  (its `CLAUDE.md` maps the warcasket subsystem) and
  `../VanillaExpanded/VanillaExpandedFramework/` are upstream checkouts; never commit into them.

## Build

```bash
dotnet build OverlordWarcasket.sln -c Release   # builds 1.6/Assemblies/ and redeploys to Mods/OverlordWarcasket
```

Compiles against the pinned `Krafs.Rimworld.Ref` 1.6 package; the local install (auto-detected,
or `RIMWORLD_PATH`, in WSL the Windows path) only decides deployment and serves as the decompile
target. Release builds deploy atomically through the csproj's `StageMod` target, the one file
manifest, which CI calls too; Debug builds never deploy. The csproj's comments cover path
detection, the compile-only VFEP/VEF references and the manifest's globbing. The Stop hook
`.claude/hooks/sync-mod.sh` (self-documenting, byte-identical across the mod family) rebuilds
after any turn that changed mod files and feeds a failing build back to the agent.
**Warnings are build errors** (`TreatWarningsAsErrors`, severities pinned in `.editorconfig`):
fix the code, not the severity.

`Tests/1.6/` is a headless xUnit (net472) suite, never shipped: `dotnet test
Tests/1.6/OverlordWarcasket.Tests.csproj` (vstest hosts it through mono on Linux/WSL). Its csproj
documents where the game's real 1.6 runtime DLLs come from (`RIMWORLD_MANAGED_1_6`, a 1.6
install, or the frozen copy at `~/.local/share/RimWorld-1.6/Managed`); without them the build
stays green but every test touching game types fails. `CompatPatchTests` resolves every xpath in
`1.6/Mods/*/Patches/` and the SOS2 patch against the main-tree defs (the Odyssey root's against
the DLC's defs in the local install), the only check a gated root gets short of a boot with that
mod active; `CerebrexNodeArtTests` ties that root's patch to its switch, the path constant and
the shipped textures; the other two files cover the settings slider arithmetic and the trait
mirror's tag rule.

## Layout

```
About/            Mod metadata, ModIcon.png, Preview.png
1.6/              Defs, Patches, Languages/English/Keyed, Mods/<compat roots>; serves 1.6 and 1.7
Mods/             Art compat roots, version-independent (Odyssey: the cerebrex node's art)
Source/1.6/       C# (net472)
Textures/         Art, version-independent: pawn art under VFEP's Warcasketlike/ layout
Tests/1.6/        xUnit net472 suite                        (not shipped)
Scripts/          l10n shims, the sidecar, the smoke test and the preview GIF build (not shipped)
l10n/             rimworld-l10n toolkit, pinned submodule   (not shipped)
Docs/Research/    VFEP warcasket stat tables and ability mechanics (copied from the siblings), the preview GIF guide
Docs/Specs/       Design specs behind the landed features
Docs/Art/         Art the artist delivered but the set does not ship (an alternate facing)
.steamworkshop/   Workshop page text and images             (not shipped)
LoadFolders.xml   Load roots per game version, compat roots gated by package id
```

## Cross-cutting rules

- **One def per file**, named after the def with the `OLWC_` prefix stripped.
- **Log with the `[Overlord Warcasket]` prefix.** Root namespace `OverlordWarcasket`.
- **Player-facing text follows `l10n/languages/English.md`** (def labels and descriptions,
  `Keyed/`, `About.xml`, `.steamworkshop/`): read it before writing or changing any. Its
  mechanical rules (no dashes, semicolons, "Whether" openers or parenthetical asides) are
  checked by `Scripts/check-translations.py` as `[player text]` warnings, which `--strict` turns
  into release errors. Code comments, def comments and these guides are unaffected. American
  spelling, as vanilla uses.
- **Settings override def fields, never code paths**, written onto the defs at startup and on
  window close, so defaults come from the XML and the def header stays the record.
- **Every startup read or write of defs goes through `Startup.Run`**, idempotent and re-run on
  every in-process play-data reload; never a bare static ctor.
- **Biotech is a hard dependency, by decision.** The resurrect mech ability is built from
  Biotech's defs (its sounds, effecters, icon, mech weight classes and the apocriton's rules), and
  the set exists to command mechs. Biotech content goes in the main tree like everything else;
  there is no Biotech load-folder gate and references to Biotech defs need no `MayRequire`.
- **English is the source of truth for text.** Translations come from the `/translate` skill,
  one language at a time, shortly before a release and never earlier.
- **`.claude/` is partly tracked** (`skills/`, `hooks/`, `settings.json`; the rest is personal).
  Editing a skill is a team-visible change that must keep in step with what it automates.

## Releases and debugging

Run the `/release` skill. It encodes the CHANGELOG layout, the version scheme (`X.Y.Z-rc.N`
candidates are CHANGELOG-less and Workshop-less prereleases) and the pre-release checks;
`.github/workflows/release.yml` fails a stable release whose CHANGELOG section is missing.

The `rimworld-logs` skill finds `Player.log` on 1.6 and 1.7 and disassembles the game, VFEP and
VEF assemblies; the local install's assembly, now the 1.7 one, is the source of truth over the
Krafs package. In game, VFEP and Vanilla Expanded Framework must load before this mod, which
appears as `OverlordWarcasket` in the list.
