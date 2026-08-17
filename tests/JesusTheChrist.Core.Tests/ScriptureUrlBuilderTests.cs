using JesusTheChrist.Core.Models;

namespace JesusTheChrist.Core.Tests;

public sealed class ScriptureUrlBuilderTests
{
    private const string Base = "https://www.churchofjesuschrist.org/study/scriptures";

    [Theory]
    [InlineData(Volume.OldTestament, "ot")]
    [InlineData(Volume.NewTestament, "nt")]
    [InlineData(Volume.BookOfMormon, "bofm")]
    [InlineData(Volume.DoctrineAndCovenants, "dc-testament")]
    [InlineData(Volume.PearlOfGreatPrice, "pgp")]
    public void SiteCode_maps_every_volume(Volume volume, string expected) =>
        Assert.Equal(expected, volume.SiteCode());

    [Theory]
    [InlineData(Language.En, "eng")]
    [InlineData(Language.Es, "spa")]
    public void SiteCode_maps_every_language(Language language, string expected) =>
        Assert.Equal(expected, language.SiteCode());

    // The worked examples from docs/DEEP-LINKING-TO-LDS-WEBSITE.md.
    [Fact]
    public void Builds_john_3_16() =>
        Assert.Equal(
            $"{Base}/nt/john/3?lang=eng&id=p16#p16",
            ScriptureUrlBuilder.Build(Volume.NewTestament, "john", 3, [16], Language.En));

    [Fact]
    public void Builds_1_nephi_3_7() =>
        Assert.Equal(
            $"{Base}/bofm/1-ne/3?lang=eng&id=p7#p7",
            ScriptureUrlBuilder.Build(Volume.BookOfMormon, "1-ne", 3, [7], Language.En));

    [Fact]
    public void Builds_alma_32_21_in_spanish() =>
        Assert.Equal(
            $"{Base}/bofm/alma/32?lang=spa&id=p21#p21",
            ScriptureUrlBuilder.Build(Volume.BookOfMormon, "alma", 32, [21], Language.Es));

    [Fact]
    public void Builds_moses_1_39() =>
        Assert.Equal(
            $"{Base}/pgp/moses/1?lang=eng&id=p39#p39",
            ScriptureUrlBuilder.Build(Volume.PearlOfGreatPrice, "moses", 1, [39], Language.En));

    [Fact]
    public void Builds_helaman_5_12() =>
        Assert.Equal(
            $"{Base}/bofm/hel/5?lang=eng&id=p12#p12",
            ScriptureUrlBuilder.Build(Volume.BookOfMormon, "hel", 5, [12], Language.En));

    /// <summary>
    /// D&amp;C is section-based: the book segment is literally <c>dc</c> and the chapter is the
    /// section number. The corpus already stores <c>dc</c>, so no special case is needed here.
    /// </summary>
    [Fact]
    public void Builds_dc_121_7_8_as_a_range() =>
        Assert.Equal(
            $"{Base}/dc-testament/dc/121?lang=eng&id=p7-p8#p7",
            ScriptureUrlBuilder.Build(Volume.DoctrineAndCovenants, "dc", 121, [7, 8], Language.En));

    [Fact]
    public void Contiguous_run_of_three_or_more_collapses_to_a_range() =>
        Assert.Equal(
            $"{Base}/nt/matt/5?lang=eng&id=p3-p12#p3",
            ScriptureUrlBuilder.Build(Volume.NewTestament, "matt", 5, [3, 4, 5, 6, 7, 8, 9, 10, 11, 12], Language.En));

    [Fact]
    public void Discrete_verses_become_a_comma_list() =>
        Assert.Equal(
            $"{Base}/nt/john/1?lang=eng&id=p7,p9,p11#p7",
            ScriptureUrlBuilder.Build(Volume.NewTestament, "john", 1, [7, 9, 11], Language.En));

    [Fact]
    public void A_gap_anywhere_defeats_the_range_form() =>
        Assert.Equal(
            $"{Base}/nt/john/1?lang=eng&id=p7,p8,p10#p7",
            ScriptureUrlBuilder.Build(Volume.NewTestament, "john", 1, [7, 8, 10], Language.En));

    [Fact]
    public void No_verses_yields_a_chapter_url_with_no_id_or_anchor() =>
        Assert.Equal(
            $"{Base}/bofm/3-ne/11?lang=eng",
            ScriptureUrlBuilder.Build(Volume.BookOfMormon, "3-ne", 11, [], Language.En));

    /// <summary>
    /// The card's link is built from the first target segment, so a spanning reference opens at
    /// the start of its span with only that chapter's verses highlighted.
    /// </summary>
    [Fact]
    public void Reference_overload_uses_the_first_target_segment()
    {
        var reference = SpanningReference();

        Assert.Equal(
            $"{Base}/bofm/3-ne/11?lang=eng&id=p10-p11#p10",
            ScriptureUrlBuilder.Build(reference, Language.En));
    }

    [Fact]
    public void Reference_overload_can_target_a_later_chapter_of_a_span()
    {
        var reference = SpanningReference();

        Assert.Equal(
            $"{Base}/bofm/3-ne/12?lang=eng&id=p1#p1",
            ScriptureUrlBuilder.Build(reference, Language.En, 12));
    }

    /// <summary>
    /// The segment overload exists only to avoid re-grouping a reference's verses once per chapter,
    /// so it must produce exactly what the chapter-number overload does.
    /// </summary>
    [Fact]
    public void Segment_overload_matches_the_chapter_overload()
    {
        var reference = SpanningReference();

        foreach (var segment in reference.TargetSegments())
        {
            Assert.Equal(
                ScriptureUrlBuilder.Build(reference, Language.En, segment.Ch),
                ScriptureUrlBuilder.Build(reference, segment, Language.En));
        }
    }

    [Fact]
    public void Reference_overload_falls_back_to_the_chapter_when_a_reference_has_no_verses()
    {
        var reference = new Reference(
            "3 Ne. 11",
            Volume.BookOfMormon,
            "3-ne",
            "3 Nephi",
            11,
            [],
            [],
            null);

        Assert.Equal($"{Base}/bofm/3-ne/11?lang=eng", ScriptureUrlBuilder.Build(reference, Language.En));
    }

    private static Reference SpanningReference() =>
        new(
            "3 Ne. 11–12",
            Volume.BookOfMormon,
            "3-ne",
            "3 Nephi",
            11,
            [10, 11],
            [
                new ContextVerse(10, "Behold, I am Jesus Christ...", true, 11),
                new ContextVerse(11, "And behold, I am the light...", true, 11),
                new ContextVerse(1, "And it came to pass...", true, 12),
            ],
            null,
            12);
}
