# Traditional Chinese glossary, Overlord Warcasket

Seeded from the Shipcracker Warcasket glossary (machine-assisted there, no
native review yet); no translation pass has run here yet. Mod-specific terms
and grounding decisions only; family-wide mechanics, punctuation rules and
the zh-Hant/zh-Hans inversion table live in `l10n/languages/ChineseTraditional.md`.

**Community VFEP source.** The Workshop pack "Vanilla Factions Expanded - Pirates - 簡&繁中文漢化包"
(id 2727638702) attests the noun 戰棺 on its page, but its Traditional files
could not be obtained. The file-confirmed lineage is the zh-Hans pack (GitHub
q847633684/Vanilla-Expanded-ZH): 战棺, `<set>型战棺装甲` / `<set>型战棺护肩` /
`<set>型战棺头盔`, 战棺铸造厂. Terms marked "derived" are Traditional forms of
that pack's terms, each checked character by character against the zh-Hant
Core/Royalty/Odyssey tars (not a script conversion). The page's 無畏戰棺 prefix
is deliberately not used.

## Inherited rows (shared with Shipcracker Warcasket; change them there first)

| English | Traditional Chinese | Grounding / notes |
|---|---|---|
| warcasket | 戰棺 | Attested on the community pack's page; zh-Hans files use 战棺. Workshop title anchor. |
| `<set>` warcasket / shoulders / helmet (label pattern) | `<set>`型戰棺裝甲 / `<set>`型戰棺護肩 / `<set>`型戰棺頭盔 | Derived from the zh-Hans pack's pattern. 頭盔 and 裝甲 match vanilla (Core `Apparel_PowerArmorHelmet` 海陸頭盔, 海陸裝甲). |
| pauldrons / shoulder pads | 護肩 | Derived (zh-Hans 护肩, also the foundry UI's `VFEP.Shoulderpads`). |
| torso piece (Workshop) | 裝甲主體 | Derived from the foundry UI's `VFEP.ArmorFrame` (装甲主体). |
| shell (warcasket torso) | 外殼 | Core `Shell.label`, Royalty cataphract helmet 防護外殼. |
| warcasket foundry | 戰棺鑄造廠 | Derived (zh-Hans 战棺铸造厂); 鑄造廠 is Core `FoundryApprentice76`. |
| Siegebreaker / Brute / Guardian / Aerial (set names) | 破襲型 / 狂怒型 / 聖盾型 / 空襲型 | Derived from the zh-Hans pack; 空襲型 is coined there, unverified. |
| entombed / welded into the set (Workshop) | 封在戰棺內; assembling at the foundry 組裝 | zh-Hans pack's `VFEP.WarcasketText` (封入, 组装) rather than a literal "weld". |
| specialized spacer-tech | 特化型太空科技 | 特化型 from the zh-Hans pack's descriptions; 太空科技 is Core `Synthread.description`. |
| respirator | 呼吸器 | Odyssey rulestrings (組裝呼吸器). |
| Imperial | 帝國 | Royalty cataphract descriptions (帝國的甲冑騎士). |
| chemfuel | 化合燃料 | Core label. |
| jobReportString (`Using ability: {0}`) | 使用能力：{0} | Shipcracker's rendering. |
| cooldownGerund (`on cooldown`) | 冷卻 | Shipcracker's rendering of the same vanilla comp field. |
| tank (the armor's fuel tank, settings) | 燃料箱 | Odyssey `FuelStorage.label` 燃料箱 (Core Keyed 燃料槽 is the fueling port); if a tank ever lands here. |
| Default: {0}. / (default) / Off | 預設值：{0}。 / (預設) / 關閉 | Core Keyed `Default` 預設, `Off` 關閉; 預設值 per the Taiwan register list. |
| requires Odyssey (suffix) | (需要「漫遊」DLC) | Core `SimulateNotOwning*` 「漫遊」DLC. ASCII parens set solid, so the suffix drops the English leading space. |
| Workshop title | `<set>`型戰棺 | Contains 戰棺. Also the `OLWC_SettingsCategory` value (title coupling). |

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
(`l10n/lessons.md`), and Chinese falls to the base `Pluralize` (lookup, else unchanged), so one noun serves every slot and no lookup file is needed. Shipcracker's "fuel unit" row in its glossary
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
- mech resurrector (the armor's charge store), charge / charges (vanilla's
  "Charges" Keyed word), the command visor and signal processors (the
  helmet's and pauldrons' lore), the work-type names the settings quote (Core's work-tab labels).
- Core work vocabulary for the settings window: crafting, smithing, the Work
  tab's labelShort forms as the language's Core files render them.
- This mod's coinages, once the descriptions are final.

## Pending native review

Every "derived" term above: converted from the zh-Hans pack and
character-checked against the zh-Hant tars, but the actual Traditional pack
could not be compared. Add this mod's own coinages here as the pass makes them.
