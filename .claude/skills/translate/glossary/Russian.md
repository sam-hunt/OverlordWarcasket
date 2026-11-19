# Russian glossary, Overlord Warcasket

Seeded from the Shipcracker Warcasket glossary (machine-assisted there,
pending native review); the first machine-assisted pass ran 2026-11-16,
no native review yet. Only
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

The armor's mech resurrector is a reloadable (`CompMechResurrector`, a subclass
of vanilla's `CompApparelReloadable`), so the set has one `chargeNoun` to
translate: `OLWC_Warcasket_Overlord.comps.CompMechResurrector.chargeNoun`,
"charge". That is vanilla's own noun (Core's `Apparel_PsychicShockLance`
carries the same `chargeNoun`), so copy the language's vanilla rendering of it
rather than coining one: the same word resolves through the same lookups as on
vanilla's items and needs no lookup file of this mod's. The gauge's title stays
Biotech's `MechResurrectionCharges`. Should a coined noun ever replace it, it is
shown only through `{CHARGENOUN_plural}` and `{lookup: ...}` slots
(`l10n/lessons.md`), and the Russian worker reads the `plural` table and the genitive slots go through `case` lookups, so every coined noun ships a `plural.txt` line and `case.txt` rows for both numbers under `1.6/Languages/Russian/WordInfo/`. Shipcracker's "fuel unit" row in its glossary
is the worked example for this language.

## Grounded and coined in the first pass (2026-11-16)

| English | Russian | Grounding or reason |
|---|---|---|
| overlord (set name) | командирская (`командирская броня мертвеца`, `наплечники командирской брони`, `шлем командирской брони`; prose `Командирская серия`) | Coined adjective on the pack's set pattern; Biotech's own `мехлорд` / `мехкомандир` are item names, so not reused |
| Workshop title / `OLWC_SettingsCategory` | Командирская броня мертвеца | Title row above |
| pilot | пилот | Family VFEP row |
| mechanitor | механитор | Biotech `Mechanitor.label` |
| mechlink | мехлинк | Biotech `Mechlink.label` |
| mech / mechanoid | механоид (both) | Biotech uses `механоид` throughout (`ResurrectionMech.description`) |
| bandwidth | пропускная способность | Biotech Keyed `Bandwidth`, `MechBandwidth.label` |
| light / medium / heavy | лёгкий / средний / тяжёлый, quoted as `класса «{0_indefinite}»` | Core `MechWeightClassDef` labels; the fragment needs no agreement |
| resurrect mech (ability) | воскрешение механоида | Biotech `ResurrectionMech.label`, verbatim; description opens with its description |
| mech resurrector | воскреситель механоидов (m.) | Coined |
| charge | заряд | Core `Apparel_PsychicShockLance` `chargeNoun`; in Core `WordInfo`, no file here |
| command relays | командные ретрансляторы | Coined on Odyssey `орбитальный ретранслятор` |
| signal enhancers | усилители сигнала | Coined (Odyssey `глушитель сигналов`, Biotech `сигнальный чип`) |
| command range | радиус управления | Coined; cells are `{0} кл`, after Core `MoveSpeed.formatString` `{0} кл/с`, so no numeral agreement and no numCase on a decimal range |
| drafted (mechs) | в режиме призыва | Core `IsNotDrafted`, Biotech `DraftMech.label` |
| control group | группа механоидов | Biotech `MechControlGroups.label` |
| hacking | взлом / взламывать | Core `HackingSpeed.label`, `Hack.reportString` |
| research / smithing (work) | исследовать, учёный / ковать, кузнец | Core `WorkTypeDef` `verb` and `labelShort` |
| mech gestation / subcore encoding / mech repair / shredding at a machining table | выращивание механоидов / создание субъядер / починка механоидов / переработка механоидов на верстаке | Biotech `GestatingMech`, `SubcoreEncodingSpeed`, `RepairMech`; Core `ButcherCorpseMechanoid`, `TableMachining` |
| advanced components | продвинутые компоненты | Core `ComponentSpacer.label` |
| ultra mechtech | «Ультратехнологии механоидов» | Biotech `UltraMechtech.label` |
| cerebrex node / dye | узел церебрекса / краситель | Odyssey `Apparel_CerebrexNode.label`, Core `Dye.label` |
| (vanilla) suffix | (обычный) | Coined; matches `обычный радиус` in the same section |
| reportString / jobReportString | воскрешает TargetA / воскрешает механоидов | Core reportString form, Core `Resurrect.reportString` verb |
| mech stun pulse / pulse | шоковый разряд / разряд (`испускает разряд`) | Biotech Keyed `MechStunPulseWarning` ("Mech stun pulse on death") = `Шоковый разряд при смерти`; Biotech `MechBandShockwave.label` = `механоидный разряд` |
| stun (verb) | оглушать | Core `Stun.label` = `оглушение` |
| downed | обездвижен | Core Keyed `DownedLower` |
| (the pilot) falls, death or downed | выходит из строя | Coined to cover both triggers; `падёт` would read as death only |

## Pending native review

Nothing inherited is open for the VFEP rows (they are the community pack's).
This pass's coinages: `командирская` (set name), `воскреситель механоидов`,
`командные ретрансляторы`, `усилители сигнала`, `радиус управления` with
`кл`, ` (обычный)`, and the 10th-generation lore paragraph wording
(`трофейные пиратские каркасы`).
