using JesusTheChrist.Presentation.Drawing;

namespace JesusTheChrist.Presentation.Tests.Drawing;

public class RingMathTests
{
    [Fact]
    public void SweepDegrees_AtZero_IsZero()
    {
        Assert.Equal(0f, RingMath.SweepDegrees(0.0));
    }

    [Fact]
    public void SweepDegrees_AtQuarter_IsNinety()
    {
        Assert.Equal(90f, RingMath.SweepDegrees(0.25));
    }

    [Fact]
    public void SweepDegrees_AtFull_IsThreeSixty()
    {
        Assert.Equal(360f, RingMath.SweepDegrees(1.0));
    }

    [Fact]
    public void SweepDegrees_BelowZero_ClampsToZero()
    {
        Assert.Equal(0f, RingMath.SweepDegrees(-0.5));
    }

    [Fact]
    public void SweepDegrees_AboveOne_ClampsToThreeSixty()
    {
        Assert.Equal(360f, RingMath.SweepDegrees(2.0));
    }

    // The regression this suite previously missed: SweepDegrees returned a correct 360, but the
    // drawable fed that to DrawArc as an end angle, which collapsed to a zero-degree arc and
    // painted nothing. A full ring must be reported as complete so it is drawn as a circle.
    [Fact]
    public void IsComplete_AtFull_IsTrue()
    {
        Assert.True(RingMath.IsComplete(1.0));
    }

    [Fact]
    public void IsComplete_JustBelowFull_IsFalse()
    {
        Assert.False(RingMath.IsComplete(0.999));
    }

    [Fact]
    public void IsComplete_AboveOne_IsTrue()
    {
        Assert.True(RingMath.IsComplete(1.5));
    }

    [Fact]
    public void IsComplete_AtZero_IsFalse()
    {
        Assert.False(RingMath.IsComplete(0.0));
    }
}
