# French glossary, Overlord Warcasket

Seeded from the Shipcracker Warcasket glossary; no translation pass has run
here yet. Only mod-specific coinages and grounding decisions; family-wide
mechanics and style (the elision worker, where `PostProcessed` runs, register
tells) live in `l10n/languages/French.md`. Folder `French`, grounded against
the Core, Royalty and Biotech `French (Français).tar`s.

VFEP grounding: VFEP ships English only and no community French translation
survives (the "Vanilla Factions Expanded - Pirates [Fr]" Workshop item,
id 2725064005, has been removed), so a French player sees "siegebreaker
warcasket" and "warcasket foundry" in English in-game.
"warcasket" is therefore kept untranslated, as vanilla keeps "chemfuel": the
Shipcracker repo's native-speaker PR (#1, Joedox) replaced the earlier
"sarcophage", which matched nothing the player sees. Every other VFEP term
below (foundry, parts, research) is coined there and inherited here.

**Contractions are written by hand.** DefInjected values never pass through
`LanguageWorker_French.PostProcessed`, and hand-contracted text is inert
under it either way; the Workshop page is never post-processed at all.

## Inherited rows (shared with Shipcracker Warcasket; change them there first)

Rows marked *native* were settled by the Shipcracker native review and are
not reopened here.

| English | French | Grounding |
|---|---|---|
| warcasket | warcasket (m., pl. warcaskets; invariable as a modifier: `coque warcasket`) | *native*. Untranslated, matching VFEP's English labels in a French game (see above). Also the Workshop title anchor. |
| `<set>` warcasket / shoulders / helmet (label pattern) | warcasket `<set>` / épaulières de warcasket `<set>` / casque de warcasket `<set>` | *native* order. Core/Royalty apparel pattern `casque de cataphracte`, `armure de commando` (part `de` class noun); the set adjective stays invariable after the noun. |
| warcasket shell (torso) | coque warcasket | *native* (replaced the coined `carapace`); the `warcasket` modifier keeps it apart from a ship's `coque`. |
| pauldrons / shoulders | épaulières | **Coined** (no vanilla shoulder apparel; Core only has `épaule` body parts). Ordinary French armor term. Plural label: `[X_definite]` agreement may default masculine singular in vanilla keyed strings. |
| helmet | casque | Core `casque de commando`. |
| spacer / spacer-tech | spatial / de technologie spatiale | *native*. Core `TechLevel_Spacer` = `spatial`; `de technologie spatiale` for the attributive "spacer-tech". |
| warcasket foundry | fonderie de warcaskets | **Coined.** |
| a VFEP research label in prose (`spacer warcaskets`, `specialised warcaskets`) | « spacer warcaskets » (warcaskets spatiaux) | The English label in guillemets, because that is the clickable project every French player sees (VFEP is untranslated), with the French gloss in parentheses. Render `specialised warcaskets` the same way. |
| weld into / welded (entomb) | souder / soudé dans | Community VFEP Workshop page ("se soudent à l'intérieur"). |
| VFEP set names in prose | "ensemble" for set; names capitalized as proper names on the Workshop page only | **Coined** pattern. |
| onboard respirator | respirateur intégré | **Coined**, plain word (no vanilla French respirator item; Biotech has only `masque à gaz`). |
| sealed | étanche | Odyssey `GravshipHull.description` (`paroi étanche`); fine without Odyssey, it is plain French. |
| Imperial | impérial | Royalty `Empire.pawnSingular`. |
| plasteel / uranium / chemfuel | plastacier / uranium / chemfuel | Core labels; `chemfuel` stays untranslated as vanilla does. |
| ability description register | third-person present (`Allume les propulseurs...`) | *native*. The majority vanilla form (Royalty 23 third-person vs 8 infinitive); Biotech's infinitive `Longjump` is the outlier. |
| jobReportString (`Using ability: {0}`) | utilise la capacité : {0}. | Third-person present like vanilla `JobDef.reportString`s, French-spaced colon, trailing period kept. |
| cooldownGerund (`on cooldown`) | rechargement | Shipcracker's rendering of the same vanilla comp field. |
| settings window (Keyed) | Par défaut : {0}. / (par défaut) / Désactivé / (nécessite Odyssey) | Core Keyed `Default` = `Par défaut`, `Off` = `Désactivé(e)`; the DLC keeps its English name (Odyssey `ExpansionDef` label `Odyssey`). |
| cooldown / range / cells / hours | temps de recharge / portée / cases / heures | Core `FlightCooldown` = `temps de recharge du vol`, Keyed `Range` = `Portée`, `HoursLower` = `heures`; `case` is vanilla's map-tile word. |
| Workshop title | sentence case, contains `warcasket` | Also the `OLWC_SettingsCategory` value (title coupling). |

## Charge nouns

This set has no reloadable today and so no `chargeNoun` of its own: the
resurrection core recharges on its own and its charges are counted by Biotech's
own strings. If a reloadable piece is ever added, its noun is shown only through `{CHARGENOUN_plural}` and `{lookup: ...}` slots (see the
skill), and `LanguageWorker_French.Pluralize` is rule-only and never reads a `WordInfo/plural.txt`, so a charge noun must be a single word. Shipcracker's "fuel unit" row in its glossary
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

Still open from the Shipcracker review, inherited as-is: `fonderie de
warcaskets`, `épaulières`, and the set-name capitalization pattern. Add this
mod's own coinages here as the pass makes them.
