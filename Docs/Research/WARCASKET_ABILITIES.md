# Warcasket Abilities Research

> Copied from the sibling Shipcracker Warcasket repo, where it was written while scoping that set;
> the VFEP numbers are set-independent and serve this set the same way.

Working notes for designing the Shipcracker set's abilities and cross-piece synergy. This doc
covers *mechanics*: what each Vanilla Expanded warcasket piece does beyond its stats, how the
pieces of a set talk to each other, which framework levers implement that, and what the
neighbouring Odyssey and Gravship Expanded apparel does. Baseline stat tables (armor, insulation,
costs, tags, inheritance) stay in `VFEP_WARCASKET_STATS.md`; our own tuning rationale lives in the
def headers under `1.6/Defs/`; the open scoping items are in `TODOs.md`.

Sources, all read on 2026-09-15: Vanilla Factions Expanded - Pirates (VFEP) at `5a5618f`
(`../VanillaExpanded/VanillaFactionsExpanded-Pirates`, `1.6/Defs/ThingDefs_Misc/Apparel_Various.xml`,
`Apparel_Headgear.xml`, `1.6/Defs/AbilityDefs/Abilities.xml`, `1.6/Defs/Stats/Stats_Abilities.xml`,
`1.6/Defs/ThingDefs_Races/Races_Mechanoid.xml`, `1.6/Source/VFEPirates/`), Vanilla Expanded
Framework (VEF, `../VanillaExpanded/VanillaExpandedFramework`, `Source/VEF/Apparels/`,
`Source/VEF/Abilities/`, `Source/MVCF/`), Vanilla Gravship Expanded chapter 1 (`a467530`) and
chapter 2 (`e8f16e6`), Odyssey's `Data/Odyssey/Defs`, Royalty's `Data/Royalty/Defs` (2026-09-16), and the game's
`Assembly-CSharp.dll` via `ilspycmd`. Every number is what ships in that XML or code. Informational only, not mod content,
not committed.

## Set functionality, all thirteen VFEP sets

"Standard block" below means what every piece inherits or declares identically: `Forbiddable`,
`CompColorable`, `isUnifiedApparel` (helmets also `hideHead`), and on armor shells
`VEF_MassCarryCapacity +125`, the `VFEP_WarcasketTrait` on equip / `VFEP_Shellcasket` on unequip
trait swap, and `pawnCapacityMinLevels` Moving and Manipulation at 0.7. The 10th-generation
spacer sets additionally share `PsychicSensitivity -0.5`, `ToxicResistance +0.5` on armor and
helmet and `VacuumResistance` 0.2 / 0.1 / 0.7 (armor / shoulders / helmet, Odyssey only).

Shield energy shown as the in-game pool: `EnergyShieldEnergyMax` x100. Recharge
`EnergyShieldRechargeRate` r means the bubble regains r x max per second, so 0.01 is a full
refill in 100 s and 0.05 in 20 s (`CompShieldBubble.EnergyGainPerTick` = r / 60, applied x max).

### 3rd to 5th generation: stats only

| Set | Gen | Armor shell | Shoulder pads | Helmet | Identity |
| --- | --- | ----------- | ------------- | ------ | -------- |
| **Warcasket** | 3rd | Standard block. Sharp 1.06 / Blunt 0.55, Mass 50, `MoveSpeed -0.5`. | Standard. `MoveSpeed -0.2`. | Standard. | The baseline. |
| **Marine** | 4th | Standard. Sharp 1.20 / Blunt 0.65, Mass 50, `MoveSpeed -0.5`. | Standard. `MoveSpeed -0.2`. | Standard. | Straight armor upgrade at equal mass. |
| **Recon** | 5th | Standard. Sharp 1.02 / Blunt 0.6, Mass 40, `MoveSpeed -0.2`. | Standard. Mass 15, `MoveSpeed -0.1`, `Flammability` 0, `EquipDelay` 14 (every other piece is 1). | Standard. Mass 4, no `MoveSpeed` offset at all. | Lighter and faster. |
| **Cataphract** | 5th | Standard. Sharp 1.56 / Blunt 0.65, Mass 75, `MoveSpeed -0.85`. | Standard. Mass 35, `MoveSpeed -0.35`. | Standard. | Heaviest pre-spacer tank. Tags `WarcasketHeavy`, `WarcasketCata`. |

No comps, verbs or abilities on any of the twelve pieces.

### 7th generation: "designed to work as a set"

