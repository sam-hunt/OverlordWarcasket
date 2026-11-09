using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RimWorld;
using VEF.Pawns;
using Verse;

namespace OverlordWarcasket;

// Derives the Overlord trait (OLWC_WarcasketTrait_Overlord) from VFEP_WarcasketTrait at startup,
// so the Overlord's work profile and the trait's other effects follow VFEP's trait as the full
// mod list leaves it: a patch mod that opens work for every warcasket opens it for the Overlord
// too, and whatever VFEP puts on its degree or VEF extension on the running game version (1.6
// and 1.7 differ) lands on ours. The trait def's header carries the player-facing contract; the
// spec (Docs/Specs/Overlord-Work-Trait.md) the rationale. The Medic Warcasket repo's WorkProfile
// is the precedent; this copy generalises its Caring rule to a named list of work types.
//
// Openable names the work types the suit opens, by defName, each with the fixed rule's default:
// Research open, as it carries hacking (Core's Hack work giver is Research work, and every hack
// path, the autohack work giver, the Hack gizmo and the float menu, asks
// HackUtility.IsCapableOfHacking, which refuses an Intellectual-blocked pawn); Smithing open, as
// Biotech files mech gestation, subcore encoding and the mechanitor-only mech repair there;
// Crafting, Tailoring and Anomaly's DarkStudy blocked until the player opens them, the last
// skipped silently without Anomaly. Mirror records which of them the loaded game has
// (Toggleable) and what the rule makes of each (OpenByDefault: its own default, or open whenever
// VFEP's trait blocks neither its tags nor the type itself), writes that default profile, and
// Apply writes the profile the settings choose, on every ApplyToDefs. A pawn caches its disabled
// work types, so Apply refreshes every live pawn carrying the trait; at startup there is none.
//
// Opening a work type that VFEP blocks by tag means dropping that tag (ManualSkilled, for
// Smithing, Crafting and Tailoring; Intellectual, for Research and DarkStudy) and re-blocking by
// name every other work type the tag covered (Cooking, Construction, Growing, Mining,
// PlantCutting, and any modded one), since a trait's disabledWorkTypes can only add. Hacking
// cannot be had without Research: with Research and DarkStudy blocked by name the Intellectual
// skill is totally disabled (SkillDef.IsDisabled: every work type it is relevant to is blocked),
// and IsCapableOfHacking refuses that too. The Bio tab's "Incapable of" list reads tags alone, so
// it no longer names skilled or intellectual labor for the pilot; the work tab, which reads both,
// is unchanged.
//
// Mirror runs first in Startup.Run, i.e. from a static constructor on startup and again after
// every in-process play-data reload, against defs fresh from XML and with every XML patch,
// VEF's toggleable ones included, already applied. Nothing it writes is cached anywhere by
// then: Trait.AllowsWorkType and Trait.GetDisabledWorkTypes read the def live, and
// Pawn.CombinedDisabledWorkTags is uncached.
public static class WorkProfile
{
    private const string VfepTraitName = "VFEP_WarcasketTrait";
    private const string OverlordTraitName = "OLWC_WarcasketTrait_Overlord";

    private static readonly (string defName, bool openByDefault)[] Openable =
    [
        ("Research", true),
        ("Smithing", true),
        ("Crafting", false),
        ("Tailoring", false),
        ("DarkStudy", false),
    ];

    // The degree's text stays ours, and so do its identity (degree: the armor grants degree 0 and
    // saves store it) and its selection weight (commonality: irrelevant at TraitDef commonality
    // 0); everything else on it follows VFEP's.
    private static readonly HashSet<string> DegreeOwnFields =
    [
        nameof(TraitDegreeData.label),
        nameof(TraitDegreeData.labelMale),
        nameof(TraitDegreeData.labelFemale),
        nameof(TraitDegreeData.untranslatedLabel),
        nameof(TraitDegreeData.description),
        nameof(TraitDegreeData.degree),
        nameof(TraitDegreeData.commonality),
    ];

