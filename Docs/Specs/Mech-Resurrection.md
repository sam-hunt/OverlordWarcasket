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

### 3.2 The resurrector

`CompMechResurrector` on the armor, a `CompApparelReloadable` subclass as the family's gauges
are (props `CompProperties_MechResurrector : CompProperties_ApparelReloadable`); the charges
are the base's `remainingCharges`. `maxCharges` 10, starts full. Two supplies, so the recharge
never competes with the reload:

- Recharge: `chargesPerDay` (1), accrued every 150 ticks into a saved fraction of the next
  charge and banked whole; a full resurrector holds no fraction, so a lowered capacity never strands
  one. This is the main supply.
- Reload: `ammoCountPerCharge` advanced components (`ComponentSpacer`) per charge, the costly
  shortcut (VFE Pirates' Controller warcasket reloads its drones with one advanced component).

A reload happens two ways, both vanilla's and both from the map only; nothing of ours looks in
the pilot's inventory. Automatic: an undrafted colonist pilot fetches components on their own
(vanilla's `JobGiver_Reload`) only while the resurrector is below the gauge's drag handle, which
starts at zero, so a pilot never spends the colony's components unasked, and loads up to the
handle and no further. Ordered: right-clicking components (vanilla's float menu, drafted too)
fills the resurrector, ignoring the handle; vanilla's finder gathers stacks outward from the
clicked one until the whole need is met, so it is not limited to the clicked stack. Two Harmony
patches bend vanilla's queries for this comp only: `CompApparelReloadable_Reload` (`NeedsReload`
and `MaxAmmoNeeded`, branching on `allowForcedReload`: false answers for the handle's target, true
for the whole resurrector) and `JobGiver_Reload_TryGiveJob` (a `Priority.First` prefix that, when
the reloadable vanilla would serve is the resurrector, repeats vanilla's own steps and caps the
automatic job's count at the target). The capacity, the recharge and the reload cost are mod
settings.

### 3.3 The gauge

`Gizmo_MechResurrector : Gizmo_Slider` (Toxin's `Gizmo_ToxTank` shape): Biotech's
`MechResurrectionCharges` as the title, "3 / 10" on the bar, a tick per charge, draggable for
the player's pilots to set the automatic reload level, which starts at zero.
Red: fill (0.62, 0.12, 0.12), hover (0.85, 0.30, 0.30), handle (0.40, 0.07, 0.07). The
tooltip: the count, the recharge line (rate and wait, or full, or never), vanilla's reload per
charge line, the automatic reload level line (or that it is off), the cost per weight class and
the corpse window.

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
- Enabled only for a mechanitor (`requireMechlink`, a setting) with a charge in the resurrector.
  Autocast pinned off (VEF never fires an apparel ability's).

### 3.5 Settings

Charge capacity, charges recharged per day, reload cost per charge, corpse window (game hours),
casting time, cooldown, require a mechlink; written onto the comp props, the extension and the
def by `OverlordWarcasketSettings.ApplyToDefs`.

## 4. Residuals

- Which mechs: the pilot's faction, as the apocriton; not only the mechs the pilot oversees.
- An UltraHeavy mech costs 8 charges, where the apocriton refuses the class: the cost table
  alone decides which classes come back (the ability def's header has why).
- One mech per cast, not the apocriton's 5 cell area: the resurrector's few charges are spent one
  chosen mech at a time.
- A resurrected mech comes back as `ResurrectionUtility.TryResurrect` leaves it: missing
  parts restored, wounds partly healed, no weapon regenerated beyond `MechRepairUtility`.
- The pilot's own death does not spend or drop the resurrector's charges; they stay with the armor.

## 5. Verification (pending)

Dev mode: spawn a mechanitor in the set, kill a light, a medium and a heavy mech beside each
other, cast on one; expect all three back, 9 charges spent, the warmup sparks on the clicked
corpse, the glow on each mech, the resolve burst at their centre. Wait past a minute and
expect "has been dead too long". Remove the mechlink and expect the gizmo disabled with the
mechlink reason. Lower the capacity in settings to 5 and expect the gauge trimmed at once.

Reload: with the handle at zero and the resurrector below full, expect no pilot to fetch components;
raise the handle and expect an undrafted pilot to fetch advanced components up to that level and
no further. Right-click components drafted and undrafted and expect a reload past the handle,
taking stacks beyond the clicked one when it falls short. Expect the charge-per-day and reload
cost sliders to reach a worn resurrector, the tooltip to show the wait for the next charge, and
the armor's stat sheet to show the reload cost.
