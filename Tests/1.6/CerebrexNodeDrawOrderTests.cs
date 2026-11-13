#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Xunit;

namespace OverlordWarcasket.Tests;

// The cerebrex draw order patch (1.6/Mods/Odyssey/Patches/CerebrexNodeDrawOrder.xml) puts
// Odyssey's worn pack between a VFEP warcasket's armor and its shoulder pads when the pawn faces
// north, and only runs with Odyssey active, so each assumption it rests on would break silently:
// a def's drawData replaces vanilla's default whole, so the patch must restate vanilla's pack
// layers (93 north, -3 south) and nothing else; Odyssey's def must still carry no drawData of its
// own, or the add would write a second node; and the pack's north altitude (layer times
// AltitudeForLayer's step plus the patch's offset y) must clear the armor and stay under the
// shoulders in every VFEP version folder, whose abstract bases order the pieces by drawData
// offset y, and for our two defs, which inherit those bases. Pure XML and arithmetic: the VFEP
// folder and the RimWorld install are read off disk, and without them the tests fail and say so.
public class CerebrexNodeDrawOrderTests
{
    private const string PatchPath = "1.6/Mods/Odyssey/Patches/CerebrexNodeDrawOrder.xml";
    private const string NodeDefName = "Apparel_CerebrexNode";
    private const string ArmorBase = "VFEP_WarcasketArmorBase";
    private const string ShoulderBase = "VFEP_WarcasketShoulderPadBase";
    private const string OurArmor = "OLWC_Warcasket_Overlord";
    private const string OurShoulders = "OLWC_WarcasketShoulders_Overlord";

    // PawnRenderUtility.AltitudeForLayer's step per layer unit.
    private const double LayerStep = 0.0003658537;
    // The pack's own north layer, vanilla's default for a utility pack.
    private const int PackNorthLayer = 93;
    // The body apparel parent's base layer; a piece's layer is this plus its index among the
    // pawn's body apparel.
    private const int BodyApparelBaseLayer = 20;
    // A generous cap on the body apparel pieces a pawn wears at once.
    private const int MaxBodyApparelPieces = 10;

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

    private static string? RimWorldPath()
    {
        string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        string?[] candidates =
        [
            Environment.GetEnvironmentVariable("RIMWORLD_PATH"),
            @"C:\Program Files (x86)\Steam\steamapps\common\RimWorld",
            "/mnt/c/Program Files (x86)/Steam/steamapps/common/RimWorld",
            Path.Combine(home, ".local/share/Steam/steamapps/common/RimWorld"),
            Path.Combine(home, "Library/Application Support/Steam/steamapps/common/RimWorld"),
        ];
        return candidates.FirstOrDefault(c => !string.IsNullOrEmpty(c) && Directory.Exists(Path.Combine(c, "Data")));
    }

    private static string VfepPath()
    {
        var install = RimWorldPath();
        var candidates = new List<string?>
        {
            Environment.GetEnvironmentVariable("VFEP_PATH"),
            Path.Combine(RepoRoot, "..", "VanillaExpanded", "VanillaFactionsExpanded-Pirates"),
        };
        if (install != null)
        {
            candidates.Add(Path.Combine(install, "Mods", "VanillaFactionsExpanded-Pirates"));
            candidates.Add(Path.Combine(install, "..", "..", "workshop", "content", "294100", "2723801948"));
        }
        var found = candidates.FirstOrDefault(c => !string.IsNullOrEmpty(c) && Directory.Exists(Path.Combine(c, "1.6", "Defs")));
        Assert.True(found != null, "no VFEP checkout found (needs a 1.6/Defs folder): set VFEP_PATH");
        return found!;
    }

    public static TheoryData<string> VfepVersions()
    {
        var data = new TheoryData<string>();
        foreach (var version in new[] { "1.6", "1.7" })
        {
            if (Directory.Exists(Path.Combine(VfepPath(), version, "Defs")))
                data.Add(version);
        }
        return data;
    }

    private static IEnumerable<XElement> DefsIn(string folder) =>
        Directory.EnumerateFiles(folder, "*.xml", SearchOption.AllDirectories)
            .SelectMany(f => XDocument.Load(f).Root!.Elements());

    private static XElement Patch() => XDocument.Load(Path.Combine(RepoRoot, PatchPath)).Root!;

    private static XElement PatchDrawData()
    {
        var all = Patch().Descendants("drawData").ToList();
        Assert.True(all.Count == 1, $"{PatchPath} should hold exactly one drawData, found {all.Count}");
        return all[0];
    }

    private static double[] ParseVector(string text)
    {
        var parts = text.Trim().Trim('(', ')').Split(',')
            .Select(p => double.Parse(p.Trim(), CultureInfo.InvariantCulture))
            .ToArray();
        Assert.True(parts.Length == 3, $"'{text}' is not a 3-vector");
        return parts;
    }