    // The mirror's result, replaced on every run and empty until the first or after a failed
    // one, so Apply then writes nothing and the trait keeps its XML profile.
    private static TraitDef overlord;
    private static List<WorkTypeDef> baseTypes = [];
    private static List<WorkTypeDef> toggleable = [];
    private static HashSet<WorkTypeDef> openByDefault = [];

    // The openable work types the loaded game has, those open by default first and each group
    // in the work tab's order: what the settings may open or block, row by row.
    public static IReadOnlyList<WorkTypeDef> Toggleable => toggleable;

    // The fixed rule for a toggleable work type: its own default from Openable, or open when
    // VFEP's trait blocks neither its tags nor the type itself (a patch mod that opened skilled
    // labor for every warcasket opens it here too).
    public static bool OpenByDefault(WorkTypeDef work) => openByDefault.Contains(work);

    // Never throws out of Startup.Run: a failure here is logged and the settings still apply.
    public static void Mirror()
    {
        overlord = null;
        baseTypes = [];
        toggleable = [];
        openByDefault = [];
        try
        {
            MirrorFromVfep();
        }
        catch (Exception e)
        {
            Log.Error($"[Overlord Warcasket] Mirroring the overlord warcasket trait from {VfepTraitName} failed; the trait keeps its XML profile and effects. {e}");
        }
    }

    // VFEP's tags with ManualDumb swapped for Cleaning and every tag an openable work type
    // carries removed; removed reports which. The swap: ManualDumb covers only Hauling and
    // Cleaning, and VFEP's hauling exemption is a Harmony postfix keyed on its own def, so a
    // copied ManualDumb would lose hauling. Pure, for the tests.
    public static WorkTags ResolveTags(WorkTags vfep, WorkTags openable, out WorkTags removed)
    {
        WorkTags tags = vfep;
        if ((tags & WorkTags.ManualDumb) != 0)
            tags = (tags & ~WorkTags.ManualDumb) | WorkTags.Cleaning;
        removed = tags & openable;
        return tags & ~removed;
    }

    private static void MirrorFromVfep()
    {
        TraitDef vfep = DefDatabase<TraitDef>.GetNamedSilentFail(VfepTraitName);
        TraitDef ours = DefDatabase<TraitDef>.GetNamedSilentFail(OverlordTraitName);
        if (vfep == null || ours == null)
        {
            Log.Error($"[Overlord Warcasket] Cannot mirror the overlord warcasket trait: {(vfep == null ? VfepTraitName : OverlordTraitName)} is missing. VFE Pirates is a hard dependency; this is a broken install.");
            return;
        }

        // The openable work types the loaded game has, each with its default.
        var openable = new Dictionary<WorkTypeDef, bool>();
        foreach (WorkTypeDef work in WorkTypeDefsUtility.WorkTypeDefsInPriorityOrder)
        {
            foreach ((string defName, bool open) in Openable)
            {
                if (work.defName == defName)
                    openable[work] = open;
            }
        }
        WorkTags openableTags = WorkTags.None;
        foreach (WorkTypeDef work in openable.Keys)
            openableTags |= work.workTags;

        WorkTags tags = ResolveTags(vfep.disabledWorkTags, openableTags, out WorkTags removed);
        ours.disabledWorkTags = tags;

        // Work types: VFEP's own list by value, never aliased, plus every work type the removed
        // tags no longer block, split into the base the settings never touch and the toggleables.
        var vfepTypes = new List<WorkTypeDef>(vfep.disabledWorkTypes ?? Enumerable.Empty<WorkTypeDef>());
        var types = new List<WorkTypeDef>();
        foreach (WorkTypeDef work in WorkTypeDefsUtility.WorkTypeDefsInPriorityOrder)
        {
            bool vfepBlocks = (vfep.disabledWorkTags & work.workTags) != 0 || vfepTypes.Contains(work);
            if (openable.TryGetValue(work, out bool open))
            {
                toggleable.Add(work);
                if (open || !vfepBlocks)
                    openByDefault.Add(work);
            }
            else if ((removed & work.workTags) != 0 || vfepTypes.Contains(work))
            {
                types.Add(work);
            }
        }
        baseTypes = types;
        toggleable = toggleable.OrderBy(work => !openByDefault.Contains(work)).ToList();
        overlord = ours;
        Write(OpenByDefault);

        // Effects: full overwrites, nulls included, so a field VFEP leaves at its default
        // replaces whatever our XML carried and the Overlord matches VFEP's wearers on the
        // running version (1.7 has the pain factor and rest-fall on the degree; 1.6 has the pain
        // factor on the extension and nothing on the degree).
        if (vfep.degreeDatas.Count > 0 && ours.degreeDatas.Count > 0)
            CopyPublicFields(vfep.degreeDatas[0], ours.degreeDatas[0], DegreeOwnFields);
        else
            Log.Warning($"[Overlord Warcasket] {VfepTraitName} or {OverlordTraitName} has no degree; the overlord trait keeps its XML effects.");

        TraitExtension vfepExtension = vfep.GetModExtension<TraitExtension>();
        TraitExtension ourExtension = ours.GetModExtension<TraitExtension>();
        if (vfepExtension != null && ourExtension != null)
            CopyPublicFields(vfepExtension, ourExtension, null);
        else
            Log.Warning($"[Overlord Warcasket] {VfepTraitName} or {OverlordTraitName} has no VEF TraitExtension; the overlord trait keeps its XML extension.");

        Log.Message($"[Overlord Warcasket] Overlord warcasket trait mirrored from {VfepTraitName}: disabled work tags {tags}; {Describe(toggleable)} open to settings, {Describe(openByDefault)} open by default.");
    }

