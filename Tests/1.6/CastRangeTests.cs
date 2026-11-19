using System.Collections.Generic;
using Verse;
using Xunit;

namespace OverlordWarcasket.Tests;

// The resurrect mech ability's cast range (CastRange): the no-limit rule, the settings slider's
// stops and what each writes to the def, and the ring's cells, held against the disc counted
// cell by cell with the walk's own comparison.
public class CastRangeTests
{
    private static readonly IntVec3 Center = new(40, 0, 25);

    [Fact]
    public void ZeroOrLess_IsNoLimit()
    {
        Assert.False(CastRange.Limited(0f));
        Assert.False(CastRange.Limited(-1f));
        Assert.True(CastRange.Limited(9.9f));
        Assert.True(CastRange.Within(0f, 400f));
        Assert.True(CastRange.Within(9.9f, 9.9f));
        Assert.False(CastRange.Within(9.9f, 10f));
    }

    // A cell a stop to 29.9, five to 59.9, ten to 119.9, then no limit, every range ending in
    // .9 and the shipped 9.9 among them.
    [Fact]
    public void Stops_EndInNineTenthsAndThenNoLimit()
    {
        float[] stops = Tunable.Stops(CastRange.SliderMin, CastRange.SliderNoLimit, CastRange.SliderBands);
        Assert.Equal(29 + 6 + 6 + 1, stops.Length);
        var ranges = new List<float>();
        foreach (float stop in stops)
            ranges.Add(CastRange.FromStop(stop));
        Assert.Equal(1.9f, ranges[0]);
        Assert.Contains(9.9f, ranges);
        Assert.Contains(29.9f, ranges);
        Assert.Contains(34.9f, ranges);
        Assert.DoesNotContain(30.9f, ranges);
        Assert.Contains(69.9f, ranges);
        Assert.DoesNotContain(64.9f, ranges);
        Assert.Equal(119.9f, ranges[^2]);
        Assert.Equal(0f, ranges[^1]);
        Assert.True(CastRange.IsNoLimitStop(stops[^1]));
        Assert.False(CastRange.IsNoLimitStop(stops[^2]));
    }

    // What the settings read off the def comes back as it was, on the stops and off them, and a
    // def range above the slider reads as its top stop, never as no limit.
    [Theory]
    [InlineData(9.9f)]
    [InlineData(1.9f)]
    [InlineData(119.9f)]
    [InlineData(12.5f)]
    [InlineData(1.5f)]
    public void ADefRange_SurvivesTheSlider(float range)
    {
        Assert.Equal(range, CastRange.FromStop(CastRange.ToStop(range)));
    }

    [Fact]
    public void ADefRange_OutsideTheSliderKeepsItsMeaning()
    {
        Assert.Equal(CastRange.SliderNoLimit, CastRange.ToStop(0f));
        Assert.Equal(0f, CastRange.FromStop(CastRange.ToStop(-5f)));
        Assert.Equal(119.9f, CastRange.FromStop(CastRange.ToStop(150f)));
    }

    // The shipped range sits on a stop, so the row shows it as the default.
    [Fact]
    public void TheShippedRange_IsAStop()
    {
        float[] stops = Tunable.Stops(CastRange.SliderMin, CastRange.SliderNoLimit, CastRange.SliderBands);
        float shipped = CastRange.ToStop(9.9f);
        Assert.Equal(10f, stops[Tunable.Nearest(stops, shipped)]);
        Assert.Equal(9.9f, CastRange.FromStop(stops[Tunable.Nearest(stops, shipped)]));
    }

    // The widest ring the slider offers stays within one instanced draw, and the next stop of
    // its band would not.
    [Fact]
    public void TheTopStopsRing_FitsOneDraw()
    {
        float top = CastRange.FromStop(CastRange.SliderMax);
        Assert.True(CastRange.RingEdges(top) <= CastRange.MaxRingEdges);
        Assert.True(CastRange.RingEdges(top + 10f) > CastRange.MaxRingEdges);
    }

    // The rim is exactly the disc's cells with a cardinal neighbour outside it, each once, and
    // every side of a rim cell either faces out of the disc, where the edge is drawn, or faces
    // a rim cell or an inside cell, where DrawFieldEdges draws none. The edges drawn are then
    // the disc's whole outline, RingEdges of them.
    [Theory]
    [InlineData(0.5f)]
    [InlineData(1f)]
    [InlineData(1.9f)]
    [InlineData(9.9f)]
    [InlineData(10f)]
    [InlineData(24.9f)]
    [InlineData(29.9f)]
    [InlineData(119.9f)]
    public void Rim_IsTheDiscsOutline(float range)
    {
        var rim = new List<IntVec3>();
        var inside = new HashSet<IntVec3>();
        CastRange.Rim(Center, range, rim, inside);

        var disc = new HashSet<IntVec3>();
        int reach = (int)range + 2;
        for (int x = -reach; x <= reach; x++)
        {
            for (int z = -reach; z <= reach; z++)
            {
                var cell = new IntVec3(Center.x + x, 0, Center.z + z);
                if (CastRange.Within(range, Center.DistanceTo(cell)))
                    disc.Add(cell);
            }
        }

        var expected = new HashSet<IntVec3>();
        foreach (IntVec3 cell in disc)
        {
            foreach (IntVec3 side in GenAdj.CardinalDirections)
            {
                if (!disc.Contains(cell + side))
                    expected.Add(cell);
            }
        }

        var rimSet = new HashSet<IntVec3>(rim);
        Assert.Equal(rim.Count, rimSet.Count);
        Assert.True(expected.SetEquals(rimSet));

        int edges = 0;
        foreach (IntVec3 cell in rim)
        {
            foreach (IntVec3 side in GenAdj.CardinalDirections)
            {
                IntVec3 neighbour = cell + side;
                if (!disc.Contains(neighbour))
                {
                    Assert.DoesNotContain(neighbour, inside);
                    edges++;
                }
                else
                {
                    Assert.True(rimSet.Contains(neighbour) || inside.Contains(neighbour));
                }
            }
        }
        Assert.Equal(CastRange.RingEdges(range), edges);
        Assert.True(inside.IsSubsetOf(disc));
    }

    // The rim grows with the range's edge, not its area.
    [Fact]
    public void Rim_IsSmallBesideTheDisc()
    {
        var rim = new List<IntVec3>();
        var inside = new HashSet<IntVec3>();
        CastRange.Rim(Center, 119.9f, rim, inside);
        Assert.True(rim.Count < 1000);
    }
}
