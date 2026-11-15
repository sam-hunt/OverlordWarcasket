using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Xunit;

namespace OverlordWarcasket.Tests;

// The two halves of keeping the set off generated pawns. ListMask is what
// Patches/PawnGenerator_GeneratePawn_Patch_Postfix hides the parts from VFEP's slot fill with:
// the foundry reads the same lists, so a restore must leave them exactly as they were. And the
// parts carry no apparel tags, which is what keeps vanilla's gear roll from picking them.
public class ListMaskTests
{
    private static bool Ours(string name) => name.StartsWith("OLWC_");

    [Fact]
    public void Hide_TakesEveryMatchOutAndLeavesTheRestInOrder()
    {
        var list = new List<string> { "OLWC_A", "VFEP_1", "OLWC_B", "VFEP_2", "OLWC_C" };
        var mask = new ListMask<string>();

        mask.Hide(list, Ours);

        Assert.Equal(new[] { "VFEP_1", "VFEP_2" }, list);
    }

    [Fact]
    public void Restore_PutsEveryItemBackWhereItWas()
    {
        var armors = new List<string> { "VFEP_1", "OLWC_A", "VFEP_2", "OLWC_B" };
        var helmets = new List<string> { "OLWC_C", "VFEP_3" };
        var untouched = new List<string> { "VFEP_4" };
        var mask = new ListMask<string>();

        mask.Hide(armors, Ours);
        mask.Hide(helmets, Ours);
        mask.Hide(untouched, Ours);
        mask.Restore();

        Assert.Equal(new[] { "VFEP_1", "OLWC_A", "VFEP_2", "OLWC_B" }, armors);
        Assert.Equal(new[] { "OLWC_C", "VFEP_3" }, helmets);
        Assert.Equal(new[] { "VFEP_4" }, untouched);
    }

    // The patch's finalizer restores after every call, masked or not, and the next call hides
    // afresh: a restore leaves nothing behind to put back twice.
    [Fact]
    public void Restore_IsSpentAfterOneUse()
    {
        var list = new List<string> { "OLWC_A", "VFEP_1" };
        var mask = new ListMask<string>();

        mask.Hide(list, Ours);
        mask.Restore();
        mask.Restore();
        mask.Hide(list, Ours);
        mask.Restore();

        Assert.Equal(new[] { "OLWC_A", "VFEP_1" }, list);
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
