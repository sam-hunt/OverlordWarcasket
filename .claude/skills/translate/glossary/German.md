# German glossary, Overlord Warcasket

Seeded from the Shipcracker Warcasket glossary (machine-assisted there,
pending native review); the first machine-assisted pass ran 2026-11-16,
no native review yet. Only
mod-specific coinages and grounding decisions; family-wide mechanics and
style live in `l10n/languages/German.md`.

**No community VFEP German translation could be grounded against.** A
Workshop pack "Vanilla Factions Expanded - Pirates Deutsch" (id 2810180496)
exists, but its page shows none of its terms and its files were not
available. Every VFEP term below is therefore coined in the Shipcracker repo
and inherited here. If that pack's files turn up, re-ground against them in
the Shipcracker glossary first and prefer its renderings.

## Inherited rows (shared with Shipcracker Warcasket; change them there first)

| English | German | Grounding |
|---|---|---|
| warcasket (coined) | Kriegssarg (m.), pl. Kriegssärge | Literal war + coffin, mirroring the English compound and vanilla's war- → Kriegs- pattern (`MeleeWeapon_Zeushammer` = Kriegshammer). Does not collide with Core's coffin furniture (`Sarcophagus` = Sarkophag). Masculine, so the Gender-table miss default is correct. |
| compound rule for Kriegssarg | always hyphenated to a set name or part: `<Set>`-Kriegssarg, Kriegssarg-Helm, Kriegssarg-Hülle, Kriegssarg-Schulterpanzer, Kriegssarg-Gießerei | Durchkopplung once a set prefix is present; kept uniform in descriptions too so labels and prose match. |
| `<set>` warcasket / shoulders / helmet (label pattern) | `<Set>`-Kriegssarg / `<Set>`-Kriegssarg-Schulterpanzer / `<Set>`-Kriegssarg-Helm | VFEP's `<set> warcasket <part>` label pattern. |
| pauldrons / shoulders / shoulder pads | Schulterpanzer | Ordinary German armor term; "Schultern" alone reads as the body part. |
| warcasket shell | Kriegssarg-Hülle | Per the compound rule. |
| warcasket foundry (coined) | Kriegssarg-Gießerei | foundry = Gießerei per Core `FoundryApprentice76.title` = Gießereilehrling. |
| entomb (coined) | einsargen | Real German verb (to put into a coffin), derived directly from Kriegssarg. The Workshop page's "weld" stays einschweißen, as in English. |
| spacer warcaskets (VFEP research, cited in prose) | 'Raumfahrt-Kriegssärge' | Core `TechLevel_Spacer` = Raumfahrt; single-quoted as a cited label per the language file. Render `specialised warcaskets` in the same shape. |
| VFEP set names (coined) | Belagerungsbrecher (siegebreaker), Rohling (brute), Wächter (guardian), Kontrolleur (controller), Sarkophag (sarcophagus), Luft-Kriegssarg (aerial) | Plain renderings, pending native review there. |
| spacer-tech | aus der Raumfahrttechnik | Core `Plasteel.description` / `Synthread.description` render "spacer-tech" exactly so. |
| Imperial | imperial (imperiale, imperialen ...) | Royalty `PawnKinds_Empire` (imperialer Grenadier). |
| respirator / breathing | Atemgerät / Atmen, Atemleistung | Odyssey research story "Atemgeräten"; Core `Breathing` = Atmen. |
| sealed | luftdicht versiegelt | Odyssey `GravshipHull.description` "luftdichte Wand". |
| chemfuel | Sprit | Core's chemfuel label. |
| mod (noun) | die Mod | Core Keyed `ModLoadFolder...` "About.xml der Mod". |
| jobReportString (`Using ability: {0}`) | setzt Fähigkeit '{0}' ein. | Shipcracker's rendering. |
| cooldownGerund (`on cooldown`) | klingt gerade ab | Shipcracker's rendering of the same vanilla comp field. |
| Off / On / default (settings) | Aus / an / Standard | Core Keyed `Off`, `On`, `Default`; "When enabled/disabled" = Wenn aktiviert / Wenn deaktiviert after Core `CommandAllowCutDesc`. |
| cooldown / cells | Abklingzeit / Zellen | Core Keyed `CooldownTime`, `MapSizeDesc`. |
| Odyssey (DLC) | Odyssey | Core `ExpansionDef` `Odyssey.label` keeps the English name. |
| Workshop title | `<Set>`-Kriegssarg, byte-identical to the body label | Contains Kriegssarg. Also the `OLWC_SettingsCategory` value (title coupling). |

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
(`l10n/lessons.md`), and the German worker delegates to the English one (lookup, else `+s`) and Core's German `plural.txt` is thin, so any coined noun ships its own `singular;plural` line in `1.6/Languages/German/WordInfo/plural.txt`. Shipcracker's "fuel unit" row in its glossary
is the worked example for this language.

