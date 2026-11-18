using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace OverlordWarcasket.Patches;

// Tells the armor's mech stun pulse that its pilot was downed. Vanilla (verified on 1.7.5013 and
// the frozen 1.6 assembly, the methods the same on both) gives worn apparel a callback for the wearer's death
// (ThingComp.Notify_WearerDied) and none for a downing: Pawn.Notify_Downed, which
// Pawn_HealthTracker.MakeDowned calls once the pawn is Downed, walks the pawn's own comps. A
// postfix on it walks the worn apparel for the one comp that asks.
//
// The comp decides what a downing means (CompMechStunPulse.Fires, its header), so this runs for
// every downing in the game and does nothing for a pawn without the armor.
[HarmonyPatch(typeof(Pawn), nameof(Pawn.Notify_Downed))]
public static class Pawn_Notify_Downed
{
    public static void Postfix(Pawn __instance)
    {
        List<Apparel> worn = __instance.apparel?.WornApparel;
        if (worn == null)
            return;
        for (int i = 0; i < worn.Count; i++)
            worn[i].GetComp<CompMechStunPulse>()?.Notify_WearerDowned();
    }
}
