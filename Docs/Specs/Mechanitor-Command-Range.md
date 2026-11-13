# SPEC: Mechanitor command range on the Overlord helmet

> Implemented. The code, its def headers and its tests are the tested current state and supersede this spec wherever the two differ.

Status: IMPLEMENTED on the shared `1.6/` tree (`Source/1.6/CommandRange/`,
`Source/1.6/Patches/MechanitorUtility_InMechanitorCommandRange.cs`,
`Source/1.6/Patches/Pawn_MechanitorTracker_DrawCommandRadius.cs`,
`Source/1.6/Settings/CommandRangeSetting.cs`, the `CommandRangeExtension` entry on
`1.6/Defs/ThingDefs_Misc/WarcasketHelmet_Overlord.xml`), not yet run in game (see `TODOs.md`).
Vanilla facts verified against RimWorld 1.7.5000's `Assembly-CSharp.dll` and the frozen 1.6
assembly; the seven call sites, the constant and the ring cap are identical on both. Nothing in
VFEP or VEF touches any of it.

## 1. Goal

The brief: Biotech mechanitors can only order drafted mechs within a moderate radius, which
forces micromanagement on large maps. Remove the limit, tied to a set piece (the helmet by
default) or to all mechanitors, or off, with a choice of reach: the whole current map, anywhere
(dropping the same-map rule), or a radius slider annotated with vanilla's 24.9.

## 2. What vanilla does (verified)

- Every drafted order for a colony mech asks the public static
  `MechanitorUtility.InMechanitorCommandRange(Pawn mech, LocalTargetInfo target)`. Seven call
  sites: `FloatMenuOptionProvider_DraftedAttack` (two), `FloatMenuOptionProvider_DraftedMove.PawnCanGoto`,
  `FloatMenuUtility.GetRangedAttackAction` and `GetMeleeAttackAction`,
  `MultiPawnGotoController.RecomputeDestinations` (two, the drag-to-goto).
- It returns false when the overseer is null or on another map (`MapHeld` differs), else defers
  to `Pawn_MechanitorTracker.CanCommandTo(target)`: the target cell in bounds of the overseer's
  map and `pawn.Position.DistanceToSquared(target.Cell) < 620.01f`, the square of 24.9.
- The 24.9 is a private const `MechCommandRange` on `Pawn_MechanitorTracker`. No stat and no
  def field sit behind it.
- `DrawCommandRadius`, called from `Pawn.DrawAt` each frame, draws
  `GenDraw.DrawRadiusRing(pawn.Position, 24.9f, Color.white, CanCommandTo)` while the pawn is
  spawned and `AnySelectedDraftedMechs`.
- Not range-gated: drafting (`CanDraftMech` only checks `CanControlMechs`: not downed, not a
  prisoner, spawned, not in a mental state), work-mode mechs, mech AI. An accepted order is
  never recalled when the overseer walks away.
- The refusal players see is Biotech's Keyed `CannotGoOutOfRange` plus `OutOfCommandRange`:
  "Cannot go here: Out of command range".
- The ring cap: `GenDraw.DrawRadiusRing` logs an error and draws nothing above
  `GenRadial.MaxRadialPatternRadius`. `RadialPatternCount` is 20000 on both versions; the
  20,000th cell lies at squared distance 6370, radius 79.81. So 79.8 is drawable and 79.9 is
  not.

## 3. Design

### 3.1 The extension

`CommandRangeExtension`, a `DefModExtension` with a `scope` (`CurrentMap`, `Anywhere`, `Radius`)
and a `radius` (default 24.9), sits on the helmet def in XML. The def stays the record and the
defaults come from XML, as the project rule asks: settings override def fields, never code
paths. `ConfigErrors` rejects a non-positive radius.

### 3.2 Who it applies to

The settings' "applies to" is Helmet, Shoulders, AllMechanitors or None. `CommandRange.Apply`
runs at startup and on window close, moves the extension between the helmet and shoulders defs
for the first two, and writes scope and radius back onto it. AllMechanitors and None are the two
states a def cannot express, so they live as static state in `CommandRange`. The default is read
as whichever piece carries the extension in XML (`TryReadDefs`, `LoadDefaults`), so a patch mod
that moves it moves the default too.

