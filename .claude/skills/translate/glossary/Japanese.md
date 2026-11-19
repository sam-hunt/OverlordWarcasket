# Japanese glossary, Overlord Warcasket

Seeded from the Shipcracker Warcasket glossary (machine-assisted there,
pending native review); first machine-assisted pass ran 2026-11-16, no
native review yet. Only
mod-specific coinages and grounding decisions; family-wide mechanics and
style (ASCII `,` `.`, no LanguageWorker, polite register in descriptions,
「」 vs ASCII quotes per slot) live in `l10n/languages/Japanese.md`.

**Community VFEP translation:** only the noun is attested. The Japanese mod
database rimworld.2game.info tags VFE Pirates content as ウォーキャスケット and
marks VFEP as 日本語化対応, but no translation files were found, so every other
VFEP term below (foundry, set names, part labels, entomb) is coined in the
Shipcracker repo and inherited here. Vanilla Armour Expanded renders
"siegebreaker" as シージブレーカー, which is the basis for keeping set names
in katakana.

## Inherited rows (shared with Shipcracker Warcasket; change them there first)

| English | Japanese | Grounding |
|---|---|---|
| warcasket | ウォーキャスケット | Community VFEP noun (rimworld.2game.info tagging). Required in the Workshop title. |
| `<set>` warcasket / `<set>` warcasket helmet (label pattern) | `<セット>`・ウォーキャスケット / `<セット>`・ウォーキャスケットヘルメット | Core/Royalty attach ヘルメット and アーマー directly (カタフラクトヘルメット); Biotech/Odyssey separate a katakana modifier from a katakana compound with ・. |
| shoulders / pauldrons / shoulder pads | ショルダーアーマー | Coined; no vanilla shoulder piece exists. Chosen over bare ショルダー and ショルダーパッド. |
| warcasket shell | ウォーキャスケット外殻 | Coined; plain word for "shell". |
| spacer-tech | 宇宙技術を用いた | Royalty `Apparel_PackJump.description` renders "spacer militaries" as 宇宙技術を用いる一部の軍隊. |
| spacer (tech tier, Workshop) | 宇宙世紀 | Core Keyed `TechLevel_Spacer`. |
| Imperial | 帝国 / 帝国の | Royalty (`Empire.pawnSingular` 帝国; `Apparel_ArmorCataphract` 帝国のカタフラクト部隊). |
| sealed | 気密 | Odyssey `GravshipHull.description` (気密壁). |
| respirator | 呼吸装置 | Coined plain term; no vanilla respirator item. |
| breathing (capacity) | 肺機能 | Core PawnCapacityDef `Breathing.label`. |
| plasteel / uranium / chemfuel | プラスチール / ウラン / バイオ液化燃料 | Core. |
| ability | 能力 | Royalty Keyed (この能力を獲得しますか). |
| pilot (warcasket wearer) | パイロット | Odyssey `PilotAssistant` (パイロットアシスタント). |
| warcasket foundry | ウォーキャスケット鋳造台 | Coined; Korean community uses 주조대, the same "casting bench" idea. |
| entomb | 封入 | Proposed coinage; not used in any shipped string of this mod. |
| siegebreaker / brute / guardian / aerial | シージブレーカー / ブルート / ガーディアン / エアリアル系 | Katakana; シージブレーカー attested in Vanilla Armour Expanded's Japanese, the others coined to match. |
| jobReportString (`Using ability: {0}`) | 能力を使用中: {0} | Shipcracker's rendering. |
| cooldownGerund (`on cooldown`) | クールダウン中 | Shipcracker's rendering of the same vanilla comp field. |
| cells (settings slider unit) | タイル | Core Keyed `RoomRequirementArea` ({0}タイル); no space before the unit. |
| cooldown | クールダウン | Core Keyed `StatsReport_Cooldown`, `AbilityOnCooldown`. |
| Default / Off / On | デフォルト / Off / On | Core Keyed `Default`, `Off`, `On` (vanilla keeps On/Off in Latin script). |
| (requires Odyssey) / (default) suffixes | ` (Odysseyが必要)` / ` (デフォルト)` | ASCII parens with a leading space, as in vanilla Keyed; が必要 from Core Alerts (`医者が必要`). Odyssey stays Latin. |
| Workshop title | `<セット>`・ウォーキャスケット, equal to the armor label | Also the `OLWC_SettingsCategory` value (title coupling). |

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
(`l10n/lessons.md`), and Japanese falls to the base `Pluralize` (lookup, else unchanged), so one noun serves every slot and no lookup file is needed. Shipcracker's "fuel unit" row in its glossary
is the worked example for this language.

## Grounded and coined in the first pass (2026-11-16)

