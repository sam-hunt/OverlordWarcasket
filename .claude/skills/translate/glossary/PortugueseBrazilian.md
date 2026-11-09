# Brazilian Portuguese glossary: Overlord Warcasket

Seeded from the Shipcracker Warcasket glossary (machine-assisted there,
pending native review); no translation pass has run here yet. Only
mod-specific coinages and grounding decisions; family-wide mechanics and
style live in `l10n/languages/PortugueseBrazilian.md`.

Community VFEP translation grounded against: RafaelNaymaier's
"Tradução Vanilla Expanded PT-BR" (GitHub `RafaelNaymaier/Traducao-Vanilla-Expanded-PT-BR`,
Workshop 3643091941), confirmed from its ThingDef and Keyed files. It also
carries the Siegebreaker set and the 10th-generation paragraph.

## Inherited rows (shared with Shipcracker Warcasket; change them there first)

| English | Brazilian Portuguese | Grounding |
|---|---|---|
| warcasket | carcaça de guerra | Community VFEP pack throughout (`VFEP_Warcasket_*.label`, `VFEP.RequiresWarcasket`). "shell" in the English first line folds into the same noun, as the pack does. |
| `<set>` warcasket / shoulders / helmet (label pattern) | carcaça de guerra `<set>` / ombreiras de carcaça de guerra `<set>` / capacete de carcaça de guerra `<set>` | Pack label pattern (`carcaça de guerra quebra-cerco`, `ombreiras de carcaça de guerra quebra-cerco`, `capacete de carcaça de guerra quebra-cerco`). |
| pauldrons / shoulders | ombreiras | Pack uses `ombreiras` for both the label and the "pauldrons" in descriptions. |
| warcasket foundry | fundição de carcaças de guerra | Pack's `VFEP_WarcasketFoundry` label. |
| entomb / welded into (Workshop) | integrar (em carcaça de guerra) | Pack's `VFEPirates.EntombInWarcasket` = "Integrar em carcaça de guerra". "welded" uses the same verb, since "soldado" reads as "soldier". |
| siegebreaker / brute / guardian (set names) | Quebra-cerco / Bruto / Guardião | Pack's lore paragraphs ("A série Quebra-cerco"). |
| Specialized spacer-tech warcasket ... | Carcaça de guerra de tecnologia espacial especializada ... | Verbatim opening of the pack's 10th-gen descriptions; the 10th-gen opener is this set's too. |
| respirator | respirador de bordo | Pack's `telemetria de bordo` for "onboard". |
| wearer | portador | Core `Apparel_ShieldBelt.description`. |
| plasteel / uranium / chemfuel | plastiaço / urânio / combustível químico | Core material labels. The pack writes "plastoaço" in lore; vanilla's label wins. |
| Imperial | imperial (lowercase adjective) | Royalty `Empire.pawnSingular`. |
| jobReportString (`Using ability: {0}`) | usando habilidade: {0}. | Shipcracker's rendering. |
| cooldownGerund (`on cooldown`) | em recarga | Shipcracker's rendering of the same vanilla comp field. |
| cooldown (settings) | tempo de recarga | Royalty Keyed `Cooldown`. Core's `CooldownTime` "Esfriamento" is the weapon sense, not an ability's. |
| cells (unit) / radius | células / raio | Core Keyed `MapSizeDesc` "Células", `AbilityEffectRadius` "Raio do efeito". |
| Off / default / Odyssey | Desligado / padrão / Odyssey | Core Keyed `Off`, `Default`; Core `ExpansionDef` `Odyssey.label` keeps the English name. |
| Workshop title | Carcaça de Guerra `<Set>`, Title Case | Contains the pack's "carcaça de guerra". Also the `OLWC_SettingsCategory` value (title coupling). |

## Charge nouns

This set has no reloadable today and so no `chargeNoun` of its own: the
resurrection core recharges on its own and its charges are counted by Biotech's
own strings. If a reloadable piece is ever added, its noun is shown only through `{CHARGENOUN_plural}` and `{lookup: ...}` slots (see the
skill), and Portuguese runs the base `Pluralize` (lookup, else the word unchanged), so every coined noun ships a `singular;plural` line in `1.6/Languages/PortugueseBrazilian/WordInfo/plural.txt`. Shipcracker's "fuel unit" row in its glossary
is the worked example for this language.

## To ground in the first pass

- the trait label `warcasket` (Bio tab): VFEP's own trait label, so render it
  exactly as the community pack renders VFEP's, else the bare inherited
  warcasket noun; the description reuses the pilot and pain vocabulary
  settled for the armor.
- controller (the chassis, the Workshop page): the inherited set-name row above.
- the 10th-generation paragraph: no community pack to mirror; follow the meaning, not VFEP's dashes.
- overlord (this set's name) and the Workshop title.
- pilot (the wearer, every description), where no inherited row covers it.
- Biotech's mechanitor vocabulary, grounded against the Biotech tar: mechanitor,
  mechlink, mechanoid / mech, bandwidth, the three weight classes (light, medium,
  heavy) as `MechWeightClassDef` labels, "resurrect mech" (the apocriton's ability
  label, which this ability reuses) and "Mech resurrection charges" (Biotech's
  Keyed `MechResurrectionCharges`, the gauge's title; never re-rendered here).
- resurrection core (the armor's charge store), charge / charges (vanilla's
  "Charges" Keyed word), the uplink (the pauldrons' and helmet's lore), bench
  work and the work-type names the settings quote (Core's work-tab labels).
- Core work vocabulary for the settings window: crafting, smithing, the Work
  tab's labelShort forms as the language's Core files render them.
- This mod's coinages, once the descriptions are final.

## Pending native review

Nothing inherited is open for the VFEP rows (they are the community pack's).
Add this mod's own coinages here as the pass makes them.
