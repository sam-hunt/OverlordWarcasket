# 1.6/Mods/CombatExtended/: the Combat Extended root

Loaded when work touches this root. Gated on `CETeam.CombatExtended` in `LoadFolders.xml`, never
shipped beyond its `Patches/` (the deploy manifest takes XML only). Each patch file's header
carries its numbers and the CE file each one mirrors; this file maps what CE takes over from
vanilla in the set's domain to where the answer lives, and records what was looked at and left.

Scanned against CE 16.7.4 (Workshop 2890901044, `Assemblies/CombatExtended.dll` and its XML).
CE ships no source: `ilspycmd -p -o <scratch dir> CombatExtended.dll` gives a tree to grep, and
`grep -rhoE 'HarmonyPatch\(typeof\([A-Za-z_.]+\)(, *"[A-Za-z_.]+")?'` over it lists every method
CE patches, the quickest check after a CE update or before patching a new vanilla method.

## What CE takes over, and the answer

- **Armor, bulk, carry, shield, smoke, costs**: `Patches/ApparelStats.xml`, on the numbers CE
  gives VFEP's Controller set and, for the helmet's shooting price and radio, Biotech's mechlord
  helmet. CE reaches only the defNames it patches, so every new stat on a piece needs its CE
  value here; a vanilla-scale `ShootingAccuracyPawn` is the trap, CE counting 0.2 of it a
  Shooting level where vanilla counts whole levels.
- **The command range**: CE's 43.9 cells for every mechanitor, a `CanCommandTo` postfix and a
  `DrawCommandRadius` prefix. `Patches/CommandRange.xml` writes it onto the def as the range
  without the set; the ring's prefix order is `Source/1.6/Patches/Pawn_MechanitorTracker_DrawCommandRadius.cs`;
  the number is also `CommandRange.CombatExtendedRadius`, a rung the slider names in every game.
- **The reload**: CE's prefix on `JobGiver_Reload.TryGiveJob` and its "Reload worn armor" button
  on every worn reloadable. `Source/1.6/Patches/JobGiver_Reload_TryGiveJob.cs` answers first for
  the core and `CompResurrectionCore` drops the button; `Patches/ResurrectionCore.xml` turns the
  automatic reload from inventory on, CE's own habit.
- **A prefix of ours on a method CE also prefixes takes `Priority.First`** (`Source/1.6/CLAUDE.md`
  has the rule): CE patches from its `Mod` constructor, ahead of ours.

## Looked at and left alone

- **CE's inventory weight goes stale after a carried reload that takes part of a stack.** CE
  caches a pawn's carried weight and bulk (`CompInventory`) and refreshes on `ThingOwner` adds,
  removals and takes; a partial `Thing.SplitOff`, which `ReloadFromInventory` and vanilla's
  `ReloadFrom` both do, only lowers a stack's count and notifies no one. The pilot reads a few
  components heavy until their inventory next changes. Not worth a patch or a CE reference;
  do not "fix" it by reflecting into `CompInventory`.
- **A mech raised under CE comes back armed and loaded.** Vanilla's `ResurrectionUtility`
  regenerates a missing mech weapon through `PawnWeaponGenerator.TryGenerateWeaponFor`, which CE
  postfixes with the pawn kind's loadout (a loaded weapon and spare magazines). The apocriton's
  raise does the same, so it is CE's behavior, not the core's.
- **CE's pick up order replaces vanilla's** (its own float menu, gated on CE bulk and weight)
  and still lands the components in `inventory.innerContainer`, where the load button reads them.
  A pilot on a non-default CE loadout sheds items the loadout does not list and CE's own pick up
  did not record; that is the player's loadout to set.
- **Untouched by CE**: the `ResurrectionMech` ability def and its effecters, mech weight classes
  and charge costs, mech corpses, `ComponentSpacer`, the mech gestator's and the mechanitor's
  work givers (CE adds none outside Hauling and Smithing), VEF abilities and VEF's shield bubble.
- **CE widens `GenRadial`'s pattern to 119 cells**, which `CommandRange.RingCap` reads at
  runtime: the settings slider runs that far under CE with no change here.
