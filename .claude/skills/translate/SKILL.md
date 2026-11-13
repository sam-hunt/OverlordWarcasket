---
name: translate
description: Generate, update, or audit mod localization (DefInjected plus one Keyed file) for a target language, grounded in Vanilla Factions Expanded - Pirates warcasket terminology plus vanilla Core, Biotech and Odyssey apparel and mechanitor terminology for Overlord Warcasket's single warcasket set. Use when asked to add a language, update translations, or check translation freshness.
argument-hint: "[language, e.g. German | update | check]"
---

# Translate

Produce or refresh localization files for Overlord Warcasket. English is the
source of truth; every other language derives from it.

**Translation passes are deferred until shortly before release** and run
only on explicit request, one language at a time (see `l10n/process.md`'s
non-negotiables). Generating the key expectations sidecar is always fine;
generating translations is not, until the English text is final.

**The family-wide process lives in the `l10n/` submodule, load these first,
and only these** (progressive disclosure; if `l10n/` is empty, run
`git submodule update --init`):

- `l10n/process.md`, non-negotiables, file/format conventions, terminology
  grounding method, and the generation / update / audit workflows. This is
  the workflow authority; follow it step by step.
- `l10n/languages/<Language>.md`, the target language's engine mechanics,
  style rules, and vanilla-grounded common vocabulary. Read ONLY the target
  language's file.
- `glossary/<Language>.md` (beside this file), this mod's own coined-term
  table for the target language. Read it in the same pass. Every file is
  seeded: its inherited rows are the sibling Shipcracker Warcasket repo's
  VFEP warcasket vocabulary (warcasket, parts, foundry, entomb, set names,
  the VEF ability strings), copied verbatim through the Medic Warcasket
  repo's seed so the sets read as one family in-game, and its closing list
  names the terms the first pass must still ground or coin. The pass fills
  that list in and never reopens an inherited row (a change there is made
  in the Shipcracker glossary first, then mirrored here and in the Toxin
  and Medic repos).
- `l10n/lessons.md`, cross-language lessons; read when generating a new
  language, skim otherwise. Its free-prose register lesson applies with
  full force here: the three descriptions are lore paragraphs with no
  vanilla sentence to mirror, so a separate register read of every
  description and Workshop sentence is part of every pass.
- `l10n/workshop.md`, the `.steamworkshop/` conventions, whenever the pass
  touches the Workshop description (every initial generation does).