| Set | Armor shell | Shoulder pads | Helmet | Role and synergy model |
| --- | ----------- | ------------- | ------ | ---------------------- |
| **Aerial** | No comps. Feeds the jump through `equippedStatOffsets` on custom stats: `VFEP_PowerJumpRange +25`, `VFEP_PowerJumpDetonationRadius +1`; `equippedStatFactors` `VFEP_FlightSpeed` x1.5. Sharp 1.65 / Blunt 0.72. | Grants `VFEP_PowerJump` (`CompAbilitiesApparel`) fueled by `CompApparelReloadable`: 100 charges, 1 Chemfuel per charge, 60-tick reload. `Ability_PowerJump` burns 20 charges per jump (5 per tank), 60-tick cast, `targetMode` Location, target must be a walkable cell within the wearer's `VFEP_PowerJumpRange`; no line-of-sight test. Bomb explosion, 15 damage, radius = wearer's `VFEP_PowerJumpDetonationRadius` (base 3), at takeoff and again at landing, wearer excluded. Adds `VFEP_PowerJumpRange +5`, `VFEP_FlightSpeed` x1.5. | `VFEP_PowerJumpRange +10` only. | Mobility skirmisher. Shoulders are the engine; armor and helmet feed it through custom StatDefs the ability reads off the pawn. Full set jumps 40 tiles with a radius-4 blast; shoulders alone 5 tiles, radius 3. |
| **Barrage** | Grants `VFEP_SiegeMode` (`CompAbilitiesApparel`): a self-target toggle (`Ability_SiegeMode`, `CommandAbilityToggle`) applying hediff `VFEP_SiegeMode`, `VEF_VerbRangeFactor +0.25` and `RangedCooldownFactor -0.25`. `Hediff_SiegeMode` removes itself and flips the toggle off the moment the pawn starts moving. `cooldownTime` 600 ticks scaled by stat `VFEP_SiegeModeCooldown`. `MoveSpeed -1.0`. | `tickerType` Normal. `CompApparelReloadable`: 4 charges, 25 Steel per charge, "grenade". Verb `Verb_LaunchProjectileStaticMultiple` with `VerbProps_MultipleProjectiles`: "grenade barrage", `projectileCount` 4 of `VFEP_Proj_GrenadierGrenade` (vanilla frag grenade stats), `range` 12.9, `warmupTime` 1.5, `forcedMissRadius` 1.9, `ai_IsBuildingDestroyer`; one activation loops `TryCastShot` four times. `equippedStatFactors` `ShootingAccuracyPawn` x1.1. `MoveSpeed -0.4`. | Standard. `MoveSpeed -0.2`. | Planted fire support. Each piece is independent: armor buffs any ranged weapon while stationary, shoulders add an area burst, helmet is inert. No cross-piece reads. |
| **Hazard** | `statBases` `Flammability` 0 and `equippedStatFactors` `Flammability` x0. No comps. | `equippedStatFactors` `Flammability` x0.5. `MoveSpeed -0.4`. | `equippedStatFactors` `PsychicSensitivity` x0; `ToxicResistance +1`. | Environmental immunity, pure XML. The `WarcasketFlamer` tag it shares with the heavy flamer only pairs them at pawn generation (`VFEP_Pyro` kind); no runtime fire interaction. |
| **Shock** | Grants `VFEP_BlastOff` (`CompAbilitiesApparel`) fueled by a 100-charge Chemfuel `CompApparelReloadable`; `Ability_BlastOff` needs and burns the whole tank. `worldTargeting`: drop-pods the pawn to any map within `maxLaunchDistance` 30 world tiles (`CanHitTargetTile` requires a `MapParent`), 60-tick cast. On landing `PawnIncoming.Impact` fires a radius-15 Bomb explosion for 15 damage that ignores the wearer. Mass 65, `MoveSpeed -0.75`, Blunt 1.24. | `equippedStatFactors` `MeleeHitChance` x1.2, `MeleeDodgeChance` x1.2. `MoveSpeed -0.2`. | `equippedStatFactors` `MeleeDodgeChance` x1.2. | Orbital-drop melee. Armor delivers the pilot, shoulders and helmet make them a better brawler once there. "Benefits from enemy friendly fire" is flavor; nothing implements it. |

### 10th generation: spacer sets