    // Writes the profile the settings choose, open deciding each toggleable work type, and
    // refreshes every live pawn carrying the trait: Pawn.GetDisabledWorkTypes is cached per
    // pawn, and its Notify_DisabledWorkTypesChanged clears the cache and zeroes the work
    // priorities the write blocked (a type it opened stays unassigned until the player sets
    // it). World pawns too, since a pilot away on a quest comes back with the cache it left
    // with and nothing on its return clears it before the next save.
    public static void Apply(Func<WorkTypeDef, bool> open)
    {
        if (overlord == null)
            return;
        Write(open);
        Log.Message($"[Overlord Warcasket] Overlord warcasket trait disabled work types: {Describe(overlord.disabledWorkTypes)}.");
        if (Current.ProgramState != ProgramState.Playing)
            return;
        foreach (Pawn pawn in PawnsFinder.AllMapsWorldAndTemporary_Alive)
        {
            if (pawn.story?.traits != null && pawn.story.traits.HasTrait(overlord))
                pawn.Notify_DisabledWorkTypesChanged();
        }
    }

    private static void Write(Func<WorkTypeDef, bool> open)
    {
        var types = new List<WorkTypeDef>(baseTypes);
        foreach (WorkTypeDef work in toggleable)
        {
            if (!open(work))
                types.Add(work);
        }
        overlord.disabledWorkTypes = types;
    }

    private static string Describe(IEnumerable<WorkTypeDef> works) =>
        works.Any() ? works.Select(work => work.defName).ToCommaList() : "none";

    // Copies every public instance field of T from source onto target by reflection, so a field
    // T gains in a later game or VEF version is carried without a code change. Enumerated on T,
    // not the source's runtime type, so a patch that subclasses VFEP's degree or extension still
    // copies cleanly. List<T> values are copied into new lists so the two defs never share one;
    // the elements (StatModifiers, curves) stay shared, which is fine as nothing mutates them.
    private static void CopyPublicFields<T>(T source, T target, ICollection<string> skip)
    {
        foreach (FieldInfo field in typeof(T).GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            if (skip != null && skip.Contains(field.Name))
                continue;
            object value = field.GetValue(source);
            if (value is IList list && field.FieldType.IsGenericType && field.FieldType.GetGenericTypeDefinition() == typeof(List<>))
            {
                var copy = (IList)Activator.CreateInstance(field.FieldType);
                foreach (object item in list)
                    copy.Add(item);
                value = copy;
            }
            field.SetValue(target, value);
        }
    }
}
