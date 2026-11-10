# SPEC: Resurrect mech on the Overlord armor

> Implemented. The code, its def headers and its tests are the tested current state and supersede this spec wherever the two differ.

Status: IMPLEMENTED on the shared `1.6/` tree (`Source/1.6/Abilities/`, `Source/1.6/Comps/`,
`Source/1.6/Gizmos/`, `1.6/Defs/AbilityDefs/ResurrectMech.xml`), not yet run in game (see
`TODOs.md`). Vanilla facts verified against RimWorld 1.7.4990 rev1069's `Assembly-CSharp.dll`
and Biotech's `Defs/AbilityDefs/Abilities.xml`; the 1.6 members used (`ResurrectionUtility.TryResurrect`,
`EffecterDef.SpawnAttached`, `Pawn_StanceTracker.stagger`, `Corpse.timeOfDeath`,
`MechanitorUtility.IsMechanitor`) checked against the frozen 1.6 assembly. VEF facts from the
checkout at upstream `main` (`Source/VEF/Abilities/`).

## 1. Goal

The brief: the torso's ability reimplements Biotech's apocriton "resurrect mech", with a gauge
in the Toxin and Medic idiom, red, capped at 10 charges instead of 30, recharging slowly at 1%
per day.

## 2. What the apocriton does (verified)

`ResurrectionMech` is a vanilla `AbilityDef` with `CompProperties_ResurrectMech` and
`CompAbilityEffect_ResurrectMech`:

- `resurrectCharges` starts at 30 on the comp instance, saved per pawn, never regenerating.
  `Gizmo_MechResurrectionCharges` draws the count under Biotech's Keyed
  `MechResurrectionCharges`; dev mode adds "DEV: Add charge".
- `CanResurrect(corpse)`: the inner pawn is a mechanoid whose `mechWeightClass` is non-null
  and `canResurrect`; its faction equals the caster's; its kind does not itself carry
  `ResurrectionMech` (no apocriton chains); `corpse.timeOfDeath` is within
  `maxCorpseAgeTicks` (3600, a minute); its weight class has a cost (`costs`: Light 1,
  Medium 3, Heavy 5; UltraHeavy absent) within the charges held.
- `Apply` per target: pay the cost, `ResurrectionUtility.TryResurrect(pawn)`, play
  `appliedEffecterDef` (`MechResurrected`) attached to the mech, stagger it for 60 ticks.
  `PostApplied` plays `ApocrionAoeResolve` at the average of the targets' cells.
- Targeting: corpses, `Ability_EffectRadius` 5 so every corpse in the radius is a target,
  range 9.9, warmup 4 s with `ApocrionAoeWarmup`, `MechResurrect_Warmup` and
  `MechResurrect_Cast` sounds, cooldown 120 ticks, `CastAbilityGoToThing` for the AI's walk.

## 3. Design

### 3.1 Framework

A VEF ability (`VEF.Abilities.AbilityDef`, `Ability_ResurrectMech : VEF.Abilities.Ability`),
granted by the armor's `CompProperties_AbilitiesApparel`, as every ability in the family is:
vanilla has no apparel-granted abilities, and `CompAbilityEffect` attaches only to vanilla
ones. The tuning the vanilla comp props held moves to `ResurrectMechExtension`, a
`DefModExtension` on the def, reusing vanilla's `MechChargeCosts` entry type so the XML reads
as the apocriton's.

### 3.2 The core

`CompResurrectionCore` on the armor, a plain `ThingComp` (nothing reloads it; the family's
`CompApparelReloadable` gauges carry a reload target this one has no use for). `maxCharges`
10, starts full. Regen: `regenPerDay` is a fraction of the capacity per day (0.01), accrued
every 150 ticks into a saved fraction and banked as whole charges; a full core holds no
fraction, so a lowered capacity never strands one. "1% per day" is read literally as 1% of the
capacity: a charge every ten days, a reserve rationed between raids. Both numbers are mod
settings.

### 3.3 The gauge

`Gizmo_ResurrectionCore : Gizmo_Slider` (Toxin's `Gizmo_ToxTank` shape): Biotech's
`MechResurrectionCharges` as the title, "3 / 10" on the bar, a tick per charge, not draggable.
Red: fill (0.62, 0.12, 0.12), hover (0.85, 0.30, 0.30), handle (0.40, 0.07, 0.07). The
tooltip: the count, the recharge line (rate and wait, or full, or never), the cost per weight
class and the corpse window.

### 3.4 The cast

- Click a mech corpse (`targetMode` Thing, corpse-only `targetingParametersList` with
  `canTargetMechs`, which vanilla's corpse rule reads for a mechanoid corpse), validated with
  messages. Single-target: no `hasAoE`, so only the clicked corpse comes back, not the
  apocriton's 5 cell area; each charge spent is a choice of which mech matters.
- The click is not range-limited (`GetRangeForPawn` returns 10000, the Medic's idiom); the
  goto job (`VFEA_GotoTargetAndUseAbility`, `distanceToTarget` 9.9) walks the pilot to the
  apocriton's cast range with line of sight, then VEF's warmup (`castTime` 240) with the
  warmup effecter through `AbilityExtension_WarmupEffecter`, then `Cast`.
- `Cast`: `base.Cast` (cooldown, cast sound), then the corpse re-checked through
  `CanResurrect` and paid through `TryConsume` before `TryResurrect`, the applied effecter and
  the stagger; the centre effecter at the corpse's cell.
- Enabled only for a mechanitor (`requireMechlink`, a setting) with a charge in the core.
  Autocast pinned off (VEF never fires an apparel ability's).

### 3.5 Settings

Charge capacity, recharge per day (percent), corpse window (game hours), casting time, cooldown,
require a mechlink; written onto the comp props, the extension and the def by
`OverlordWarcasketSettings.ApplyToDefs`.

## 4. Residuals

- Which mechs: the pilot's faction, as the apocriton; not only the mechs the pilot oversees.
- An UltraHeavy mech cannot be brought back, as for the apocriton.
- One mech per cast, not the apocriton's 5 cell area: the core's few charges are spent one
  chosen mech at a time.
- A resurrected mech comes back as `ResurrectionUtility.TryResurrect` leaves it: missing
  parts restored, wounds partly healed, no weapon regenerated beyond `MechRepairUtility`.
- The pilot's own death does not spend or drop the core's charges; they stay with the armor.

## 5. Verification (pending)

Dev mode: spawn a mechanitor in the set, kill a light, a medium and a heavy mech beside each
other, cast on one; expect all three back, 9 charges spent, the warmup sparks on the clicked
corpse, the glow on each mech, the resolve burst at their centre. Wait past a minute and
expect "has been dead too long". Remove the mechlink and expect the gizmo disabled with the
mechlink reason. Lower the capacity in settings to 5 and expect the gauge trimmed at once.
