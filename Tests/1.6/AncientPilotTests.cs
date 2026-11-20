using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using RimWorld;
using RimWorld.QuestGen;
using Verse;
using Xunit;

namespace OverlordWarcasket.Tests;

// The ancient pilot's pure rule (AncientPilotExtension.ChanceFor): each source has a chance of
// its own, and neither has any while the switch is off. Then the def wiring, checked off the XML
// since the WarcasketDef type pulls in VFEP, which the test output never carries: the armor
// declares the extension once, off, with the two chances the setting's tooltip quotes in words,
// and names the set's other two pieces, each a def of this mod. Last, the two vanilla methods
// the patches bind by name, on the game's 1.6 assembly: a rename there would otherwise only show
// as a warning in a player's log.
public class AncientPilotTests
{
    private const string DefsFolder = "1.6/Defs/ThingDefs_Misc";
    private const string ArmorFile = "Warcasket_Overlord.xml";
    private const string ExtensionClass = "OverlordWarcasket.AncientPilotExtension";
    private const string KeyedPath = "1.6/Languages/English/Keyed/OverlordWarcasket.xml";

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

    private static XElement ExtensionEntry()
    {
        XDocument doc = XDocument.Load(Path.Combine(RepoRoot, DefsFolder, ArmorFile));
        return doc.Descendants("li").Single(li => (string)li.Attribute("Class") == ExtensionClass);
    }

    private static float Number(XElement element) => float.Parse(element.Value, CultureInfo.InvariantCulture);

    [Theory]
    [InlineData(true, AncientPilotSource.Ship, 1f)]
    [InlineData(true, AncientPilotSource.Pod, 0.3f)]
    [InlineData(false, AncientPilotSource.Ship, 0f)]
    [InlineData(false, AncientPilotSource.Pod, 0f)]
    public void ChanceFor_IsTheSourcesOwn_AndNoneWhileOff(bool enabled, AncientPilotSource source, float expected)
    {
        var extension = new AncientPilotExtension { enabled = enabled, shipChance = 1f, podChance = 0.3f };

        Assert.Equal(expected, extension.ChanceFor(source));
    }

    [Fact]
    public void Armor_DeclaresTheExtensionOff()
    {
        Assert.False(bool.Parse(ExtensionEntry().Element("enabled").Value));
    }

    // The tooltip says the ship's mechanitor "will wear" the set and "30%" of the complexes'
    // do, so a chance moved here moves that sentence with it.
    [Fact]
    public void Armor_CarriesTheChancesTheTooltipQuotes()
    {
        XElement entry = ExtensionEntry();
        string tooltip = XDocument.Load(Path.Combine(RepoRoot, KeyedPath)).Root.Element("OLWC_AncientPilotDesc").Value;

        Assert.Equal(1f, Number(entry.Element("shipChance")));
        Assert.Equal(0.3f, Number(entry.Element("podChance")));
        Assert.Contains("30%", tooltip);
    }

    [Fact]
    public void Armor_NamesTheSetsOtherTwoPieces()
    {
        string[] pieces = ExtensionEntry().Element("otherPieces").Elements("li").Select(li => li.Value).ToArray();
        string[] defNames = Directory.GetFiles(Path.Combine(RepoRoot, DefsFolder), "*.xml")
            .SelectMany(file => XDocument.Load(file).Root.Elements())
            .Select(def => (string)def.Element("defName"))
            .ToArray();

        Assert.Equal(2, pieces.Distinct().Count());
        Assert.All(pieces, piece => Assert.Contains(piece, defNames));
        Assert.DoesNotContain("OLWC_Warcasket_Overlord", pieces);
    }

    // Patches/QuestNode_Root_MechanitorShip_RunInt: the method only. The slate name its postfix
    // reads is set in the method's body, out of a test's reach.
    [Fact]
    public void Vanilla_HasTheShipQuestsRunInt()
    {
        MethodInfo method = typeof(QuestNode_Root_MechanitorShip).GetMethod(
            "RunInt", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

        Assert.NotNull(method);
        Assert.Empty(method.GetParameters());
    }

    // Patches/LayoutWorkerComplex_Mechanitor_TryPlaceDeceasedMechanitor: the method, its bool
    // result and the out parameter the postfix takes by the name "casket".
    [Fact]
    public void Vanilla_HasTheComplexsPodPlacement()
    {
        MethodInfo method = typeof(LayoutWorkerComplex_Mechanitor).GetMethod(
            "TryPlaceDeceasedMechanitor", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

        Assert.NotNull(method);
        Assert.Equal(typeof(bool), method.ReturnType);
        ParameterInfo casket = method.GetParameters().Single(parameter => parameter.Name == "casket");
        Assert.True(casket.IsOut);
        Assert.Equal(typeof(Building_AncientCryptosleepPod), casket.ParameterType.GetElementType());
    }
}
