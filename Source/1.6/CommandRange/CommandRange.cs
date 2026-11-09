using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace OverlordWarcasket;

// Who the extended command range applies to: mechanitors wearing the set's helmet, or its
// shoulders (the piece the CommandRangeExtension sits on, moved there by Apply), every
// mechanitor whatever they wear, or no one, which leaves vanilla's range untouched.
public enum CommandRangeAppliesTo
{
    Helmet,
    Shoulders,
    AllMechanitors,
    None,
}

// The mechanitor command range the set lifts, and the rules the two patches apply. Vanilla
// (verified on 1.7.5000 and the frozen 1.6 assembly, identical call sites): every drafted order
// for a colony mech, the move, the ranged and melee attacks (FloatMenuOptionProvider_DraftedMove,
// FloatMenuOptionProvider_DraftedAttack, FloatMenuUtility) and the drag-to-goto of several pawns
// (MultiPawnGotoController.RecomputeDestinations), asks
// MechanitorUtility.InMechanitorCommandRange(mech, target), which refuses a mech whose overseer
// is null or on another map and otherwise defers to Pawn_MechanitorTracker.CanCommandTo: the
// target cell in bounds and its squared distance from the overseer's Position below 620.01, the
// square of 24.9, a private constant with no stat or def behind it. The white ring shown while a
// drafted mech is selected is Pawn_MechanitorTracker.DrawCommandRadius with the same literal.
// Nothing else is range-gated: drafting (CanDraftMech) asks only that the overseer be spawned,
// not downed, imprisoned or in a mental state, work-mode mechs ignore range, and an accepted
// order is never recalled when the overseer walks away. Nothing in VFEP or VEF touches any of
// this.
//
// The patches: Patches/MechanitorUtility_InMechanitorCommandRange replaces vanilla's answer
// with Allows whenever TryEvaluate finds a governing extension, and
// Patches/Pawn_MechanitorTracker_DrawCommandRadius draws the ring at the extension's radius,
// or not at all for the map-wide scopes. The extension that governs an order is For: the set's
// own instance for every mechanitor under AllMechanitors, else the one on a worn piece, so a
// patch mod that puts a CommandRangeExtension on its own apparel grants the range too, at its
// own scope.
//
// State: AppliesTo and Extension are written by Apply from the settings, at startup and when
// the window closes, and read by the patches; until Apply runs (or when another mod removed the
// extension from both pieces) AppliesTo is None and vanilla stands. Apply also moves the set's
// extension onto the chosen piece, so a wearing check reads the def alone, and writes the scope
// and radius back onto it; LoadDefaults reads them before the first Apply of a load, off defs
// fresh from XML (Startup's header).
//
// The ring: GenDraw.DrawRadiusRing refuses, with a logged error, any radius above
// GenRadial.MaxRadialPatternRadius, the radius of the last of its 20,000 precalculated cells,
// 79.81 on 1.6 and 1.7 (the 20,000th cell lies at squared distance 6370). Ladder caps the
// settings slider there, rounded down to a tenth, and the ring patch skips a radius above it
// rather than erroring on an XML value.
public static class CommandRange
{
    public const string HelmetDefName = "OLWC_WarcasketHelmet_Overlord";
    public const string ShouldersDefName = "OLWC_WarcasketShoulders_Overlord";

    // Vanilla's command range, Pawn_MechanitorTracker's MechCommandRange.
    public const float VanillaRadius = 24.9f;

    // The slider's rungs: five cells apart, each a tenth short of the whole as vanilla's is.
    private const int LadderStep = 5;

    public static CommandRangeAppliesTo AppliesTo { get; private set; } = CommandRangeAppliesTo.None;

    // The set's extension instance, on whichever piece Apply left it; null while vanilla stands.
    public static CommandRangeExtension Extension { get; private set; }

    // The widest ring the game can draw.
    public static float RingCap => GenRadial.MaxRadialPatternRadius;

    // The set's two pieces, which one carries the extension now and the extension itself; false,
    // with AppliesTo left for the caller, when a piece is missing or neither carries it.
    public static bool TryReadDefs(out ThingDef helmet, out ThingDef shoulders,
        out CommandRangeAppliesTo bearer, out CommandRangeExtension extension)
    {
        helmet = DefDatabase<ThingDef>.GetNamedSilentFail(HelmetDefName);
        shoulders = DefDatabase<ThingDef>.GetNamedSilentFail(ShouldersDefName);
        bearer = CommandRangeAppliesTo.Helmet;
        extension = helmet?.GetModExtension<CommandRangeExtension>();
        if (extension == null && shoulders != null)
        {
            bearer = CommandRangeAppliesTo.Shoulders;
            extension = shoulders.GetModExtension<CommandRangeExtension>();
        }
        return helmet != null && shoulders != null && extension != null;
    }

