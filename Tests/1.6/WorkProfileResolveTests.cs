using System.Collections.Generic;
using System.Linq;
using Verse;
using Xunit;

namespace OverlordWarcasket.Tests;

// The trait mirror's work type rule (WorkProfile.Resolve): which work types the default profile
// leaves a pilot, which it re-blocks by name, and how a patched VFEP trait moves the defaults.
// The fixture quotes the work types of Core, Biotech, Anomaly and Odyssey on 1.7 with their
// tags, in the work tab's order, and VFEP's shipped trait; work types stand in by name, which
// Resolve allows by being generic. A pawn's verdict on a work type is the game's own
// (Trait.AllowsWorkType and Trait.GetDisabledWorkTypes): blocked on any shared tag, or by name.
public class WorkProfileResolveTests
{
    private const WorkTags Common = WorkTags.Commoner | WorkTags.AllWork;

    private const WorkTags Vfep = WorkTags.ManualSkilled | WorkTags.ManualDumb | WorkTags.Animals
        | WorkTags.Artistic | WorkTags.Caring | WorkTags.Intellectual;

    private static readonly (string name, WorkTags tags)[] Vanilla =
    [
        ("Firefighter", WorkTags.Firefighting | Common),
        ("Patient", WorkTags.None),
        ("Doctor", WorkTags.Caring | Common),
        ("PatientBedRest", WorkTags.None),
        ("Childcare", WorkTags.Social | WorkTags.Caring | WorkTags.AllWork),
        ("BasicWorker", Common),
        ("Warden", WorkTags.Social | WorkTags.AllWork),
        ("Handling", WorkTags.Animals | Common),
        ("Cooking", WorkTags.Cooking | WorkTags.ManualSkilled | Common),
        ("Hunting", WorkTags.Violent | WorkTags.Hunting | WorkTags.Shooting | Common),
        ("Construction", WorkTags.ManualSkilled | WorkTags.Constructing | Common),
        ("Growing", WorkTags.ManualSkilled | WorkTags.PlantWork | Common),
        ("Mining", WorkTags.ManualSkilled | WorkTags.Mining | Common),
        ("PlantCutting", WorkTags.ManualSkilled | WorkTags.PlantWork | Common),
        ("Smithing", WorkTags.Crafting | WorkTags.ManualSkilled | Common),
        ("Tailoring", WorkTags.Crafting | WorkTags.ManualSkilled | Common),
        ("Art", WorkTags.Artistic | Common),
        ("Crafting", WorkTags.Crafting | WorkTags.ManualSkilled | Common),
        ("Fishing", WorkTags.Animals | Common),
        ("Hauling", WorkTags.ManualDumb | WorkTags.Hauling | Common),
        ("Cleaning", WorkTags.ManualDumb | WorkTags.Cleaning | Common),
        ("DarkStudy", WorkTags.Intellectual | Common),
        ("Research", WorkTags.Intellectual | Common),
    ];

    private static WorkProfile.Resolved<string> Resolve(
        WorkTags vfepTags,
        IEnumerable<(string name, WorkTags tags)> workTypes = null,
        params string[] vfepTypes)
    {
        Dictionary<string, WorkTags> tags = (workTypes ?? Vanilla).ToDictionary(work => work.name, work => work.tags);
        return WorkProfile.Resolve(tags.Keys.ToList(), name => tags[name], WorkProfile.OpenableDefault, vfepTags, vfepTypes);
    }

    // The work types a pilot can be assigned under the default profile: every one the trait's
    // tags do not touch and neither the base nor a blocked toggleable names.
    private static List<string> OpenByDefault(
        WorkProfile.Resolved<string> profile,
        IEnumerable<(string name, WorkTags tags)> workTypes = null) =>
        (workTypes ?? Vanilla)
            .Where(work => (profile.Tags & work.tags) == 0
                && !profile.BaseTypes.Contains(work.name)
                && (!profile.Toggleable.Contains(work.name) || profile.OpenByDefault.Contains(work.name)))
            .Select(work => work.name)
            .ToList();

