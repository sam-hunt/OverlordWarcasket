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
// HackUtility.IsCapableOfHacking, which refuses an Intellectual-blocked pawn) and keeps the
// Intellectual skill alive for the mech boss chips (their study is no work giver but an order
// off the chip, and CompAnalyzable.CanInteract refuses a pawn whose ResearchSpeed stat is
// disabled, which a totally disabled Intellectual skill does; verified on 1.7); Smithing open,
// as Biotech files mech gestation, subcore encoding and the mechanitor-only mech repair there
// and Core the machining table, where mechanoids are shredded (the Crafting spot's smash recipe
// is Crafting work); Crafting, Tailoring and Anomaly's DarkStudy blocked until the player opens
// them, the last skipped silently without Anomaly. Resolve, pure and tested, sorts the loaded
// game's work types into the trait's tags, the types blocked by name whatever the settings say
// and the toggleables, with what the rule makes of each (OpenByDefault: its own default, or
// open whenever VFEP's trait blocks neither its tags nor the type itself). Mirror writes that
// default profile and Apply the profile the settings choose, on every ApplyToDefs. A pawn
// caches its disabled work types, so Apply refreshes every live pawn carrying the trait; at
// startup there is none.
//
// No tag an openable work type carries may stay on the trait: a trait blocks a work type on any
// shared tag (Trait.AllowsWorkType), and Smithing carries the Crafting tag as well as
// ManualSkilled, so a Crafting tag left behind would close Smithing and, with Crafting and
// Tailoring blocked, disable the Crafting skill the mech work's speed stats need.
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

    // The fixed rule's default for an openable work type, by defName; null for every other.
    public static bool? OpenableDefault(string defName)
    {
        foreach ((string name, bool open) in Openable)
        {
            if (name == defName)
                return open;
        }
        return null;
    }

    // What Resolve makes of a game's work types: the trait's tags, the work types blocked by
    // name whatever the settings say, and the toggleables with those the fixed rule opens.
    public sealed class Resolved<T>
    {
        public WorkTags Tags { get; set; }
        public List<T> BaseTypes { get; } = [];
        public List<T> Toggleable { get; set; } = [];
        public HashSet<T> OpenByDefault { get; } = [];
    }

    // Sorts workTypes, in the work tab's order, against VFEP's trait: its tags through
    // ResolveTags with every openable work type's tags as the ones to free; every work type the
    // freed tags covered that is not openable, and every one VFEP lists by name, into the base;
    // each openable one into the toggleables, open by default when its own default says so or
    // VFEP blocks neither its tags nor the type, and those open by default first. Pure and
    // generic over the work type, for the tests.
    public static Resolved<T> Resolve<T>(
        IReadOnlyList<T> workTypes,
        Func<T, WorkTags> tagsOf,
        Func<T, bool?> openableDefault,
        WorkTags vfepTags,
        ICollection<T> vfepTypes)
    {
        WorkTags openableTags = WorkTags.None;
        foreach (T work in workTypes)
        {
            if (openableDefault(work) != null)
                openableTags |= tagsOf(work);
        }

        var profile = new Resolved<T> { Tags = ResolveTags(vfepTags, openableTags, out WorkTags removed) };
        foreach (T work in workTypes)
        {
            WorkTags workTags = tagsOf(work);
            bool vfepBlocks = (vfepTags & workTags) != 0 || vfepTypes.Contains(work);
            if (openableDefault(work) is bool open)
            {
                profile.Toggleable.Add(work);
                if (open || !vfepBlocks)
                    profile.OpenByDefault.Add(work);
            }
            else if ((removed & workTags) != 0 || vfepTypes.Contains(work))
            {
                profile.BaseTypes.Add(work);
            }
        }
        profile.Toggleable = profile.Toggleable.OrderBy(work => !profile.OpenByDefault.Contains(work)).ToList();
        return profile;
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

        // VFEP's own list by value, never aliased.
        var vfepTypes = new List<WorkTypeDef>(vfep.disabledWorkTypes ?? Enumerable.Empty<WorkTypeDef>());
        Resolved<WorkTypeDef> profile = Resolve(
            WorkTypeDefsUtility.WorkTypeDefsInPriorityOrder.ToList(),
            work => work.workTags,
            work => OpenableDefault(work.defName),
            vfep.disabledWorkTags,
            vfepTypes);
        ours.disabledWorkTags = profile.Tags;
        baseTypes = profile.BaseTypes;
        toggleable = profile.Toggleable;
        openByDefault = profile.OpenByDefault;
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
