# Source/1.6/: C# guidance

Loaded when work touches the code. Each file's header carries its decompile-verified call paths
and the rationale for its numbers; this file holds only the rules that cross files. The csproj's
comments cover install-path detection, the compile-only VFEP/VEF references (`VFEP_PATH` and
`VEF_PATH` override the Workshop copy) and the deploy manifest.

- **Patch classes live in `Patches/`** under the `.Patches` namespace suffix, to avoid RimWorld
  type-name conflicts. Log with the `[Overlord Warcasket]` prefix.
- **Patch timing is the load-bearing hazard.** `PatchAll()` runs from the
  `[StaticConstructorOnStartup]` in `ModInit.cs`, never from the `Mod` subclass constructor: a
  detour on a VFEP method applied before defs load runs that type's static ctor early and breaks
  it for good (the BetterTradersGuild v1.1.0 CWTL incident). `ModInit`'s header has the
  rationale; `Settings/OverlordWarcasketMod.cs` owns settings only.
- **A prefix that answers in vanilla's place takes `Priority.First` where another mod prefixes
  the same method.** Harmony skips every later bool prefix once one returns false, and a mod
  that patches from its `Mod` constructor (Combat Extended does) sits ahead of ours at equal
  priority, so its prefix would answer for our case too. Ours runs first and returns true
  wherever the set does not govern, which hands the call on to theirs. The ring and the
  automatic reload are the two such prefixes; their headers name the CE patch each one meets.
- **Startup def-derived state must survive an in-process play-data reload** (a language switch
  reloads every def). It goes in the idempotent `Startup.Run`, called once from `ModInit` and
  again from the `StaticConstructorOnStartupUtility.CallAll` postfix in `Patches/`. Mod-shipped
  textures are destroyed on the same reload: resolve them through a getter that re-resolves on a
  Unity-null `== null` check, never `??` or `??=`
  (`~/dev/PersonaWeaponsUnbound/Source/1.6/Defs/PWU_Textures.cs` is the pattern;
  `CerebrexNode.Icon` is the one here). Vanilla-path textures are safe as plain statics; the
  settings' work icon is one.
- **A switch an XML patch reads is a restart-time setting.** `PatchOperationSetting`
  (`PatchOperations/`, root namespace, as the XML names it) reads a public `BoolSetting` field by
  name while the patches apply, after the `Mod` constructor loaded the saved settings, so the
  switch's row carries the restart suffix (`BoolSetting.RequiresRestart`) and the settings code
  writes nothing for it. The cerebrex node's art is the one such switch
  (`1.6/Mods/Odyssey/Patches/CerebrexNodeArt.xml`): a def's graphic, icon and atlas entry are
  built once from its paths at load, so a runtime write would have to rebuild all three through
  private members. `Settings/CerebrexNode.cs` only reads the def for the window.
- **Settings override def fields, never code paths.** `OverlordWarcasketSettings.ApplyToDefs`
  writes each tunable onto its def at startup and on window close (its header lists the fields),
  so vanilla, VEF and our code read the setting through the def. Live state a write leaves behind
  (a running cooldown, charges above a lowered capacity) is reconciled by the instance on its next
  use, keyed on `OverlordWarcasketSettings.Generation`, or by the write itself.
- **The command range is two postfixes and one prefix** (`Patches/MechanitorUtility_InMechanitorCommandRange`,
  `Patches/Pawn_MechanitorTracker_CanControlMechs`, `Patches/Pawn_MechanitorTracker_DrawCommandRadius`)
  over the rules in `CommandRange/`, which
  reads the `CommandRangeExtension` off the def: the settings move that extension between the
  helmet and the shoulders and write its scope and radius (`CommandRange.Apply`), so the def
  rule above holds; the two choices no def can express, every mechanitor and off, are the one
  piece of static settings state (`CommandRange.AppliesTo`). `CommandRange`'s header carries the
  vanilla call paths; the ring cap it quotes is read from `GenRadial` at runtime, never hard-coded.
- **The mech resurrector is a `CompApparelReloadable` subclass on the armor**, with a recharge
  of its own. Vanilla's reload queries are not virtual, so two patch classes bend them for this
  comp only: `CompApparelReloadable_Reload` and `JobGiver_Reload_TryGiveJob` (their headers have
  the call paths). The ability reads the resurrector off `holder` and `CompResurrectorSparks` off
  its own parent, so all three comps share the armor def. `CompMechResurrector`'s header has the
  arithmetic, the three reloads and the trim rules.
- **The ability is a thin VEF `Ability`** whose rules mirror Biotech's
  `CompAbilityEffect_ResurrectMech` (the file header quotes the build they were verified on), but
  single-target, and with the extension's cost table alone deciding which weight classes come
  back: every rule lives in `CanResurrect`, which the cast re-runs on the corpse. Its cast is
  an ordered job of ours (`OLWC_CastResurrectMech`, `Jobs/JobDriver_CastResurrectMech`), never
  VEF's started one, so that casts queue: VEF keeps the cast in one slot per pawn, which the
  driver fills only as its job starts. Medic Warcasket's `JobDriver_MedicCast` is the precedent;
  the driver's header has the order-time trap.
- **The overlord trait is mirrored from VFEP's at startup** by `WorkProfile.Mirror`, first in
  `Startup.Run`, so editing or patching its work tags, work types, degree effects or VEF
  extension on our def does not stick, and VFEP's 1.6/1.7 differences need no version check.
  Only the degree's text is ours. The openable work types are the `Openable` table in that file
  and nowhere else; the trait def's header carries the contract.
- **Keyed strings a DLC owns are available only when the DLC is a hard dependency.** Biotech's
  are (`MechResurrectionCharges`); Odyssey's and Ideology's are not. Before using a vanilla key
  from code, grep `Data/*/Languages/English/Keyed/`; if neither Core nor Biotech owns it, add an
  `OLWC_` key to the one Keyed file instead.
- **`MechanitorUtility` lives in the global namespace** (no `using` needed, none possible);
  `MechWeightClassDef` in `Verse`, `MechChargeCosts` in `RimWorld`. All three exist on 1.6.
- **Verification notes name their game version.** The local install's assembly is now the 1.7
  one; a header call path verified on 1.6 says so if re-verified on 1.7 and differs.
