using Xunit;

namespace OverlordWarcasket.Tests;

// The gauge's count (CompResurrectionCore.FormatProgress): the charges held with the part
// charge the recharge has accrued, to one decimal.
public class CoreGaugeTests
{
    // A whole count shows no decimal, and a part charge under a tenth does not show yet.
    [Theory]
    [InlineData(0, 0f, 10, "0 / 10")]
    [InlineData(3, 0f, 10, "3 / 10")]
    [InlineData(3, 0.09f, 10, "3 / 10")]
    public void WholeCharges_ShowNoDecimal(int charges, float progress, int max, string label)
    {
        Assert.Equal(label, CompResurrectionCore.FormatProgress(charges, progress, max));
    }

    // The part charge is cut, never rounded up: the whole number is the charges a cast can
    // spend, so 3.96 must not read 4.
    [Theory]
    [InlineData(3, 0.4f, 10, "3.4 / 10")]
    [InlineData(3, 0.96f, 10, "3.9 / 10")]
    [InlineData(0, 0.55f, 10, "0.5 / 10")]
    [InlineData(9, 0.999f, 10, "9.9 / 10")]
    public void PartCharge_IsCutToATenth(int charges, float progress, int max, string label)
    {
        Assert.Equal(label, CompResurrectionCore.FormatProgress(charges, progress, max));
    }

    // A full core shows its capacity whatever progress is left over, and so does a count the
    // settings lowered the capacity under before the next trim.
    [Theory]
    [InlineData(10, 0f, 10, "10 / 10")]
    [InlineData(10, 0.5f, 10, "10 / 10")]
    [InlineData(12, 0.5f, 10, "10 / 10")]
    public void FullCore_ShowsItsCapacity(int charges, float progress, int max, string label)
    {
        Assert.Equal(label, CompResurrectionCore.FormatProgress(charges, progress, max));
    }
}
