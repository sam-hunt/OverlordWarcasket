using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;
using Ability = VEF.Abilities.Ability;
using AbilityExtension_AbilityMod = VEF.Abilities.AbilityExtension_AbilityMod;
using Command_Ability = VEF.Abilities.Command_Ability;
using CompAbilitiesApparel = VEF.Abilities.CompAbilitiesApparel;

namespace OverlordWarcasket;

// The armor's active: Biotech's resurrect mech (the apocriton's CompAbilityEffect_ResurrectMech
// and its def, ResurrectionMech) as a VEF apparel ability drawing on the armor's mech
// resurrector (CompMechResurrector, read off holder, which is why the two comps share the armor
// def). The rules are the apocriton's, verified against RimWorld 1.7.4990's decompile: a corpse is
// resurrectable when its pawn is a mechanoid of the pilot's own faction, not of a kind that
// itself resurrects mechs, dead for at most the extension's maxCorpseAgeTicks (when it sets a
// limit), and of a weight class the cost table prices within the charges held. The table alone
// says which classes come back: the apocriton also reads MechWeightClassDef.canResurrect, which
// vanilla clears on the ultraheavy class, and this ability does not, since its table prices
// that class (the def header has the why). Unlike
// the apocriton's area cast, only the clicked corpse comes back (the def header has the why),
// paid from the resurrector as the apocriton pays its charges; the applied effecter plays on the
// mech, the centre effecter at its corpse, and the mech staggers for a second.
//
// Control: a mech's Overseer relation is removed when it dies (the relation def's removeOnDeath,
// through Pawn_RelationsTracker.RemoveRelationsOnDeath), so one that only stood up again would
// be the colony's but nobody's, uncontrolled and on its way to going feral. A mechanitor pilot
// therefore takes the mech they raise, as the mech gestator's resurrect bill gives its mech to
// the bill's mechanitor (Bill_ResurrectMech.CreateProducts): the cast adds the relation, whose
// worker assigns the mech to the pilot's first control group
// (PawnRelationWorker_Overseer.OnRelationCreated), and the corpse is refused beforehand unless
// the pilot has the bandwidth for it by the gestator's own rule
// (Pawn_MechanitorTracker.HasBandwidthToResurrect: the bandwidth in use and in gestation plus
// the mech's cost within the total; HasBandwidthFor is that rule). Whose mech it was before
// does not matter, since the death already cleared that. Neither applies to a pilot without a
// mechlink, who can cast only with requireMechlink off and raises the mech uncontrolled, or to
// a mech with no CompOverseerSubject, which no mechanitor can control. Verified on 1.7.5005.
//
// Targeting: the click is a mech corpse (targetMode Thing, corpse-only parameters), validated
// with messages through ValidateTarget; the def sets no hasAoE, so VEF's ModifyTargets leaves
// it the only target. The click is not
// range-limited (GetRangeForPawn returns a number VEF treats as unlimited) because the cast job
// (OLWC_CastResurrectMech, JobDriver_CastResurrectMech) walks the pilot to within the def's
// distanceToTarget, the apocriton's cast range, before the warmup; requireLineOfSight is off, as
// on the apocriton, and the walk enforces sight itself. The corpse is re-checked at cast time,
// since the walk may outlast the corpse window or the charges may have changed meanwhile.
//
// Orders and queueing: the cast is an ordered job of ours rather than VEF's started one (the
// driver's header has why VEF's cannot queue). CreateCastJob, VEF's hook between the click and
// the job, runs VEF's own steps and then orders the job through TryTakeOrderedJob, so the queue
// key queues it behind what the pilot is doing, as for any order. MultiSelect, which VEF leaves
// false and non-virtual, is answered true through the interface, so the targeter stays open
// while shift is held (Targeter.ProcessInputEvents) and several corpses can be clicked in a
// row; it reopens on the ability without passing through DoAction, which is what resets VEF's
// targeting index, so OrderForceTarget resets it. A corpse clicked with the queue key down is
// checked against what the casts already ordered will take (Pending): the charges and the
// bandwidth left after them, so the queue never holds a cast the resurrector could not pay for.
// Without the key the new order replaces them and they count for nothing, as at cast time.
//
// Enabled only for a mechanitor while the extension's requireMechlink holds (a mod setting), and
// only while the resurrector holds a charge; the disabled reason names which. Autocast is pinned
// off: VEF's only autocast consumer reads learned abilities, never an apparel comp's, so the toggle
// would show and never fire.
//
// Gizmo: one cached Command_ResurrectMech (VEF builds a new command, tooltip and all, on every
// request), with the enable state and the description refreshed every call. A running cooldown
// stored as an end tick is capped to the new length on the next gizmo request after a settings
// write (ReconcileSettings, keyed on the settings generation), since VEF's cooldown overlay
// would otherwise overflow the button.
public class Ability_ResurrectMech : Ability, ITargetingSource
{
    // "Unlimited" as a finite number so VEF's range arithmetic and ring drawing stay sane.
    private const float UnlimitedRange = 10000f;

