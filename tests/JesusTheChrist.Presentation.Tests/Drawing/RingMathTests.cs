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

    [Fact]
    public void StartAngleDegrees_IsTopOfCircle()
    {
        Assert.Equal(-90f, RingMath.StartAngleDegrees);
    }
}
