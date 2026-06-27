using JesusTheChrist.Core.Models;

namespace JesusTheChrist.Core.Tests;

public sealed class ReferenceSegmentsTests
{
    [Fact]
    public void SpansChapters_is_false_for_single_chapter_reference() =>
        Assert.False(SingleChapter().SpansChapters);

    [Fact]
    public void SpansChapters_is_false_when_end_chapter_equals_start() =>
        Assert.False((SingleChapter() with { EndCh = 9 }).SpansChapters);

    [Fact]
    public void SpansChapters_is_true_when_end_chapter_differs() =>
        Assert.True(CrossChapter().SpansChapters);

    [Fact]
    public void TargetSegments_single_chapter_yields_one_labelled_segment()
    {
        var segments = SingleChapter().TargetSegments();

        var segment = Assert.Single(segments);
        Assert.Equal(9, segment.Ch);
        Assert.Equal("Matthew 9", segment.ChapterLabel);
        Assert.Equal([35, 36], segment.Verses.Select(v => v.Vs).ToArray());
    }

    [Fact]
    public void TargetSegments_groups_contiguous_chapters_in_source_order()
    {
        var segments = CrossChapter().TargetSegments();

        Assert.Equal(
            [(9, "Matthew 9"), (10, "Matthew 10"), (11, "Matthew 11")],
            segments.Select(s => (s.Ch, s.ChapterLabel)).ToArray());
        Assert.Equal([35, 36, 37, 38], segments[0].Verses.Select(v => v.Vs).ToArray());
        Assert.Equal([1, 2], segments[1].Verses.Select(v => v.Vs).ToArray());
        Assert.Equal([1], segments[2].Verses.Select(v => v.Vs).ToArray());
    }

    [Fact]
    public void TargetSegments_ignores_non_target_context_verses()
    {
        var reference = SingleChapter() with
        {
            Context =
            [
                new ContextVerse(34, "context before", Target: false),
                new ContextVerse(35, "target", Target: true),
                new ContextVerse(36, "after target", Target: false),
            ],
        };

        var segment = Assert.Single(reference.TargetSegments());
        Assert.Equal([35], segment.Verses.Select(v => v.Vs).ToArray());
    }

    [Fact]
    public void MissingChapters_is_empty_for_single_chapter_reference() =>
        Assert.Empty(SingleChapter().MissingChapters());

    [Fact]
    public void MissingChapters_is_empty_when_every_chapter_has_targets() =>
        Assert.Empty(CrossChapter().MissingChapters());

    [Fact]
    public void MissingChapters_lists_gap_chapters_in_ascending_order()
    {
        // Targets only in chapters 11 and 13 across an 11..14 span -> 12 and 14 are interstitial.
        var reference = CrossChapter() with
        {
            Ch = 11,
            EndCh = 14,
            Context =
            [
                new ContextVerse(1, "eleven", Target: true, Ch: 11),
                new ContextVerse(1, "thirteen", Target: true, Ch: 13),
            ],
        };

        Assert.Equal([12, 14], reference.MissingChapters().ToArray());
    }

    [Fact]
    public void Id_cross_chapter_uses_start_and_end_chapter_verses() =>
        Assert.Equal("summary:newtestament/matt/9:35-11:1", CrossChapter().Id("summary"));

    private static Reference SingleChapter() =>
        new(
            RefLabel: "Matt. 9:35–36",
            Vol: Volume.NewTestament,
            Book: "matt",
            BookTitle: "Matthew",
            Ch: 9,
            Verses: [35, 36],
            Context:
            [
                new ContextVerse(35, "and jesus went about all the cities", Target: true),
                new ContextVerse(36, "but when he saw the multitudes", Target: true),
            ],
            Note: null);

    private static Reference CrossChapter() =>
        new(
            RefLabel: "Matt. 9:35–11:1",
            Vol: Volume.NewTestament,
            Book: "matt",
            BookTitle: "Matthew",
            Ch: 9,
            Verses: [35, 36, 37, 38],
            Context:
            [
                new ContextVerse(35, "and jesus went about all the cities", Target: true, Ch: 9),
                new ContextVerse(36, "but when he saw the multitudes", Target: true, Ch: 9),
                new ContextVerse(37, "the harvest truly is plenteous", Target: true, Ch: 9),
                new ContextVerse(38, "pray ye therefore the lord of the harvest", Target: true, Ch: 9),
                new ContextVerse(1, "and when he had called unto him his twelve disciples", Target: true, Ch: 10),
                new ContextVerse(2, "now the names of the twelve apostles", Target: true, Ch: 10),
                new ContextVerse(1, "and it came to pass when jesus had made an end", Target: true, Ch: 11),
            ],
            Note: "sends disciples forth by twos",
            EndCh: 11);
}
