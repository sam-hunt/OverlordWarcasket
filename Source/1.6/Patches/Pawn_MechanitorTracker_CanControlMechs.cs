using HarmonyLib;
using RimWorld;
using Verse;

namespace OverlordWarcasket.Patches;

// Lets a mechanitor traveling in a caravan keep control of their mechs under the Anywhere scope.
// Vanilla (verified on 1.7.5005 and the frozen 1.6 assembly, the getter identical) refuses all
// control, before any range is asked, once the mechanitor is not spawned: CanControlMechs
// checks downed, then prisoner, then unspawned (a reason only when held in a building, a bare
// false otherwise), then a mental state. Its five readers are the mech's draft toggle and the
// mechs tab's draft column (both through MechanitorUtility.CanDraftMech), the control group
// gizmo a mech shows for its group, the work mode column and the control group designator, so
// this one getter opens the draft, and with it every order, and the work mode together.
//
// A postfix that answers only vanilla's unspawned refusal, and only where
// CommandRange.ControlsFromCaravan holds: the checks vanilla made before it are asked again to
// tell which refusal this is, and the one it never reached, the mental state, is applied here
// with vanilla's own text (Biotech's key). Every other answer stands, a pilot in a casket's or
// a transport pod's included.
//
// What vanilla still does: a mechanitor leaving a map undrafts every mech
// (Pawn_MechanitorTracker.Notify_DeSpawned), so the mechs left behind are drafted again by hand
// once the caravan is under way.
[HarmonyPatch(typeof(Pawn_MechanitorTracker), nameof(Pawn_MechanitorTracker.CanControlMechs), MethodType.Getter)]
public static class Pawn_MechanitorTracker_CanControlMechs
{
    public static void Postfix(Pawn_MechanitorTracker __instance, ref AcceptanceReport __result)
    {
        if (__result.Accepted || CommandRange.AppliesTo == CommandRangeAppliesTo.None)
            return;
        Pawn pawn = __instance.Pawn;
        if (pawn.Spawned || pawn.Downed || pawn.IsPrisoner || !CommandRange.ControlsFromCaravan(pawn))
            return;
        if (pawn.InMentalState)
            __result = "MechControllerMentalState".Translate(pawn.Named("PAWN"), pawn.MentalStateDef.Named("MENTALSTATE"));
        else
            __result = AcceptanceReport.WasAccepted;
    }
}