The governing extension for an overseer is `CommandRange.For`: the set's instance under
AllMechanitors, else the first `CommandRangeExtension` on a worn apparel def. Another mod can
therefore grant the range from its own apparel through XML, at its own scope.

### 3.3 The order check

A postfix on `MechanitorUtility.InMechanitorCommandRange` calls `TryEvaluate`. When a governing
extension is found it replaces vanilla's answer whole, so a radius below 24.9 is a working
nerf; otherwise vanilla's answer stands untouched, including for a mech with no overseer. The
rule, `Allows`:

- The target must be in bounds of the mech's map, not the overseer's, since under Anywhere the
  two differ. A mech held off any map is refused.
- Anywhere: allowed.
- Otherwise the overseer must be on the mech's map.
- CurrentMap: allowed. Radius: `Within`, the squared distance strictly below the radius squared,
  vanilla's own comparison, so a .9 radius excludes the next whole distance.

Anywhere also covers a mechanitor traveling in a player caravan: a postfix on
`Pawn_MechanitorTracker.CanControlMechs` lifts vanilla's refusal of an unspawned mechanitor for
that case alone (the patch's header has the readers it opens). A transport pod in flight and a
casket keep vanilla's refusal.

### 3.4 The ring

A prefix on `Pawn_MechanitorTracker.DrawCommandRadius`. Where no extension governs, vanilla runs.
Where one does, the ring is dropped under the map-wide scopes, since it would lie, and drawn at
the extension's radius under Radius, with the same `DrawRadiusRing` call and a predicate that is
the order rule. A radius above `CommandRange.RingCap` draws nothing instead of the logged error.

### 3.5 Settings

Modeled on Unique Weapons Unbound's skill-check prerequisite idiom. The section "Mechanitor
command range" carries the bearer piece's icon and a summary line. Two radio groups follow:

- "Applies to": four radios, labels built from the pieces' own def labels (Helmet, Shoulders,
  All mechanitors, No one).
- "Command range": "Anywhere on the same map" (default), "Anywhere", and "Within {0} cells",
  whose label doubles as the slider value and adds " (vanilla)" at 24.9.

A slider under the last radio runs over 16 rungs: 4.9, 9.9, ..., 74.9, then the cap floored to
a tenth (79.8). `Ladder` computes them at runtime from the game's constant, not hard-coded.
Pressing or moving the slider selects the Radius scope. While applies-to is None the second
group is grey and non-interactive with one inert tooltip, and its stored choices persist. Only
overrides are saved: enum names as strings, so an unknown name reads as the default, and the
radius as a float with -1 unset. The player-facing wording is the `OLWC_CommandRange*` block in
`Keyed/OverlordWarcasket.xml`.

### 3.6 Tests

`Tests/1.6/CommandRangeTests.cs` covers `Within` parity with 620.01, each rung excluding the next
whole distance, the ladder at the real cap and at two hypothetical caps, `FloorToTenth`, and the
XML wiring: the helmet carries exactly one extension, the shoulders none, the scope parses and
the radius equals 24.9.

## 4. Residuals

- The in-game run is pending (`TODOs.md`).
- The per-order cost is a walk over the overseer's worn apparel. It is negligible, even in the
  drag-goto's per-cell loop.
- The ring predicate allocates a closure per frame, as vanilla's does.
- Balance: unlimited range removes a deliberate Biotech limiter. That is why it is a set perk by
  default and a global toggle by choice. The tuning pass may price the helmet for it.

## 5. Verification (pending)

Dev mode, with a mechanitor and drafted mechs:

- Helmet on, order a drafted mech across the map (move, attack, drag-goto of several) and expect
  every order accepted and no white ring when a mech is selected. Take the helmet off and expect
  vanilla's refusal and ring.
- Radius scope at 29.9: orders accepted to 29.9 and refused beyond, the ring drawn at that
  radius. At 9.9 (the nerf): orders refused past 9.9 where vanilla would allow them, ring at 9.9.
- Anywhere with a mech on a second map: an order on that map is accepted. CurrentMap refuses it.
- Applies-to Shoulders: the grant follows the shoulders and leaves the helmet. AllMechanitors:
  a mechanitor in no set piece gets the range.
- None: the vanilla refusal text and 24.9 ring return, and the second group is greyed.
- Close and reopen the settings window and expect every choice kept.
- Switch the language (an in-process play-data reload) and expect the behaviour kept.
