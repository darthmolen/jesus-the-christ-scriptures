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
