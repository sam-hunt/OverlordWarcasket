# Spanish (Castellano) glossary, Overlord Warcasket

Seeded from the Shipcracker Warcasket glossary (machine-assisted there,
pending native review); the first machine-assisted pass ran 2026-11-16, no native review yet. Only
mod-specific coinages and grounding decisions; family-wide mechanics and
style live in `l10n/languages/Spanish.md`. Folder `Spanish`, grounded against
the Castilian tars only (never `SpanishLatin`, whose VFEP lineage uses
"sarcotraje").

VFEP grounding: the community Castilian-leaning VFE Pirates translation by
Cito2310 (GitHub `Cito2310/translate-project-rimworld`). Its label pattern is
`ataúd de guerra <set>` for the torso, `hombreras <set> de ataúd de guerra`
and `casco <set> de ataúd de guerra` for the other parts; this mod follows
it, and takes its Siegebreaker rendering and its 10th-generation paragraph.

## Inherited rows (shared with Shipcracker Warcasket; change them there first)

| English | Spanish | Grounding |
|---|---|---|
| warcasket | ataúd de guerra (pl. ataúdes de guerra) | Community VFEP (`VFEP_Warcasket_Warcasket.label`). Also the Workshop title anchor. |
| `<set>` warcasket / shoulders / helmet (label pattern) | ataúd de guerra `<set>` / hombreras `<set>` de ataúd de guerra / casco `<set>` de ataúd de guerra | Community VFEP label pattern (`ataúd de guerra bruto`, `hombreras bruto de ataúd de guerra`, `casco bruto de ataúd de guerra`). |
| warcasket shell / pauldrons (shoulders) / helmet | armazón / hombreras / casco | Community VFEP (`Un armazón de ataúd de guerra...`, `VFEP.Shoulderpads`, `VFEP.Helmet`). |
| warcasket foundry | forja de ataúd de guerra | Community VFEP `VFEP_WarcasketFoundry.label`. |
| entomb / welded into | sepultar / sepultado en | Community VFEP `VFEPirates.EntombingWarning` ("Sepultar colonos en un ataúd de guerra"). |
| specialized spacer-tech | especializado de tecnología espacial | Community VFEP 10th-gen descriptions. |
| VFEP set names in prose | Siegebreaker, Bruto, Guardián; "series" for set | Community VFEP (`La serie Bruto`); it leaves Siegebreaker in English and is inconsistent on `guardian`/`guardián`, so the accented form is used. |
| spacer (tech level) | era espacial | Core `TechLevel_Spacer`. |
| Imperial | imperial (lowercase adjective) | Royalty (`batallón imperial`). |
| sealed | hermético | Odyssey (`paredes y puertas herméticas`). |
| respirator | respirador (integrado / propio) | No vanilla Castilian item; plain word. |
| wearer | portador | Core (`Apparel_ArmorMarineHelmetPrestige.description`). |
| plasteel / uranium / chemfuel | plastiacero / uranio / biocombustible | Core labels. |
| jobReportString (`Using ability: {0}`) | usando habilidad: {0}. | Shipcracker's rendering. |
| cooldownGerund (`on cooldown`) | en enfriamiento | Shipcracker's rendering of the same vanilla comp field. |
| settings: tank / cells / cooldown | depósito / casillas / enfriamiento | `depósito` is plain Castilian for a fuel tank (no vanilla item). Core Keyed `{0} casillas`, `CooldownTime` = `Enfriamiento`. |
| settings: Off / default / Default: on | Desactivado / (por defecto) / Por defecto: activado | Core `Off` = `Apagado` is a power state; `Desactivado` mirrors `Enabled` = `Activado`. Core `default` = `por defecto`. |
| settings: Odyssey | Odyssey | Core `ExpansionDef` keeps DLC names in English. |
| Workshop title | Ataúd de guerra `<set>`, sentence case | Contains the VFEP term. Also the `OLWC_SettingsCategory` value (title coupling). |

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
(`l10n/lessons.md`), and the Spanish worker's rule pluralizes only the last word, so a multi-word noun ships a `singular;plural` line in `1.6/Languages/Spanish/WordInfo/plural.txt`. Shipcracker's "fuel unit" row in its glossary
is the worked example for this language.

## Grounded and coined in the first pass (2026-11-16)