    // The apocriton's stagger on each resurrected mech.
    private const int StaggerTicks = 60;

    private CompMechResurrector resurrector;
    private Command_ResurrectMech gizmo;
    private int settingsGeneration = -1;

    // holder is the armor; it never changes for this instance.
    public CompMechResurrector Resurrector => resurrector ??= holder?.TryGetComp<CompMechResurrector>();

    public ResurrectMechExtension Extension => def.GetModExtension<ResurrectMechExtension>();

    bool ITargetingSource.MultiSelect => true;

    // The queue key as TryTakeOrderedJob reads it: down means the next order queues.
    private static bool Queueing => KeyBindingDefOf.QueueOrder.IsDownEvent;

    // The ability pawn's armor grants, if any: what a cast job, which names only its corpse,
    // casts with.
    public static Ability_ResurrectMech WornBy(Pawn pawn)
    {
        List<Ability> given = CompMechResurrector.WornBy(pawn)?.parent.GetComp<CompAbilitiesApparel>()?.GivenAbilities;
        if (given == null)
            return null;
        foreach (Ability ability in given)
        {
            if (ability is Ability_ResurrectMech resurrect)
                return resurrect;
        }
        return null;
    }

    public override float GetRangeForPawn() => UnlimitedRange;

    public override bool CanAutoCast => false;

    public override bool AutoCast => false;

    // VEF runs Init again when the apparel changes wearer.
    public override void Init()
    {
        base.Init();
        gizmo = null;
    }

    public override bool IsEnabledForPawn(out string reason)
    {
        if (!base.IsEnabledForPawn(out reason))
            return false;
        CompMechResurrector c = Resurrector;
        if (c == null)
            return false;
        if ((Extension?.requireMechlink ?? true) && !MechanitorUtility.IsMechanitor(pawn))
        {
            reason = "OLWC_NotMechanitor".Translate(pawn.LabelShortCap);
            return false;
        }
        if (c.Charges <= 0)
        {
            reason = "OLWC_NoCharges".Translate();
            return false;
        }
        return true;
    }

    public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
    {
        if (!base.ValidateTarget(target, showMessages))
            return false;
        if (target.Thing is not Corpse corpse)
            return false;
        if (CanResurrect(corpse, out string reason, out _, Queueing))
            return true;
        if (showMessages && reason != null)
            Messages.Message(reason, corpse, MessageTypeDefOf.RejectInput, historical: false);
        return false;
    }

    // The apocriton's CanResurrect less its canResurrect check, with the reason each refusal
    // shows; cost is the charges the corpse's weight class takes when it passes. afterPending
    // counts the charges and the bandwidth as the casts already ordered will leave them.
    public bool CanResurrect(Corpse corpse, out string reason, out int cost, bool afterPending = false)
    {
        reason = null;
        cost = -1;
        Pawn mech = corpse?.InnerPawn;
        ResurrectMechExtension extension = Extension;
        CompMechResurrector c = Resurrector;
        if (mech == null || extension == null || c == null)
            return false;
        if (!mech.RaceProps.IsMechanoid)
        {
            reason = "OLWC_NotMechCorpse".Translate(mech.LabelShortCap);
            return false;
        }
        MechWeightClassDef weightClass = mech.RaceProps.mechWeightClass;
        if (weightClass == null
            || (mech.kindDef.abilities != null && mech.kindDef.abilities.Contains(AbilityDefOf.ResurrectionMech))
            || !extension.TryGetCost(weightClass, out cost))
        {
            reason = "OLWC_CannotResurrectClass".Translate(mech.LabelShortCap);
            return false;
        }
        if (mech.Faction != pawn?.Faction)
        {
            reason = "OLWC_NotOurMech".Translate(mech.LabelShortCap);
            return false;
        }
        if (extension.LimitsCorpseAge && corpse.timeOfDeath < Find.TickManager.TicksGame - extension.maxCorpseAgeTicks)
        {
            reason = "OLWC_CorpseTooOld".Translate(mech.LabelShortCap);
            return false;
        }
        int pendingCharges = 0;
        float pendingBandwidth = 0f;
        if (afterPending)
            Pending(corpse, out pendingCharges, out pendingBandwidth);
        int charges = Mathf.Max(0, c.Charges - pendingCharges);
        if (cost > charges)
        {
            reason = "OLWC_NotEnoughCharges".Translate(mech.LabelShortCap, cost, charges);
            return false;
        }
        if (TakesControlOf(mech) && !HasBandwidthFor(mech, pendingBandwidth))
        {
            reason = "OLWC_NotEnoughBandwidth".Translate(pawn.LabelShortCap, mech.LabelShortCap);
            return false;
        }
        return true;
    }

