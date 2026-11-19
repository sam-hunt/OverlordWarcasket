# Simplified Chinese glossary, Overlord Warcasket

Seeded from the Shipcracker Warcasket glossary (machine-assisted there,
pending native review); first machine-assisted pass 2026-11-16, no native review yet. Only
mod-specific coinages and grounding decisions; family-wide mechanics and
style live in `l10n/languages/ChineseSimplified.md`.

VFEP terms are grounded against the community Vanilla Expanded Simplified
Chinese pack (GitHub `q847633684/Vanilla-Expanded-ZH`, the
`Vanilla Factions Expanded - Pirates` folder: its WarcasketDef ThingDef
injections, Keyed UI and ResearchProjectDef files), which also carries the
Siegebreaker set and the 10th-generation paragraph. Vanilla terms
are grounded against the Core, Royalty and Biotech `ChineseSimplified (简体中文).tar`.

## Inherited rows (shared with Shipcracker Warcasket; change them there first)

| English | Simplified Chinese | Grounding |
|---|---|---|
| warcasket | 战棺 | Community VFEP pack throughout (`VFEP.RequiresWarcasket` = 需要战棺, every set label). Workshop title anchor. |
| `<set>` warcasket / shoulders / helmet (label pattern) | `<set>`型战棺装甲 / `<set>`型战棺护肩 / `<set>`型战棺头盔 | Community pattern (`VFEP_Warcasket_Siegebreaker.label` = 破袭型战棺装甲 and siblings). The body piece takes 装甲 although the English label has no noun, exactly as the pack does. |
| siegebreaker / guardian / brute / aerial (set names) | 破袭型 / 圣盾型 / 狂怒型 / 突击型 | Community pack labels. The third-party 无畏 prefix (WarCasket Expanded) is not used; the VFEP pack has none. |
| Specialized spacer-tech warcasket shell/pauldrons/helmet | 特化型太空时代级别战棺装甲/护肩/头盔 | Community pack's opening phrase for every 10th-generation part. |
| warcasket foundry | 战棺铸造厂 | Community `VFEP_WarcasketFoundry.label`. |
| weld on / entomb (Workshop) | 在战棺铸造厂完成改造; 封在战棺里 | Community renders entomb loosely (进入铸造厂, 战棺改造 in `VFEPirates.EntombingWarning`); the Workshop follows that looseness. |
| spacer warcaskets (research) | 「太空时代战棺」 | Community `VFEP_SpacerWarcaskets.label`; corner brackets per vanilla research-name citation (研究「基础逆重科技」). Cite `specialised warcaskets` the same way. |
| pilot | 驾驶员 | Community Keyed/lore (成为驾驶员的殖民者). |
| respirator | 呼吸器 | Coined (no vanilla respirator item or gene in the zh tars); plain everyday word. |
| breathing | 呼吸能力 | Core `Breathing.label`. |
| Imperial | 帝国 | Royalty adjective use (帝国逃兵). |
| plasteel / uranium | 玻璃钢 / 铀 | Core labels. |
| jobReportString (`Using ability: {0}`) | 使用{0}。 | Shipcracker's rendering. |
| cooldownGerund (`on cooldown`) | 冷却中 | Shipcracker's rendering of the same vanilla comp field. |
| cooldown | 冷却时间 | Core Keyed `CooldownTime`. |
| Off / default / Default: on | 关 / 默认 / 默认：启用 | Core Keyed `Off`, `Default`, `Enabled`. |
| settings suffixes | （需要奥德赛） / （默认） | Full-width parens with no leading space, as vanilla 锻造台（燃料）. |
| Royalty / Odyssey | 皇权 / 奥德赛 | Core `ExpansionDefs`; ground Biotech's name the same way. |
| Workshop title | `<set>`型战棺 | Set name + 战棺 (the anchor term). Also the `OLWC_SettingsCategory` value (title coupling). |

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