## Grounded or coined in the first pass (2026-11-16)

| English | German | Grounding or reason |
|---|---|---|
| overlord (set name) | Gebieter (m.); labels Gebieter-Kriegssarg, Gebieter-Kriegssarg-Schulterpanzer, Gebieter-Kriegssarg-Helm | **Coined.** Translated like the sibling set names (Sanitäter, Schiffsknacker, Kontrolleur). Rejected: Oberherr (archaic), Overlord (the family translates set names; Biotech's Mechlord keeps its loan but is not a set name). |
| Workshop title / `OLWC_SettingsCategory` | Gebieter-Kriegssarg | Glossary title row; equals the armor label. |
| pilot | Pilot | Inherited family row. |
| mechanitor | Mechanitor, pl. Mechanitoren | Biotech `PawnKindDef Mechanitor`, `ConceptDef Mechanitors`. |
| mechlink | Mechlink | Biotech `Mechlink.label`. |
| mech / mechanoid | Mech, pl. Mechs / Mechanoid, pl. Mechanoiden | Biotech prose (`Apparel_ArmorHelmetMechCommander.description`, ability label). |
| bandwidth | Bandbreite (Mech-Bandbreite as the stat) | Biotech Keyed `Bandwidth`, `MechBandwidth.label`. |
| weight class / light, medium, heavy, ultraheavy | Gewichtsklasse / leicht, mittel, schwer, ultraschwer | Core `MechWeightClassDef`, Biotech Keyed `MechWeightClass`. In `OLWC_SummaryCost` the `{0_indefinite}` ("ein leicht") is rewritten by a `{replace:}` to "einen leichten / mittelschweren / schweren / ultraschweren", after Core `PsychicDrone`'s `{replace: {0_plural}; ...}`; mittelschwer per Biotech `LargeMechGestator.description`. |
| resurrect mech (ability) | Mechanoiden erwecken | Biotech `ResurrectionMech.label`, verbatim. resurrect / resurrection = erwecken / Erweckung. |
| mech resurrector | Mech-Erwecker | **Coined** after Biotech's Mech-Gestator and its "erwecken". |
| charge | Ladung, pl. Ladungen | Core `Apparel_PsychicShockLance` chargeNoun, Keyed `Charges`. |
| command relays | Befehlsrelais | **Coined**: Biotech Keyed `OutOfCommandRange` (Befehlsreichweite) plus Odyssey's Relais. |
| signal enhancers | Signalverstärker | Biotech `Apparel_ArmorHelmetMechCommander.description`. |
| command range | Befehlsreichweite | Biotech Keyed `OutOfCommandRange`. |
| drafted | im Kampfmodus | Core Keyed `IsNotDrafted`, Biotech `DraftMech.label`. |
| control group | Kontrollgruppe | Biotech Keyed `ControlGroup`. |
| hacking / hack | Hacken / hacken | Core Keyed `IncapableOfHacking`, `Hack.reportString`. |
| work names | Forschen, Schmieden, Handwerk, Schneidern, Bauen (verbs forschen, schmieden) | Core `WorkTypeDef` labelShort and verb. |
| aim | Fernkampfgenauigkeit | Core `ShootingAccuracyPawn.label`. |
| advanced component | Hightech-Bauteil | Core `ComponentSpacer.label`. |
| machining table / shred mechanoids | Werkbank / tote Mechanoiden zerlegen | Core `TableMachining.label` and its description. |
| corpse | Leiche | Core `Corpses.label`. |
| apocriton | Apokrit | Biotech `Mech_Apocriton.label`. |
| cerebrex node / dye | Cerebrex-Knoten / Farbstoff | Odyssey `Apparel_CerebrexNode.label`, Core `Dye.label`. |
| art (texture option) | Grafik | Family Workshop "Grafik von"; **coined** for the settings. |
| vanilla (range suffix) | Vanilla | Loan as German players use it. |

## Pending native review

Inherited from the Shipcracker glossary, still open there: Kriegssarg for
"warcasket" (rejected: Kriegssarkophag, Kriegskapsel, Panzersarg, Kampfsarg,
Kriegsrüstung, the English loan), Kriegssarg-Gießerei, einsargen,
Schulterpanzer, the set names and 'Raumfahrt-Kriegssärge'. This mod's own:
Gebieter (set name), Mech-Erwecker, Befehlsrelais, Grafik (art option),
and the `{replace:}` rewrite in `OLWC_SummaryCost`, unverified in game.
