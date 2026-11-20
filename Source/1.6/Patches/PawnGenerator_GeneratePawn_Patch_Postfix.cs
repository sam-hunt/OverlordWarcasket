using System;
using System.Reflection;
using HarmonyLib;
using Verse;
using VFEPirates;

namespace OverlordWarcasket.Patches;

// Keeps the set off generated pawns: raiders, hired veterans, anyone the game makes rather than
// the foundry. The parts carry no apparel tags, so no pawnkind's gear roll picks them (the armor
// def's header has why), but VFEP has a second source. Its postfix on PawnGenerator.GeneratePawn
// (PawnGenerator_GeneratePawn_Patch.Postfix, VFEP v4.03, the same on its 1.6 and 1.7 assemblies)
// completes any generated pawn that wears a warcasket piece, filling each empty slot with a
// RandomElement of VFEPiratesMod.allArmorDefs, allShoulderPadsDefs or allHelmetDefs. Those lists
// hold every WarcasketDef, tags and price unread, and the foundry's dialog builds its choices
// from them, so the parts cannot simply leave them. The prefix here takes this mod's parts out
// of the three lists for the length of that one call, and the finalizer puts them back where
// they were whether or not the call threw.
//
// Back by Def.index, not by the position each part left: the sibling warcasket mods mask the
// same three lists around the same call, and their masks overlap this one without nesting (the
// ListMask header has the order Harmony runs them in). VFEP fills the lists from
// DefDatabase<ThingDef>.AllDefs (StaticStartup.FillWarcasketDefLists), and DefDatabase numbers
// its defs in that same order (SetIndices), so the index is each part's place in the lists.
//
// Out of the draw rather than swapped out afterwards: taking a worn part off again runs VEF's
// unequip path (Apparel_Patches.UnequipGear), which adds the armor's traitsOnUnequip,
// VFEP_Shellcasket, to the pawn.
//
// The parts are told by the OLWC_ def prefix, not by instance: VFEP fills the lists from a
// static constructor, once per process, so after an in-process play-data reload they hold the
// previous load's defs, each still carrying the index of the load that made it.
//
// The target is VFEP's own patch method, an internal no test of ours can pin (the suite loads
// no VFEP assembly), so it is bound by name: should VFEP move it, this patch stands down with a
// warning where a typed target would fail PatchAll and take every other patch with it.
[HarmonyPatch]
public static class PawnGenerator_GeneratePawn_Patch_Postfix
{
    private const string Target = "VFEPirates.PawnGenerator_GeneratePawn_Patch:Postfix";

    private static readonly ListMask<WarcasketDef> Mask = new(def => def.index);

    private static readonly Predicate<WarcasketDef> IsOurs =
        def => def.defName.StartsWith("OLWC_", StringComparison.Ordinal);

    public static bool Prepare()
    {
        if (AccessTools.Method(Target) != null)
            return true;
        Log.Warning($"[Overlord Warcasket] {Target} not found, so VFE Pirates may complete a generated pawn's warcasket with Overlord parts.");
        return false;
    }

    public static MethodBase TargetMethod() => AccessTools.Method(Target);

    public static void Prefix()
    {
        Mask.Hide(VFEPiratesMod.allArmorDefs, IsOurs);
        Mask.Hide(VFEPiratesMod.allShoulderPadsDefs, IsOurs);
        Mask.Hide(VFEPiratesMod.allHelmetDefs, IsOurs);
    }

    public static void Finalizer() => Mask.Restore();
}
