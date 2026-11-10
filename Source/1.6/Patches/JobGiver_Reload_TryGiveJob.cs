using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace OverlordWarcasket.Patches;

// Caps an automatic reload of the resurrection core at its gauge's target. TryGiveJob builds its
// job through MakeReloadJob, which sizes the count for a whole core whoever asks (verified on 1.6
// and 1.7.5000: job.count is the chosen stacks' total, capped by MaxAmmoNeeded(true)), and
// JobDriver_Reload carries the count and loads all of it. The stacks themselves were chosen
// against the target (Patches/CompApparelReloadable_Reload), so the last of them can be larger
// than what is wanted but none is surplus: the lower count still visits every one. The float
// menu builds its job through MakeReloadJob directly and never comes through here, so an ordered
// reload keeps the whole core's count.
[HarmonyPatch(typeof(JobGiver_Reload), "TryGiveJob")]
public static class JobGiver_Reload_TryGiveJob
{
    public static void Postfix(Job __result)
    {
        if (__result == null || __result.def != JobDefOf.Reload)
            return;
        if (__result.targetA.Thing.TryGetComp<CompResurrectionCore>() is not { ReloadsPerCharge: true } core)
            return;
        __result.count = Mathf.Min(__result.count, core.MaxAmmoNeeded(allowForcedReload: false));
    }
}
