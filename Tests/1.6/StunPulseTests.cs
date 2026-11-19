using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Xunit;

namespace OverlordWarcasket.Tests;

// The mech stun pulse's pure rule (CompMechStunPulse.Fires): the pulse answers the one event
// the setting names, a death or a downing and never both, and a pilot who goes under at rest,
// in a bed or under anesthetic, has not fallen; and the def wiring the settings read before
// any Apply, checked off the XML since the WarcasketDef type pulls in VFEP, which the test
// output never carries: the armor declares the pulse once, with a trigger the code names, the
// damage def vanilla's stun rule admits for mechanoids alone, and a radius that is one of the
// slider's stops, as the apocriton's is. The display's rule (CompMechStunPulse.DisplayScale) is
// here too, a pulse drawn as large against the apocriton's as it reaches, with its wiring: the
// armor names the effecter, drawn for the slider's last stop, and every fleck of that effecter
// is one of our own, since the settings write the size onto them.
public class StunPulseTests
{
    private const string ArmorPath = "1.6/Defs/ThingDefs_Misc/Warcasket_Overlord.xml";
    private const string PropsClass = "OverlordWarcasket.CompProperties_MechStunPulse";
    private const string EffectsFolder = "1.6/Defs/Effects";

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

    private static XElement PulseEntry()
    {
        XDocument doc = XDocument.Load(Path.Combine(RepoRoot, ArmorPath));
        return doc.Descendants("li").Single(li => (string)li.Attribute("Class") == PropsClass);
    }

    private static float Radius() => Number(PulseEntry().Element("radius"));

    private static float Number(XElement element) => float.Parse(element.Value, CultureInfo.InvariantCulture);

    // The one def of that type and name among the effect files.
    private static XElement EffectDef(string type, string defName) =>
        Directory.GetFiles(Path.Combine(RepoRoot, EffectsFolder), "*.xml")
            .SelectMany(file => XDocument.Load(file).Root.Elements(type))
            .Single(def => (string)def.Element("defName") == defName);

    [Theory]
    [InlineData(StunPulseTrigger.Death, StunPulseTrigger.Death, true)]
    [InlineData(StunPulseTrigger.Death, StunPulseTrigger.Downed, false)]
    [InlineData(StunPulseTrigger.Downed, StunPulseTrigger.Downed, true)]
    [InlineData(StunPulseTrigger.Downed, StunPulseTrigger.Death, false)]
    public void Fires_OnlyOnTheEventTheSettingNames(StunPulseTrigger setting, StunPulseTrigger happened, bool expected)
    {
        Assert.Equal(expected, CompMechStunPulse.Fires(setting, happened, atRest: false));
    }

    [Fact]
    public void Fires_NotForADowningAtRest()
    {
        Assert.False(CompMechStunPulse.Fires(StunPulseTrigger.Downed, StunPulseTrigger.Downed, atRest: true));
    }

    // A death is a death, in bed or on the operating table too.
    [Fact]
    public void Fires_ForADeathAtRest()
    {
        Assert.True(CompMechStunPulse.Fires(StunPulseTrigger.Death, StunPulseTrigger.Death, atRest: true));
    }

    [Fact]
    public void TheArmor_DeclaresThePulseWithATriggerTheCodeNames()
    {
        XElement entry = PulseEntry();
        Assert.True(Enum.TryParse(entry.Element("trigger").Value, out StunPulseTrigger trigger));
        Assert.Equal(StunPulseTrigger.Death, trigger);
        Assert.Equal("MechBandShockwave", entry.Element("damageDef").Value);
    }

    // The slider's stops are whole cells up from its minimum, so the shipped radius must be one
    // for the row to open on it, and the apocriton's is the last.
    [Fact]
    public void TheShippedRadius_IsASliderStop()
    {
        float radius = Radius();
        Assert.InRange(radius, StunPulseSetting.MinRadius, StunPulseSetting.MaxRadius);
        Assert.Equal(radius, Tunable.Snap(radius, StunPulseSetting.MinRadius, StunPulseSetting.MaxRadius, 1f, 1f), 4);
        Assert.Equal(StunPulseSetting.MaxRadius, Tunable.Snap(99f, StunPulseSetting.MinRadius, StunPulseSetting.MaxRadius, 1f, 1f), 4);
    }

    [Theory]
    [InlineData(30.9f, 30.9f, 1f)]
    [InlineData(10.3f, 30.9f, 1f / 3f)]
    [InlineData(1.9f, 30.9f, 1.9f / 30.9f)]
    public void DisplayScale_IsThePulsesReachAgainstTheReachItIsDrawnFor(float radius, float drawnFor, float expected)
    {
        Assert.Equal(expected, CompMechStunPulse.DisplayScale(radius, drawnFor), 4);
    }

    // A def without the radius its display is drawn for leaves the display as it is drawn.
    [Fact]
    public void DisplayScale_LeavesTheDisplayAloneWithNothingToMeasureAgainst()
    {
        Assert.Equal(1f, CompMechStunPulse.DisplayScale(9.9f, 0f));
    }

    // Drawn for the slider's last stop, the apocriton's radius, so that stop plays the
    // apocriton's display untouched.
    [Fact]
    public void TheArmor_NamesADisplayDrawnForTheApocritonsRadius()
    {
        XElement entry = PulseEntry();
        Assert.NotNull(EffectDef("EffecterDef", entry.Element("centerEffecter").Value));
        Assert.Equal(StunPulseSetting.MaxRadius, Number(entry.Element("centerEffecterRadius")), 4);
    }

    // The settings write the radius onto the growthRate of every fleck the display names, which
    // must therefore be a def of this mod's and not Biotech's, whose apocriton plays it too.
    [Fact]
    public void TheDisplaysFlecks_AreOurOwn()
    {
        XElement display = EffectDef("EffecterDef", PulseEntry().Element("centerEffecter").Value);
        string[] flecks = display.Descendants("fleckDef").Select(fleck => fleck.Value).ToArray();
        Assert.NotEmpty(flecks);
        foreach (string fleck in flecks)
        {
            Assert.StartsWith("OLWC_", fleck);
            Assert.NotNull(EffectDef("FleckDef", fleck));
        }
    }
}