    // The gestator's rule (Pawn_MechanitorTracker.HasBandwidthToResurrect), with the bandwidth
    // the casts already ordered will take counted as in use.
    private bool HasBandwidthFor(Pawn mech, float pending)
    {
        Pawn_MechanitorTracker tracker = pawn.mechanitor;
        return tracker.UsedBandwidth + pending + mech.GetStatValue(StatDefOf.BandwidthCost) <= tracker.TotalBandwidth;
    }

    // The corpses the pilot's current and queued cast jobs name.
    private IEnumerable<Corpse> OrderedCorpses()
    {
        if (pawn?.jobs == null)
            yield break;
        foreach (Job job in pawn.jobs.AllJobs())
        {
            if (job.def == OLWC_DefOf.OLWC_CastResurrectMech && job.targetA.Thing is Corpse corpse)
                yield return corpse;
        }
    }

    // What the casts already ordered will take from the resurrector and from the pilot's bandwidth,
    // leaving out any for corpse itself, which a second click does not order twice.
    private void Pending(Corpse corpse, out int charges, out float bandwidth)
    {
        charges = 0;
        bandwidth = 0f;
        ResurrectMechExtension extension = Extension;
        foreach (Corpse ordered in OrderedCorpses())
        {
            Pawn mech = ordered.InnerPawn;
            if (ordered == corpse || mech == null)
                continue;
            if (extension.TryGetCost(mech.RaceProps.mechWeightClass, out int cost))
                charges += cost;
            if (TakesControlOf(mech))
                bandwidth += mech.GetStatValue(StatDefOf.BandwidthCost);
        }
    }

    // The targeter reopened on a shift-click (see the header) still holds the index
    // CreateCastJob left at -1.
    public override void OrderForceTarget(LocalTargetInfo target)
    {
        if (currentTargetingIndex < 0)
        {
            currentTargetingIndex = 0;
            currentTargets = new GlobalTargetInfo[def.targetCount];
        }
        base.OrderForceTarget(target);
    }

    // VEF's CreateCastJob (verified on VEF's source at 58c884b9) with the job ordered instead of
    // started: the extensions' Valid and PreCast hooks, which may put the start off and call back
    // for it, then the order.
    public override void CreateCastJob(params GlobalTargetInfo[] targets)
    {
        foreach (AbilityExtension_AbilityMod extension in AbilityModExtensions)
        {
            if (!extension.Valid(targets, this, true))
            {
                currentTargetingIndex--;
                return;
            }
        }
        currentTargetingIndex = -1;
        currentTargets = new GlobalTargetInfo[def.targetCount];
        Corpse corpse = targets.Length > 0 ? targets[0].Thing as Corpse : null;
        bool orderNow = true;
        PreCast(targets, ref orderNow, () => Order(corpse));
        if (orderNow)
            Order(corpse);
    }

