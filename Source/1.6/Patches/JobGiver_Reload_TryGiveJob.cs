using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using RimWorld.Utility;
using UnityEngine;
using Verse;
using Verse.AI;

namespace OverlordWarcasket.Patches;

// The automatic reload of the mech resurrector, capped at its gauge's target. When the
// reloadable vanilla would serve is the resurrector, this prefix answers in vanilla's place with
// vanilla's own steps (TryGiveJob, identical on 1.6 and 1.7.5005: manipulation, the carry space
// for one charge, FindEnoughAmmo, MakeReloadJob) and one change, the job's count. MakeReloadJob
// sizes it for a whole resurrector whoever asks (the chosen stacks' total, capped by
// MaxAmmoNeeded(true)), and JobDriver_Reload carries the count and loads all of it. The stacks
// themselves were chosen against the target (Patches/CompApparelReloadable_Reload), so the last
// of them can be larger than what is wanted but none is surplus: the lower count still visits
// every one. The float menu builds its job through MakeReloadJob directly and never comes
// through here, so an ordered reload keeps the whole resurrector's count. Every other reloadable
// falls through to vanilla.
//
// A prefix, and first among them, rather than a postfix on the count: another mod's prefix can
// answer before vanilla does, and Harmony skips every later bool prefix once one returns false.
// Combat Extended's is the known one (Harmony_JobGiver_Reload_TryGiveJob, CE 16.7.4, applied
// from the Mod constructor and so ahead of ours at equal priority): for any worn reloadable
// whose ammo the pawn carries it drops MaxAmmoNeeded(true) of it, a whole resurrector's worth, at
// the pawn's feet and reloads from that, which a capped count would leave lying where the pilot
// stood. Answering first keeps the automatic reload the same with CE as without: fetched from
// the map up to the target, the carried ammo neither spent nor dropped.
//
// CE's "Reload worn armor" button (Command_ReloadArmor, on every worn reloadable of a player's
// colonist) calls that prefix of CE's as a plain method, outside Harmony's chain, so it is not
// answered here. It asks FindSomeReloadableComponent with allowForcedReload false, as the
// automatic reload does, so on a resurrector at or above the target it does nothing; below it
// the carried stack is dropped for what the resurrector lacks and loaded whole, an order's
// reload from the inventory.
[HarmonyPatch(typeof(JobGiver_Reload), "TryGiveJob")]
public static class JobGiver_Reload_TryGiveJob
{
    [HarmonyPriority(Priority.First)]
    public static bool Prefix(Pawn pawn, ref Job __result)
    {
        if (ReloadableUtility.FindSomeReloadableComponent(pawn, allowForcedReload: false) is not CompMechResurrector { ReloadsPerCharge: true } resurrector)
            return true;
        __result = MakeJob(pawn, resurrector);
        return false;
    }

    private static Job MakeJob(Pawn pawn, CompMechResurrector resurrector)
    {
        if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
            return null;
        if (pawn.carryTracker.AvailableStackSpace(resurrector.AmmoDef) < resurrector.MinAmmoNeeded(allowForcedReload: true))
            return null;
        List<Thing> ammo = ReloadableUtility.FindEnoughAmmo(pawn, pawn.Position, resurrector, forceReload: false);
        if (ammo.NullOrEmpty())
            return null;
        Job job = JobGiver_Reload.MakeReloadJob(resurrector, ammo);
        job.count = Mathf.Min(job.count, resurrector.MaxAmmoNeeded(allowForcedReload: false));
        return job;
    }
}
