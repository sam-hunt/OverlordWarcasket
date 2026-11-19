# Traditional Chinese glossary, Overlord Warcasket

Seeded from the Shipcracker Warcasket glossary (machine-assisted there, no
native review yet); first machine-assisted pass 2026-11-16, no native review. Mod-specific terms
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

## Grounded or coined in the first pass (2026-11-16)

| English | Traditional Chinese | Grounding / notes |
|---|---|---|
| overlord (set name) | 霸主型 | Coined, same `<set>型` shape as 醫療型 / 毒素型; avoids Biotech's 御機王 (mechlord suit). Labels 霸主型戰棺裝甲 / 護肩 / 頭盔. |
| Workshop title / `OLWC_SettingsCategory` | 霸主型戰棺 | Title row pattern. |
| pilot | 駕駛員 | Family row (language file). |
| mechanitor | 御機使 | Biotech `Mechanitors.label`, `PawnKindDef Mechanitor`. |
| mechlink | 機絆 | Biotech `Mechlink.label`. |
| mech / mechanoid | 機械族 (prose); 機械- in compound labels | Biotech prose throughout; 機械培養艙, 機械操控組. |
| bandwidth / mech bandwidth | 頻寬 / 機械族頻寬 | Biotech Keyed `Bandwidth`, StatDef `MechBandwidth`. |
| light / medium / heavy | 輕量級 / 中量級 / 重量級 | `MechWeightClassDef` labels. |
| resurrect mech (ability) | 復活機械族 | Biotech `ResurrectionMech.label`; description opens on its official text. |
| mech resurrector | 機械復活器 | Coined, compound-label 機械- prefix. |
| charge / charges | 充能, counted 次 (`{0}次充能`) | Core `chargeNoun`, Keyed `Charges`. No WordInfo needed. |
| command relays | 指令中繼器 | Coined; 中繼 per Odyssey 中繼站. |
| signal enhancers | 訊號增強器 | Biotech `Apparel_ArmorHelmetMechCommander.description`. |
| command range | 指揮範圍 | Coined; cells 格 per Core `{0}格`. |
| control group | 操控組 | Biotech Keyed `ControlGroup`. |
| hacking | 破解 | Core `Hack` WorkGiver / JobDef, StatDef 破解速度. |
| research / smithing work | 研究 / 鍛造; Work-tab short labels 研究員 / 鍛造師 | Core `WorkTypeDef`. `OLWC_WorkSummary` uses 擔任{0} for the agent-noun labelShort list. |
| mech gestation / subcore encoding / mech repair / shredding mechanoids | 培養機械族 / 次核心編碼 / 修理機械族 / 拆解機械族 | Biotech `MechlinkImplant`, `SubcoreEncodingSpeed`, Core `Smithing.description`, `ButcherCorpseMechanoid`. |
| drafted | 受徵召 | Core Keyed. |
| cerebrex node / dye | 超智腦節點 / 染料 | Odyssey `Apparel_CerebrexNode.label`, Core `Dye.label`. |
| art (settings) | 美術 | Family Workshop line 美術由…繪製. |
| (vanilla) suffix | (原版) | Coined, ASCII parens set solid like (預設). |
| mech bosses / chips | 超級機械族 / 晶片 | Biotech `Mechanitors.helpText`, `Bossgroup` letters. |
| apocriton | 阿布機頓 | Biotech `Mech_Apocriton.label`. |
| mech stun pulse / pulse | 機械族暈眩脈衝 / 脈衝 | Biotech Keyed `MechStunPulseWarning` 機械族死亡時發出暈眩脈衝 (the apocriton's death pulse). Not `MechBandShockwave` 機頻衝擊波, the damage's own label. |
| stun (verb) | 使…暈眩, duration after: 暈眩{1} | Odyssey 可以使目標短暫暈眩; Core `Stun.label` 昏迷 reads as a coma, so not used in prose. |
| downed / falls (pilot) | 倒地 (downed) / 倒下 (falls, death or downed) | Core `Downed.label` 倒地, 救援倒地的人員. |
| resurrect mech range (setting) | 距離 | Core `Ability_Range.label`; Keyed `Range` 射程 is the weapon slot. Line of sight phrased 位於駕駛員的視線內. |

## Pending native review

Every "derived" term above: converted from the zh-Hans pack and
character-checked against the zh-Hant tars, but the actual Traditional pack
could not be compared. This mod's coinages: 霸主型, 機械復活器, 指令中繼器, 指揮範圍, (原版), 美術 for the cerebrex art, and the summary fragments' 次 counter on 充能.
