# French glossary, Overlord Warcasket

Seeded from the Shipcracker Warcasket glossary; first machine-assisted pass
2026-11-16, no native review yet. Only mod-specific coinages and grounding decisions; family-wide
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

The armor's mech resurrector is a reloadable (`CompMechResurrector`, a subclass
of vanilla's `CompApparelReloadable`), so the set has one `chargeNoun` to
translate: `OLWC_Warcasket_Overlord.comps.CompMechResurrector.chargeNoun`,
"charge". That is vanilla's own noun (Core's `Apparel_PsychicShockLance`
carries the same `chargeNoun`), so copy the language's vanilla rendering of it
rather than coining one: the same word resolves through the same lookups as on
vanilla's items and needs no lookup file of this mod's. The gauge's title stays
Biotech's `MechResurrectionCharges`. Should a coined noun ever replace it, it is
shown only through `{CHARGENOUN_plural}` and `{lookup: ...}` slots
(`l10n/lessons.md`), and `LanguageWorker_French.Pluralize` is rule-only and never reads a `WordInfo/plural.txt`, so a charge noun must be a single word. Shipcracker's "fuel unit" row in its glossary
is the worked example for this language.

## Grounded or coined in the first pass (2026-11-16)

| English | French | Grounding |
|---|---|---|
| overlord (set name) | suzerain; labels `warcasket suzerain` / `épaulières de warcasket suzerain` / `casque de warcasket suzerain`; prose `le Suzerain`, `la série Suzerain` | **Coined**, the plain French for overlord, translated like the siblings' `médecin`, `toxine`, `brise-coque` (Biotech keeps `mechlord` English, a different word). |
| Workshop title / `OLWC_SettingsCategory` | Warcasket suzerain | Sibling pattern `Warcasket médecin`. |
| pilot | pilote | Inherited family row. |
| mechanitor | méchanitor | Biotech `PawnKindDef Mechanitor.label`, `ConceptDef Mechanitors`. |
| mechlink | mécalien | Biotech `Mechlink.label`. |
| mech / mechanoid | mécanoïde everywhere (Biotech also says `mech`; one word kept for consistency) | Biotech `ResurrectionMech`, `RepairMech` WorkGiver. |
| bandwidth | bande passante | Biotech `MechBandwidth.label` (Keyed gizmo `Bande conso` not used in prose). |
| weight classes | léger / moyen / lourd / ultra-lourd, used as nouns after `{0_indefinite}` (`1 pour un léger`) | Core `MechWeightClassDef`; all masculine, so `un` is always right. |
| resurrect mech | ressusciter mécanoïde (label, verb label, section header) | Biotech `ResurrectionMech.label` verbatim. |
| mech resurrector | résurrecteur de mécanoïdes, short `résurrecteur` | **Coined.** |
| charge (chargeNoun) | charge | Core `Apparel_PsychicShockLance` chargeNoun. |
| command relays | relais de commande | **Coined** on Odyssey `relais orbital` and Biotech `portée de commande`. |
| signal enhancers | amplificateurs de signal | Biotech `Apparel_ArmorHelmetMechCommander.description`, `BandNode.description`. |
| command range | portée de commande | Biotech Keyed `OutOfCommandRange`. |
| drafted | mobilisé | Core Keyed `IsNotDrafted`. |
| control group | groupe de contrôle | Biotech Keyed `ControlGroup`. |
| hacking | piratage | Core `HackingSpeed.label`, `Hack.reportString`. |
| work names (trait text / summary) | recherche, forgeage, piratage / labelShort chercheur, forgeron, artisan, couturier, constructeur | Core `WorkTypeDef` gerundLabel / labelShort. |
| mech gestation / subcore encoding / mech repair / shredding | gestation des mécanoïdes / encodage de sous-noyaux / réparation des mécanoïdes / déchiquetage à l'établi d'assemblage | Biotech `SubcoreEncoder`, `RepairMech`; Core `ButcherCorpseMechanoid`, `TableMachining`; gestation coined. |
| advanced component | composant avancé | Core `ComponentSpacer.label`. |
| apocriton | apocrite | Biotech `Mech_Apocriton.label`. |
| aim | précision de tir | Core `ShootingAccuracyPawn.label`. |
| ultra mechtech | « mechtech de pointe » | Biotech `UltraMechtech.label`, a clickable project in guillemets. |
| cerebrex node / dye (noun, verb) | nœud cérébrex / colorant, teindre | Odyssey `Apparel_CerebrexNode.label`; Core `Dye.label`, `Dye.description`. |
| art (cerebrex setting) | apparence | **Coined**, `Nouvelle apparence, cadre teint`. |
| mech bosses | boss mécanoïdes | **Coined**; no vanilla French rendering found. |
| helmet/shoulders label in a `{0}` slot | `portant "{0}"` | ASCII-quoted cited label, so neither article nor agreement depends on casque (m.) vs épaulières (f. pl.). |
| (vanilla) suffix | (jeu de base) | **Coined.** |

## Pending native review

Still open from the Shipcracker review, inherited as-is: `fonderie de
warcaskets`, `épaulières`, and the set-name capitalization pattern. This
mod's coinages: `suzerain`, `résurrecteur de mécanoïdes`, `relais de
commande`, `apparence` (cerebrex art), `boss mécanoïdes`, `(jeu de base)`,
`gestation des mécanoïdes`, and the elliptical `{1} pour un léger` summary
fragment.