| English | Spanish | Grounding or reason |
|---|---|---|
| overlord (set name) | soberano (`ataúd de guerra soberano`, `hombreras soberano de ataúd de guerra`, `casco soberano de ataúd de guerra`; prose `el Soberano`, `la serie Soberano`) | Coined. Noun and adjective in one word, invariable in apposition like `médico`, `bruto`. |
| Workshop title / `OLWC_SettingsCategory` | Ataúd de guerra soberano | Inherited title row. |
| pilot | piloto | Inherited VFEP row. |
| mechanitor / mechlink | mecanitor / mecaenlace | Biotech `NotAMechanitor`, `Mechlink.label`. |
| mech / mechanoid | meca (m., pl. mecas) / mecanoide | Biotech Keyed and `ResurrectionMech.description`. |
| bandwidth / mech bandwidth | ancho de banda / ancho de banda meca | Biotech `Bandwidth`, `MechBandwidth.label`. |
| light / medium / heavy (weight classes) | liviano / mediano / pesado | Core `MechWeightClassDef`; `{0_indefinite}` gives `un liviano`, so `OLWC_SummaryCost` reads `{1} por {0_indefinite}`. |
| resurrect mech (ability, section) | resucitar meca / Resucitar meca | Biotech `ResurrectionMech.label`. |
| resurrection | resurrección | Core usage. |
| mech resurrector | resucitador de mecas (short: el resucitador) | Coined. |
| charge / charges | carga / cargas | Core `chargeNoun`, Keyed `Charges`; vanilla noun, no WordInfo. |
| recharge (on its own) vs reload | recuperar (`Recuperación: {0} al día`) vs recargar | Coined split: Spanish `recargar` covers both, so self-recharge is `recuperar`. |
| command relays | repetidores de comando | Coined from Odyssey `repetidor orbital` and Biotech `alcance del comando`. |
| signal enhancers | potenciadores de señal | Coined. |
| command range | alcance de comando | Biotech `OutOfCommandRange` "Fuera de alcance del comando". |
| drafted | reclutado | Core `CommandDraftLabel` Reclutar. |
| control group | grupo de control | Biotech `ControlGroup`. |
| hacking / hack | hackeo / hackear | Core `HackingSpeed`, `Hack`. |
| work names (research, smithing, crafting, tailoring, construction) | investigar, forjar, fabricar, sastrería, construir | Core `WorkTypeDef.labelShort`; prose `trabajos de forja`. |
| mechtech research / ultra mechtech | investigación mecatrónica / tecnología meca ultra | Biotech chip descriptions, `UltraMechtech.label`. |
| shredding mechanoids | desarmar mecanoides | Core `ButcherCorpseMechanoid.label`. |
| mech bosses | jefes mecanoides | Coined (no vanilla noun). |
| cerebrex node / dye / undyed | nodo Cerébrex / tinte / sin teñir | Odyssey `Apparel_CerebrexNode.label`, Core `Dye.label`. |
| art (settings) | gráficos | Coined, reads better than `arte` for textures. |
| maximum corpse age | plazo (máximo) tras la muerte | Coined, fits the 40-character gauge line. |
| (vanilla) suffix | (juego base) | Coined; vanilla es has no "vanilla". |
| mech stun pulse (setting section) / pulse | pulso aturdidor de mecas / pulso | Coined on Core `pulsos PEM` and Biotech `pulso de señal meca`; parallels `resucitador de mecas`. |
| stun / apocriton / downed | aturdir / apocritón / quedar incapacitado | Core `Stun.label` aturdido, Biotech `Mech_Apocriton.label`, Core `MechControllerDowned`. |
| range (resurrect mech setting) | Alcance | Siblings' settings rows (`l10n/languages/Spanish.md` Range note); `Requiere línea de visión` from Core `Apparel_PsychicShockLance`. |

## Pending native review

Nothing inherited is open for the VFEP rows (they are the community pack's).
This mod's coinages: `soberano`, `resucitador de mecas`, `recuperar` for
self-recharge, `repetidores de comando`, `potenciadores de señal`, `jefes
mecanoides`, `gráficos`, `plazo tras la muerte`, `(juego base)`, and the
summary fragment `{1} por {0_indefinite}` (`1 por un liviano`).
