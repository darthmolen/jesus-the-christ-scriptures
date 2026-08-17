using JesusTheChrist.Presentation.ViewModels;

namespace JesusTheChrist.Presentation.Tests.ViewModels;

public sealed class ChapterSegmentViewModelTests
{
    [Fact]
    public void CollapsedSegment_ExposesNoVisibleVerses() =>
        Assert.Empty(Make(isExpanded: false).VisibleVerses);

    [Fact]
    public void ExpandedSegment_ExposesItsVerses()
    {
        var vm = Make(isExpanded: true);
        Assert.Same(vm.Verses, vm.VisibleVerses);
    }

    [Fact]
    public async Task Toggle_RealizesThenHidesVerses()
    {
        var vm = Make(isExpanded: false);

        await vm.ToggleExpandedCommand.ExecuteAsync(null);
        Assert.True(vm.IsExpanded);
        Assert.Same(vm.Verses, vm.VisibleVerses);

        await vm.ToggleExpandedCommand.ExecuteAsync(null);
        Assert.False(vm.IsExpanded);
        Assert.Empty(vm.VisibleVerses);
    }

    [Fact]
    public async Task Toggle_RaisesChangeForVisibleVersesAndChevron()
    {
        var vm = Make(isExpanded: false);
        var changed = new List<string?>();
        vm.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        await vm.ToggleExpandedCommand.ExecuteAsync(null);

        Assert.Contains(nameof(ChapterSegmentViewModel.VisibleVerses), changed);
        Assert.Contains(nameof(ChapterSegmentViewModel.ChevronGlyph), changed);
    }

    /// <summary>
    /// Only expanding is reported. Collapsing the open chapter is a legitimate move — it turns a
    /// long card into a chapter index — and must not be mistaken for the reader moving on.
    /// </summary>
    [Fact]
    public async Task ExpansionListener_FiresOnExpandOnly()
    {
        var vm = Make(isExpanded: false);
        var fired = 0;
        vm.AttachExpansionListener(_ =>
        {
            fired++;
            return Task.CompletedTask;
        });

        await vm.ToggleExpandedCommand.ExecuteAsync(null);
        Assert.Equal(1, fired);

        await vm.ToggleExpandedCommand.ExecuteAsync(null);
        Assert.Equal(1, fired);
    }

    private static ChapterSegmentViewModel Make(bool isExpanded) =>
        new(
            10,
            "Matthew 10",
            showHeader: true,
            [new ContextLineViewModel(1, "and when he had called", isTarget: true)],
            isExpanded,
            new Uri("https://www.churchofjesuschrist.org/study/scriptures/nt/matt/10?lang=eng&id=p1#p1"),
            _ => Task.CompletedTask);
}