    // Writes the settings onto the defs: the extension onto the chosen piece (left where it is
    // for AllMechanitors and None, which read no piece), its scope and radius, then the state
    // the patches read. Idempotent, so the window may close any number of times.
    public static void Apply(CommandRangeAppliesTo appliesTo, CommandRangeScope scope, float radius)
    {
        if (!TryReadDefs(out ThingDef helmet, out ThingDef shoulders, out CommandRangeAppliesTo bearer, out CommandRangeExtension extension))
        {
            Clear();
            return;
        }
        extension.scope = scope;
        extension.radius = radius;
        if (appliesTo == CommandRangeAppliesTo.Helmet && bearer != appliesTo)
            Move(extension, shoulders, helmet);
        else if (appliesTo == CommandRangeAppliesTo.Shoulders && bearer != appliesTo)
            Move(extension, helmet, shoulders);
        AppliesTo = appliesTo;
        Extension = extension;
    }

    public static void Clear()
    {
        AppliesTo = CommandRangeAppliesTo.None;
        Extension = null;
    }

    private static void Move(CommandRangeExtension extension, ThingDef from, ThingDef to)
    {
        from.modExtensions?.Remove(extension);
        to.modExtensions ??= [];
        if (!to.modExtensions.Contains(extension))
            to.modExtensions.Add(extension);
    }

    // The extension whose scope governs this mechanitor's orders, or null where vanilla's range
    // stands: the set's for every mechanitor under AllMechanitors, else the first on a worn
    // piece. Cheap enough for the ring's once-a-frame call and the drag-goto's once-a-cell one.
    public static CommandRangeExtension For(Pawn mechanitor)
    {
        if (AppliesTo == CommandRangeAppliesTo.None || Extension == null)
            return null;
        if (AppliesTo == CommandRangeAppliesTo.AllMechanitors)
            return Extension;
        List<Apparel> worn = mechanitor.apparel?.WornApparel;
        if (worn == null)
            return null;
        for (int i = 0; i < worn.Count; i++)
        {
            CommandRangeExtension extension = worn[i].def.GetModExtension<CommandRangeExtension>();
            if (extension != null)
                return extension;
        }
        return null;
    }

    // Whether the set's rule replaces vanilla's answer for this order, and what it is: false
    // leaves vanilla's answer, so a mech without an overseer, or one whose overseer the range
    // does not apply to, is refused or allowed exactly as before.
    public static bool TryEvaluate(Pawn mech, LocalTargetInfo target, out bool allowed)
    {
        allowed = false;
        if (AppliesTo == CommandRangeAppliesTo.None)
            return false;
        Pawn overseer = mech.GetOverseer();
        if (overseer == null)
            return false;
        CommandRangeExtension extension = For(overseer);
        if (extension == null)
            return false;
        allowed = Allows(overseer, mech.MapHeld, target, extension);
        return true;
    }

    // The scope's rule. The bounds check is against the mech's map, not the overseer's as
    // vanilla's is, since under Anywhere the two differ; a mech held off any map is refused as
    // vanilla refuses it.
    public static bool Allows(Pawn overseer, Map mechMap, LocalTargetInfo target, CommandRangeExtension extension)
    {
        if (mechMap == null || !target.Cell.InBounds(mechMap))
            return false;
        if (extension.scope == CommandRangeScope.Anywhere)
            return true;
        if (overseer.MapHeld != mechMap)
            return false;
        return extension.scope == CommandRangeScope.CurrentMap || Within(overseer.Position, target.Cell, extension.radius);
    }

    // Vanilla's comparison, the squared distance strictly below the squared radius. Pure, for
    // the tests.
    public static bool Within(IntVec3 from, IntVec3 to, float radius) =>
        from.DistanceToSquared(to) < radius * radius;

    // The settings slider's stops for a ring cap: 4.9, 9.9, ... while they fit under the cap,
    // then the cap itself rounded down to a tenth where the last rung falls short of it, so
    // the slider ends on the widest ring the game draws (79.8 today) rather than below it.
    // Pure, for the tests.
    public static float[] Ladder(float cap)
    {
        float top = FloorToTenth(cap);
        var stops = new List<float>();
        for (int k = 1; ; k++)
        {
            float stop = (LadderStep * 10 * k - 1) / 10f;
            if (stop > top + 1e-3f)
                break;
            stops.Add(stop);
        }
        if (stops.Count == 0 || top - stops[stops.Count - 1] > 1e-3f)
            stops.Add(top);
        return stops.ToArray();
    }

    // Rounded down to a tenth, with a hair of tolerance so a value that is a tenth already is
    // not pushed below itself by float error.
    public static float FloorToTenth(float value) => Mathf.Floor(value * 10f + 1e-3f) / 10f;
}
