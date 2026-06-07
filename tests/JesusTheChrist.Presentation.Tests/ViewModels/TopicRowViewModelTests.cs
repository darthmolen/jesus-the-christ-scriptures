using JesusTheChrist.Presentation.ViewModels;

namespace JesusTheChrist.Presentation.Tests.ViewModels;

public class TopicRowViewModelTests
{
    [Fact]
    public void ProgressLabel_FormatsReadOverTotal()
    {
        var row = new TopicRowViewModel("advocate", "Jesus Christ, Advocate", "Advocate", read: 2, total: 5);
        Assert.Equal("2 / 5", row.ProgressLabel);
    }

    [Fact]
    public void Fraction_IsReadOverTotal()
    {
        var row = new TopicRowViewModel("advocate", "Jesus Christ, Advocate", "Advocate", read: 1, total: 4);
        Assert.Equal(0.25, row.Fraction);
    }

    [Fact]
    public void Fraction_WithZeroTotal_IsZero()
    {
        var row = new TopicRowViewModel("empty", "Empty", "Empty", read: 0, total: 0);
        Assert.Equal(0.0, row.Fraction);
    }
}
