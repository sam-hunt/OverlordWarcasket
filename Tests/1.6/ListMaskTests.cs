using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Xunit;

namespace OverlordWarcasket.Tests;

// The two halves of keeping the set off generated pawns. ListMask is what
// Patches/PawnGenerator_GeneratePawn_Patch_Postfix hides the parts from VFEP's slot fill with:
// the foundry reads the same lists, so a restore must leave them exactly as they were, also
// while a sibling mod's mask of its own parts is at work on them. And the parts carry no apparel
// tags, which is what keeps vanilla's gear roll from picking them.
//
// A name here ends in its rank, as a def carries its DefDatabase index.
public class ListMaskTests
{
    private static bool Ours(string name) => name.StartsWith("OLWC_");

    private static bool Theirs(string name) => name.StartsWith("HPWC_");

    private static int Rank(string name) => int.Parse(name.Substring(name.IndexOf('_') + 1));

    [Fact]
    public void Hide_TakesEveryMatchOutAndLeavesTheRestInOrder()
    {
        var list = new List<string> { "OLWC_0", "VFEP_1", "OLWC_2", "VFEP_3", "OLWC_4" };
        var mask = new ListMask<string>(Rank);

        mask.Hide(list, Ours);

        Assert.Equal(new[] { "VFEP_1", "VFEP_3" }, list);
    }

    [Fact]
    public void Restore_PutsEveryItemBackWhereItWas()
    {
        var armors = new List<string> { "VFEP_0", "OLWC_1", "VFEP_2", "OLWC_3" };
        var helmets = new List<string> { "OLWC_4", "VFEP_5" };
        var untouched = new List<string> { "VFEP_6" };
        var mask = new ListMask<string>(Rank);

        mask.Hide(armors, Ours);
        mask.Hide(helmets, Ours);
        mask.Hide(untouched, Ours);
        mask.Restore();

        Assert.Equal(new[] { "VFEP_0", "OLWC_1", "VFEP_2", "OLWC_3" }, armors);
        Assert.Equal(new[] { "OLWC_4", "VFEP_5" }, helmets);
        Assert.Equal(new[] { "VFEP_6" }, untouched);
    }

    // The patch's finalizer restores after every call, masked or not, and the next call hides
    // afresh: a restore leaves nothing behind to put back twice.
    [Fact]
    public void Restore_IsSpentAfterOneUse()
    {
        var list = new List<string> { "OLWC_0", "VFEP_1" };
        var mask = new ListMask<string>(Rank);

        mask.Hide(list, Ours);
        mask.Restore();
        mask.Restore();
        mask.Hide(list, Ours);
        mask.Restore();

        Assert.Equal(new[] { "OLWC_0", "VFEP_1" }, list);
    }

    // Two mods masking the same lists around the same call. Harmony runs their finalizers in
    // the order of their prefixes, so the first to hide is the first to restore, into a list
    // the other still has its parts out of. Every generated pawn is one round, and the lists
    // must come through any number of them: a restore that is one place off shows on the next.
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Restore_PutsEveryItemBackWhileAnotherMaskOverlaps(bool oursFirst)
    {
        var armors = new List<string> { "VFEP_0", "OLWC_1", "HPWC_2" };
        var helmets = new List<string> { "HPWC_3", "VFEP_4", "OLWC_5" };
        var shoulders = new List<string> { "OLWC_6", "HPWC_7", "OLWC_8", "HPWC_9", "VFEP_10" };
        var lists = new[] { armors, helmets, shoulders };
        var expected = lists.Select(list => list.ToArray()).ToArray();
        var ours = new ListMask<string>(Rank);
        var theirs = new ListMask<string>(Rank);
        var first = oursFirst ? (mask: ours, match: (System.Predicate<string>)Ours) : (mask: theirs, match: Theirs);
        var second = oursFirst ? (mask: theirs, match: (System.Predicate<string>)Theirs) : (mask: ours, match: Ours);

        for (var round = 0; round < 3; round++)
        {
            foreach (var list in lists)
                first.mask.Hide(list, first.match);
            foreach (var list in lists)
                second.mask.Hide(list, second.match);

            Assert.All(lists, list => Assert.All(list, name => Assert.StartsWith("VFEP_", name)));

            first.mask.Restore();
            second.mask.Restore();

            for (var i = 0; i < lists.Length; i++)
                Assert.Equal(expected[i], lists[i]);
        }
    }

    // The other order, one mask's hide and restore inside the other's, is what a nested call
    // or a mod patching at another priority gives.
    [Fact]
    public void Restore_PutsEveryItemBackWhenAnotherMaskNests()
    {
        var list = new List<string> { "HPWC_0", "OLWC_1", "VFEP_2", "OLWC_3", "HPWC_4" };
        var ours = new ListMask<string>(Rank);
        var theirs = new ListMask<string>(Rank);

        for (var round = 0; round < 3; round++)
        {
            ours.Hide(list, Ours);
            theirs.Hide(list, Theirs);
            theirs.Restore();
            ours.Restore();

            Assert.Equal(new[] { "HPWC_0", "OLWC_1", "VFEP_2", "OLWC_3", "HPWC_4" }, list);
        }
    }

    [Fact]
    public void WarcasketParts_CarryNoApparelTags()
    {
        var folder = Path.Combine(FindRepoRoot(), "1.6", "Defs", "ThingDefs_Misc");
        var parts = Directory.EnumerateFiles(folder, "*.xml")
            .SelectMany(file => XDocument.Load(file).Root!.Elements("VFEPirates.WarcasketDef"))
            .ToList();

        Assert.Equal(3, parts.Count);
        Assert.All(parts, part => Assert.Empty(part.Elements("apparel").Elements("tags")));
    }

    private static string FindRepoRoot()
    {
        for (var dir = new DirectoryInfo(System.AppDomain.CurrentDomain.BaseDirectory); dir != null; dir = dir.Parent)
        {
            if (File.Exists(Path.Combine(dir.FullName, "OverlordWarcasket.sln")))
                return dir.FullName;
        }
        throw new System.InvalidOperationException("OverlordWarcasket.sln not found above the test output folder");
    }
}