    // Research and Smithing join what a VFEP pilot keeps; with them the Intellectual and
    // Crafting skills stay alive, which hacking, chip study and the mech work's stats need.
    [Fact]
    public void DefaultProfileOpensResearchAndSmithingOnly()
    {
        WorkProfile.Resolved<string> profile = Resolve(Vfep);
        Assert.Equal(
            ["Firefighter", "Patient", "PatientBedRest", "BasicWorker", "Warden", "Hunting", "Smithing", "Hauling", "Research"],
            OpenByDefault(profile));
    }

    // A trait blocks a work type on any shared tag, so no tag of an openable work type may
    // stay: the Crafting tag in particular, which Smithing shares with Crafting and Tailoring.
    [Fact]
    public void NoTagOfAnOpenableWorkTypeStaysOnTheTrait()
    {
        WorkTags patched = Vfep | WorkTags.Crafting | WorkTags.Commoner;
        WorkProfile.Resolved<string> profile = Resolve(patched);
        Assert.Equal(WorkTags.Cleaning | WorkTags.Animals | WorkTags.Artistic | WorkTags.Caring, profile.Tags);
        Assert.Contains("Smithing", OpenByDefault(profile));
        Assert.Contains("Research", OpenByDefault(profile));
    }

    [Fact]
    public void ReblocksByNameWhatTheFreedTagsCovered()
    {
        WorkProfile.Resolved<string> profile = Resolve(Vfep);
        Assert.Equal(["Cooking", "Construction", "Growing", "Mining", "PlantCutting"], profile.BaseTypes);
        Assert.Equal(["Smithing", "Research", "Tailoring", "Crafting", "DarkStudy"], profile.Toggleable);
        Assert.Equal(["Smithing", "Research"], profile.OpenByDefault.OrderBy(name => name == "Research"));
    }

    // A modded skilled work type is no business of the suit's and stays blocked.
    [Fact]
    public void ReblocksAModdedWorkTypeTheFreedTagsCovered()
    {
        var modded = Vanilla.Append(("Brewing", WorkTags.ManualSkilled | Common)).ToList();
        WorkProfile.Resolved<string> profile = Resolve(Vfep, modded);
        Assert.Contains("Brewing", profile.BaseTypes);
        Assert.DoesNotContain("Brewing", OpenByDefault(profile, modded));
    }

    // "Warcaskets can work!" style patches: once VFEP's trait no longer blocks skilled labor,
    // the skilled toggleables are open by default and nothing skilled is re-blocked; Dark study
    // follows the Intellectual tag, which this patch leaves blocked.
    [Fact]
    public void OpensTheSkilledToggleablesOnceVfepOpensSkilledLabor()
    {
        WorkProfile.Resolved<string> profile = Resolve(Vfep & ~WorkTags.ManualSkilled);
        Assert.Empty(profile.BaseTypes);
        Assert.Contains("Crafting", profile.OpenByDefault);
        Assert.Contains("Tailoring", profile.OpenByDefault);
        Assert.DoesNotContain("DarkStudy", profile.OpenByDefault);
        Assert.Contains("Cooking", OpenByDefault(profile));
    }

    // A patch that blocks work types by name on VFEP's trait: the suit's own two stay open and
    // toggleable, a blocked toggleable stays blocked, and every other named type is carried.
    [Fact]
    public void KeepsItsOwnWorkOpenWhenVfepBlocksByName()
    {
        WorkProfile.Resolved<string> profile = Resolve(Vfep & ~WorkTags.ManualSkilled, null, "Smithing", "Crafting", "Warden");
        Assert.Contains("Smithing", profile.OpenByDefault);
        Assert.DoesNotContain("Crafting", profile.OpenByDefault);
        Assert.Equal(["Warden"], profile.BaseTypes);
        Assert.DoesNotContain("Warden", OpenByDefault(profile));
    }

    [Fact]
    public void OnlyTheFiveWorkTypesAreOpenable()
    {
        Assert.True(WorkProfile.OpenableDefault("Research"));
        Assert.True(WorkProfile.OpenableDefault("Smithing"));
        Assert.False(WorkProfile.OpenableDefault("Crafting"));
        Assert.False(WorkProfile.OpenableDefault("Tailoring"));
        Assert.False(WorkProfile.OpenableDefault("DarkStudy"));
        Assert.Null(WorkProfile.OpenableDefault("Cooking"));
    }
}
