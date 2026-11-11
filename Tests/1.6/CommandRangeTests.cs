using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Verse;
using Xunit;

namespace OverlordWarcasket.Tests;

// The command range's pure rules (CommandRange): the distance comparison against vanilla's own
// constant and the slider ladder against the ring cap; and the def wiring the settings read
// before any Apply, checked off the XML since the WarcasketDef type pulls in VFEP, which the
// test output never carries: the shoulders alone carry the extension, with the scope the code
// names and vanilla's radius as the slider's default; and Combat Extended's range, a constant
// in code and two values in the CE root's patch, held together.
public class CommandRangeTests
{
    private const string HelmetPath = "1.6/Defs/ThingDefs_Misc/WarcasketHelmet_Overlord.xml";
    private const string ShouldersPath = "1.6/Defs/ThingDefs_Misc/WarcasketShoulders_Overlord.xml";
    private const string CombatExtendedPatchPath = "1.6/Mods/CombatExtended/Patches/CommandRange.xml";
    private const string ExtensionClass = "OverlordWarcasket.CommandRangeExtension";

    // The 20,000th precalculated radial cell on 1.6 and 1.7 lies at squared distance 6370.
    private const float RingCap = 79.81228f;

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

    // Vanilla admits a cell while its squared distance is below 620.01: the cells at exactly 25
    // are out, those a hair inside are in.
    [Theory]
    [InlineData(24, 0, true)]
    [InlineData(24, 6, true)]
    [InlineData(25, 0, false)]
    [InlineData(15, 20, false)]
    [InlineData(7, 24, false)]
    [InlineData(17, 18, true)]
    public void Within_MatchesVanillaAtItsRadius(int dx, int dz, bool expected)
    {
        var from = new IntVec3(100, 0, 100);
        var to = new IntVec3(100 + dx, 0, 100 + dz);
        Assert.Equal(expected, CommandRange.Within(from, to, CommandRange.VanillaRadius));
        Assert.Equal(expected, from.DistanceToSquared(to) < 620.01f);
    }

    [Fact]
    public void Within_EveryRungExcludesTheNextWholeDistance()
    {
        var origin = IntVec3.Zero;
        foreach (float rung in CommandRange.Ladder(RingCap).Where(r => r < RingCap - 1f))
        {
            int whole = (int)Math.Round(rung + 0.1f);
            Assert.True(CommandRange.Within(origin, new IntVec3(whole - 1, 0, 0), rung));
            Assert.False(CommandRange.Within(origin, new IntVec3(whole, 0, 0), rung));
        }
    }

    [Fact]
    public void Ladder_EndsOnTheCapRoundedDown()
    {
        float[] stops = CommandRange.Ladder(RingCap);
        Assert.Equal(17, stops.Length);
        Assert.Equal(4.9f, stops[0]);
        Assert.Contains(CommandRange.VanillaRadius, stops);
        Assert.Equal(74.9f, stops[15]);
        Assert.Equal(79.8f, stops[16]);
        Assert.True(stops.SequenceEqual(stops.OrderBy(s => s)));
    }

    // The one named range the five-cell step misses sits between its neighbours, once, and is
    // left out where the cap falls short of it.
    [Fact]
    public void Ladder_CarriesCombatExtendedsRangeInItsPlace()
    {
        float[] stops = CommandRange.Ladder(RingCap);
        int index = Array.IndexOf(stops, CommandRange.CombatExtendedRadius);
        Assert.Equal(39.9f, stops[index - 1]);
        Assert.Equal(44.9f, stops[index + 1]);
        Assert.Single(stops, s => s == CommandRange.CombatExtendedRadius);
        Assert.DoesNotContain(CommandRange.CombatExtendedRadius, CommandRange.Ladder(40f));
    }

    // The CE root writes the range without the set and the slider's default; both are the
    // number the slider names as Combat Extended's.
    [Fact]
    public void CombatExtendedPatchWritesTheNamedRung()
    {
        XElement value = XDocument.Load(Path.Combine(RepoRoot, CombatExtendedPatchPath)).Descendants("value").Single();
        foreach (string field in new[] { "radius", "normalRadius" })
        {
            Assert.Equal(CommandRange.CombatExtendedRadius,
                float.Parse(value.Element(field)?.Value ?? "0", System.Globalization.CultureInfo.InvariantCulture));
        }
    }

    [Fact]
    public void Ladder_KeepsARungThatSitsOnTheCap()
    {
        float[] stops = CommandRange.Ladder(79.9f);
        Assert.Equal(17, stops.Length);
        Assert.Equal(79.9f, stops[16]);
    }

    [Fact]
    public void Ladder_AppendsALowerCapAfterItsLastRung()
    {
        float[] stops = CommandRange.Ladder(56.41896f);
        Assert.Equal(54.9f, stops[stops.Length - 2]);
        Assert.Equal(56.4f, stops[stops.Length - 1]);
    }

    [Theory]
    [InlineData(79.81228f, 79.8f)]
    [InlineData(79.9f, 79.9f)]
    [InlineData(24.9f, 24.9f)]
    [InlineData(80f, 80f)]
    public void FloorToTenth_RoundsDownWithoutSlippingAWholeTenth(float value, float expected)
    {
        Assert.Equal(expected, CommandRange.FloorToTenth(value));
    }

    [Fact]
    public void ShouldersCarryTheExtensionAndTheHelmetDoesNot()
    {
        var shoulders = Extensions(ShouldersPath).ToList();
        Assert.Single(shoulders);
        Assert.Empty(Extensions(HelmetPath));

        XElement extension = shoulders[0];
        string scope = extension.Element("scope")?.Value;
        Assert.True(Enum.TryParse(scope, out CommandRangeScope _), $"{ShouldersPath}: scope '{scope}' is not a CommandRangeScope");
        Assert.Equal(CommandRange.VanillaRadius, float.Parse(extension.Element("radius")?.Value ?? "0", System.Globalization.CultureInfo.InvariantCulture));
    }

    private static System.Collections.Generic.IEnumerable<XElement> Extensions(string path) =>
        XDocument.Load(Path.Combine(RepoRoot, path)).Descendants("li")
            .Where(e => (string)e.Attribute("Class") == ExtensionClass);
}
