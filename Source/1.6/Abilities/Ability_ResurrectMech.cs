using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Ability = VEF.Abilities.Ability;
using Command_Ability = VEF.Abilities.Command_Ability;

namespace OverlordWarcasket;

// The armor's active: Biotech's resurrect mech (the apocriton's CompAbilityEffect_ResurrectMech
// and its def, ResurrectionMech) as a VEF apparel ability drawing on the armor's resurrection
// core (CompResurrectionCore, read off holder, which is why the two comps share the armor def).
// The rules are the apocriton's, verified against RimWorld 1.7.4990's decompile: a corpse is
// resurrectable when its pawn is a mechanoid of a weight class that allows it, of the pilot's own
// faction, not of a kind that itself resurrects mechs, dead for at most the extension's
// maxCorpseAgeTicks, and of a weight class the cost table prices within the charges held. Every
// such corpse within the def's radius of the clicked one comes back, nearest first, each paid
// from the core as the apocriton pays its charges; the applied effecter plays on each, the
// centre effecter once at the targets' middle, and each mech staggers for a second.
//
// Targeting: the click is a mech corpse (targetMode Thing, corpse-only parameters) and VEF's
// AoE collects the corpses around it (hasAoE, radius, targetingParametersForAoE) through
// ValidateTarget asked quietly; the clicked one is validated with messages. The click is not
// range-limited (GetRangeForPawn returns a number VEF treats as unlimited) because the goto job
// (VFEA_GotoTargetAndUseAbility) walks the pilot to within the def's distanceToTarget, the
// apocriton's cast range, before the warmup; requireLineOfSight is off, as on the apocriton,
// and the walk enforces sight itself. The corpses are re-checked at cast time, since the walk
// may outlast the corpse window or another core may have spent its charges.
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

    // The apocriton's CanResurrect, with the reason each refusal shows; cost is the charges the
    // corpse's weight class takes when it passes.
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
        if (weightClass == null || !weightClass.canResurrect
            || (mech.kindDef.abilities != null && mech.kindDef.abilities.Contains(AbilityDefOf.ResurrectionMech))
            || !TryGetCost(extension, weightClass, out cost))
        {
            reason = "OLWC_CannotResurrectClass".Translate(mech.LabelShortCap);
            return false;
        }
        if (mech.Faction != pawn?.Faction)
        {
            reason = "OLWC_NotOurMech".Translate(mech.LabelShortCap);
            return false;
        }
        if (corpse.timeOfDeath < Find.TickManager.TicksGame - extension.maxCorpseAgeTicks)
        {
            reason = "OLWC_CorpseTooOld".Translate(mech.LabelShortCap);
            return false;
        }
        if (cost > c.Charges)
        {
            reason = "OLWC_NotEnoughCharges".Translate(mech.LabelShortCap, cost, c.Charges);
            return false;
        }
        return true;
    }

    private static bool TryGetCost(ResurrectMechExtension extension, MechWeightClassDef weightClass, out int cost)
    {
        foreach (MechChargeCosts entry in extension.costs)
        {
            if (entry.weightClass == weightClass)
            {
                cost = entry.cost;
                return true;
            }
        }
        cost = -1;
        return false;
    }

    // base.Cast first: the cooldown, the cast sound and the extension hooks. Then the corpses,
    // nearest the clicked one first, each re-checked and paid before it comes back.
    public override void Cast(params GlobalTargetInfo[] targets)
    {
        base.Cast(targets);
        CompResurrectionCore c = Core;
        ResurrectMechExtension extension = Extension;
        Map map = pawn?.Map;
        if (c == null || extension == null || map == null || targets.Length == 0)
            return;
        IntVec3 center = firstTarget.IsValid ? firstTarget.Cell : targets[0].Cell;
        var corpses = targets
            .Select(target => target.Thing as Corpse)
            .Where(corpse => corpse != null && corpse.Spawned && corpse.Map == map)
            .OrderBy(corpse => corpse.Position.DistanceToSquared(center))
            .ToList();
        foreach (Corpse corpse in corpses)
        {
            if (!CanResurrect(corpse, out _, out int cost) || !c.TryConsume(cost))
                continue;
            Pawn mech = corpse.InnerPawn;
            ResurrectionUtility.TryResurrect(mech);
            if (extension.appliedEffecterDef != null && mech.MapHeld != null)
            {
                Effecter effecter = extension.appliedEffecterDef.SpawnAttached(mech, mech.MapHeld);
                effecter.Trigger(mech, mech);
                effecter.Cleanup();
            }
            mech.stances?.stagger.StaggerFor(StaggerTicks);
        }
        if (extension.centerEffecterDef == null)
            return;
        Vector3 sum = Vector3.zero;
        foreach (GlobalTargetInfo target in targets)
            sum += target.Cell.ToVector3Shifted();
        IntVec3 middle = (sum / targets.Length).ToIntVec3();
        var at = new TargetInfo(middle, map);
        Effecter centre = extension.centerEffecterDef.Spawn(middle, map);
        centre.EffectTick(at, at);
        centre.Cleanup();
    }

    // The label beside the cursor: the charges held and, over a resurrectable corpse, what it
    // costs; over a refused one, why.
    public override string ExtraLabelMouseAttachment(LocalTargetInfo target)
    {
        CompResurrectionCore c = Core;
        if (c == null)
            return null;
        string first = "OLWC_ChargesLabel".Translate(c.Charges, c.MaxCharges);
        if (target.Thing is not Corpse corpse)
            return first;
        if (CanResurrect(corpse, out string reason, out int cost))
            return first + "\n" + "OLWC_TargetCost".Translate(corpse.InnerPawn.LabelShortCap, cost);
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
