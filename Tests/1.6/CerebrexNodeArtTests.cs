using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Xunit;

namespace OverlordWarcasket.Tests;

// The cerebrex art patch (1.6/Mods/Odyssey/Patches/CerebrexNodeArt.xml) only runs with Odyssey
// active and a choice other than the default, so a drift between its loose ends would surface in
// game only for a player who opted in. Each branch names a setting and a choice: the setting
// must be a public field of the settings that a patch can read, as PatchOperationSetting looks
// it up (read off the settings' source, since the type itself pulls in VEF, which the test
// output never carries), and the choice one of CerebrexArt's names; every choice but Odyssey's
// own art, which is the patch not running, must have exactly one branch. A branch must write
// its choice's path, CerebrexNode.TexPath or RingTexPath, which the settings window reads off
// the def, onto the item and the worn pack alike, and turn the two mask switches on exactly when
// the choice is a masked one, since a mask the def never asks for is never read and a mask
// shader with no mask file paints nothing. Each path must have its four textures in the Odyssey
// art root, the item and the worn pack's three facings (the game flips east for west), and each
// of the four its mask beside it. The two folders must carry the same art and different masks,
// the ring folder being a copy made only because the worn pack finds its masks by name. And the
// path the picker draws Odyssey's own art from must be the one Odyssey's def ships with.
public class CerebrexNodeArtTests
{
    private const string PatchPath = "1.6/Mods/Odyssey/Patches/CerebrexNodeArt.xml";
    private const string ArtRoot = "Mods/Odyssey/Textures";
    private const string SettingOperation = "OverlordWarcasket.PatchOperationSetting";
    private const string SettingsSource = "Source/1.6/Settings/OverlordWarcasketSettings.cs";
    private const string ArtSetting = "CerebrexArtSetting";
    private static readonly Regex SettingField = new(@"public\s+readonly\s+(BoolSetting|CerebrexArtSetting)\s+(\w+)\s*=", RegexOptions.Compiled);

    private static readonly string[] ArtSuffixes = ["", "_north", "_east", "_south"];
    private static readonly string[] MaskSuffixes = ["_m", "_northm", "_eastm", "_southm"];

    private static readonly string RepoRoot = FindRepoRoot();

