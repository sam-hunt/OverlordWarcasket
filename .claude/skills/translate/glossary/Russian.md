# Russian glossary, Overlord Warcasket

Seeded from the Shipcracker Warcasket glossary (machine-assisted there,
pending native review); no translation pass has run here yet. Only
mod-specific coinages and grounding decisions; family-wide mechanics and
style live in `l10n/languages/Russian.md`.

VFEP grounding source: the community Russian translation of VFE Pirates,
OneCodeUnit/VanillaRussianExpanded on GitHub (Vanilla Russian Expanded). Its
set pattern is an adjective on `броня мертвеца`; a named set's shoulders and
helmet drop `мертвеца` (`наплечники прорывной брони`, `шлем прорывной брони`).
This mod follows that pattern, and takes the pack's Siegebreaker
adjective and its 10th-generation paragraph.

## Inherited rows (shared with Shipcracker Warcasket; change them there first)

| English | Russian | Grounding |
|---|---|---|
| warcasket | броня мертвеца | Community VFEP pack (`VFEP_Warcasket_Warcasket.label`). No separate word for "shell"; it is dropped in the part descriptions, as the pack does. |
| `<set>` warcasket / shoulders / helmet (label pattern) | `<set adj.>` броня мертвеца / наплечники `<set adj. gen.>` брони / шлем `<set adj. gen.>` брони | Pack pattern for named sets (Siegebreaker: `прорывная броня мертвеца`, `наплечники прорывной брони`, `шлем прорывной брони`). |
| warcasket foundry | цех мертвеца | Community VFEP pack (`VFEP_WarcasketFoundry.label`). Foundries in general are `цеха`. |
| entomb / weld into (the set) | заковать в броню мертвеца | Community VFEP pack (`VFEPirates.EntombInWarcasket`). |
| Siegebreaker / Brute / Guardian (set adjectives) | прорывная / беспощадная / охранная | Community pack set adjectives. |
| spacer-tech (warcasket) | космического уровня | Core `TechLevel_Spacer` = `космический`; the pack's spacer sets say `сделанная по технологиям космического уровня`. |
| chassis (the warcasket frame) | каркас | The pack's word for the warcasket frame. |
| Imperial | имперский | Royalty (`имперский дезертир`). |
| plasteel / uranium / chemfuel | пласталь / уран / химтопливо | Core; short form as in Odyssey and the pack (`химтоплива`). |
| jobReportString (`Using ability: {0}`) | применяет способность «{0}» | Shipcracker's rendering (no trailing period, guillemets around the label). |
| cooldownGerund (`on cooldown`) | перезаряжается | Shipcracker's rendering of the same vanilla comp field. |
| Settings: "Default: {0}." / " (default)" / Off | По умолчанию: {0}. / (по умолчанию) / Выкл | Core Keyed `Default`, `default`, `Off`. |
| Settings: cooldown / range | перезарядка / дальность | Core Keyed `CooldownTime`, `Range`. Slider units read `(в клетках)`, `(в часах)`, `(%)`. |
| Settings: quantities with the fuel name | `{0} ед. ({1})`, `Ёмкость бака ({0})` | Core uses `{0} ед.` for mass quantities (`LowMedicineDesc`); the fuel name stays nominative in parentheses, so no numeral or case agreement is needed. |
| (requires Odyssey) | (требуется Odyssey) | Core `ExpansionDef` keeps `Odyssey` in Latin; `требуется` as in Core `SkillNeededForConstructing`. |
| Workshop title | `<Set adj.>` броня мертвеца, sentence case | Contains `броня мертвеца`. Also the `OLWC_SettingsCategory` value (title coupling). |

## Charge nouns

This set has no reloadable today and so no `chargeNoun` of its own: the
resurrection core recharges on its own and its charges are counted by Biotech's
own strings. If a reloadable piece is ever added, its noun is shown only through `{CHARGENOUN_plural}` and `{lookup: ...}` slots (see the
skill), and the Russian worker reads the `plural` table and the genitive slots go through `case` lookups, so every coined noun ships a `plural.txt` line and `case.txt` rows for both numbers under `1.6/Languages/Russian/WordInfo/`. Shipcracker's "fuel unit" row in its glossary
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
  "Charges" Keyed word), the command visor and signal processors (the
  helmet's and pauldrons' lore), the work-type names the settings quote (Core's work-tab labels).
- Core work vocabulary for the settings window: crafting, smithing, the Work
  tab's labelShort forms as the language's Core files render them.
- This mod's coinages, once the descriptions are final.

## Pending native review

Nothing inherited is open for the VFEP rows (they are the community pack's).
Add this mod's own coinages here as the pass makes them.
