using System.Linq;
using System.Reflection;
using RimWorld;
using Verse.AI;
using Xunit;

namespace OverlordWarcasket.Tests;

// The mech resurrector's reload rule (CompMechResurrector.ChargesWanted): an automatic reload
// stops at the gauge's target and an ordered one fills the resurrector; and the vanilla members the
// two reload patches bind to by name, since Harmony resolves a prefix's allowForcedReload parameter
// against the patched method's own and a miss fails the whole PatchAll at startup.
public class ResurrectorReloadTests
{
    // The handle starts at zero: nothing is fetched unasked, however empty the resurrector.
    [Theory]
    [InlineData(0, 10, 0)]
    [InlineData(7, 10, 0)]
    [InlineData(10, 10, 0)]
    public void AutomaticReload_WantsNothingAtATargetOfZero(int charges, int max, int target)
    {
        Assert.Equal(0, CompMechResurrector.ChargesWanted(charges, max, target, ordered: false));
    }

    // Below the target it wants the difference; at or above it, nothing. A target saved above a
    // capacity the settings lowered since counts as the capacity.
    [Theory]
    [InlineData(2, 10, 5, 3)]
    [InlineData(5, 10, 5, 0)]
    [InlineData(8, 10, 5, 0)]
    [InlineData(0, 10, 10, 10)]
    [InlineData(3, 6, 10, 3)]
    public void AutomaticReload_FillsToTheTarget(int charges, int max, int target, int wanted)
    {
        Assert.Equal(wanted, CompMechResurrector.ChargesWanted(charges, max, target, ordered: false));
    }

    // An ordered reload ignores the target; a count above the capacity (before a trim) wants
    // nothing rather than a negative amount, which vanilla's SplitOff would throw on.
    [Theory]
    [InlineData(0, 10, 0, 10)]
    [InlineData(7, 10, 0, 3)]
    [InlineData(7, 10, 9, 3)]
    [InlineData(10, 10, 0, 0)]
    [InlineData(12, 10, 0, 0)]
    public void OrderedReload_FillsTheResurrector(int charges, int max, int target, int wanted)
    {
        Assert.Equal(wanted, CompMechResurrector.ChargesWanted(charges, max, target, ordered: true));
    }

    // Patches/CompApparelReloadable_Reload: both queries exist, public, taking the one flag
    // under the name the prefixes declare.
    [Theory]
    [InlineData(nameof(CompApparelReloadable.NeedsReload), typeof(bool))]
    [InlineData(nameof(CompApparelReloadable.MaxAmmoNeeded), typeof(int))]
    public void ReloadQueries_TakeTheFlagThePrefixesName(string method, System.Type returns)
    {
        MethodInfo info = typeof(CompApparelReloadable).GetMethod(method, BindingFlags.Instance | BindingFlags.Public);
        Assert.NotNull(info);
        Assert.Equal(returns, info.ReturnType);
        ParameterInfo flag = Assert.Single(info.GetParameters());
        Assert.Equal(typeof(bool), flag.ParameterType);
        Assert.Equal("allowForcedReload", flag.Name);
    }

    // Patches/JobGiver_Reload_TryGiveJob: the automatic reload's job giver still declares the
    // method the postfix names, returning the job it caps.
    [Fact]
    public void JobGiverReload_DeclaresTryGiveJob()
    {
        MethodInfo info = typeof(JobGiver_Reload)
            .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly)
            .SingleOrDefault(m => m.Name == "TryGiveJob");
        Assert.NotNull(info);
        Assert.Equal(typeof(Job), info.ReturnType);
    }

    // The resurrector is vanilla's reloadable, so the reload job, the float menu and the stat sheet
    // pick it up; the props carry the recharge beside vanilla's reload fields.
    [Fact]
    public void Resurrector_IsVanillasReloadable()
    {
        Assert.True(typeof(CompApparelReloadable).IsAssignableFrom(typeof(CompMechResurrector)));
        var props = new CompProperties_MechResurrector();
        Assert.Equal(typeof(CompMechResurrector), props.compClass);
        Assert.Equal(1f, props.chargesPerDay);
    }
}
