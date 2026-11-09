using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Xunit;

namespace OverlordWarcasket.Tests;

// The cerebrex art patch (1.6/Mods/Odyssey/Patches/CerebrexNodeArt.xml) only runs with Odyssey
// active and the setting on, so a drift between its loose ends would surface in game only for a
// player who opted in: the switch it names must be a public BoolSetting field of the settings,
// as PatchOperationSetting looks it up (read off the settings' source, since the type itself
// pulls in VEF, which the test output never carries); every path it writes must be
// CerebrexNode.TexPath, which the settings window reads off the def; and that path must have its
// four textures in the Odyssey art root, the item and the worn pack's three facings (the game
// flips east for west).
public class CerebrexNodeArtTests
{
    private const string PatchPath = "1.6/Mods/Odyssey/Patches/CerebrexNodeArt.xml";
    private const string ArtRoot = "Mods/Odyssey/Textures";
    private const string SettingOperation = "OverlordWarcasket.PatchOperationSetting";
    private const string SettingsSource = "Source/1.6/Settings/OverlordWarcasketSettings.cs";
    private static readonly Regex SwitchField = new(@"public\s+readonly\s+BoolSetting\s+(\w+)\s*=", RegexOptions.Compiled);

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

    private static XDocument Patch() => XDocument.Load(Path.Combine(RepoRoot, PatchPath));

    [Fact]
    public void SwitchIsAPublicBoolSettingOfTheSettings()
    {
        var names = Patch().Descendants()
            .Where(e => (string)e.Attribute("Class") == SettingOperation)
            .Select(e => e.Element("setting")?.Value)
            .ToList();
        Assert.NotEmpty(names);
        var switches = SwitchField.Matches(File.ReadAllText(Path.Combine(RepoRoot, SettingsSource)))
            .Cast<Match>()
            .Select(m => m.Groups[1].Value)
            .ToList();
        Assert.NotEmpty(switches);
        foreach (var name in names)
            Assert.True(switches.Contains(name ?? ""), $"{PatchPath}: '{name}' is not a public BoolSetting field in {SettingsSource}");
    }

    [Fact]
    public void EveryPathWrittenIsCerebrexNodeTexPath()
    {
        var values = Patch().Descendants("value").Elements().Select(e => e.Value).ToList();
        Assert.Equal(2, values.Count);
        Assert.All(values, value => Assert.Equal(CerebrexNode.TexPath, value));
    }

    [Theory]
    [InlineData("")]
    [InlineData("_north")]
    [InlineData("_east")]
    [InlineData("_south")]
    public void TextureShipsAtTheWrittenPath(string suffix)
    {
        var file = Path.Combine(RepoRoot, ArtRoot, CerebrexNode.TexPath + suffix + ".png");
        Assert.True(File.Exists(file), $"missing {file.Substring(RepoRoot.Length + 1)}");
    }
}
