using Xunit;

namespace OverlordWarcasket.Tests;

// The settings sliders' pure arithmetic (Tunable): where a release lands (Snap, StoredStep), the
// stops of a banded row (Stops) and which stop a stored value sits on (Nearest). Pure functions
// over floats, so the suite needs no game context beyond loading the type.
public class TunableTests
{
    [Fact]
    public void Snap_LandsOnTheStepAndClamps()
    {
        Assert.Equal(22f, Tunable.Snap(22.9f, 0f, 50f, 2f, 1f));
        Assert.Equal(24f, Tunable.Snap(23.1f, 0f, 50f, 2f, 1f));
        Assert.Equal(50f, Tunable.Snap(80f, 0f, 50f, 2f, 1f));
        Assert.Equal(10f, Tunable.Snap(3f, 10f, 300f, 10f, 1f));
    }

    [Fact]
    public void StoredStep_RoundsToWholeShownUnitsWhenScaled()
    {
        Assert.Equal(2f, Tunable.StoredStep(2f, 1f));
        Assert.Equal(3f, Tunable.StoredStep(2f, 3f));
        Assert.Equal(2f, Tunable.StoredStep(2f, 2f));
        Assert.Equal(4f, Tunable.StoredStep(3f, 2f));
    }

    // The recharge row's bands: quarter percents to five, whole to twenty, fives to a hundred,
    // the minimum first and each band's end a stop.
    [Fact]
    public void Stops_FollowTheBands()
    {
        float[] stops = Tunable.Stops(0f, 100f, [(5f, 0.25f), (20f, 1f), (100f, 5f)]);
        Assert.Equal(0f, stops[0]);
        Assert.Equal(0.25f, stops[1]);
        Assert.Contains(5f, stops);
        Assert.Contains(6f, stops);
        Assert.DoesNotContain(5.5f, stops);
        Assert.Contains(25f, stops);
        Assert.DoesNotContain(21f, stops);
        Assert.Equal(100f, stops[^1]);
        Assert.Equal(20 + 15 + 16 + 1, stops.Length);
    }

    // A minimum may sit off its band's grid: five, then the multiples of five.
    [Fact]
    public void Stops_KeepAnOffGridMinimum()
    {
        float[] stops = Tunable.Stops(5f, 60f, [(60f, 5f)]);
        Assert.Equal(5f, stops[0]);
        Assert.Equal(10f, stops[1]);
        Assert.Equal(60f, stops[^1]);
        Assert.Equal(12, stops.Length);
    }

    [Fact]
    public void Nearest_PicksTheClosestStopLowerOnATie()
    {
        float[] stops = [0f, 1f, 2f, 4f];
        Assert.Equal(0, Tunable.Nearest(stops, -3f));
        Assert.Equal(2, Tunable.Nearest(stops, 2.4f));
        Assert.Equal(2, Tunable.Nearest(stops, 3f));
        Assert.Equal(3, Tunable.Nearest(stops, 9f));
    }
}