| Set | Armor shell | Shoulder pads | Helmet | Role and synergy model |
| --- | ----------- | ------------- | ------ | ---------------------- |
| **Siegebreaker** | Ranged-only shield bubble (`CompShieldBubble`, `blockRangedAttack`, melee passes, wearer shoots through): 250 energy, recharge 0.01 (100 s to full). Breaks on depletion, hardcoded 3200-tick reset, returns at 20%. EMP zeroes it and passes. Sharp 2.0 / Blunt 1.0. | No comps. `EnergyShieldRechargeRate +1` as an `equippedStatOffsets` entry, meant to speed the armor's recharge. **Dead XML** (discrepancy 1). | Standard. | Ranged fire support. Armor carries the mechanic, shoulders were meant to amplify it, helmet is inert. |
| **Guardian** | Same bubble at 150 energy, 0.01. Plus `CompShieldField`: `manualActivation`, `workingTimeTicks` 600 (10 s), `cooldownTicks` 5400 (90 s), radius from `VEF_EnergyShieldRadiusApparel` 5 read off the armor Thing. No energy stats set, so the field is `Indestructible`: it cannot be drained or EMP'd, only time-gated. `EnergyShieldTick` intercepts every projectile inside the radius whose launcher is outside it, protecting everyone inside; shots from inside pass; `flyOverhead` projectiles past midpoint are exempt. No melee blocking. | `thingClass` `VFEPirates.Apparel_GuardianShoulders`: `CheckPreAbsorbDamage` fully negates any damage instance with a flat 25% chance. `MoveSpeed -0.5` (shoulders elsewhere are -0.1 to -0.4). | Standard. | Squad protector. Armor projects a 10-second area dome for allies, shoulders give the wearer a personal 25% negate. Independent pieces. |
| **Controller** | Bubble at 150 energy, 0.01. `CompApparelReloadable` 1 charge, 1 ComponentSpacer, "drone deployment". Verb `Verb_DroneDeployment` (`targetable` false, non-violent): spawns 3 `VFEP_Mech_Wardrone` at the wearer's feet, **4 if `VFEP_WarcasketHelmet_Controller` is worn** (explicit def check in C#). Wardrone: `MoveSpeed` 4.7, `baseHealthScale` 0.22, autonomous `VFEP_WarDrone` think tree, miniturret gun range 28.9, `CompProperties_DestroyAfterDelay` 3600 ticks, small explosion on death. | `CompApparelReloadable` 1 charge, 1 ComponentSpacer, "spider mine deployment". Verb `Verb_Spidermine`: `range` 14.9, `warmupTime` 0.5, `VFEP_SpidermineProjectile` spawns one `VFEP_Mech_Spidermine` at impact; it walks to the nearest enemy and explodes (flame, radius 4.9), self-destructs if nothing to hunt. | Standard. Its only function is the +1 drone the armor's verb grants when it is worn. | Summoner. The one set where a piece checks for a sibling piece by def in code. |
| **Sarcophagus** | Bubble at 150 energy, 0.01. `pawnCapacityMinLevels` at 1.0: Moving, Manipulation, BloodFiltration, BloodPumping, Metabolism. `preventDowning`, `preventBleeding`, `preventKilling` gated by `preventKillingUntilHealthHPPercentage` 0.6 and `preventKillingUntilBrainMissing`: the pilot dies only once remaining body-part max-HP falls to 60% or the brain is gone. | `pawnCapacityMinLevels` at 1.0: BloodFiltration, BloodPumping, Metabolism (duplicates the armor). Description promises Manipulation; the XML does not floor it. | `pawnCapacityMinLevels` at 1.0: Consciousness, Sight, Hearing, Talking, Breathing. | Unkillable pilot. Each piece floors the capacities of the body region it covers. All death suppression lives on the armor. |
| **Brute** | Bubble at **500 energy, recharge 0.05** (20 s to full), `blockRangedAttack`, `blockMeleeAttack` false, **`dontAllowRangedAttack`**: `CompAllowVerbCast` refuses every non-melee verb, so the wearer is melee-only while shielded. Sharp 2.0 / Blunt 1.5. Tags `WarcasketVeteran`, `WarcasketMelee`, no `WarcasketAll`. | `equippedStatOffsets` `VEF_EnergyShieldEnergyMaxFactor +0.5`: a pawn-level stat `CompShieldBubble.EnergyMax` does read, so the bubble becomes 750. Sharp 2.0 / Blunt 1.5. No melee stats despite the "melee assistor" flavor. | `MVCF.Comps.CompProperties_VerbGiver` exposes verb `Verb_ShieldDetonation` ("shield detonation", self-cast, `IsMeleeAttack`): sums `Energy` over every worn `CompShieldBubble`, Bomb explosion centered on the wearer with radius energy / 50 and damage energy / 20 (750 energy: radius 15, 37 damage), wearer excluded, then zeroes every shield without calling `Break`, so no reset timer. Available only while some shield has energy. No cooldown of its own. | Melee bomb. Armor is the battery, shoulders enlarge it, helmet spends it. The tightest three-piece loop in VFEP, and the pawn-level shield stat is the mechanism that makes the shoulders work. |

### Synergy models VFEP actually uses

1. **Custom StatDefs read off the pawn** (Aerial): the ability calls `pawn.GetStatValue` on
   `VFEP_PowerJumpRange` etc., so any worn piece can contribute an offset. Cleanest and fully XML
   on the contributing pieces.
2. **Pawn-level VEF shield stats** (Brute shoulders): `VEF_EnergyShieldEnergyMaxFactor` and
   `VEF_EnergyShieldEnergyMaxOffset` are read off the pawn by `CompShieldBubble.EnergyMax`. There
   is no recharge or radius equivalent; those are read off the apparel Thing (discrepancy 1).
3. **Explicit sibling check in C#** (Controller helmet): `Verb_DroneDeployment` looks for the
   helmet def in `WornApparel` and adds a drone. Simple, but hard-codes the def.
4. **`equippedStatFactors` via `ApparelExtension`** (Shock, Barrage, Hazard): multiplicative stat
   tweaks applied by VEF's stat patch; independent of other pieces.
