using Verse;
using Xunit;

namespace OverlordWarcasket.Tests;

// The trait mirror's tag arithmetic (WorkProfile.ResolveTags): the hauling swap and the removal
// of every tag an openable work type carries, reported back so the mirror can re-block by name.
// VFEP's shipped tags and vanilla's work type tags are quoted as the fixtures.
public class WorkProfileTagsTests
{
    private const WorkTags Vfep = WorkTags.ManualSkilled | WorkTags.ManualDumb | WorkTags.Animals
        | WorkTags.Artistic | WorkTags.Caring | WorkTags.Intellectual;

    // Smithing, Crafting and Tailoring carry Crafting, ManualSkilled, Commoner and AllWork;
    // Research and DarkStudy carry Intellectual, Commoner and AllWork.
    private const WorkTags Openable = WorkTags.Crafting | WorkTags.ManualSkilled | WorkTags.Intellectual
        | WorkTags.Commoner | WorkTags.AllWork;

    [Fact]
    public void SwapsManualDumbForCleaning()
    {
        WorkTags tags = WorkProfile.ResolveTags(Vfep, WorkTags.None, out WorkTags removed);
        Assert.Equal(WorkTags.None, removed);
        Assert.False((tags & WorkTags.ManualDumb) != 0);
        Assert.True((tags & WorkTags.Cleaning) != 0);
    }

    [Fact]
    public void RemovesOnlyTheOpenableTagsVfepBlocks()
    {
        WorkTags tags = WorkProfile.ResolveTags(Vfep, Openable, out WorkTags removed);
        Assert.Equal(WorkTags.ManualSkilled | WorkTags.Intellectual, removed);
        Assert.Equal(WorkTags.Cleaning | WorkTags.Animals | WorkTags.Artistic | WorkTags.Caring, tags);
    }

    // A patched VFEP trait that no longer blocks skilled or intellectual labor leaves nothing to
    // remove.
    [Fact]
    public void RemovesNothingVfepDoesNotBlock()
    {
        WorkTags tags = WorkProfile.ResolveTags(Vfep & ~(WorkTags.ManualSkilled | WorkTags.Intellectual), Openable, out WorkTags removed);
        Assert.Equal(WorkTags.None, removed);
        Assert.True((tags & WorkTags.Caring) != 0);
    }
}
