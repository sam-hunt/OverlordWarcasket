using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Ability = VEF.Abilities.Ability;
using Command_Ability = VEF.Abilities.Command_Ability;

namespace OverlordWarcasket;

// The armor's active: Biotech's resurrect mech (the apocriton's CompAbilityEffect_ResurrectMech
// and its def, ResurrectionMech) as a VEF apparel ability drawing on the armor's resurrection
// core (CompResurrectionCore, read off holder, which is why the two comps share the armor def).
// The rules are the apocriton's, verified against RimWorld 1.7.4990's decompile: a corpse is
// resurrectable when its pawn is a mechanoid of the pilot's own faction, not of a kind that
// itself resurrects mechs, dead for at most the extension's maxCorpseAgeTicks (when it sets a
// limit), and of a weight class the cost table prices within the charges held. The table alone
// says which classes come back: the apocriton also reads MechWeightClassDef.canResurrect, which
// vanilla clears on the ultraheavy class, and this ability does not, since its table prices
// that class (the def header has the why). Unlike
// the apocriton's area cast, only the clicked corpse comes back (the def header has the why),
// paid from the core as the apocriton pays its charges; the applied effecter plays on the mech,
// the centre effecter at its corpse, and the mech staggers for a second.
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
// the mech's cost within the total). Whose mech it was before does not matter, since the death
// already cleared that. Neither applies to a pilot without a mechlink, who can cast only with
// requireMechlink off and raises the mech uncontrolled, or to a mech with no CompOverseerSubject,
// which no mechanitor can control. Verified on 1.7.5005.
//
// Targeting: the click is a mech corpse (targetMode Thing, corpse-only parameters), validated
// with messages through ValidateTarget; the def sets no hasAoE, so VEF's ModifyTargets leaves
// it the only target. The click is not
// range-limited (GetRangeForPawn returns a number VEF treats as unlimited) because the goto job
// (VFEA_GotoTargetAndUseAbility) walks the pilot to within the def's distanceToTarget, the
// apocriton's cast range, before the warmup; requireLineOfSight is off, as on the apocriton,
// and the walk enforces sight itself. The corpse is re-checked at cast time, since the walk
// may outlast the corpse window or the charges may have changed meanwhile.
//
// Enabled only for a mechanitor while the extension's requireMechlink holds (a mod setting), and
// only while the core holds a charge; the disabled reason names which. Autocast is pinned off:
// VEF's only autocast consumer reads learned abilities, never an apparel comp's, so the toggle
// would show and never fire.
//
// Gizmo: one cached Command_ResurrectMech (VEF builds a new command, tooltip and all, on every
// request), with the enable state and the description refreshed every call. A running cooldown
// stored as an end tick is capped to the new length on the next gizmo request after a settings
// write (ReconcileSettings, keyed on the settings generation), since VEF's cooldown overlay
// would otherwise overflow the button.
public class Ability_ResurrectMech : Ability
{
    // "Unlimited" as a finite number so VEF's range arithmetic and ring drawing stay sane.
    private const float UnlimitedRange = 10000f;

    // The apocriton's stagger on each resurrected mech.
    private const int StaggerTicks = 60;

    private CompResurrectionCore core;
    private Command_ResurrectMech gizmo;
    private int settingsGeneration = -1;

    // holder is the armor; it never changes for this instance.
    public CompResurrectionCore Core => core ??= holder?.TryGetComp<CompResurrectionCore>();

    public ResurrectMechExtension Extension => def.GetModExtension<ResurrectMechExtension>();

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
        CompResurrectionCore c = Core;
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
        if (CanResurrect(corpse, out string reason, out _))
            return true;
        if (showMessages && reason != null)
            Messages.Message(reason, corpse, MessageTypeDefOf.RejectInput, historical: false);
        return false;
    }

    // The apocriton's CanResurrect less its canResurrect check, with the reason each refusal
    // shows; cost is the charges the corpse's weight class takes when it passes.
    public bool CanResurrect(Corpse corpse, out string reason, out int cost)
    {
        reason = null;
        cost = -1;
        Pawn mech = corpse?.InnerPawn;
        ResurrectMechExtension extension = Extension;
        CompResurrectionCore c = Core;
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
        if (cost > c.Charges)
        {
            reason = "OLWC_NotEnoughCharges".Translate(mech.LabelShortCap, cost, c.Charges);
            return false;
        }
        if (TakesControlOf(mech) && !pawn.mechanitor.HasBandwidthToResurrect(corpse))
        {
            reason = "OLWC_NotEnoughBandwidth".Translate(pawn.LabelShortCap, mech.LabelShortCap);
            return false;
        }
        return true;
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
        CompResurrectionCore c = Core;
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
        CompResurrectionCore c = Core;
        if (c == null)
            return null;
        string first = "OLWC_ChargesLabel".Translate(c.Charges, c.MaxCharges);
        if (target.Thing is not Corpse corpse)
            return first;
        if (CanResurrect(corpse, out string reason, out int cost))
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
