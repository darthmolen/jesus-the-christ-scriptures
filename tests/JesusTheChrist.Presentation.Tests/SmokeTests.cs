using JesusTheChrist.Presentation;

namespace JesusTheChrist.Presentation.Tests;

public class SmokeTests
{
    [Fact]
    public void PresentationAssemblyIsReferencedAndUsable()
    {
        Assert.True(PresentationMarker.IsAvailable);
    }
}
