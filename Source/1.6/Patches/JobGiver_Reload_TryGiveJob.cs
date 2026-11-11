using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using RimWorld.Utility;
using UnityEngine;
using Verse;
using Verse.AI;

namespace OverlordWarcasket.Patches;

// The automatic reload of the resurrection core, capped at its gauge's target. When the
// reloadable vanilla would serve is the core, this prefix answers in vanilla's place with
// vanilla's own steps (TryGiveJob, identical on 1.6 and 1.7.5005: manipulation, the carry space
// for one charge, FindEnoughAmmo, MakeReloadJob) and one change, the job's count. MakeReloadJob
// sizes it for a whole core whoever asks (the chosen stacks' total, capped by
// MaxAmmoNeeded(true)), and JobDriver_Reload carries the count and loads all of it. The stacks
// themselves were chosen against the target (Patches/CompApparelReloadable_Reload), so the last
// of them can be larger than what is wanted but none is surplus: the lower count still visits
// every one. The float menu builds its job through MakeReloadJob directly and never comes
// through here, so an ordered reload keeps the whole core's count. Every other reloadable falls
// through to vanilla.
//
// Where the core's autoReloadFromInventory is on, the ammo the pilot carries comes first: while
// a carried stack holds a charge's worth the job given is the carried reload's (OLWC_LoadCore),
// which loads up to the target and no further, and the fetch above serves only what is still
// missing once no carried stack does.
//
// A prefix, and first among them, rather than a postfix on the count: another mod's prefix can
// answer before vanilla does, and Harmony skips every later bool prefix once one returns false.
// Combat Extended's is the known one (Harmony_JobGiver_Reload_TryGiveJob, CE 16.7.4, applied
// from the Mod constructor and so ahead of ours at equal priority): for any worn reloadable
// whose ammo the pawn carries it drops MaxAmmoNeeded(true) of it, a whole core's worth, at the
// pawn's feet and reloads from that, which a capped count would leave lying where the pilot
// stood. Answering first keeps the automatic reload the same with CE as without, the carried
// ammo spent or left as the core's own switch says, and never dropped.
[HarmonyPatch(typeof(JobGiver_Reload), "TryGiveJob")]
public static class JobGiver_Reload_TryGiveJob
{
    [HarmonyPriority(Priority.First)]
    public static bool Prefix(Pawn pawn, ref Job __result)
    {
        if (ReloadableUtility.FindSomeReloadableComponent(pawn, allowForcedReload: false) is not CompResurrectionCore { ReloadsPerCharge: true } core)
            return true;
        __result = MakeJob(pawn, core);
        return false;
    }

    private static Job MakeJob(Pawn pawn, CompResurrectionCore core)
    {
        if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
            return null;
        if (core.Props.autoReloadFromInventory && core.CarriesACharge(pawn))
            return JobMaker.MakeJob(OLWC_DefOf.OLWC_LoadCore, core.parent);
        if (pawn.carryTracker.AvailableStackSpace(core.AmmoDef) < core.MinAmmoNeeded(allowForcedReload: true))
            return null;
        List<Thing> ammo = ReloadableUtility.FindEnoughAmmo(pawn, pawn.Position, core, forceReload: false);
        if (ammo.NullOrEmpty())
            return null;
        Job job = JobGiver_Reload.MakeReloadJob(core, ammo);
        job.count = Mathf.Min(job.count, core.MaxAmmoNeeded(allowForcedReload: false));
        return job;
    }
}
