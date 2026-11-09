using HarmonyLib;
using Verse;

namespace OverlordWarcasket.Patches;

// Lifts the mechanitor command range for the mechs of a qualifying overseer. Every drafted
// order for a colony mech on 1.6 and 1.7 (the seven call sites CommandRange's header lists)
// asks this one static method, so one postfix covers the move, both attacks and the drag-goto.
// A postfix rather than a prefix: vanilla's answer stands untouched wherever the range does not
// apply (no overseer, a mechanitor the setting excludes, the feature off), and where it does the
// answer is replaced whole, which lets a radius below vanilla's refuse what vanilla allows.
[HarmonyPatch(typeof(MechanitorUtility), nameof(MechanitorUtility.InMechanitorCommandRange))]
public static class MechanitorUtility_InMechanitorCommandRange
{
    public static void Postfix(Pawn mech, LocalTargetInfo target, ref bool __result)
    {
        if (CommandRange.TryEvaluate(mech, target, out bool allowed))
            __result = allowed;
    }
}
