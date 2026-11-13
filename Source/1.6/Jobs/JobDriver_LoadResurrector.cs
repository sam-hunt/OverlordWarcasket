using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace OverlordWarcasket;

// The carried reload (OLWC_LoadResurrector): the pilot loads the mech resurrector, target A's, from
// the ammo in their own inventory. Ordered by Command_LoadResurrector it fills the resurrector;
// given by the automatic reload (Patches/JobGiver_Reload_TryGiveJob, behind the resurrector's
// autoReloadFromInventory) it stops at the gauge's target. The two are told apart by the job's
// playerForced, which Pawn_JobTracker.TryTakeOrderedJob sets and a think tree's job never
// carries. Vanilla's JobDriver_Reload without the fetch: the same one wait of the comp's
// BaseReloadTicks under a progress bar, whatever the charges loaded, then the load. The ammo
// never leaves the inventory until ReloadFrom splits it off, so an interrupted job drops and
// spends nothing.
public class JobDriver_LoadResurrector : JobDriver
{
    private CompMechResurrector Resurrector => job.GetTarget(TargetIndex.A).Thing?.TryGetComp<CompMechResurrector>();

    public override bool TryMakePreToilReservations(bool errorOnFailed) => true;

    protected override IEnumerable<Toil> MakeNewToils()
    {
        CompMechResurrector resurrector = Resurrector;
        this.FailOn(() => resurrector == null || resurrector.Wearer != pawn || !resurrector.ReloadsPerCharge
            || resurrector.ChargesWanted(job.playerForced) <= 0
            || resurrector.CarriedAmmo(pawn) < resurrector.Props.ammoCountPerCharge);
        this.FailOnIncapable(PawnCapacityDefOf.Manipulation);
        yield return Toils_General.Wait(resurrector?.BaseReloadTicks ?? 0).WithProgressBarToilDelay(TargetIndex.A);
        Toil load = ToilMaker.MakeToil("LoadResurrector");
        load.initAction = () => resurrector.ReloadFromInventory(pawn, job.playerForced);
        load.defaultCompleteMode = ToilCompleteMode.Instant;
        yield return load;
    }
}