    private static string FindRepoRoot()
    {
        for (var dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory); dir != null; dir = dir.Parent)
        {
            if (File.Exists(Path.Combine(dir.FullName, "OverlordWarcasket.sln")))
                return dir.FullName;
        }
        throw new InvalidOperationException("OverlordWarcasket.sln not found above the test output folder");
    }

    private static List<XElement> Branches() => XDocument.Load(Path.Combine(RepoRoot, PatchPath)).Descendants()
        .Where(e => (string)e.Attribute("Class") == SettingOperation)
        .ToList();

    private static XElement Branch(string choice) => Assert.Single(Branches(), b => b.Element("choice")?.Value == choice);

    private static string TextureFile(string texPath, string suffix) => Path.Combine(RepoRoot, ArtRoot, texPath + suffix + ".png");

    [Fact]
    public void EveryBranchNamesAPatchableSettingAndOneOfItsChoices()
    {
        var fields = SettingField.Matches(File.ReadAllText(Path.Combine(RepoRoot, SettingsSource)))
            .Cast<Match>()
            .ToDictionary(m => m.Groups[2].Value, m => m.Groups[1].Value);
        Assert.NotEmpty(fields);
        var branches = Branches();
        Assert.NotEmpty(branches);
        foreach (var branch in branches)
        {
            string name = branch.Element("setting")?.Value ?? "";
            string choice = branch.Element("choice")?.Value;
            Assert.True(fields.TryGetValue(name, out var type), $"{PatchPath}: '{name}' is not a public setting field a patch can read in {SettingsSource}");
            string[] choices = type == ArtSetting ? Enum.GetNames(typeof(CerebrexArt)) : ["true", "false"];
            Assert.True(choice == null ? type != ArtSetting : choices.Contains(choice), $"{PatchPath}: '{name}' has no choice '{choice}'");
        }
    }

    [Fact]
    public void EveryChoiceButOdysseysHasOneBranch()
    {
        var expected = Enum.GetNames(typeof(CerebrexArt)).Where(n => n != nameof(CerebrexArt.Odyssey)).OrderBy(n => n);
        var written = Branches().Select(b => b.Element("choice")?.Value).OrderBy(n => n);
        Assert.Equal(expected, written);
    }

    [Theory]
    [InlineData(nameof(CerebrexArt.DyeAll), CerebrexNode.TexPath)]
    [InlineData(nameof(CerebrexArt.DyeFrame), CerebrexNode.TexPath)]
    [InlineData(nameof(CerebrexArt.DyeFrameAndRing), CerebrexNode.RingTexPath)]
    public void BranchWritesItsPathOnTheItemAndThePack(string choice, string texPath)
    {
        var values = Branch(choice).Descendants("value").Elements()
            .Where(e => e.Name.LocalName is "texPath" or "wornGraphicPath")
            .ToList();
        Assert.Equal(["texPath", "wornGraphicPath"], values.Select(e => e.Name.LocalName).OrderBy(n => n));
        Assert.All(values, value => Assert.Equal(texPath, value.Value));
    }

    // The item's graphic reads its mask under a mask shader (graphicData/shaderType) and the worn
    // pack's under the apparel's own flag; a masked branch writes each once, under the parent
    // that owns it, and the unmasked one neither.
    [Theory]
    [InlineData(nameof(CerebrexArt.DyeAll), false)]
    [InlineData(nameof(CerebrexArt.DyeFrame), true)]
    [InlineData(nameof(CerebrexArt.DyeFrameAndRing), true)]
    public void BranchSwitchesTheMasksOnOnlyWhenMasked(string choice, bool masked)
    {
        foreach (var (parent, field, expected) in new[] { ("graphicData", "shaderType", "CutoutComplex"), ("apparel", "useWornGraphicMask", "true") })
        {
            var written = Branch(choice).Descendants("value").Elements(field).ToList();
            if (!masked)
            {
                Assert.Empty(written);
                continue;
            }
            Assert.Single(written);
            Assert.Equal(expected, written[0].Value);
            var xpath = written[0].Parent!.Parent!.Element("xpath")?.Value ?? "";
            Assert.EndsWith("/" + parent, xpath);
        }
    }

    [Theory]
    [InlineData(CerebrexNode.TexPath)]
    [InlineData(CerebrexNode.RingTexPath)]
    public void ArtAndMasksShipAtTheWrittenPath(string texPath)
    {
        foreach (var suffix in ArtSuffixes.Concat(MaskSuffixes))
        {
            var file = TextureFile(texPath, suffix);
            Assert.True(File.Exists(file), $"missing {file.Substring(RepoRoot.Length + 1)}");
        }
    }

    [Fact]
    public void RingFolderCarriesTheSameArtAndItsOwnMasks()
    {
        foreach (var suffix in ArtSuffixes)
        {
            Assert.True(File.ReadAllBytes(TextureFile(CerebrexNode.TexPath, suffix)).SequenceEqual(File.ReadAllBytes(TextureFile(CerebrexNode.RingTexPath, suffix))),
                $"CerebrexNodePack{suffix}.png differs between the two art folders: the ring folder's is a copy");
        }
        foreach (var suffix in MaskSuffixes)
        {
            Assert.False(File.ReadAllBytes(TextureFile(CerebrexNode.TexPath, suffix)).SequenceEqual(File.ReadAllBytes(TextureFile(CerebrexNode.RingTexPath, suffix))),
                $"CerebrexNodePack{suffix}.png is the same mask in both art folders");
        }
    }

    // Odyssey's def from the local install, as CompatPatchTests reads it; without an install the
    // test fails and says so.
    [Fact]
    public void OdysseyTexPathIsTheOneOdysseysDefShipsWith()
    {
        var install = CompatPatchTests.RimWorldPath();
        Assert.True(install != null, "no RimWorld install was found to read Odyssey's cerebrex node from: set RIMWORLD_PATH");
        var def = Directory.EnumerateFiles(Path.Combine(install!, "Data", "Odyssey", "Defs"), "*.xml", SearchOption.AllDirectories)
            .SelectMany(file => XDocument.Load(file).Root!.Elements("ThingDef"))
            .Single(e => e.Element("defName")?.Value == CerebrexNode.DefName);
        Assert.Equal(CerebrexNode.OdysseyTexPath, def.Element("graphicData")?.Element("texPath")?.Value);
        Assert.Equal(CerebrexNode.OdysseyTexPath, def.Element("apparel")?.Element("wornGraphicPath")?.Value);
    }
}
