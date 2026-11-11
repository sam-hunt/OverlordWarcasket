using RimWorld;
using Verse;
using Xunit;

namespace OverlordWarcasket.Tests;

// The resurrect mech ability's cost table (ResurrectMechExtension.TryGetCost) is the one rule for
// which weight classes come back: a listed class is priced even when vanilla marks it
// canResurrect false, as it does the ultraheavy class the ability def lists, and an unlisted
// class is refused.
public class ResurrectCostTests
{
    private static readonly MechWeightClassDef Heavy = new() { defName = "Heavy" };
    private static readonly MechWeightClassDef UltraHeavy = new() { defName = "UltraHeavy", canResurrect = false };

    private static ResurrectMechExtension Table(params (MechWeightClassDef weightClass, int cost)[] entries)
    {
        var extension = new ResurrectMechExtension();
        foreach (var (weightClass, cost) in entries)
            extension.costs.Add(new MechChargeCosts { weightClass = weightClass, cost = cost });
        return extension;
    }

    [Fact]
    public void AListedClass_IsPricedWhateverItsCanResurrect()
    {
        Assert.True(Table((Heavy, 5), (UltraHeavy, 8)).TryGetCost(UltraHeavy, out int cost));
        Assert.Equal(8, cost);
    }

    [Fact]
    public void AnUnlistedClass_IsRefused()
    {
        Assert.False(Table((Heavy, 5)).TryGetCost(UltraHeavy, out _));
    }
}
