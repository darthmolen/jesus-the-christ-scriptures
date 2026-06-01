using JesusTheChrist.Core.Content;
using JesusTheChrist.Core.Json;
using JesusTheChrist.Core.Models;

namespace JesusTheChrist.Core.Tests;

public sealed class ScopeFilterTests
{
    [Fact]
    public void Full_keeps_everything()
    {
        var g = ScopeFilter.Apply(Load(), Scope.Full);
        Assert.Equal(2, g.SubTopics.Count);
        Assert.Equal(2, g.SubTopics[1].References.Count);
    }

    [Fact]
    public void BibleOnly_drops_non_bible_refs_and_empty_subtopics()
    {
        var g = ScopeFilter.Apply(Load(), Scope.BibleOnly);
        var advocate = g.SubTopics.Single(t => t.ShortTitle == "Advocate");
        Assert.Single(advocate.References);
        Assert.True(advocate.References.All(r => r.Vol.IsBible()));
    }

    private static TopicalGuide Load()
    {
        using var s = File.OpenRead(Path.Combine("Fixtures", "sample.json"));
        return TopicalGuideLoader.Load(s);
    }
}