| English | Simplified Chinese | Grounding |
|---|---|---|
| overlord (set name) / Workshop title | 霸主型 / 霸主型战棺 | Coined, standard rendering of "overlord"; avoids Biotech's 机械领主 (mechlord suit). Title equals `OLWC_SettingsCategory`. |
| pilot | 驾驶员 | Inherited row. |
| mechanitor / mechlink | 机械师 / 机控中枢 | Biotech `Mechanitors.label`, `Mechlink.label`. |
| mech / mechanoid | 机械族 (both) | Biotech uses 机械族 for mechs throughout (`MechResurrectionCharges`, `DesignatorMechControlGroupDesc`). |
| bandwidth / mech bandwidth | 带宽 / 机械带宽 | Biotech Keyed `Bandwidth`, StatDef `MechBandwidth`. |
| light / medium / heavy (weight classes) | 轻型 / 中型 / 重型 | Core `MechWeightClassDef`; `{0_indefinite}` passes the label unchanged (no zh worker). |
| resurrect mech (ability) | 机械复活 | Biotech `ResurrectionMech.label`, verbatim; also the settings section. |
| mech resurrector / resurrector | 机械族复活器 / 复活器 | Coined on Biotech's 机械族复活充能. |
| charge (chargeNoun, counted) | 充能, counted as {0}点充能 | Core `Apparel_PsychicShockLance` chargeNoun; mass noun, counter in the mod's strings. |
| command relays / signal enhancers | 指令中继器 / 信号增强器 | Coined. |
| command range | 指挥范围 | Coined (no vanilla hit). |
| control group | 控制组 | Biotech Keyed `ControlGroup`. |
| drafted / cells / caravan | 征召 / 格 / 远行队 | Core Keyed (`IsNotDrafted`, `ConfirmSettleNearFactionBase`, `Caravan`). |
| hacking | 骇入 | Core `Hack.reportString`, `HackingSpeed`. |
| research / smithing / crafting / tailoring / construction (Work tab) | 研究 / 锻造 / 制作 / 缝制 / 建造 | Core `WorkTypeDef.labelShort`. |
| mech gestation / subcore encoding / shredding mechanoids | 机械培育 / 次核编译 / 拆解机械族 | Biotech `MechGestator`, `SubcoreEncoder`; Core `ButcherCorpseMechanoid`. |
| skilled / intellectual labor | 专业工作 / 智识工作 | Core Keyed `WorkTagManualSkilled`, `WorkTagIntellectual`. |
| apocriton / mech bosses / ultra mechtech | 憎恶毒蜂 / 超级机械族 / 极致机械科技 | Biotech labels, `Mechanitors.helpText`. |
| cerebrex node / dye / art | 主脑节点 / 染料 / 贴图 | Odyssey `Apparel_CerebrexNode.label`, Core `Dye.label`; 贴图 coined for "art". |
| maximum corpse age | 最长死亡时间 | Coined. |
| Biotech (DLC) | 生物技术 | Core `ExpansionDef` `Biotech.label`. |
| warcasket (trait label) | 战棺 | Pre-filled, the inherited noun. |
| mech stun pulse / pulse | 机械眩晕脉冲 / 脉冲 | Biotech Keyed `MechStunPulseWarning` (死亡后释放机械眩晕脉冲。), the apocriton's own warning. |
| stun (verb) | 击晕 | Core `Stun.label`, Keyed `StunnedByEMP` (被EMP击晕); 眩晕 stays inside the pulse's name. |
| downed (pilot state) / falls | 倒地 / 倒下 | Core `TimeDowned.label` (倒地时间), `DoctorRescue.label` (救援倒地的殖民者); 倒下 for "falls", which covers death or downing. |
| pulse radius | 脉冲半径 | Core Keyed `radius` 半径, as `AbilityEffectRadius` 效果半径. |
| Triggered when (dropdown) | 触发条件 | Coined; its values 驾驶员死亡 / 驾驶员倒地 read as completions. |
| range (resurrect mech setting) / line of sight required | 射程 / 目标必须可视 | Core Keyed `Range`, `AbilityRequiresLOS`, the vanilla ability-range terms. |

## Pending native review

Inherited and still open there: 呼吸器 (coined, no vanilla or community
source). This mod's: 霸主型, 机械族复活器, 指令中继器, 信号增强器, 指挥范围, 最长死亡时间, 贴图, 自动装填阈值, 触发条件.
