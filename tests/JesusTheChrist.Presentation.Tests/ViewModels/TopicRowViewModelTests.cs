using System.Globalization;
using JesusTheChrist.Presentation.Resources;
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

    [Fact]
    public void IsComplete_WhenAllRead_IsTrue()
    {
        var row = new TopicRowViewModel("advocate", "Jesus Christ, Advocate", "Advocate", read: 5, total: 5);
        Assert.True(row.IsComplete);
    }

    [Fact]
    public void IsComplete_WhenPartiallyRead_IsFalse()
    {
        var row = new TopicRowViewModel("advocate", "Jesus Christ, Advocate", "Advocate", read: 4, total: 5);
        Assert.False(row.IsComplete);
    }

    [Fact]
    public void IsComplete_WithZeroTotal_IsFalse()
    {
        // An empty sub-topic has nothing to finish; calling it complete would earn it a gold ring.
        var row = new TopicRowViewModel("empty", "Empty", "Empty", read: 0, total: 0);
        Assert.False(row.IsComplete);
    }

    [Fact]
    public void ProgressDescription_WhenComplete_AnnouncesCompletion()
    {
        var prev = AppResources.Culture;
        try
        {
            AppResources.Culture = new CultureInfo("en");
            var row = new TopicRowViewModel("advocate", "Jesus Christ, Advocate", "Advocate", read: 5, total: 5);

            // The gold ring is a colour-only cue; the description carries it for screen readers.
            Assert.Equal("5 / 5 — complete", row.ProgressDescription);
        }
        finally
        {
            AppResources.Culture = prev;
        }
    }

    [Fact]
    public void ProgressDescription_WhenIncomplete_MatchesProgressLabel()
    {
        var row = new TopicRowViewModel("advocate", "Jesus Christ, Advocate", "Advocate", read: 2, total: 5);
        Assert.Equal(row.ProgressLabel, row.ProgressDescription);
    }

    [Fact]
    public void ProgressLabel_WhenComplete_StaysNumericOnly()
    {
        // ProgressLabel is visible text in the row; only the screen-reader description changes.
        var row = new TopicRowViewModel("advocate", "Jesus Christ, Advocate", "Advocate", read: 5, total: 5);
        Assert.Equal("5 / 5", row.ProgressLabel);
    }
}