| English | Japanese | Grounding or reason |
|---|---|---|
| overlord (set name) | オーバーロード | Katakana per the family set-name habit. |
| Workshop title / `OLWC_SettingsCategory` | オーバーロード・ウォーキャスケット | Equals the armor label. |
| labels | オーバーロード・ウォーキャスケット / 〜ヘルメット / 〜ショルダーアーマー | Inherited label pattern. |
| pilot | パイロット | Inherited row. |
| mechanitor / mechlink | メカニター / メカリンク | Biotech `Mechanitors.label`, `Mechlink.label`. |
| mech / mechanoid | メカ / メカノイド | Biotech throughout (`CommandSelectAllMechs`). |
| bandwidth / mech bandwidth | 帯域幅 / メカ帯域幅 | Biotech Keyed `Bandwidth`, StatDef `MechBandwidth`. |
| weight classes (light / medium / heavy) | 軽 / 中 / 重 | Core `MechWeightClassDef`; framed as `{0}量級` (軽量級, 中量級, 重量級, 超重量級). `_indefinite` is a no-op with no worker. |
| resurrect mech | リザレクトメカ | Biotech `ResurrectionMech.label`, verbatim; description opens with its two official sentences. |
| Resurrecting mechs. / resurrecting TargetA | メカを復活中 / TargetAを復活させている | Biotech `ResurrectXMech.jobString`; `RepairMech.reportString` shape. |
| mech resurrector | メカ復活装置 | Coined from Biotech's 復活 (`ResurrectLightMech`). |
| charge | チャージ | Core `Charges`, `Apparel_PsychicShockLance` chargeNoun. Recharge = 再チャージ / 回復. |
| reload | 充填 | Main language file row. |
| corpse (mech) | 死骸 | Core `SmashCorpseMechanoid.description`. |
| command relays | コマンドリレー | Coined; コマンド from Biotech `OutOfCommandRange`, リレー from Odyssey `OrbitalRelay`. |
| signal enhancers | 信号増幅器 | Biotech `Apparel_ArmorHelmetMechCommander.description`. |
| command range | コマンド範囲 | Biotech Keyed `OutOfCommandRange` (コマンド範囲外). |
| control group | メカ制御グループ | Biotech StatDef `MechControlGroups` (the stat the shoulders raise). |
| drafted | 徴兵中 | Core Keyed `MechNameDisplayMode_WhileDrafted`. |
| hacking | ハッキング | Core WorkGiverDef `Hack.label`. |
| research / smithing / crafting / tailoring / construction | 研究 / 鍛冶 / 工芸 / 裁縫 / 建築 | Core `WorkTypeDef.labelShort`. |
| mech gestation / subcore encoding / mech repair / shred at machining table | メカの培養 / サブコアのエンコード / メカの修理 / 精密工作機械でのメカノイドの寸断 | Biotech `MechGestator`, `SubcoreEncoder`, `RepairMech`; Core `ButcherCorpseMechanoid`, `TableMachining`. |
| ultra mechtech (research) | 「機械技術 超高度」 | Biotech `UltraMechtech.label`. |
| mech bosses | ボス級メカノイド | Coined, Workshop FAQ only. |
| cerebrex node / dye | セレブレックスノード / 染料 | Odyssey `Apparel_CerebrexNode.label`, Core `Dye.label`. |
| art (cerebrex setting) | 外観 | Coined; avoids modder words. |
| (vanilla) suffix | ` (バニラ)` | Coined; community usage. |
| Enabled work types (Workshop h2) | 有効な作業 | Pre-filled sibling `MDWC_SectionWork`. |
| mech stun pulse | メカのスタンパルス | Biotech Keyed `MechStunPulseWarning` (死亡時のメカのスタンパルス, "Mech stun pulse on death"). |
| stun (verb, in prose) | 麻痺させる | Core `TrapIED_EMP.description` (メカノイドを数秒間,麻痺させる); Core `Stun.label` 麻痺. |
| apocriton | アポクリトン | Biotech `Mech_Apocriton.label`. |
| pilot falls / is downed | パイロットが倒れる | Main language file's downed row (倒れた, never 失神 in prose); one verb for both "falls" and "downed". |
| pulse radius | パルスの半径 | Core Keyed `radius` 半径. |
| Triggered when (dropdown) | 発動タイミング, choices 〜したとき | Coined; the choices read as label: value. |
| range (resurrect mech) | 射程 | Core Keyed `Range`, as Medic's `MDWC_Range`. |

## Pending native review

Inherited and still open there: ショルダーアーマー and the resulting label
length, ウォーキャスケット鋳造台 (foundry) and 封入 (entomb), both unattested.
If the community VFEP translation surfaces, align these and the set names to
it in the Shipcracker glossary first. Add this mod's own coinages here as the
pass makes them.

This pass's own coinages, pending native review: オーバーロード (set
name), メカ復活装置, コマンドリレー, the `{0}量級` frame for weight classes,
ボス級メカノイド, 外観 for the cerebrex art, ` (バニラ)`, and the
10th-generation paragraph's Overlord wording (鹵獲した宙賊の装備は政府のパワーアーマーを核に作り直され).
