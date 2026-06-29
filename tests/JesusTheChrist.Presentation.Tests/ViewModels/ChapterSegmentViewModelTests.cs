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
    public void Toggle_RealizesThenHidesVerses()
    {
        var vm = Make(isExpanded: false);

        vm.ToggleExpandedCommand.Execute(null);
        Assert.True(vm.IsExpanded);
        Assert.Same(vm.Verses, vm.VisibleVerses);

        vm.ToggleExpandedCommand.Execute(null);
        Assert.False(vm.IsExpanded);
        Assert.Empty(vm.VisibleVerses);
    }

    [Fact]
    public void Toggle_RaisesChangeForVisibleVersesAndChevron()
    {
        var vm = Make(isExpanded: false);
        var changed = new List<string?>();
        vm.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        vm.ToggleExpandedCommand.Execute(null);

        Assert.Contains(nameof(ChapterSegmentViewModel.VisibleVerses), changed);
        Assert.Contains(nameof(ChapterSegmentViewModel.ChevronGlyph), changed);
    }

    private static ChapterSegmentViewModel Make(bool isExpanded) =>
        new(
            "Matthew 10",
            showHeader: true,
            [new ContextLineViewModel(1, "and when he had called", isTarget: true)],
            isExpanded);
}
