using System.IO;
using System.Linq;
using JesusTheChrist.Core.Json;
using JesusTheChrist.Core.Models;
using Xunit;

public class RealDataSmokeTests
{
    static TopicalGuide Load()
    {
        using var s = File.OpenRead(Path.Combine("RealData", "jesus-christ.en.json"));
        return TopicalGuideLoader.Load(s);
    }

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
        Assert.DoesNotContain("", keys);
        Assert.Equal(keys.Count, keys.Distinct().Count());
    }

    // The Topical Guide orders references THEMATICALLY, not by book/chapter — e.g. under
    // "Atonement through" it places Mosiah 14:6 right after Isa. 53:6 because Mosiah 14 quotes
    // Isaiah 53. The loader must preserve that source order exactly (a canonical re-sort would
    // scatter these pairings). This test guards against an accidental re-sort.
    [Fact]
    public void Preserves_the_topical_guides_thematic_order()
    {
        var t = Load().SubTopics.Single(x => x.Short == "Atonement through");
        var first3 = t.References.Take(3).Select(r => (r.Vol, r.Book, r.Ch)).ToArray();

        Assert.Equal(new[]
        {
            (Volume.OldTestament, "lev", 17),
            (Volume.OldTestament, "isa", 53),
            (Volume.BookOfMormon, "mosiah", 14), // BoM at position 3 — thematic, not canonical
        }, first3);
    }
}