**Where learnings land:** mod-independent findings (engine mechanics, a
language's grammar rule, corpus style facts) go in the `l10n/` submodule,
edit the canonical checkout at `~/dev/rimworld-l10n`, commit and tag there.
Mod-specific findings (coined terms, phrasing decisions) go in
`glossary/<Language>.md`.

**Before any pass, bump the pin:** run `l10n/tools/bump-consumer.sh` (fetches
upstream's release tags, checks out the latest, commits the pointer as `chore:
Bump l10n submodule vOLD -> vNEW`; no-op when already current). This is one of
the three moments a pin moves (release, pass start, new upstream major), never
per upstream commit. If it reports a MAJOR bump, read the upstream release
notes for the shim or flow edit this repo owes before continuing.

## This mod's translation surface

- **Two surfaces plus the Workshop page.** The defs' English is served by
  the def XML's own fields (DefInjected, below). Everything code prints is
  the one Keyed file, `1.6/Languages/English/Keyed/OverlordWarcasket.xml`,
  translated to `1.6/Languages/<Language>/Keyed/OverlordWarcasket.xml` with
  the same keys:
  - the mod settings window: `OLWC_SettingsCategory` (its header),
    `OLWC_SettingsUnavailable`, the section headers (`OLWC_SectionResurrect`,
    `OLWC_SectionWork`), and each setting's label and `...Desc` tooltip
    (charge capacity, charges recharged per day, reload cost per charge,
    corpse window, casting time (Core's `AbilityCastingTime`), cooldown, the
    mechlink switch). Labels carry no unit: the row's value shows it, as
    Core's short period form does (`LetterSecond`, `LetterHour`,
    `LetterDay`; a count as "1"). Each tooltip takes one `{0}`, the default
    value as the row shows it; keep it. The reload cost's also takes `{1}`,
    the reload item's plural label.
    The switch's tooltip takes none; `OLWC_SwitchDefault` takes `{0}`, the
    `OLWC_SwitchOn` / `OLWC_SwitchOff` word already coloured.
    `OLWC_ResurrectSummary` takes `{0}` the charge capacity, `{1}` the
    recharge clause (`OLWC_SummaryRegen` with `{0}` the charges per day, or
    `OLWC_SummaryNoRegen`, either one then wrapped by `OLWC_SummaryReload`
    with `{0}` that clause and `{1}` the reload cost, such as "1 advanced
    component"), `{2}` the cost list (each `OLWC_SummaryCost` with
    `{0}` the weight class label, lowercase, in the game's own translation,
    and `{1}` the charges, joined with Core's comma-and list), `{3}` the
    corpse window, `{4}` the casting time and `{5}` the cooldown, each
    already carrying its unit letter; `OLWC_ResurrectSummaryNoCooldown`
    takes `{0}` to `{4}` only. `OLWC_DefaultSuffix` keeps its leading space.
  - the resurrect mech's runtime strings. `OLWC_NoCharges` (the disabled
    reason, no placeholder); `OLWC_NotMechanitor` takes `{0}` the pilot's
    name; `OLWC_NotMechCorpse`, `OLWC_CannotResurrectClass`,
    `OLWC_NotOurMech` and `OLWC_CorpseTooOld` (invalid targets) take `{0}`
    the dead mech's name; `OLWC_NotEnoughCharges` takes `{0}` the mech's
    name, `{1}` the cost and `{2}` the charges held;
    `OLWC_NotEnoughBandwidth` `{0}` the pilot's name and `{1}` the mech's
    ("bandwidth" as Biotech renders its `Bandwidth` key); `OLWC_ChargesLabel` (the
    cursor label) takes `{0}` the charges held and `{1}` the maximum, and
    `OLWC_TargetCost` (its second line) `{0}` the mech's name and `{1}` the
    cost; the gauge's tooltip is short "label: value" lines under Biotech's
    own `MechResurrectionCharges` header (never re-rendered here), each kept
    under about forty characters with its value filled in, since a tooltip
    is that wide and a wrapped stat line reads as two:
    `OLWC_GaugeRegen` takes `{0}` the charges per day, a plain number;
    `OLWC_GaugeNoRegen` none; `OLWC_GaugeReload` `{0}` the reload cost of a
    charge, a count and the item's label ("1 advanced component");
    `OLWC_GaugeAutoReload` `{0}` the charges the drag handle is set to and
    `OLWC_GaugeNoAutoReload` none; `OLWC_GaugeCorpseAge` `{0}` the window as
    a period and `OLWC_GaugeNoCorpseAge` none; `OLWC_GaugeCosts` none, then
    one `OLWC_GaugeCost` per weight class with `{0}` its capitalised label
    and `{1}` the charges, which the code bullets; `OLWC_GaugeDragHint`
    (the closing paragraph, free to wrap) none.
  - the carried reload's command beside the gauge: `OLWC_LoadCarried` (its
    label) takes `{0}` the reload item's plural label, as the language's own
    pluraliser gives it; `OLWC_LoadCarriedDesc` `{0}` that label and `{1}`
    the cost per charge, a count and the item's label ("1 advanced
    component"); `OLWC_CoreFull` none; `OLWC_NotEnoughCarried` `{0}` the
    pilot's name and `{1}` that cost.
  Name the set's things with the same renderings as the DefInjected text
  (resurrection core, charge, mechanitor, warcasket).
- **Enumerate the key set from `Scripts/expected-injections.json`, never
  from a Languages folder or by scanning `1.6/Defs/`.** The sidecar is a
  dump of what the live game walks; regenerate it (game closed) with
  `python3 Scripts/refresh-translation-expectations.py` whenever the
  checker reports it stale. Take the English source text for each
  `<!-- EN: -->` comment from the sidecar's `english` field.
- **Def type folders.** Never translate or place a non-`required` sidecar
  entry (texture paths such as `shieldTexPath`) in any language file.
  - `DefInjected/ThingDef/` for the three `VFEPirates.WarcasketDef`s
    (`OLWC_Warcasket_Overlord`, `OLWC_WarcasketShoulders_Overlord`,
    `OLWC_WarcasketHelmet_Overlord`): `label`, `description`,
    `shortDescription` (a VFEP field shown in the foundry's part picker;
    translate it like any other). The game rolls a def type without its own
    database into its base and the checker maps the element tag via
    `DEF_TYPE_ALIASES` in `Scripts/check-translations.py`; a `WarcasketDef`
    folder would never load.
  - `DefInjected/VEF.Abilities.AbilityDef/` (the full type name; it is its
    own def database, so no alias) for `OLWC_ResurrectMech`: `label`,
    `description`, `jobReportString` and `verbProperties.label` (VEF copies
    the label into the verb it generates for every ability; render it
    identically to `label`, as Shipcracker's languages do). The label is
    Biotech's own apocriton ability label, "resurrect mech"; render it as
    Biotech's translation does.
  - `DefInjected/TraitDef/` for `OLWC_WarcasketTrait_Overlord`:
    `degreeDatas.0.label` and `degreeDatas.0.description`. The label is
    VFEP's own trait label, `warcasket`, shown in the Bio tab's traits
    list, so it renders exactly as VFEP's does in the language, with no set
    name; the description uses `{PAWN_nameDef}` and the pronoun tokens like
    vanilla's trait descriptions. Nothing else on the trait is text: its work
    profile and effects are overwritten at startup from VFEP's trait, so no
    language file should carry them.
- **The three descriptions share their second and third paragraphs
  verbatim** (the refit lore and the overlord-series paragraph); only the
  first paragraph differs per part. Keep the shared paragraphs
  byte-identical across the three defs in every language, and keep each
  def's `shortDescription` identical to its description's first paragraph,
  as the English does. Paragraph breaks are the literal two-character
  `\n` sequences the def XML uses.
- **Compat roots carry no strings.** `1.6/Mods/VanillaGravshipExpanded/`
  and `1.6/Mods/CombatExtended/` ship only patches with no translatable
  fields. Everything lands in the main `1.6/Languages/<Language>/` tree
  today. If a gated root ever gains a labelled def, its DefInjected must
  move into that root's own `Languages/` with a gate-suffixed filename (see
  `1.6/CLAUDE.md`'s gating section), never the main tree.
- **Workshop page:** `.steamworkshop/Description/<Language>.txt`, per
  `l10n/workshop.md` and the folder's own `README.md`. The title's anchor
  term is "warcasket"; every localized title must contain the rendering of
  "warcasket" recorded in that language's glossary. The title is coupled to
  the Keyed `OLWC_SettingsCategory` (the settings window's header,
  `WORKSHOP_TITLE_KEY`), so translate the two together.

## This mod's grounding domain

Domain mod: **Vanilla Factions Expanded - Pirates (VFEP)**, plus vanilla
Core, Biotech and Odyssey. **VFEP ships English only**, so its warcasket
vocabulary is not available from VFEP itself for any other language. For
each target language, check first whether a community "Vanilla Expanded"
translation covers VFEP in that language and ground terms against it; where
none does, coin the term and record it in `glossary/<Language>.md` rather
than inventing silently at translation time. The sibling Shipcracker
Warcasket repo's glossaries already record that search per language; start
there (see `glossary/README.md`).

Terms that MUST be grounded before use:

- from VFEP's own vocabulary: "warcasket" itself, the VFEP set names our
  text or Workshop page references (controller, siegebreaker), "warcasket
  foundry", "shoulders" / "pauldrons", "helmet", "shell";
- from vanilla Core (ground against the Core tar per `l10n/process.md`):
  apparel terms (armor, helmet, shoulder pads), plasteel, uranium, spacer
  tech level, energy shield / shield bubble, "charges" (Core's Keyed
  `Charges`), "cooldown", the work types the settings name (crafting,
  smithing, and the Work tab's short labels);
- from vanilla Biotech (ground against the Biotech tar): mechanitor,
  mechlink, mechanoid / mech, the weight classes light / medium / heavy,
  "resurrect mech" and "Mech resurrection charges" (its own strings, reused
  verbatim where the game prints them), corpse;
- from vanilla Odyssey (ground against the Odyssey tar): vacuum;
- this mod's own vocabulary, coined once and recorded in the glossary:
  resurrection core (the armor's charge store), the command visor and
  signal processors (the helmet's and pauldrons' lore).

Grep the tars for just this handful of terms; never extract or read a
whole tar. The vanilla-grounded answers for common words live in
`l10n/languages/<Language>.md`; this mod's own coined terms and VFEP-term
decisions live in `glossary/<Language>.md`.

## Workflows

Follow `l10n/process.md`'s Initial generation / Update pass / Audit-only
workflows verbatim. This mod's specifics on top:

- The checker: `python3 Scripts/check-translations.py` (`--strict` for new
  languages). Sidecar regen: `python3
  Scripts/refresh-translation-expectations.py` (game must be closed; drives
  the deployed L10nProbe, which must have this mod ticked in its settings).
- There is no compat-root routing to do today (see above); everything,
  Keyed included, lands in the main tree.
- The public roster is CONTRIBUTING.md's localization table, update it in
  the same commit as any language addition or native review.
- Machine-assisted passes are run as one Opus subagent per language with a
  bounded brief (the key list, the grounded VFEP and Biotech terms, the
  language file and glossary, the register gate); the lead reviews every
  diff and owns the commit.
