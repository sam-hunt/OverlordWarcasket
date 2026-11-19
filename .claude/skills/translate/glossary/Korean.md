# Korean glossary, Overlord Warcasket

Seeded from the Shipcracker Warcasket glossary (machine-assisted there,
pending native review); first machine-assisted pass 2026-11-16, pending
native review. Only
mod-specific coinages and grounding decisions; family-wide mechanics, josa
markers and style live in `l10n/languages/Korean.md`.

Community VFEP translation grounded against: the RimWorldKorea `RMK` pack
(GitHub RimWorldKorea/RMK), VFE Pirates' ThingDef, WarcasketDef and Keyed
files. Its warcasket set labels follow `<bare set prefix> 워캐스킷 [어깨|헬멧]`
and every set description opens with `우주시대 기술로 특화된 워캐스킷 ...입니다.`;
this mod copies both patterns, and takes the pack's Siegebreaker
prefix and its 10th-generation paragraph.

## Inherited rows (shared with Shipcracker Warcasket; change them there first)

| English | Korean | Grounding |
|---|---|---|
| warcasket | 워캐스킷 | RMK VFEP, every warcasket label. No separate word for "shell": RMK drops it (`...워캐스킷입니다.`), and so does this mod. |
| `<set>` warcasket / shoulders / helmet (label pattern) | `<prefix>` 워캐스킷 / `<prefix>` 워캐스킷 어깨 / `<prefix>` 워캐스킷 헬멧 | RMK `VFEP_Warcasket*_*.label`; "pauldrons" in the description is rendered the same way (`워캐스킷 어깨입니다.`). |
| warcasket foundry | 워캐스킷 주조대 | RMK `VFEP_WarcasketFoundry.label`; bare "foundries" in lore is `주조대`. |
| entomb / weld on | 안치 (워캐스킷에 안치) | RMK `VFEPirates.EntombInWarcasket`; "welded into" / "weld it on" both use 안치. |
| siegebreaker / brute / guardian (set prefixes) | 돌격 / 야수 / 수호 | RMK set prefixes. |
| Imperial | 제국 (bare prefix) | Royalty backstories: `제국 부대`, `제국군`. |
| spacer-tech / spacer | 우주시대 기술 / 우주시대 | RMK VFEP descriptions (`우주시대 기술로 특화된`); Core `TechLevel_Spacer` = `우주`. |
| chassis | 프레임 | RMK's foundry text `워캐스킷 프레임`. |
| pilot | 조종사 | RMK `VFEP_WarcasketHelmet_Shock` (`조종사의 반응 시간`). |
| onboard respirator | 내장 호흡 장치 | Coined; Core `Breathing.label` = `호흡`. |
| sealed (helmet) | 밀폐형 | `밀폐` is the usual word for an airtight suit. |
| plasteel / uranium / chemfuel | 플라스틸 / 우라늄 / 화학연료 | Core / Odyssey labels. |
| jobReportString (`Using ability: {0}`) | {0} 능력 사용 중 | Shipcracker's rendering. |
| cooldownGerund (`on cooldown`) | 재사용 대기 중 | Shipcracker's rendering of the same vanilla comp field. |
| cooldown (slider caption) | 재사용 대기 | Core Keyed `StatsReport_Cooldown` = `재사용 대기시간`; `시간` dropped from the caption so it does not repeat the `(시간)` unit. |
| radius | 범위 | Core Keyed `AbilityEffectRadius` = `효과 범위`. |
| Off (slider value at 0) | 없음 | Core Keyed `None` = `없음`; Core `Off` = `끄기` is a toggle verb. A toggle's "on" is `켜짐`. |
| default / (requires Odyssey) | 기본값 / (오디세이 필요) | Core Keyed `Default`; ExpansionDef `Odyssey.label` = `오디세이`; shape after Core `BlockedByTerrain` (`({0_label} 필요)`). |
| fuel tank (settings) | 탱크 / 연료 탱크 | Shipcracker's Workshop description; if a tank ever lands here. |
| Workshop title | `<prefix>` 워캐스킷, the armor label | Contains `워캐스킷`. Also the `OLWC_SettingsCategory` value (title coupling). |

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
(`l10n/lessons.md`), and Korean falls to the base `Pluralize` (lookup, else unchanged), so one noun serves every slot and no lookup file is needed. Shipcracker's "fuel unit" row in its glossary
is the worked example for this language.

## Grounded in the first pass (2026-11-16)