    private static double? NorthY(XElement def)
    {
        var offset = def.Element("apparel")?.Element("drawData")?.Element("dataNorth")?.Element("offset");
        return offset == null ? null : ParseVector(offset.Value)[1];
    }

    private static double PatchNorthY()
    {
        var offset = PatchDrawData().Element("dataNorth")?.Element("offset");
        Assert.True(offset != null, $"{PatchPath} has no dataNorth/offset");
        return ParseVector(offset!.Value)[1];
    }

    private static double Node() => PackNorthLayer * LayerStep + PatchNorthY();

    private static double BaseNorthY(string vfepFolder, string version, string name)
    {
        var bases = DefsIn(Path.Combine(vfepFolder, version, "Defs"))
            .Where(d => (string?)d.Attribute("Name") == name)
            .ToList();
        Assert.True(bases.Count == 1, $"VFEP {version}: expected one def with Name '{name}', found {bases.Count}");
        var y = NorthY(bases[0]);
        Assert.True(y != null, $"VFEP {version}: '{name}' has no apparel/drawData/dataNorth/offset");
        return y!.Value;
    }

    private static double OurNorthY(string defName, double baseY)
    {
        var defs = DefsIn(Path.Combine(RepoRoot, "1.6", "Defs"))
            .Where(d => (string?)d.Element("defName") == defName)
            .ToList();
        Assert.True(defs.Count == 1, $"expected one def '{defName}' in 1.6/Defs, found {defs.Count}");
        return NorthY(defs[0]) ?? baseY;
    }

    private static void AssertBetween(double armorY, double shoulderY, string who)
    {
        double node = Node();
        double floor = armorY + (BodyApparelBaseLayer + MaxBodyApparelPieces) * LayerStep;
        double ceiling = shoulderY + BodyApparelBaseLayer * LayerStep;
        Assert.True(floor < node, $"{who}: pack at {node:F6} must clear the armor's highest altitude {floor:F6}");
        Assert.True(node < ceiling, $"{who}: pack at {node:F6} must stay under the shoulders' lowest altitude {ceiling:F6}");
    }

    [Fact]
    public void PatchRestatesVanillasPackLayersAndNothingElse()
    {
        var data = PatchDrawData();
        Assert.Equal("93", data.Element("dataNorth")?.Element("layer")?.Value.Trim());
        Assert.Equal("-3", data.Element("dataSouth")?.Element("layer")?.Value.Trim());
        Assert.Null(data.Element("dataEast"));
        Assert.Null(data.Element("dataWest"));
        Assert.Null(data.Element("defaultData"));
        Assert.Null(data.Element("dataSouth")?.Element("offset"));
        var north = ParseVector(data.Element("dataNorth")?.Element("offset")?.Value ?? "");
        Assert.Equal(0, north[0]);
        Assert.Equal(0, north[2]);
    }

    [Fact]
    public void OdysseysNodeStillHasNoDrawDataOfItsOwn()
    {
        var install = RimWorldPath();
        Assert.True(install != null, "no RimWorld install was found: set RIMWORLD_PATH");
        var defs = DefsIn(Path.Combine(install!, "Data", "Odyssey", "Defs"))
            .Where(d => d.Name == "ThingDef" && (string?)d.Element("defName") == NodeDefName)
            .ToList();
        Assert.True(defs.Count == 1, $"expected one ThingDef '{NodeDefName}' in Odyssey, found {defs.Count}");
        var apparel = defs[0].Element("apparel");
        Assert.Equal("true", apparel?.Element("wornGraphicData")?.Element("renderUtilityAsPack")?.Value.Trim());
        Assert.True(apparel?.Element("drawData") == null,
            $"Odyssey's {NodeDefName} now ships its own apparel/drawData: the patch's PatchOperationAdd would write a duplicate node and its restated layers 93 and -3 may be stale");
    }

    [Theory]
    [MemberData(nameof(VfepVersions))]
    public void NorthAltitudeSitsBetweenVfepsArmorAndShoulders(string version)
    {
        var vfep = VfepPath();
        AssertBetween(BaseNorthY(vfep, version, ArmorBase), BaseNorthY(vfep, version, ShoulderBase), $"VFEP {version}");
    }

    [Fact]
    public void NorthAltitudeSitsBetweenOurArmorAndShoulders()
    {
        var vfep = VfepPath();
        AssertBetween(
            OurNorthY(OurArmor, BaseNorthY(vfep, "1.6", ArmorBase)),
            OurNorthY(OurShoulders, BaseNorthY(vfep, "1.6", ShoulderBase)),
            "Overlord set");
    }
}
