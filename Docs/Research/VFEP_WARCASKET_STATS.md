# VFE Pirates Warcasket Stat Reference

> Copied from the sibling Shipcracker Warcasket repo, where it was written while scoping that set;
> the VFEP numbers are set-independent and serve this set the same way.

This is a reference for every stat Vanilla Factions Expanded - Pirates (VFEP) assigns to its
warcasket apparel defs (armor shells, shoulder pads, helmets), whether set directly on the
concrete def or inherited through its abstract parent chain.

Source: `VanillaExpanded/VanillaFactionsExpanded-Pirates` at commit `e237aba` (2026-08-29),
files `1.6/Defs/ThingDefs_Misc/Apparel_Various.xml` and
`1.6/Defs/ThingDefs_Misc/Apparel_Headgear.xml`. Values below are exactly what ships in that XML.
In-game numbers also depend on VEF/game code that reads these defs (`VFEPirates.Apparel_Warcasket`,
`VEF.Apparels.ApparelExtension`, the shield-bubble/shield-field comps, `Apparel_Warcasket`'s
equip/unequip logic, etc.); that code is out of scope here, this doc only covers what the XML
declares. Per-set mechanics, abilities and the Gravship Expanded comparison live in
`WARCASKET_ABILITIES.md`.

This doc is informational only, generated for our own reference while scoping Shipcracker
Warcasket's tuning. It is not itself mod content.

## Inheritance chain

RimWorld's XML inheritance resolver (`XmlInheritance`) merges a def with its `ParentName` chain as follows:

- A node whose children are all `<li>` (`apparel/tags`, `layers`, `bodyPartGroups`, `comps`,
  `modExtensions`, `thingSetMakerTags`, `tradeTags`, `researchPrerequisites`, ...) is a **list**
  node: the resolved node is the parent's `<li>` items followed by the child's `<li>` items
  (concatenation), unless the child node carries `Inherit="False"` (not used anywhere in the
  warcasket defs; only on two unrelated `recipeMaker/recipeUsers` blocks elsewhere in the file).
- Any other node (`statBases`, `equippedStatOffsets`, `costList`, `apparel`, `graphicData`,
  `drawData`, ...) merges recursively by child element name, with the child's element winning
  when both sides define the same tag. A true leaf (no element children on either side, e.g.
  `<Mass>50</Mass>`) is replaced wholesale by the child, including any `MayRequire` attribute.

The practical consequence: every concrete armor/shoulder/helmet def ends up with **more** list
entries than it declares itself. In particular every def carries `CompProperties_Forbiddable`
and `CompColorable` (from `VFEP_WarcasketPartBase`) in addition to any comps of its own (e.g.
Siegebreaker armor ends up with three comps total), and ends up with **two separate**
`<li Class="VEF.Apparels.ApparelExtension">` modExtension entries rather than one merged entry
whenever both an abstract base and the concrete def each declare one (which is the normal case
for armor and helmet defs).

