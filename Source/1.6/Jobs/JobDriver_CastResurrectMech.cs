using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;
using VEF.Abilities;

namespace OverlordWarcasket;

// The resurrect mech ability's cast (OLWC_CastResurrectMech): the pilot walks to within cast
// range of the mech corpse, target A, waits out a running cooldown, checks the corpse and then
// runs VEF's warmup and cast toils (JobDriver_CastAbilityOnce: a progress-bar wait of
// GetCastTimeForPawn ticks with the aim pie, the ability's PreWarmupAction and WarmupToil, then
// an instant cast toil, and EndCastJob on every job end). Medic Warcasket's JobDriver_MedicCast
// is the family precedent, seen queueing in game; this is that driver for a corpse.
//
// Why not VEF's job (VFEA_GotoTargetAndUseAbility). VEF keeps the cast being made in one slot
// per pawn (CompAbilities.currentlyCasting and currentlyCastingTargets, written by
// Ability.StartAbilityJob as it starts the job) and its drivers read the ability, the cast
// time and the targets from there, the job carrying only the walk's destination. A second cast
// ordered behind the first would overwrite the slot the first is still reading, and
// StartAbilityJob never queues anyway: it ends the current job and starts its own. Here the job
// is the whole order: the corpse is its target A and the ability is the one the pilot's armor
// grants (Ability_ResurrectMech.WornBy), so any number can wait in the queue, and the ability
// orders it through Pawn_JobTracker.TryTakeOrderedJob, which queues on the queue key as it does
// for every ordered job.
//
// The binding: VEF's toils still read the slot, so it is filled here, in
// TryMakePreToilReservations, and only when the job is starting (StartJob sets CurJob before
// calling it). Vanilla also calls this at order time through the job's cached driver, while the
// pilot may be mid-cast of another VEF ability whose toils read the slot, so at order time it
// returns true without touching the slot or calling the base, which would dereference a
// possibly null currentlyCasting. A job that starts with the armor gone returns false, which
// ends a queued job quietly. GetReport reads the ability def directly for the same reason:
// vanilla reports queued jobs through the cached driver, before any binding.
//
// The walk is modelled on VEF's JobDriver_GotoTargetAndCastAbilityOnce: toward the corpse until
// it is within the ability def's distanceToTarget with line of sight and not mid-door. It
// paths to touch the corpse, and one the pilot cannot reach ends the job, where VEF's goto,
// finding no cell beside it, would move on and cast from wherever the pilot stood. A pilot who
// arrives beside the corpse casts from there whatever the sight check says, rather than
// repathing every tick.
//
// The corpse is checked as the job starts and again before the warmup, with the ability's own
// message: a queued cast may start long after its order, and the cast's own re-check comes only
// after the warmup and the cooldown are spent.
//
// Nothing is reserved, as VEF's job and vanilla's ability casts reserve nothing: a corpse
// hauled away before the cast fails the job on its despawn (the base driver's
// FailOnDespawnedOrNull).
public class JobDriver_CastResurrectMech : JobDriver_CastAbilityOnce
{
    private Ability_ResurrectMech Casting => CompAbilities?.currentlyCasting as Ability_ResurrectMech;

    private Corpse Corpse => TargetA.Thing as Corpse;

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        if (pawn.CurJob != job)
            return true;
        Ability_ResurrectMech ability = Ability_ResurrectMech.WornBy(pawn);
        CompAbilities comp = CompAbilities;
        Corpse corpse = Corpse;
        if (ability == null || comp == null || corpse == null)
            return false;
        ability.pawn = pawn;
        comp.currentlyCasting = ability;
        comp.currentlyCastingTargets = [new GlobalTargetInfo(corpse)];
        return base.TryMakePreToilReservations(errorOnFailed);
    }

    public override string GetReport() => OLWC_DefOf.OLWC_ResurrectMech.JobReportString;

    protected override IEnumerable<Toil> MakeNewToils()
    {
        yield return GotoCorpse();
        yield return AwaitCooldown();
        yield return Check();
        foreach (Toil toil in base.MakeNewToils())
            yield return toil;
    }

    private Toil GotoCorpse()
    {
        Toil toil = ToilMaker.MakeToil("GotoCorpse");
        toil.initAction = () =>
        {
            pawn.pather.StopDead();
            EndUnlessResurrectable();
        };
        toil.tickAction = () =>
        {
            LocalTargetInfo target = job.targetA;
            IntVec3 cell = target.Cell;
            Map map = pawn.Map;
            pawn.rotationTracker.FaceTarget(cell);
            bool moving = pawn.pather.Moving;
            bool inRange = pawn.Position.DistanceTo(cell) <= (Casting?.def.distanceToTarget ?? 0f)
                && GenSight.LineOfSight(pawn.Position, cell, map, skipFirstCell: true)
                && (!moving || pawn.pather.nextCell.GetDoor(map) == null);
            if (inRange || (!moving && pawn.Position.AdjacentTo8WayOrInside(cell)))
            {
                pawn.pather.StopDead();
                ReadyForNextToil();
            }
            else if (!moving)
            {
                if (!pawn.CanReach(target, PathEndMode.Touch, Danger.Deadly))
                {
                    EndJobWith(JobCondition.Incompletable);
                    return;
                }
                pawn.pather.StartPath(target, PathEndMode.Touch);
            }
        };
        toil.handlingFacing = true;
        toil.socialMode = RandomSocialMode.Off;
        toil.defaultCompleteMode = ToilCompleteMode.Never;
        return toil;
    }

    // Reachable only from the queue, since the gizmo is disabled while the cooldown runs: the
    // pilot waits by the corpse until it ends. With a cooldown of 0 it never waits.
    private Toil AwaitCooldown()
    {
        Toil toil = ToilMaker.MakeToil("AwaitCooldown");
        toil.initAction = () => pawn.pather.StopDead();
        toil.tickAction = () =>
        {
            pawn.rotationTracker.FaceTarget(job.targetA);
            if (Casting == null || Casting.cooldown <= Find.TickManager.TicksGame)
                ReadyForNextToil();
        };
        toil.handlingFacing = true;
        toil.socialMode = RandomSocialMode.Off;
        toil.defaultCompleteMode = ToilCompleteMode.Never;
        return toil;
    }

    private Toil Check()
    {
        Toil toil = ToilMaker.MakeToil("CheckCorpse");
        toil.initAction = EndUnlessResurrectable;
        toil.defaultCompleteMode = ToilCompleteMode.Instant;
        return toil;
    }

    // Ends the job, with the ability's reason as a message, when the corpse can no longer be
    // resurrected.
    private void EndUnlessResurrectable()
    {
        Corpse corpse = Corpse;
        string reason = null;
        if (Casting != null && Casting.CanResurrect(corpse, out reason, out _))
            return;
        if (reason != null)
            Messages.Message(reason, corpse, MessageTypeDefOf.RejectInput, historical: false);
        EndJobWith(JobCondition.Incompletable);
    }
}
