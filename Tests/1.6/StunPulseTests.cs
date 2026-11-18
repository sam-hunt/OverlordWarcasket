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
// slider's stops, as the apocriton's is.
public class StunPulseTests
{
    private const string ArmorPath = "1.6/Defs/ThingDefs_Misc/Warcasket_Overlord.xml";
    private const string PropsClass = "OverlordWarcasket.CompProperties_MechStunPulse";

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

    private static float Radius() => float.Parse(PulseEntry().Element("radius").Value, CultureInfo.InvariantCulture);

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
}