    // Orders the cast on corpse; with the queue key down it queues, unless the pilot already has
    // that corpse ordered.
    private void Order(Corpse corpse)
    {
        if (corpse == null || pawn?.jobs == null)
            return;
        if (Queueing)
        {
            foreach (Corpse ordered in OrderedCorpses())
            {
                if (ordered == corpse)
                    return;
            }
        }
        pawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(OLWC_DefOf.OLWC_CastResurrectMech, corpse), JobTag.Misc);
    }

    // Whether the pilot becomes the overseer of mech when it comes back: a mechanitor raising a
    // mech that can be overseen at all.
    private bool TakesControlOf(Pawn mech) =>
        MechanitorUtility.IsMechanitor(pawn) && mech.OverseerSubject != null;

    // base.Cast first: the cooldown, the cast sound and the extension hooks. Then the one
    // corpse, re-checked and paid before it comes back, and taken under the pilot's control.
    public override void Cast(params GlobalTargetInfo[] targets)
    {
        base.Cast(targets);
        CompMechResurrector c = Resurrector;
        ResurrectMechExtension extension = Extension;
        Map map = pawn?.Map;
        if (c == null || extension == null || map == null || targets.Length == 0)
            return;
        if (targets[0].Thing is not Corpse corpse || !corpse.Spawned || corpse.Map != map)
            return;
        if (!CanResurrect(corpse, out _, out int cost) || !c.TryConsume(cost))
            return;
        IntVec3 cell = corpse.Position;
        Pawn mech = corpse.InnerPawn;
        ResurrectionUtility.TryResurrect(mech);
        if (!mech.Dead && TakesControlOf(mech) && mech.GetOverseer() == null)
            pawn.relations.AddDirectRelation(PawnRelationDefOf.Overseer, mech);
        if (extension.appliedEffecterDef != null && mech.MapHeld != null)
        {
            Effecter effecter = extension.appliedEffecterDef.SpawnAttached(mech, mech.MapHeld);
            effecter.Trigger(mech, mech);
            effecter.Cleanup();
        }
        mech.stances?.stagger.StaggerFor(StaggerTicks);
        if (extension.centerEffecterDef == null)
            return;
        var at = new TargetInfo(cell, map);
        Effecter centre = extension.centerEffecterDef.Spawn(cell, map);
        centre.EffectTick(at, at);
        centre.Cleanup();
    }

    // A length of game time as vanilla's ToStringTicksToPeriod words it, but with a decimal on
    // the hours between one hour and a day, which vanilla rounds whole (a 1.5 hour window would
    // read "2 hours").
    internal static string PeriodText(int ticks)
    {
        if (ticks < GenDate.TicksPerHour || ticks >= GenDate.TicksPerDay)
            return ticks.ToStringTicksToPeriod();
        string hours = ((float)ticks / GenDate.TicksPerHour).ToString("0.#");
        return hours == "1" ? "Period1Hour".Translate() : "PeriodHours".Translate(hours);
    }

    // The label beside the cursor: the charges held and, over a resurrectable corpse, what it
    // costs and how much of the corpse window it has left, which the corpse's own inspect pane
    // (vanilla's "Dead for less than a day") never tells; over a refused one, why.
    public override string ExtraLabelMouseAttachment(LocalTargetInfo target)
    {
        CompMechResurrector c = Resurrector;
        if (c == null)
            return null;
        string first = "OLWC_ChargesLabel".Translate(c.Charges, c.MaxCharges);
        if (target.Thing is not Corpse corpse)
            return first;
        if (CanResurrect(corpse, out string reason, out int cost, Queueing))
        {
            ResurrectMechExtension extension = Extension;
            string label = first + "\n" + "OLWC_TargetCost".Translate(corpse.InnerPawn.LabelShortCap, cost);
            if (extension.LimitsCorpseAge)
                label += "\n" + "OLWC_TargetTimeLeft".Translate(PeriodText(extension.maxCorpseAgeTicks - corpse.Age));
            return label;
        }
        return reason == null ? first : first + "\n" + reason;
    }

    public override Gizmo GetGizmo()
    {
        ReconcileSettings();
        if (gizmo == null)
        {
            gizmo = (Command_ResurrectMech)(Command_Ability)base.GetGizmo();
            return gizmo;
        }
        gizmo.Disabled = !IsEnabledForPawn(out string reason);
        gizmo.disabledReason = reason.Colorize(ColorLibrary.RedReadable);
        gizmo.defaultDesc = GetDescriptionForPawn();
        return gizmo;
    }

    private void ReconcileSettings()
    {
        if (settingsGeneration == OverlordWarcasketSettings.Generation)
            return;
        settingsGeneration = OverlordWarcasketSettings.Generation;
        int ceiling = Find.TickManager.TicksGame + GetCooldownForPawn();
        if (cooldown > ceiling)
            cooldown = ceiling;
    }
}
