using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace OverlordWarcasket.Patches;

// The dead mechanitor of Biotech's ancient mechanitor complex, offered to AncientPilot.
// TryPlaceDeceasedMechanitor (1.7.5014 and the frozen 1.6 assembly, the same on both) spawns an
// ancient cryptosleep pod, generates the pawn dead through PawnGenerator.GeneratePawn, ages its
// corpse and shuts the corpse in the pod, which it hands back through its out parameter with a
// true result. A postfix reads the corpse out of the pod. The method tries one room at a time
// and PreSpawnThreats stops at the first that takes the pod, so a true result comes once for
// each complex.
//
// The method is private, so it is bound by name: should it move, this patch stands down with a
// warning where a failed lookup would stop PatchAll and take every other patch with it.
[HarmonyPatch]
public static class LayoutWorkerComplex_Mechanitor_TryPlaceDeceasedMechanitor
{
    private const string Method = "TryPlaceDeceasedMechanitor";

    private static MethodBase Target => AccessTools.Method(typeof(LayoutWorkerComplex_Mechanitor), Method);

    public static bool Prepare()
    {
        if (Target != null)
            return true;
        Log.Warning($"[Overlord Warcasket] {nameof(LayoutWorkerComplex_Mechanitor)}.{Method} not found, so the ancient mechanitor complex's dead mechanitor will never wear the overlord warcasket.");
        return false;
    }

    public static MethodBase TargetMethod() => Target;

    public static void Postfix(bool __result, Building_AncientCryptosleepPod casket)
    {
        if (__result)
            AncientPilot.TryDress((casket?.ContainedThing as Corpse)?.InnerPawn, AncientPilotSource.Pod);
    }
}