5. **`pawnCapacityMinLevels` per body region** (Sarcophagus): each piece floors the capacities it
   plausibly covers.
6. **`thingClass` override** (Guardian shoulders): `Apparel.CheckPreAbsorbDamage` for a per-hit
   effect with no comp at all.
7. **Verb-on-apparel** via vanilla `<verbs>` plus `CompApparelReloadable` (Barrage shoulders,
   Controller) or MVCF `Comp_VerbGiver` (Brute helmet), versus **ability-on-apparel** via VEF
   `CompAbilitiesApparel` (Aerial, Barrage armor, Shock). Verbs get reload/ammo for free;
   abilities get cooldowns, cast times, toggles and world targeting.

### Flavor text that the code does not back

1. **Siegebreaker shoulders' recharge bonus never reaches the shield.** `CompShieldBubble` reads
   `EnergyShieldRechargeRate` and `EnergyShieldEnergyMax` via `parent.GetStatValue(...)` on the
   apparel Thing (statBases and quality only), and vanilla `StatWorker.GetValueUnfinalized`
   applies `equippedStatOffsets` only when the stat request is for a Pawn. The wearer's stat goes
   up; nothing reads it. `SCWC_WarcasketShoulders_Shipcracker` carries the same copied offset, so
   it is dead for us too. `CompShieldField` reads its radius stat the same way.
2. Brute's description says the shield blocks melee as well; `blockMeleeAttack` is false. It also
   calls the recharge "particularly slow"; at 0.05 it is five times Siegebreaker's.
3. Brute shoulders' "melee assistor modules" have no stat behind them.
4. Sarcophagus shoulders promise Manipulation replacement and do not floor it.
5. Shock "benefits from enemy friendly fire": nothing in VFEP code or patches.
6. Controller helmet's "deployment module but no controller" is exactly the +1 drone check; fine,
   but the helmet has no comp of its own.
7. Hazard's flamer link is a pawn-kind loadout pairing only.

### Tag gating worth knowing

`StaticStartup` collects every tag used only by `WarcasketDef`s; `PawnGenerator_GeneratePawn_Patch`
fills a warcasket wearer's missing slots with pieces whose tags intersect the pawn kind's
`apparelTags`. Sets carrying `WarcasketAll` are in every warcasket kind's pool; Brute
(`WarcasketVeteran`, `WarcasketMelee` only) appears only on kinds that list those tags. Our set's
tags decide which raiders and mercenaries can spawn in it.

## Framework levers (no new C# needed unless noted)

