using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace OverlordWarcasket;

// The carried reload (OLWC_LoadCore, ordered by Command_LoadCore): the pilot loads the
// resurrection core, target A's, from the ammo in their own inventory. Vanilla's
// JobDriver_Reload without the fetch: the same one wait of the comp's BaseReloadTicks under a
// progress bar, whatever the charges loaded, then the load. The ammo never leaves the
// inventory until ReloadFrom splits it off, so an interrupted job drops and spends nothing.
public class JobDriver_LoadCore : JobDriver
{
    private CompResurrectionCore Core => job.GetTarget(TargetIndex.A).Thing?.TryGetComp<CompResurrectionCore>();

    public override bool TryMakePreToilReservations(bool errorOnFailed) => true;

    protected override IEnumerable<Toil> MakeNewToils()
    {
        CompResurrectionCore core = Core;
        this.FailOn(() => core == null || core.Wearer != pawn || !core.ReloadsPerCharge || core.Full
            || core.CarriedAmmo(pawn) < core.Props.ammoCountPerCharge);
        this.FailOnIncapable(PawnCapacityDefOf.Manipulation);
        yield return Toils_General.Wait(core?.BaseReloadTicks ?? 0).WithProgressBarToilDelay(TargetIndex.A);
        Toil load = ToilMaker.MakeToil("LoadCore");
        load.initAction = () => core.ReloadFromInventory(pawn);
        load.defaultCompleteMode = ToilCompleteMode.Instant;
        yield return load;
    }
}
