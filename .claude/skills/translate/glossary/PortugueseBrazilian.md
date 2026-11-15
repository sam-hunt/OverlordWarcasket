# Brazilian Portuguese glossary: Overlord Warcasket

Seeded from the Shipcracker Warcasket glossary (machine-assisted there,
pending native review); the first machine-assisted pass ran 2026-11-16,
no native review yet. Only
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

The armor's mech resurrector is a reloadable (`CompMechResurrector`, a subclass
of vanilla's `CompApparelReloadable`), so the set has one `chargeNoun` to
translate: `OLWC_Warcasket_Overlord.comps.CompMechResurrector.chargeNoun`,
"charge". That is vanilla's own noun (Core's `Apparel_PsychicShockLance`
carries the same `chargeNoun`), so copy the language's vanilla rendering of it
rather than coining one: the same word resolves through the same lookups as on
vanilla's items and needs no lookup file of this mod's. The gauge's title stays
Biotech's `MechResurrectionCharges`. Should a coined noun ever replace it, it is
shown only through `{CHARGENOUN_plural}` and `{lookup: ...}` slots
(`l10n/lessons.md`), and Portuguese runs the base `Pluralize` (lookup, else the word unchanged), so every coined noun ships a `singular;plural` line in `1.6/Languages/PortugueseBrazilian/WordInfo/plural.txt`. Shipcracker's "fuel unit" row in its glossary
is the worked example for this language.

## Grounded and coined in the first pass (2026-11-16)

| English | Brazilian Portuguese | Source or reason |
|---|---|---|
| overlord (set name) | Suserano, invariant noun like Guardião: `carcaça de guerra suserano`, `A série Suserano`, `O Suserano` | Coined. StarCraft's pt-BR "Suserano" for Overlord; noun kept invariant per the VFEP set-names row |
| Workshop title / `OLWC_SettingsCategory` | Carcaça de Guerra Suserano | Workshop title row |
| pilot | piloto | VFEP section row |
| mechanitor | mecanizador | Biotech `MechlinkImplant.description`, `Mechanitors.helpText` |
| mechlink | meca-conexão | Biotech `Mechlink.label` |
| mech / mechanoid | mecanoide (both) | Biotech uses no short form (`RepairMech.label` "reparar mecanoide") |
| bandwidth / mech bandwidth | banda-larga / banda-larga mecanoide | Biotech Keyed `Bandwidth`, `MechBandwidth.label` |
| light / medium / heavy | leve / médio / pesado | `MechWeightClassDef` labels; `{0_indefinite}` gives "um leve" (masculine default, agrees with the elided mecanoide) |
| resurrect mech | ressuscitar mecanoide | Biotech `ResurrectionMech.label` |
| mech resurrector | ressuscitador mecanoide | Coined on Biotech's `gestador mecanoide` pattern |
| charge (chargeNoun) / charges (prose) | carga / cargas | Odyssey's reloadables (`Gun_AssaultRifle_Unique` and siblings, chargeNoun "carga") and Core Keyed `Charges` "Cargas". Core's `Apparel_PsychicShockLance` has the verb "carregar" there, a vanilla slip that is not copied. Ships `carga;cargas` in `WordInfo/plural.txt`. |
| resurrection | ressurreição | Correct spelling; Biotech's `MechResurrectionCharges` writes "ressureição" (typo, not copied) |
| command relays | retransmissores de comando | Coined on Odyssey `OrbitalRelay` "retransmissor orbital" |
| signal enhancers | amplificadores de sinal | Coined |
| command range | faixa de comando | Biotech Keyed `OutOfCommandRange` "Fora da faixa de comando" |
| control group | grupo de controle | Biotech Keyed `ControlGroup` |
| drafted | alistado | Core Keyed `CommandDraftLabel` "Alistar", `IsNotDrafted` |
| hacking / hack | hacking / hackear | Core `HackingSpeed.label`, `Hack.verb` |
| work names | pesquisa, forja, fabricação, costura, constr. | Core `WorkTypeDef` `labelShort` |
| mech gestation / subcore encoding / shredding | gestar mecanoides / codificar subnúcleos / destruir mecanoides | Biotech `SubcoreEncoder.label`, Core `ButcherCorpseMechanoid.label` |
| machining table | mesa de usinagem | Core `TableMachining.label` |
| apocriton | apocritão | Biotech `PawnKindDef` label |
| cerebrex node | nó de cerebrex (lowercase mid-sentence) | Odyssey `Apparel_CerebrexNode.label` "Nó de Cerebrex", cased like `CerebrexCore` |
| dye / to dye | corante / tingir | Core `Dye.label`, `Dye.description` |
| mech bosses / ultra mechtech | líderes mecanoides / ultra meca-tecnologia | Biotech Keyed `CommandCallBossgroupDesc`, `UltraMechtech.label` |
| power armor (lore) | armadura de fuzileiro | Core `Apparel_PowerArmor.label` |
| (vanilla) suffix | (vanilla) | Kept, common pt-BR gamer usage |

## Pending native review

Nothing inherited is open for the VFEP rows (they are the community pack's).
This mod's coinages: Suserano, ressuscitador mecanoide, retransmissores de
comando, amplificadores de sinal, the `{1} para {0_indefinite}` cost fragment,
and the feminine chargeNoun "carga" inside vanilla's masculine reload frames
("um carga"), which Odyssey's own reloadables share.