- **VEF `AbilityDef`** (`Source/VEF/Abilities/Defs/AbilityDef.cs`): `range` plus
  `rangeStatOffsets`/`rangeStatFactors` and `maxRange`, `cooldownTime`, `castTime`,
  `requireLineOfSight` (default true; VFEP's Power Jump overrides `CanHitTarget` and skips it),
  `worldTargeting`, `showUndrafted`, `requiredTrait`, `requiredHediff`, `verbProperties`.
- **VEF `Ability`** virtuals (`Source/VEF/Abilities/Misc/Ability.cs`): `ShowGizmoOnPawn`,
  `IsEnabledForPawn(out reason)`, `GetRangeForPawn`, `CanHitTarget(target, sightCheck)`,
  `ValidateTarget`, `Cast`, `PostCast`. A subclass can switch range and sight rules on
  `pawn.Map.Biome.inVacuum`, so "normal jump on a planet, unlimited line-of-sight jump in space"
  is one ability class, not two defs. `CompAbilitiesApparel` builds its list once at
  `Initialize`, so two defs toggled via `ShowGizmoOnPawn` also work.
- **Vanilla `CompProperties_ApparelReloadable`**: single `ammoDef`, `ammoCountPerCharge`,
  `ammoCountToRefill`, `baseReloadTicks`, `replenishAfterCooldown`. Swapping Chemfuel for
  `VGE_Astrofuel` (VGE1 item: stack 75, market value 6, mass 0.1, made 35 per batch at the
  astrofuel synthesizer recipe in `1.6/Defs/RecipeDefs/Recipes_Production.xml`) is a one-node
  patch on `ammoDef`, shippable from a `1.6/Mods/<VGE1>/Patches/` compat root gated on its
  package id. No C# needed.
- **VFEP Power Jump internals** (`Ability_PowerJump.cs`): `CanHitTarget` = walkable cell within
  `VFEP_PowerJumpRange` (base 0), blast radius `VFEP_PowerJumpDetonationRadius` (base 3, min 3, so
  Aerial's +1 gives 4), flight speed `VFEP_FlightSpeed` (base 12). Both detonations are
  `GenExplosion.DoExplosion(..., DamageDefOf.Bomb, damage 15)` ignoring the wearer.
- **Demolition landing**: Core has a `Demolish` DamageDef (`DamageDefs/Damages_MeleeWeapon.xml`)
  with `buildingDamageFactor` 10 and `buildingDamageFactorImpassable` 0.75; `DamageDef` also
  exposes `buildingDamageFactorPassable`. A landing explosion using a custom DamageDef with those
  factors hits structures hard without scaling pawn damage.

## The Royalty "shipcracker" backstory

The set's name and role come from Royalty's `Shipcracker37` adulthood `BackstoryDef`
(`Data/Royalty/Defs/BackstoryDefs/Shuffled/ImperialFighter_Adult.xml`, read 2026-09-16; the only
"shipcracker" in any DLC's data). Title and short title `shipcracker`; spawn categories
`ImperialFighter` and `ImperialRoyal`, so it appears on Empire fighters and royals; `requiredWorkTags`
Violent; body type Hulk for both sexes; skill gains Shooting 3, **Melee 5, Mining 4**; forced trait
`DrugDesire` 1 (chemical interest). Description verbatim:

> [PAWN_nameDef]'s role was to launch through the vacuum of space, land on the enemy ship's hull,
> punch holes to the interior, and conquer it in room-to-room combat. [PAWN_pronoun] became very
> good at wielding heavy cataphract weapons to cut both steel and flesh.
>
> [PAWN_pronoun] also developed a taste for the drugs [PAWN_pronoun] used to cope with the
> overwhelming stress.

Read as a design brief it gives four beats, each with a mechanic already traced above:

| Beat | Mechanic in the brief | Lever |
| ---- | --------------------- | ----- |
| "launch through the vacuum of space" | A jump that has no range limit where there is no gravity, but needs a clear line | VEF `Ability.GetRangeForPawn` / `CanHitTarget(target, sightCheck)` overrides switched on `map.Biome.inVacuum` (Odyssey `Space` biome) or `map.Tile.LayerDef.isSpace` (`Orbit` layer); VEF `moveSpeedFactorByTerrainTag` on the `Space` terrain tag for walking in vacuum |
| "land on the enemy ship's hull, punch holes to the interior" | The landing wrecks structures more than people | Landing explosion with a custom `DamageDef` using `buildingDamageFactor` / `buildingDamageFactorPassable` / `buildingDamageFactorImpassable` (Core `Demolish` is 10 / 1 / 0.75). Odyssey's `GravshipHull` is a `Wall` child at 420 HP, `Flammability` 0 |
| "heavy cataphract weapons to cut both steel and flesh"; Melee 5, Mining 4 | Melee and structure damage from the arms | `equippedStatFactors` via `ApparelExtension` on the shoulders (Shock's model) for melee stats; the Mining skill gain is the backstory's own nod to breaching |
| "room-to-room combat" | Close-quarters work once inside | Helmet-side melee or sight stat, or a helmet-only relaxation of the jump's sight rule (Controller's sibling-check model, or a helmet-granted stat the ability reads) |

The drug line is flavor for the description text; nothing in the set should implement it.

## Neighbouring Odyssey and Gravship Expanded apparel

Reference for the non-warcasket items the Shipcracker abilities are likely to borrow from.
Traced 2026-09-15 from Vanilla Gravship Expanded chapter 1 (`../VanillaGravshipExpanded`,
`a467530`) and chapter 2 (`../VanillaGravshipExpanded2`, `e8f16e6`), Odyssey's own defs, and the
game DLL. "Vac armor" is only an art folder name (`Things/Equipment/Vacarmor/`) for the VGE2 combat
vacsuit; no def by that name exists in VGE1, VGE2 or Odyssey.

### Astrorig (`VGE_Apparel_Astrorig`, VGE2 `1.6/Defs/ThingDefs_Misc/Apparel_Packs.xml`)

Belt-layer utility pack (Waist, `renderUtilityAsPack`), so it stacks with any shell or armor. Spacer,
research `VGE_AdvancedOrbitalTech`, Crafting 5 at the machining table, cost ComponentIndustrial 2,
GravlitePanel 20, VGE_OxygenCanister 100. Mass 4. `ToxicEnvironmentResistance +0.8`. Three
mechanisms, all on the one def:

| Mechanism | Class | What it does |
| --------- | ----- | ------------ |
| Oxygen tank | `VanillaGravshipExpanded.CompApparelOxygenProvider` (VGE1 `Source/Comps/`) | `maxCharges` 100, `fuelDef` VGE_OxygenCanister, 1 canister per charge, auto-refill at 33.4%, `minResistanceToActivate` 0.12. Drains `consumptionPerTick` (1/2000) once per 60 ticks while the pawn breathes, the biome is `inVacuum`, local vacuum is at least 0.5 and the pawn's base `VacuumResistance` is between 0.12 and 1. VGE1's `StatPart_OxygenPack` (added to `VacuumResistance` by VGE1 `1.6/Patches/Stats.xml`) then forces the pawn's resistance to 1.0 while charges remain. It tops up existing protection; it does not rescue a pawn below 0.12. |
| Oxygen jump | `VanillaGravshipExpanded2.CompApparelVerbOwner_Oxygen` + `Verb_OxygenJump` (subclass of vanilla `Verb_Jump`) | No charge pool of its own: charges = oxygen / `chargePerUse` 10, so 10 jumps per full tank. `cooldownTicks` 3000. Range from the quality-scaled custom stat `VGE_OxygenJumpRange` (base 32.9; awful 0.75 to legendary 1.13). `requireLineOfSight` true, locations only, non-violent, `warmupTime` 0.5. `layerWhitelist: Orbit`, so it casts only on the Orbit planet layer. Drafted-only gizmo. Flight is vanilla `JumpUtility.DoJump` with a `PawnFlyer`. No damage on takeoff or landing. |
| Space movement | `VEF.Apparels.ApparelExtension.moveSpeedFactorByTerrainTag` | Terrain tag `Space` gets `moveSpeedFactor` 27.5, which is the "eliminates movement penalties on space terrain" line. |

`VGE_Apparel_DisposableOxygenPack` (same file) is the cut-down comparison: same oxygen comp,
`destroyOnDrop`, no jump, industrial, single use.

### Vacsuits (stats only, no comps)

| Stat                    | VGE2 combat vacsuit | VGE2 combat helmet | Odyssey vacsuit | Odyssey helmet | VGE1 prestige vacsuit | VGE1 prestige helmet |
| ----------------------- | ------------------- | ------------------ | --------------- | -------------- | --------------------- | -------------------- |
| ArmorRating_Sharp       | 0.88                | 0.88               | 0.52            | 0.52           | 0.52                  | 0.52                 |
| ArmorRating_Blunt       | 0.42                | 0.42               | 0.25            | 0.24           | 0.25                  | 0.24                 |
| ArmorRating_Heat        | 0.78                | 0.78               | 0.66            | 0.66           | 0.66                  | 0.66                 |
| Insulation_Cold / Heat  | 90 / 15             | 6 / 4              | 90 / 15         | 6 / 4          | 90 / 15               | 6 / 4                |
| MaxHitPoints            | 240                 | 160                | 180             | 120            | 180                   | 120                  |
| Mass                    | 18                  | 2                  | 12              | 1.5            | 12                    | 1.5                  |
| EquipDelay              | 14                  | 4                  | 14              | 4              | 14                    | 4                    |
| eq. MoveSpeed           | -0.5                |                    | -1.25           |                | -1.25                 |                      |
| eq. VacuumResistance    | 0.33                | 0.66               | 0.32            | 0.69 (0.65 under VGE1) | 0.32          | 0.65                 |
| eq. ToxicEnvironmentRes | | 0.8 | | 0.8 | | 0.8 |
| eq. Psychic (Royalty)   | | | | | Sensitivity 0.05, EntropyRecovery 0.033 | same |
| costList                | ComponentSpacer 3, Plasteel 90, GravlitePanel 20 | ComponentSpacer 1, Plasteel 20, GravlitePanel 10 | ComponentIndustrial 3, Steel 90, Plasteel 20 | ComponentIndustrial 2, Steel 40 | ComponentIndustrial 3, Steel 100, Plasteel 30, Gold 12 | ComponentIndustrial 2, Steel 50, Gold 6 |
| WorkToMake              | 58000               | 15000              | 48000           | 15000          | 100000                | 31500                |
| research                | VGE_AdvancedOrbitalTech | same           | OrbitalTech     | same           | VGE_OxygenNetwork     | same                 |

Files: VGE2 `1.6/Defs/ThingDefs_Misc/Apparel_CombatVacsuit.xml`; Odyssey
`Defs/ThingDefs_Misc/Apparel_Various.xml` and `Apparel_Headgear.xml`; VGE1
`1.6/Mods/Royalty/Defs/ThingDefs_Misc/Apparel_Royal.xml`. All six are `SpacerMilitary`-tagged
Middle+Shell or Overhead items; the combat pair has its `Vacsuit` tag commented out.

### Vacuum mechanics and the VGE1 resistance patch

- `Verse.VacuumUtility.PawnVacuumTickInterval`: every 60 ticks, if the biome is `inVacuum` and the
  cell's vacuum is at least 0.5, `VacuumExposure` severity rises by
  `0.02 * vacuum * max(1 - VacuumResistance, 0)`. Resistance 1.0 is full immunity; the stat is
  clamped to 0..1. `VacuumExposure` (Odyssey `HediffDefs/Hediffs_Global_Misc.xml`) is lethal at
  severity 1 and recovers at 0.1/s once unexposed. `IsProtectiveApparel` is any def with a
  `VacuumResistance` offset of at least 0.1.
- Space detection in code: `map.Biome.inVacuum` (only the Odyssey `Space` biome sets it) or
  `map.Tile.LayerDef.isSpace` (the `Orbit` planet layer). The astrorig uses the layer route.
- **VGE1 `1.6/Patches/ArmorVacuumResistancePatch.xml`** rewrites helmet and armor offsets so that
  no suit reaches 1.0 without an oxygen pack: Odyssey helmet 0.69 to 0.65 (pair total 0.97), recon
  0.5, power 0.55, cataphract 0.62, and for the five VFEP spacer warcaskets every helmet to
  **0.65** and every armor to **0.22** (with the 0.1 shoulders, 0.97). Gated by
  `PatchOperationFindMod` on display names. It does not touch our defs, so with VGE1 active the
  Shipcracker set (0.2 + 0.1 + 0.7) would be the only warcasket at a full 1.0 unless we mirror the
  nerf. Odyssey alone: VFEP spacer sets and ours both total 1.0.

## Mapping onto the TODOs.md scoping items

- **Astrorig-style built-in on the torso.** The oxygen comp and the stat part that grants full
  vacuum immunity are VGE1 classes; a hard reference is off the table for an optional mod. XML-only
  options that work without VGE: the VEF terrain-tag speed factor on `Space` and a
  `VacuumResistance` offset. Anything oxygen-flavored needs our own comp, or a VGE-gated compat
  root under `1.6/Mods/`.
- **Demolition landing.** Power Jump detonates Bomb damage at takeoff and landing. Core's
  `Demolish` DamageDef has a tenfold `buildingDamageFactor`, and `DamageDef` exposes passable and
  impassable building factors separately, so a custom DamageDef hits structures hard without
  scaling pawn damage.
- **Unlimited line-of-sight jump in space.** VEF `Ability` exposes `GetRangeForPawn` and
  `CanHitTarget(target, sightCheck)` as virtuals; the game marks space via `map.Biome.inVacuum` or
  `map.Tile.LayerDef.isSpace`. One ability class can switch rules by map. VFEP's jump skips line
  of sight entirely, so the space mode would add it back. The astrorig's `layerWhitelist: Orbit`
  is the vanilla verb-side equivalent.
- **Astrofuel instead of chemfuel.** `CompProperties_ApparelReloadable` takes a single `ammoDef`;
  a one-node patch from a VGE1-gated compat root swaps it. Astrofuel is worth about 2.6 times
  chemfuel per unit and stacks to 75.
- **Vacuum resistance under VGE1.** VGE1 caps every VFEP spacer warcasket at 0.97 total so an
  oxygen source is required. Our defs are not in that patch and would sit at 1.0. **Decided
  2026-09-16: mirror the nerf.** `1.6/Mods/VanillaGravshipExpanded/Patches/VacuumResistance.xml`,
  loaded via an `IfModActive="vanillaexpanded.gravship"` root, sets the armor to 0.22 and the
  helmet to 0.65, the same numbers VGE1 gives the five VFEP spacer sets.
- **The dead recharge offset on our shoulders** (discrepancy 1) is dropped: the shoulders carry no
  `EnergyShieldRechargeRate`.

## Design proposal (drafted 2026-09-16; approved with changes and landed the same day)

Approved 2026-09-16 with these changes, all landed (the def headers under `1.6/Defs/` are the
record of what shipped; this section is the original proposal): shoulders keep breach power
but give no jump range, and their melee stat is MeleeHitChance x1.15 plus MeleeDodgeChance
x1.15 instead of a damage factor; the helmet gives no jump range, floors Breathing at 1.0
(built-in respirator), and takes AimingDelayFactor x0.9 instead of a melee stat; under VGE1 the
helmet carries VGE1's oxygen-provider comp (50 charges) so the set restores full vacuum
immunity on its own, and the astrofuel tank burns 10 per jump so the refined fuel is never a
penalty; every piece costs gravlite panels under Odyssey (20 / 10 / 10); the gizmo icons are
VFEP's Power Jump (planet) and Blast Off (space) until the artist's textures arrive; CI fetches
VEF and VFEP with SteamCMD. Original proposal follows. It follows the backstory's four beats and
uses synergy model 1 (custom StatDefs read off the pawn), so every piece contributes through plain
`equippedStatOffsets` / `equippedStatFactors` XML and only the armor's ability is C#.

**Concept: the boarding-assault warcasket.** The armor is the drop engine, the shoulders are the
breaching arms, the helmet is close-quarters targeting. Role gap in VFEP's roster: Aerial is
mobility with a firecracker landing, Shock is a one-shot orbital drop, nothing breaches walls.

### Armor: Breach Jump (the only ability)

- **Comps:** vanilla `CompApparelReloadable` (100 charges, Chemfuel 1 per charge, 60-tick reload,
  `chargeNoun` jump; Aerial's numbers) plus VEF `CompAbilitiesApparel` granting
  `SCWC_BreachJump`. The ability reads the tank off its `holder`, so both comps must sit on the
  same piece; that is why the engine is the armor and not, as on Aerial, the shoulders. 20 fuel
  per jump, 5 per tank.
- **On a planet:** `targetMode` Location, walkable cell within `SCWC_BreachJumpRange` (pawn stat,
  base 0; armor +20, shoulders +5, helmet +10, full set 35 versus Aerial's 40), no line-of-sight
  test (Aerial parity), 60-tick cast.
- **In space** (`pawn.Map.Biome.inVacuum` or `pawn.Map.Tile.LayerDef.isSpace`): no range limit,
  but line of sight is required (`GenSight.LineOfSight` plus lean sources, the check VEF's base
  `CanHitTarget(target, true)` already does). Flight speed reuses VFEP's `VFEP_FlightSpeed`
  (base 12 cells/s) with the space flight capped at about 4 s so a 150-cell hop is not a 12-second
  hang. Both space checks are vanilla fields, so no Odyssey gate is needed; without Odyssey the
  branch simply never fires. VEF's `DrawHighlight` skips the range ring once range exceeds
  `GenRadial.MaxRadialPatternRadius`, so unlimited range needs no drawing work.
- **Landing = breach.** No takeoff blast (the drop is the weapon). On landing,
  `GenExplosion.DoExplosion` with a new `SCWC_Breach` DamageDef, a `Bomb` clone with
  `buildingDamageFactor` 20 and both passable and impassable factors 1: base damage 30, radius 3,
  wearer excluded. Buildings take 600: breaches steel walls (300), granite-block walls (about 510)
  and Odyssey's `GravshipHull` (420); plasteel (840) and uranium (750) walls survive the armor
  alone. Pawn damage 30 is twice Aerial's 15 in the same radius, against end-game armor. Damage
  is multiplied by `SCWC_BreachPower` (pawn stat, base 1) so other pieces can add to it.
- **No cooldown** beyond cast time; fuel is the limiter, as on Aerial.
- **Space walking** (Astrorig-style built-in): VEF `moveSpeedFactorByTerrainTag` on the `Space`
  tag. Stock Odyssey marks `Space` terrain `Impassable`, so this only matters under VGE1, whose
  `SpaceTraversal.xml` makes it walkable at pathCost 490 (the Astrorig's 27.5 divides that back
  to normal walking; VEF's `CostToMoveIntoCell` transpiler divides the whole cell cost). Proposed:
  factor 10 (roughly half walking speed in space, a clear step below the Astrorig) shipped as a
  `PatchOperationAdd` from the existing VGE1 compat root, so the main tree carries no dead XML.
- **Fuel under VGE1:** a one-node `ammoDef` swap to `VGE_Astrofuel` from the same VGE1 root
  (astrofuel is VGE1's item, not VGE2's as `TODOs.md` says). Undecided: keep 1 per charge (5 jumps
  per 100 astrofuel, about 2.6x the chemfuel cost) or halve the tank to match value.

### Shoulders: the breaching arms

- Drop the dead `EnergyShieldRechargeRate +1` (discrepancy 1) rather than implement a pawn-level
  recharge stat.
- `SCWC_BreachPower +0.5`: landing damage 45, buildings 900, so with the shoulders on the set
  breaches plasteel and uranium walls too. This is the "cut both steel and flesh" beat, and the
  Mining 4 skill gain's nod to breaching.
- `SCWC_BreachJumpRange +5`.
- `ApparelExtension.equippedStatFactors` `MeleeDamageFactor` x1.2 (vanilla pawn stat, base 1;
  Shock's shoulders use x1.2 on hit and dodge, so the magnitude has precedent).

### Helmet: room-to-room

- `SCWC_BreachJumpRange +10` (Aerial's helmet gives +10).
- `equippedStatFactors` `MeleeHitChance` x1.15. Considered and rejected: letting the helmet waive
  the space line-of-sight rule, because the sight rule is the only thing balancing an unlimited
  range jump.

### What it costs to build

- **C#, VEF types only, no VFEP types, no Harmony patch:** `Ability_BreachJump : VEF.Abilities.Ability`
  (overrides `GetRangeForPawn`, `CanHitTarget`, `GetGizmo` for the fuel check, `Cast`),
  `PawnFlyer_BreachJump : VEF.Abilities.AbilityPawnFlyer` (flight time from `VFEP_FlightSpeed`
  as VFEP's flyer does, effecter reuse, breach explosion in `RespawnPawn`), a `DefModExtension`
  for fuel per jump / base damage / radius / space flight cap, and a `[DefOf]` class. VFEP's
  effecter and sounds are reused by def name, not by type.
- **Defs, one per file:** `AbilityDefs/BreachJump.xml`, `StatDefs/BreachJumpRange.xml`,
  `StatDefs/BreachPower.xml`, `DamageDefs/Breach.xml`, `ThingDefs_Misc/BreachJumpFlyer.xml`
  (`PawnFlyerBase` child), plus the two VGE1-root patches. New labels and descriptions mean the
  l10n sidecar must be regenerated before the release checker passes (it refuses stale
  expectations).
- **CI blocker:** first C# use of a VEF type, so CI needs `VEF.dll` (see the `TODOs.md`
  infrastructure item): commit the Workshop DLL under `Source/refs/` with its version noted, or
  fetch it with SteamCMD in the workflow. Recommendation: commit it, so a release build cannot
  change under us when the Workshop copy updates.
- **Unchanged:** shield bubble, stats, costs, tags, research prerequisite.
