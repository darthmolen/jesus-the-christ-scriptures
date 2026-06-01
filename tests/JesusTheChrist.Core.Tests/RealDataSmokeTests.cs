using JesusTheChrist.Core.Json;
using JesusTheChrist.Core.Models;

namespace JesusTheChrist.Core.Tests;

public sealed class RealDataSmokeTests
{
    [Fact]
    public void Has_expected_topic_and_counts()
    {
        var g = Load();
        Assert.Equal("Jesus Christ", g.Topic);
        Assert.Equal(53, g.SubTopics.Count);
        Assert.Equal(2196, g.SubTopics.Sum(t => t.References.Count));
    }

    [Fact]
    public void Subtopic_keys_are_unique_and_non_empty()
    {
        var keys = Load().SubTopics.Select(t => t.Key).ToList();
        Assert.DoesNotContain(string.Empty, keys);
        Assert.Equal(keys.Count, keys.Distinct().Count());
    }

    // The Topical Guide orders references THEMATICALLY, not by book/chapter — e.g. under
    // "Atonement through" it places Mosiah 14:6 right after Isa. 53:6 because Mosiah 14 quotes
    // Isaiah 53. The loader must preserve that source order exactly.
    [Fact]
    public void Preserves_the_topical_guides_thematic_order()
    {
        var t = Load().SubTopics.Single(x => x.ShortTitle == "Atonement through");
        var first3 = t.References.Take(3).Select(r => (r.Vol, r.Book, r.Ch)).ToArray();

        Assert.Equal(
            [
                (Volume.OldTestament, "lev", 17),
                (Volume.OldTestament, "isa", 53),
                (Volume.BookOfMormon, "mosiah", 14),
            ],
            first3);
    }

    private static TopicalGuide Load()
    {
        using var s = File.OpenRead(Path.Combine("RealData", "jesus-christ.en.json"));
        return TopicalGuideLoader.Load(s);
    }
}
