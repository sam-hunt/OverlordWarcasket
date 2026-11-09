using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace OverlordWarcasket.Patches;

// The white command ring a mechanitor shows while one of its drafted mechs is selected, drawn
// by vanilla at its constant 24.9 from Pawn.DrawAt once a frame. Where the set's range governs
// this mechanitor (CommandRange.For), the ring would lie: under a map-wide scope it is dropped,
// and under the Radius scope it is drawn at the extension's radius with the same vanilla call,
// its predicate the same rule the order check applies. A radius the game cannot draw
// (CommandRange.RingCap) draws nothing rather than the error DrawRadiusRing would log. Vanilla's
// own method runs untouched for everyone else.
[HarmonyPatch(typeof(Pawn_MechanitorTracker), nameof(Pawn_MechanitorTracker.DrawCommandRadius))]
public static class Pawn_MechanitorTracker_DrawCommandRadius
{
    public static bool Prefix(Pawn_MechanitorTracker __instance)
    {
        if (CommandRange.AppliesTo == CommandRangeAppliesTo.None)
            return true;
        Pawn pawn = __instance.Pawn;
        CommandRangeExtension extension = CommandRange.For(pawn);
        if (extension == null)
            return true;
        if (extension.scope != CommandRangeScope.Radius || !pawn.Spawned || !__instance.AnySelectedDraftedMechs)
            return false;
        float radius = extension.radius;
        if (radius > CommandRange.RingCap)
            return false;
        IntVec3 center = pawn.Position;
        Map map = pawn.MapHeld;
        GenDraw.DrawRadiusRing(center, radius, Color.white, c => c.InBounds(map) && CommandRange.Within(center, c, radius));
        return false;
    }
}
