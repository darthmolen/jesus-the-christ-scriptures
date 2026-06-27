using System.Text;
using JesusTheChrist.Core.Json;
using JesusTheChrist.Core.Models;

namespace JesusTheChrist.Core.Tests;

public sealed class TopicalGuideLoaderTests
{
    [Fact]
    public void Loads_subtopics_in_order_with_invariant_keys()
    {
        var g = Load();
        Assert.Equal("Jesus Christ", g.Topic);
        Assert.Equal(["Summary", "Advocate"], g.SubTopics.Select(t => t.ShortTitle).ToArray());
        Assert.Equal(["summary", "advocate"], g.SubTopics.Select(t => t.Key).ToArray());
    }

    [Fact]
    public void Maps_reference_fields()
    {
        var r = Load().SubTopics[1].References[0];
        Assert.Equal("Heb. 7:25", r.RefLabel);
        Assert.Equal(Volume.NewTestament, r.Vol);
        Assert.Equal("heb", r.Book);
        Assert.Equal(7, r.Ch);
        Assert.Equal([25], r.Verses.ToArray());
        Assert.Single(r.Context);
        Assert.True(r.Context[0].Target);
    }

    [Fact]
    public void Maps_cross_chapter_fields()
    {
        const string json =
            """
            {
              "topic": "Jesus Christ", "language": "en",
              "subtopics": [ { "title": "Jesus Christ", "short": "Summary", "references": [
                { "ref": "Matt. 9:35-11:1", "vol": "newtestament", "book": "matt",
                  "book_title": "Matthew", "ch": 9, "end_ch": 11, "verses": [35],
                  "context": [
                    { "vs": 35, "text": "and jesus went about", "target": true, "ch": 9 },
                    { "vs": 1, "text": "and when he had called", "target": true, "ch": 10 }
                  ],
                  "note": "sends disciples forth by twos" }
              ] } ]
            }
            """;
        using var s = new MemoryStream(Encoding.UTF8.GetBytes(json));

        var r = TopicalGuideLoader.Load(s).SubTopics[0].References[0];

        Assert.Equal(9, r.Ch);
        Assert.Equal(11, r.EndCh);
        Assert.True(r.SpansChapters);
        Assert.Equal(9, r.Context[0].Ch);
        Assert.Equal(10, r.Context[1].Ch);
    }

    [Fact]
    public void Single_chapter_reference_has_null_end_chapter()
    {
        var r = Load().SubTopics[1].References[0];
        Assert.Null(r.EndCh);
        Assert.False(r.SpansChapters);
        Assert.Null(r.Context[0].Ch);
    }

    [Fact]
    public void Load_null_json_throws_invalid_data()
    {
        using var s = new MemoryStream(Encoding.UTF8.GetBytes("null"));
        Assert.Throws<InvalidDataException>(() => TopicalGuideLoader.Load(s));
    }

    [Fact]
    public void Load_malformed_json_throws()
    {
        using var s = new MemoryStream(Encoding.UTF8.GetBytes("{ this is not json"));
        Assert.ThrowsAny<System.Exception>(() => TopicalGuideLoader.Load(s));
    }

    private static TopicalGuide Load()
    {
        using var s = File.OpenRead(Path.Combine("Fixtures", "sample.json"));
        return TopicalGuideLoader.Load(s);
    }
}