| English | Korean | Grounding or reason |
|---|---|---|
| overlord (set name) | 대군주 (`대군주 워캐스킷`, `... 어깨`, `... 헬멧`) | Coined, bare prefix like 의무병 / 독소. Not 기계군주: Biotech spends it on `Apparel_MechlordSuit` (기계군주 갑옷). |
| Workshop title / `OLWC_SettingsCategory` | 대군주 워캐스킷 | The armor label. |
| pilot | 조종사 | Inherited row. |
| mechanitor | 메카나이터 | Biotech `Mechanitor.label`, ConceptDef `Mechanitors`. |
| mechlink | 메크링크 | Biotech `Mechlink.label`. |
| mech / mechanoid | 메카노이드 (both) | Biotech uses 메카노이드 for both; `MechBandwidth` = 메카노이드 대역폭. |
| bandwidth | 대역폭 | Biotech Keyed `Bandwidth`. |
| light / medium / heavy (weight class) | 소형 / 중형 / 대형; "weight class" 체급 | Core `MechWeightClassDef`, Biotech Keyed `MechWeightClass`. |
| resurrect mech (ability) | 메카노이드 소생 | Biotech `ResurrectionMech.label`, verbatim. |
| mech resurrector / resurrector | 메카노이드 소생기 / 소생기 | Coined from the ability label. |
| charge (chargeNoun, "Charges") | 충전 (amounts: `{0}회분`) | Core `PsychicShockLance` chargeNoun, Keyed `Charges`. |
| command relays | 명령 중계기 | Coined; Odyssey 궤도 중계기 uses 중계기. |
| signal enhancers | 신호 증폭기 | Coined after Biotech ConceptDef `Mechanitors` (대역 신호 증폭기). |
| command range | 명령 범위 | Biotech Keyed `OutOfCommandRange` (명령 범위 벗어남). |
| drafted | 소집한 / 소집 중 | Core Keyed `CommandDraftLabel` = 소집. |
| cells | `{0}칸` | Core Keyed `ConfirmSettleNearFactionBase`. |
| map | 지도 | Core Keyed (다른 지도). |
| control group | 제어 집단 | Biotech Keyed `ControlGroup`. |
| hacking | 해킹 | Core Keyed `Hack`, StatDef `HackingSpeed`. |
| research / smithing / crafting / tailoring / construction (work) | 연구 / 제작 / 가공 / 재봉 / 건설 | Core `WorkTypeDef.labelShort`. |
| skilled / intellectual labor | 숙련노동 / 지적노동 | Core Keyed `WorkTagManualSkilled`, `WorkTagIntellectual`. |
| mech gestation / subcore encoding / mech repair / shredding mechanoids | 메카노이드 배양 / 서브코어 부호화 / 메카노이드 수리 / 메카노이드 분쇄 | Biotech Keyed `InitMechGestationCycle`, `SubcoreEncoder` (서브코어 부호기), `AlertMechNeedsRepair`; Core `SmashCorpseMechanoid.jobString`. |
| machining table | 기계 작업대 | Core `TableMachining.label`. |
| mechtech research / ultra mechtech | 기계공학 연구 / 최첨단 기계공학 | Biotech `ResearchProjectDef` labels. |
| corpse age | 시체 최대 경과 시간 | Coined; mechs "dead" are 파괴된 (Biotech ability text). |
| cerebrex node / core / dye | 세레브렉스 노드 / 코어 / 염료 | Odyssey `Apparel_CerebrexNode`, `CerebrexCore`; Core `Dye.label`. |
| art (cerebrex node art) / frame / ring | 외형 / 프레임 / 고리 | Coined. |
| (vanilla) suffix | ` (바닐라)` | Coined, common Korean modding usage. |
| Biotech / Odyssey (DLC) | 바이오테크 / 오디세이 | Toxin Workshop; ExpansionDef. |
| comma-and list | `A, B, 그리고 C` | Core Keyed `ToCommaListAnd` = `{0}, 그리고 {1}`, behind a hard-coded `, ` between the earlier items; the cost and work summaries are built so that joiner reads. |
| mech stun pulse / pulse / stun | 메카노이드 기절 파동 / 파동 / 기절시키다 | Biotech Keyed `MechStunPulseWarning` = `사망 시 메카노이드 기절 파동 방출`; Core DamageDef `Stun.label` = `기절`. |
| (the pilot) falls / is downed / dies | 쓰러지다 / 쓰러지다 / 사망하다 | Core `Downed.label` = 쓰러짐; `사망 시` from `MechStunPulseWarning`. "Falls" in prose covers both, as 쓰러지면 does. |
| Triggered when (dropdown) | 발동 조건, values `조종사가 사망할 때` / `조종사가 쓰러질 때` | Coined; the values read as completions of the label. |

## Pending native review

Nothing inherited is open for the VFEP rows (they are RMK's). This mod's
coinages: 대군주 (set name), 메카노이드 소생기, 명령 중계기, 신호 증폭기,
시체 최대 경과 시간, 외형 / 고리 (cerebrex art), ` (바닐라)`, and the
summary fragment `{0_indefinite} {1}` read as `체급별로 소형 1, ...`.
