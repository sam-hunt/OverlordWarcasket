using System.Reflection;
using HarmonyLib;
using RimWorld.QuestGen;
using Verse;

namespace OverlordWarcasket.Patches;

// The dead mechanitor of Biotech's mechanitor ship quest, offered to AncientPilot. RunInt
// (1.7.5014 and the frozen 1.6 assembly, the same on both) generates the pawn dead through
// quest.GeneratePawn, ages its corpse, adds the corpse to the shuttle's contents and stores the
// pawn in the quest's slate as "mechanitor", so a postfix reads it back from QuestGen.slate,
// which is still this quest's while the root node runs. RunInt leaves the name unset where it
// returns early (Biotech not active), and Slate.Get then answers null.
//
// This runs once for each such quest generated, not when the ship lands: the corpse waits in
// the quest wearing the set, and a setting changed after that does not reach it.
//
// RunInt is protected, so it is bound by name: should it move, this patch stands down with a
// warning where a failed lookup would stop PatchAll and take every other patch with it.
[HarmonyPatch]
public static class QuestNode_Root_MechanitorShip_RunInt
{
    private const string Method = "RunInt";

    private static MethodBase Target => AccessTools.Method(typeof(QuestNode_Root_MechanitorShip), Method);

    public static bool Prepare()
    {
        if (Target != null)
            return true;
        Log.Warning($"[Overlord Warcasket] {nameof(QuestNode_Root_MechanitorShip)}.{Method} not found, so the mechanitor ship's dead mechanitor will never wear the overlord warcasket.");
        return false;
    }

    public static MethodBase TargetMethod() => Target;

    public static void Postfix() =>
        AncientPilot.TryDress(QuestGen.slate?.Get<Pawn>("mechanitor"), AncientPilotSource.Ship);
}