| Abstract base                   | Parent   | Contributes                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            |
| ------------------------------- | -------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `VFEP_WarcasketPartBase`        | (none)   | `tradeability` None; `thingClass` `VFEPirates.Apparel_Warcasket`; `category` Item; `drawerType` MapMeshOnly; `techLevel` Industrial; `pathCost` 14; `useHitPoints` false; `destroyOnDrop` true; `alwaysHaulable` true; `tickerType` Never; `burnableByRecipe`/`smeltable` false; statBases `Flammability` 1.0, `DeteriorationRate` 0, `Beauty` -3, `WorkToMake` 1000; `apparel.useDeflectMetalEffect` true, `apparel.canBeGeneratedToSatisfyVacuumResistance` false; `thingSetMakerTags`/`tradeTags` = [Warcasket]; comps = [CompProperties_Forbiddable, CompColorable]; a 4-option grayscale `ColorGenerator_Options` |
| `VFEP_WarcasketArmorBase`       | PartBase | `isArmor` true; `tickerType` Normal (overrides PartBase's Never); `apparel.renderSkipFlags` [None], `apparel.drawData` (per-facing offsets); equippedStatOffsets `VEF_MassCarryCapacity` 125; modExtensions += ApparelExtension{`isUnifiedApparel` true, `showBodyInBedAlways` true, `secondaryApparelGraphics` [VFEP_Warcasket_Bodysuit]}                                                                                                                                                                                                                                                                             |
| `VFEP_WarcasketShoulderPadBase` | PartBase | `isShoulderPads` true; `apparel.renderSkipFlags`/`drawData` (own offsets); modExtensions += ApparelExtension{`isUnifiedApparel` true}. Does not touch `tickerType`, so shoulder pads stay Never unless a concrete def overrides it                                                                                                                                                                                                                                                                                                                                                                                     |
| `VFEP_WarcasketHelmetBase`      | PartBase | `isHelmet` true; `apparel.renderSkipFlags`/`drawData` (own offsets); modExtensions += ApparelExtension{`isUnifiedApparel` true, `hideHead` true}. Also stays Never for `tickerType`                                                                                                                                                                                                                                                                                                                                                                                                                                    |

`VFEP_Warcasket_Bodysuit` (the shared undersuit referenced by every armor's
`secondaryApparelGraphics`) parents directly on `VFEP_WarcasketPartBase`, not on
`VFEP_WarcasketArmorBase`: it gets none of the armor-only additions above (no `isArmor`, no
`VEF_MassCarryCapacity`, no traits/pawnCapacityMinLevels). It overrides `thingClass` back to
plain `Apparel`, sets `apparel.layers` to `[OnSkin]`, replaces `modExtensions` with a bare
`ApparelExtension{isUnifiedApparel true}`, and nulls `colorGenerator` out entirely
(`IsNull="True"`).

All 13 sets, in the order used below: Warcasket, Marine, Recon, Cataphract, Aerial, Barrage,
Hazard, Shock, Siegebreaker, Guardian, Controller, Sarcophagus, Brute. They fall into four
research tiers (see the researchPrerequisites table): Warcasket; Marine/Recon/Cataphract;
Aerial/Barrage/Hazard/Shock; and Siegebreaker/Guardian/Controller/Sarcophagus/Brute, which their
descriptions call the "10th generation" and which are the only sets carrying `VacuumResistance`.

Legend for the stat tables: _italic_ = value inherited unchanged from an abstract base (not set
directly on that def); plain = set directly on that def, possibly re-overriding an ancestor's
value. `¹` = the entry carries `MayRequire="Ludeon.RimWorld.Ideology"`. `²` = the entry carries
`MayRequire="Ludeon.RimWorld.Odyssey"`. A blank cell means that stat is not set anywhere in the
def's chain.

## Armor

### statBases

| Set          | Flammability | Mass | ArmorRating_Sharp | ArmorRating_Blunt | ArmorRating_Heat | Insulation_Cold | Insulation_Heat | EnergyShieldRechargeRate | EnergyShieldEnergyMax |
| ------------ | ------------ | ---- | ----------------- | ----------------- | ---------------- | --------------- | --------------- | ------------------------ | --------------------- |
| Warcasket    | _1_          | 50   | 1.06              | 0.55              | 0.64             | 20              | 7               |                          |                       |
| Marine       | _1_          | 50   | 1.20              | 0.65              | 0.74             | 24              | 8               |                          |                       |
| Recon        | _1_          | 40   | 1.02              | 0.60              | 0.66             | 20              | 6               |                          |                       |
| Cataphract   | _1_          | 75   | 1.56              | 0.65              | 0.78             | 43              | 14              |                          |                       |
| Aerial       | _1_          | 50   | 1.65              | 0.72              | 0.86             | 36              | 10              |                          |                       |
| Barrage      | _1_          | 75   | 2.00              | 0.90              | 1.08             | 44              | 18              |                          |                       |
| Hazard       | 0            | 60   | 1.20              | 0.65              | 2.00             | 75              | 72              |                          |                       |
| Shock        | _1_          | 65   | 1.65              | 1.24              | 0.86             | 36              | 10              |                          |                       |
| Siegebreaker | _1_          | 50   | 2.00              | 1.00              | 1.00             | 80              | 80              | 0.01                     | 2.5                   |
| Guardian     | _1_          | 50   | 2.00              | 1.50              | 1.00             | 80              | 80              | 0.01                     | 1.5                   |
| Controller   | _1_          | 50   | 2.00              | 1.00              | 1.00             | 80              | 80              | 0.01                     | 1.5                   |
| Sarcophagus  | _1_          | 50   | 2.00              | 1.00              | 1.00             | 80              | 80              | 0.01                     | 1.5                   |
| Brute        | _1_          | 50   | 2.00              | 1.50              | 1.00             | 80              | 80              | 0.05                     | 5.0                   |
| Bodysuit     | _1_          |      |                   |                   |                  |                 |                 |                          |                       |

### equippedStatOffsets

| Set          | VEF_MassCarryCapacity | MoveSpeed | VFEP_PowerJumpRange | VFEP_PowerJumpDetonationRadius | PsychicSensitivity | ToxicResistance | VacuumResistance |
| ------------ | --------------------- | --------- | ------------------- | ------------------------------ | ------------------ | --------------- | ---------------- |
| Warcasket    | _125_                 | -0.50     |                     |                                |                    |                 |                  |
| Marine       | _125_                 | -0.50     |                     |                                |                    |                 |                  |
| Recon        | _125_                 | -0.20     |                     |                                |                    |                 |                  |
| Cataphract   | _125_                 | -0.85     |                     |                                |                    |                 |                  |
| Aerial       | _125_                 | -0.50     | 25                  | 1                              |                    |                 |                  |
| Barrage      | _125_                 | -1.00     |                     |                                |                    |                 |                  |
| Hazard       | _125_                 | -0.50     |                     |                                |                    |                 |                  |
| Shock        | _125_                 | -0.75     |                     |                                |                    |                 |                  |
| Siegebreaker | _125_                 |           |                     |                                | -0.5               | 0.5             | 0.2²             |
| Guardian     | _125_                 |           |                     |                                | -0.5               | 0.5             | 0.2²             |
| Controller   | _125_                 |           |                     |                                | -0.5               | 0.5             | 0.2²             |
| Sarcophagus  | _125_                 |           |                     |                                | -0.5               | 0.5             | 0.2²             |
| Brute        | _125_                 |           |                     |                                | -0.5               | 0.5             | 0.2²             |
| Bodysuit     |                       |           |                     |                                |                    |                 |                  |

(Bodysuit gets no `equippedStatOffsets` at all because it parents on `VFEP_WarcasketPartBase`
directly, skipping `VFEP_WarcasketArmorBase`.)

### Non-stat data

costList (union of resources used across the armor set; blank = not used):

| Set          | ComponentIndustrial | ComponentSpacer | Steel | Plasteel | Uranium | DevilstrandCloth | MedicineUltratech |
| ------------ | ------------------- | --------------- | ----- | -------- | ------- | ---------------- | ----------------- |
| Warcasket    | 2                   |                 | 80    |          | 20      |                  |                   |
| Marine       | 4                   |                 | 130   |          | 40      |                  |                   |
| Recon        | 3                   |                 | 80    |          | 20      |                  |                   |
| Cataphract   | 5                   |                 | 175   |          | 50      |                  |                   |
| Aerial       | 8                   |                 | 145   | 20       | 50      |                  |                   |
| Barrage      | 8                   |                 | 240   | 30       | 50      |                  |                   |
| Hazard       | 4                   |                 | 135   |          | 50      | 100              |                   |
| Shock        | 10                  |                 | 120   | 45       | 50      |                  |                   |
| Siegebreaker |                     | 6               |       | 145      | 50      |                  |                   |
| Guardian     |                     | 6               |       | 200      | 50      |                  |                   |
| Controller   |                     | 12              |       | 150      | 50      |                  |                   |
| Sarcophagus  |                     | 6               |       | 145      | 50      |                  | 5                 |
| Brute        |                     | 4               |       | 160      | 50      |                  |                   |
| Bodysuit     |                     |                 |       |          |         |                  |                   |

researchPrerequisites and apparel tags are identical for the armor, shoulder-pad and helmet piece
of a given set (verified per-def), so they are listed once here rather than three times:

| Set          | researchPrerequisites      | apparel tags                                            |
| ------------ | -------------------------- | ------------------------------------------------------- |
| Warcasket    | VFEP_Warcaskets            | Warcasket, WarcasketVeteran, WarcasketAll               |
| Marine       | VFEP_AdvancedWarcaskets    | WarcasketVeteran, WarcasketAll                          |
| Recon        | VFEP_AdvancedWarcaskets    | WarcasketVeteran, WarcasketAll                          |
| Cataphract   | VFEP_AdvancedWarcaskets    | WarcasketHeavy, WarcasketCata, WarcasketAll             |
| Aerial       | VFEP_SpecialisedWarcaskets | WarcasketHussar, WarcasketAll                           |
| Barrage      | VFEP_SpecialisedWarcaskets | WarcasketSpecialised, WarcasketSuperHeavy, WarcasketAll |
| Hazard       | VFEP_SpecialisedWarcaskets | WarcasketSpecialised, WarcasketFlamer, WarcasketAll     |
| Shock        | VFEP_SpecialisedWarcaskets | WarcasketHussar, WarcasketAll                           |
| Siegebreaker | VFEP_SpacerWarcaskets      | WarcasketVeteran, WarcasketAll                          |
| Guardian     | VFEP_SpacerWarcaskets      | WarcasketVeteran, WarcasketAll                          |
| Controller   | VFEP_SpacerWarcaskets      | WarcasketVeteran, WarcasketAll                          |
| Sarcophagus  | VFEP_SpacerWarcaskets      | WarcasketVeteran, WarcasketAll                          |
| Brute        | VFEP_SpacerWarcaskets      | WarcasketVeteran, WarcasketMelee (no WarcasketAll)      |

All armor pieces use `apparel.layers` = [OnSkin, Middle, Shell] and `apparel.bodyPartGroups` =
[Torso, Legs, Feet] (Bodysuit uses [OnSkin] only and sets no bodyPartGroups at all). All armor
pieces resolve to `tickerType` Normal via `VFEP_WarcasketArmorBase`; Siegebreaker, Guardian,
Controller, Sarcophagus and Brute additionally redeclare `<tickerType>Normal</tickerType>`
themselves, which is a no-op restatement of the inherited value.

Comps and modExtension fields beyond the base `CompProperties_Forbiddable` +
`CompColorable` + `ApparelExtension{isUnifiedApparel, showBodyInBedAlways,
secondaryApparelGraphics}` that every armor piece already carries:

| Set                                     | Extra comps                                                                                                                                                                               | Extra modExtension fields                                                                                                                                                                                                                                                                            |
| --------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Warcasket / Marine / Recon / Cataphract | none                                                                                                                                                                                      | traitsOnEquip=[VFEP_WarcasketTrait], traitsOnUnequip=[VFEP_Shellcasket], pawnCapacityMinLevels={Moving>=0.7, Manipulation>=0.7}                                                                                                                                                                      |
| Aerial                                  | none (the jump comps live on the shoulders only; corrected 2026-09-15)                                                                                                                    | same trait/capacity block as above, plus equippedStatFactors={VFEP_FlightSpeed=1.5}                                                                                                                                                                                                                  |
| Barrage                                 | CompProperties_AbilitiesApparel (VFEP_SiegeMode)                                                                                                                                          | same trait/capacity block                                                                                                                                                                                                                                                                            |
| Hazard                                  | none                                                                                                                                                                                      | same trait/capacity block, plus equippedStatFactors={Flammability=0}                                                                                                                                                                                                                                 |
| Shock                                   | CompProperties_ApparelReloadable (100 charges, Chemfuel, "jump"); CompProperties_AbilitiesApparel (VFEP_BlastOff)                                                                         | same trait/capacity block                                                                                                                                                                                                                                                                            |
| Siegebreaker                            | CompProperties_ShieldBubble (blockRangedAttack, minShieldSize 2.1, maxShieldSize 2.7)                                                                                                     | same trait/capacity block                                                                                                                                                                                                                                                                            |
| Guardian                                | CompProperties_ShieldField (radius stat `VEF_EnergyShieldRadiusApparel`, manual activation, 600-tick activation / 5400-tick cooldown); CompProperties_ShieldBubble (same as Siegebreaker) | same trait/capacity block                                                                                                                                                                                                                                                                            |
| Controller                              | CompProperties_ApparelReloadable (1 charge, ComponentSpacer, "drone deployment"); CompProperties_ShieldBubble; verb `VFEPirates.Verb_DroneDeployment`                                     | same trait/capacity block                                                                                                                                                                                                                                                                            |
| Sarcophagus                             | CompProperties_ShieldBubble                                                                                                                                                               | traitsOnEquip/traitsOnUnequip as above, pawnCapacityMinLevels={Moving>=1, Manipulation>=1, BloodFiltration>=1, BloodPumping>=1, Metabolism>=1}, plus preventDowning=true, preventKilling=true, preventKillingUntilHealthHPPercentage=0.6, preventKillingUntilBrainMissing=true, preventBleeding=true |
| Brute                                   | CompProperties_ShieldBubble (adds dontAllowRangedAttack=true)                                                                                                                             | same trait/capacity block                                                                                                                                                                                                                                                                            |

## Shoulder Pads

### statBases

| Set          | Flammability | Mass | ArmorRating_Sharp | ArmorRating_Blunt | ArmorRating_Heat | Insulation_Cold | Insulation_Heat | EquipDelay |
| ------------ | ------------ | ---- | ----------------- | ----------------- | ---------------- | --------------- | --------------- | ---------- |
| Warcasket    | _1_          | 20   | 1.06              | 0.55              | 0.64             | 6               | 2               | 1          |
| Marine       | _1_          | 20   | 1.20              | 0.65              | 0.74             | 7               | 2               | 1          |
| Recon        | 0            | 15   | 1.02              | 0.60              | 0.66             | 7               | 2               | 14         |
| Cataphract   | _1_          | 35   | 1.56              | 0.65              | 0.78             | 18              | 6               | 1          |
| Aerial       | _1_          | 20   | 1.65              | 0.72              | 0.86             | 20              | 6               | 1          |
| Barrage      | _1_          | 35   | 2.00              | 0.90              | 1.08             | 20              | 6               | 1          |
| Hazard       | 0            | 20   | 1.20              | 0.65              | 2.00             | 22              | 14              | 1          |
| Shock        | _1_          | 25   | 1.65              | 1.24              | 0.86             | 7               | 3               | 1          |
| Siegebreaker | _1_          | 20   | 2.00              | 1.00              | 1.00             | 22              | 22              | 1          |
| Guardian     | _1_          | 20   | 2.00              | 1.50              | 1.00             | 22              | 22              | 1          |
| Controller   | _1_          | 20   | 2.00              | 1.00              | 1.00             | 22              | 22              | 1          |
| Sarcophagus  | _1_          | 20   | 2.00              | 1.00              | 1.00             | 22              | 22              | 1          |
| Brute        | _1_          | 20   | 2.00              | 1.50              | 1.00             | 22              | 22              | 1          |

### equippedStatOffsets

| Set          | MoveSpeed | SlaveSuppressionOffset | VFEP_PowerJumpRange | EnergyShieldRechargeRate | VacuumResistance | VEF_EnergyShieldEnergyMaxFactor |
| ------------ | --------- | ---------------------- | ------------------- | ------------------------ | ---------------- | ------------------------------- |
| Warcasket    | -0.20     | 0.2¹                   |                     |                          |                  |                                 |
| Marine       | -0.20     | 0.2¹                   |                     |                          |                  |                                 |
| Recon        | -0.10     | 0.2¹                   |                     |                          |                  |                                 |
| Cataphract   | -0.35     | 0.2¹                   |                     |                          |                  |                                 |
| Aerial       | -0.10     | 0.2¹                   | 5                   |                          |                  |                                 |
| Barrage      | -0.40     | 0.2¹                   |                     |                          |                  |                                 |
| Hazard       | -0.40     | 0.2¹                   |                     |                          |                  |                                 |
| Shock        | -0.20     | 0.2¹                   |                     |                          |                  |                                 |
| Siegebreaker |           | 0.2¹                   |                     | 1                        | 0.1²             |                                 |
| Guardian     | -0.50     | 0.2¹                   |                     |                          | 0.1²             |                                 |
| Controller   |           | 0.2¹                   |                     |                          | 0.1²             |                                 |
| Sarcophagus  |           | 0.2¹                   |                     |                          | 0.1²             |                                 |
| Brute        |           | 0.2¹                   |                     |                          | 0.1²             | 0.5                             |

Shoulder pads never inherit `VEF_MassCarryCapacity`: it is only added by
`VFEP_WarcasketArmorBase`, and `VFEP_WarcasketShoulderPadBase` does not add an equivalent.

### Non-stat data

costList:

| Set          | Steel | Plasteel | ComponentSpacer | DevilstrandCloth | MedicineUltratech |
| ------------ | ----- | -------- | --------------- | ---------------- | ----------------- |
| Warcasket    | 40    |          |                 |                  |                   |
| Marine       | 55    |          |                 |                  |                   |
| Recon        | 40    |          |                 |                  |                   |
| Cataphract   | 75    |          |                 |                  |                   |
| Aerial       | 20    | 20       |                 |                  |                   |
| Barrage      | 75    | 20       |                 |                  |                   |
| Hazard       | 55    |          |                 | 50               |                   |
| Shock        | 60    | 10       |                 |                  |                   |
| Siegebreaker |       | 60       |                 |                  |                   |
| Guardian     | 60    | 80       |                 |                  |                   |
| Controller   |       | 60       | 2               |                  |                   |
| Sarcophagus  |       | 60       |                 |                  | 2                 |
| Brute        |       | 60       | 4               |                  |                   |

researchPrerequisites and apparel tags match the armor table above (same set, same values). All
shoulder pads use `apparel.layers` = [OnSkin, Middle, Shell] and `apparel.bodyPartGroups` =
[Shoulders, Arms, Hands, Neck]. `tickerType` stays at the inherited Never for every shoulder-pad
def except Barrage, which explicitly overrides it to Normal, this is the only shoulder-pad
`tickerType` override and is functionally meaningful (unlike the armor redeclarations above).

Comps and modExtension fields beyond the base `CompProperties_Forbiddable` + `CompColorable` +
`ApparelExtension{isUnifiedApparel}`:

| Set                                                                       | Extra comps                                                                                                        | Extra modExtension fields                                                                                                               | Extra verbs                                                                                          |
| ------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------ | --------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------- |
| Warcasket / Marine / Recon / Cataphract / Siegebreaker / Guardian / Brute | none                                                                                                               | none                                                                                                                                    | none                                                                                                 |
| Aerial                                                                    | CompProperties_ApparelReloadable (100 charges, Chemfuel, "jump"); CompProperties_AbilitiesApparel (VFEP_PowerJump) | equippedStatFactors={VFEP_FlightSpeed=1.5}                                                                                              | none                                                                                                 |
| Barrage                                                                   | CompProperties_ApparelReloadable (4 charges, Steel, 25/charge, "grenade")                                          | equippedStatFactors={ShootingAccuracyPawn=1.1}                                                                                          | `VFEPirates.Verb_LaunchProjectileStaticMultiple` ("grenade barrage", 4-projectile burst, range 12.9) |
| Hazard                                                                    | none                                                                                                               | equippedStatFactors={Flammability=0.5}                                                                                                  | none                                                                                                 |
| Shock                                                                     | none                                                                                                               | traitsOnEquip=[VFEP_WarcasketTrait], traitsOnUnequip=[VFEP_Shellcasket], equippedStatFactors={MeleeHitChance=1.2, MeleeDodgeChance=1.2} | none                                                                                                 |
| Controller                                                                | CompProperties_ApparelReloadable (1 charge, ComponentSpacer, "spider mine deployment")                             | none                                                                                                                                    | `VFEPirates.Verb_Spidermine` (range 14.9)                                                            |
| Sarcophagus                                                               | none                                                                                                               | pawnCapacityMinLevels={BloodFiltration>=1, BloodPumping>=1, Metabolism>=1}                                                              | none                                                                                                 |

## Helmets

### statBases

| Set          | Flammability | DeteriorationRate | Beauty | WorkToMake | Mass | ArmorRating_Sharp | ArmorRating_Blunt | ArmorRating_Heat | Insulation_Cold | Insulation_Heat | EquipDelay |
| ------------ | ------------ | ----------------- | ------ | ---------- | ---- | ----------------- | ----------------- | ---------------- | --------------- | --------------- | ---------- |
| Warcasket    | _1_          | _0_               | _-3_   | _1000_     | 5    | 1.06              | 0.55              | 0.64             | 4               | 2               | 1          |
| Marine       | _1_          | _0_               | _-3_   | _1000_     | 8    | 1.20              | 0.65              | 0.74             | 5               | 2               | 1          |
| Recon        | _1_          | _0_               | _-3_   | _1000_     | 4    | 1.02              | 0.60              | 0.66             | 12              | 4               | 1          |
| Cataphract   | _1_          | _0_               | _-3_   | _1000_     | 8    | 1.56              | 0.65              | 0.78             | 4               | 2               | 1          |
| Aerial       | _1_          | _0_               | _-3_   | _1000_     | 5    | 1.65              | 0.72              | 0.86             | 4               | 2               | 1          |
| Barrage      | _1_          | _0_               | _-3_   | _1000_     | 12   | 2.00              | 0.90              | 1.08             | 4               | 2               | 1          |
| Hazard       | _1_          | _0_               | _-3_   | _1000_     | 5    | 1.20              | 0.65              | 2.00             | 14              | 12              | 1          |
| Shock        | _1_          | _0_               | _-3_   | _1000_     | 10   | 1.65              | 1.24              | 0.86             | 4               | 2               | 1          |
| Siegebreaker | _1_          | _0_               | _-3_   | _1000_     | 8    | 2.00              | 1.00              | 1.00             | 5               | 2               | 1          |
| Guardian     | _1_          | _0_               | _-3_   | _1000_     | 8    | 2.00              | 1.50              | 1.00             | 5               | 2               | 1          |
| Controller   | _1_          | _0_               | _-3_   | _1000_     | 8    | 2.00              | 1.00              | 1.00             | 10              | 10              | 1          |
| Sarcophagus  | _1_          | _0_               | _-3_   | _1000_     | 8    | 2.00              | 1.00              | 1.00             | 10              | 10              | 1          |
| Brute        | _1_          | _0_               | _-3_   | _1000_     | 8    | 2.00              | 1.00              | 1.00             | 10              | 10              | 1          |

All helmets additionally set `uiIconScale` = 1.25 directly (armor and shoulder pads never set
`uiIconScale`).

### equippedStatOffsets

| Set          | MoveSpeed | SlaveSuppressionOffset | VFEP_PowerJumpRange | ToxicResistance | PsychicSensitivity | VacuumResistance |
| ------------ | --------- | ---------------------- | ------------------- | --------------- | ------------------ | ---------------- |
| Warcasket    | -0.10     | 0.1¹                   |                     |                 |                    |                  |
| Marine       | -0.10     | 0.1¹                   |                     |                 |                    |                  |
| Recon        |           | 0.1¹                   |                     |                 |                    |                  |
| Cataphract   | -0.15     | 0.1¹                   |                     |                 |                    |                  |
| Aerial       | -0.10     | 0.1¹                   | 10                  |                 |                    |                  |
| Barrage      | -0.20     | 0.1¹                   |                     |                 |                    |                  |
| Hazard       | -0.10     | 0.1¹                   |                     | 1               |                    |                  |
| Shock        | -0.10     | 0.1¹                   |                     |                 |                    |                  |
| Siegebreaker |           | 0.1¹                   |                     | 0.5             | -0.5               | 0.7²             |
| Guardian     |           | 0.1¹                   |                     | 0.5             | -0.5               | 0.7²             |
| Controller   |           | 0.1¹                   |                     | 0.5             | -0.5               | 0.7²             |
| Sarcophagus  |           | 0.1¹                   |                     | 0.5             | -0.5               | 0.7²             |
| Brute        |           | 0.1¹                   |                     | 0.5             | -0.5               | 0.7²             |

Recon's helmet is the only piece in the entire 39-def armor/shoulder/helmet lineup with no
`MoveSpeed` offset at all (see Anomalies).

### Non-stat data

costList:

| Set          | ComponentIndustrial | ComponentSpacer | Steel | Plasteel | MedicineUltratech |
| ------------ | ------------------- | --------------- | ----- | -------- | ----------------- |
| Warcasket    | 1                   |                 | 40    |          |                   |
| Marine       | 1                   |                 | 65    |          |                   |
| Recon        | 1                   |                 | 40    |          |                   |
| Cataphract   | 2                   |                 | 75    |          |                   |
| Aerial       | 3                   |                 | 45    | 10       |                   |
| Barrage      | 3                   |                 | 45    | 25       |                   |
| Hazard       | 3                   |                 | 35    | 30       |                   |
| Shock        | 4                   |                 | 40    | 10       |                   |
| Siegebreaker |                     | 2               |       | 50       |                   |
| Guardian     |                     | 2               |       | 70       |                   |
| Controller   |                     | 4               |       | 60       |                   |
| Sarcophagus  |                     | 2               |       | 60       | 3                 |
| Brute        |                     | 4               |       | 60       |                   |

researchPrerequisites and apparel tags match the armor table above. All helmets use
`apparel.layers` = [Overhead] and `apparel.bodyPartGroups` = [FullHead]. `tickerType` stays at the
inherited Never for every helmet (none override it).

Comps and modExtension fields beyond the base `CompProperties_Forbiddable` + `CompColorable` +
`ApparelExtension{isUnifiedApparel, hideHead}`:

| Set                                                                                               | Extra comps                           | Extra modExtension fields                                                                | Extra verbs                                                                      |
| ------------------------------------------------------------------------------------------------- | ------------------------------------- | ---------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------- |
| Warcasket / Marine / Recon / Cataphract / Aerial / Barrage / Siegebreaker / Guardian / Controller | none                                  | none                                                                                     | none                                                                             |
| Hazard                                                                                            | none                                  | equippedStatFactors={PsychicSensitivity=0}                                               | none                                                                             |
| Shock                                                                                             | none                                  | equippedStatFactors={MeleeDodgeChance=1.2}                                               | none                                                                             |
| Sarcophagus                                                                                       | none                                  | pawnCapacityMinLevels={Consciousness>=1, Sight>=1, Hearing>=1, Talking>=1, Breathing>=1} | none                                                                             |
| Brute                                                                                             | `MVCF.Comps.CompProperties_VerbGiver` | none                                                                                     | `VFEPirates.Verb_ShieldDetonation` ("shield detonation", non-violent, self-cast) |

## Conditional patches

### 1.6/Patches/SOS2Patch.xml

Gated by `PatchOperationFindMod` on mod display name "Save Our Ship 2" **or** "Universum" (either
active is enough; matched by display name, not package id). If either is active, it runs two
pairs of operations, one pair for the 13 armor defs and one pair for the 13 helmet defs, each
targeted by an explicit `defName` list (`VFEP_Warcasket_Warcasket`, `VFEP_Warcasket_Marine`, ...,
`VFEP_Warcasket_Brute` for armor; the matching `VFEP_WarcasketHelmet_*` names for helmets):

- `PatchOperationAdd` appends an `EVA` entry to `apparel/tags`, on top of whatever tags are
  listed in the tables above.
- `PatchOperationReplace` overwrites `statBases/Insulation_Cold` with `100`, replacing whatever
  value is in the Insulation_Cold column above (4 to 80 depending on the piece) with a flat 100
  across all 13 armor pieces and all 13 helmet pieces uniformly.

Shoulder pads and the Bodysuit are not targeted at all, neither their `apparel/tags` nor their
`Insulation_Cold` change under this patch.

### 1.6/Patches/Odyssey.xml

Gated by `PatchOperationFindMod` on mod display name "Odyssey". It does **not** touch any
`VFEPirates.WarcasketDef` at all: it adds a new `PawnKindDef` (`VFEP_Salvager_Warcasket`, apparel
tag `WarcasketVeteran`) and adds that pawn kind to the `Salvagers` faction's `Combat`
pawnGroupMaker options (weight 1). None of the stats, costs, tags or comps documented above are
changed by this file. The Odyssey-only `VacuumResistance` entries visible in the tables above
(all `MayRequire="Ludeon.RimWorld.Odyssey"`) are baked directly into the base def XML, not
injected by this patch.

### Other files referencing "Warcasket"

`1.6/Patches/Core/Patches.xml` and `1.6/Patches/Core/Traders.xml` each contain one unrelated
string match (`VFEP_RemoveWarcasket` in a job/thought context, and the `VFEP_WarcasketWeaponExotic`
trade tag used by warcasket-branded weapons); neither touches a `WarcasketDef`. The
`1.6/Mods/*/Defs/*RangedWarcasket.xml` files (VFEV, VWEBF, VWEC, VWEL, VWENL, VWEQ) add
warcasket-branded ranged weapons (e.g. a crypto cannon), not apparel, and likewise never touch a
`WarcasketDef`. No other file under `1.6/Patches/` or `1.6/Mods/*/Patches/` references
`WarcasketDef` (there is no `1.6/Mods/*/Patches/` folder at all in this checkout).

**None of these patches key off the `VFEP_WarcasketArmorBase`/`ShoulderPadBase`/`HelmetBase`
parent type or a shared tag; they all match by literal `defName`.** A third-party warcasket def
(such as anything Medic Warcasket eventually adds) that parents on the same VFEP abstract
bases is not covered by SOS2Patch.xml's EVA tag or Insulation_Cold override, and would need its
own equivalent patch if that behavior is wanted.

## Anomalies

These are observations from reading the resolved XML, listed factually without guessing at
intent:

1. `VFEP_WarcasketShoulders_Recon` sets `EquipDelay` to 14, while every other one of the 39
   armor/shoulder/helmet defs (including Recon's own armor and helmet) uses `EquipDelay` 1.
2. `VFEP_WarcasketHelmet_Recon` has no `MoveSpeed` entry in `equippedStatOffsets` at all. Every
   other helmet in the lineup has one (-0.1 to -0.2), and Recon's own armor (-0.2) and shoulders
   (-0.1) do too.
3. Hazard's two pieces reach a reduced Flammability by two different mechanisms with two
   different net results: `VFEP_Warcasket_Hazard` (armor) sets `statBases/Flammability` to 0
   directly _and_ applies an `equippedStatFactors` Flammability factor of 0 in its modExtension
   (redundant, both already yield 0), while `VFEP_WarcasketShoulders_Hazard` leaves
   `statBases/Flammability` at the inherited 1.0 and only applies an `equippedStatFactors` factor
   of 0.5 (net 0.5 while worn). `VFEP_WarcasketShoulders_Recon` also sets `statBases/Flammability`
   to 0 directly, with no accompanying factor.
4. `VFEP_WarcasketShoulders_Shock` carries `traitsOnEquip`=[VFEP_WarcasketTrait] and
   `traitsOnUnequip`=[VFEP_Shellcasket] in its modExtension. No other shoulder-pad def in the
   lineup sets these fields; on every other set only the armor piece declares them, so Shock is
   the one set where two worn pieces both declare the same trait grant/removal.
5. `VFEP_Warcasket_Sarcophagus`'s `pawnCapacityMinLevels` requires `Moving`>=1 and
   `Manipulation`>=1. Every other armor piece that sets `pawnCapacityMinLevels`
   (Warcasket, Marine, Recon, Cataphract, Aerial, Barrage, Hazard, Shock, Siegebreaker, Guardian,
   Controller, Brute) uses 0.7 for both.
6. Five armor defs (Siegebreaker, Guardian, Controller, Sarcophagus, Brute) explicitly redeclare
   `<tickerType>Normal</tickerType>`, which is a no-op since `VFEP_WarcasketArmorBase` already
   sets `tickerType` to Normal for every armor piece. `VFEP_WarcasketShoulders_Barrage` also
   redeclares `tickerType` to Normal, but that one is functionally meaningful since
   `VFEP_WarcasketShoulderPadBase` leaves shoulder pads at the inherited Never.
7. `VFEP_Warcasket_Aerial` and `VFEP_WarcasketShoulders_Aerial` each independently apply an
   `equippedStatFactors` `VFEP_FlightSpeed` factor of 1.5, so wearing both stacks two factors;
   whether they multiply is decided by the stat code, not the XML. Only the shoulders carry the
   100-charge Chemfuel `CompProperties_ApparelReloadable` and the `CompProperties_AbilitiesApparel`
   granting `VFEP_PowerJump`; an earlier revision of this doc wrongly listed them on the armor as
   well (corrected 2026-09-15).
8. `VFEP_Warcasket_Bodysuit` resolves to an entirely empty `costList` (no `costList` tag anywhere
   in its chain), consistent with it never being built via its own recipe.
9. `VacuumResistance` (all `MayRequire="Ludeon.RimWorld.Odyssey"`) only appears on the five
   10th-generation "spacer" sets (Siegebreaker, Guardian, Controller, Sarcophagus, Brute), never
   on the eight earlier sets. `ToxicResistance` and `PsychicSensitivity` offsets mostly follow the
   same split, except `VFEP_WarcasketHelmet_Hazard` also carries a `ToxicResistance` offset of 1
   (versus 0.5 on the spacer-tier helmets) despite belonging to the earlier, non-spacer tier.
