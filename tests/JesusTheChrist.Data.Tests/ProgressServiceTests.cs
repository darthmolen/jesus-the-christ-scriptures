using System.Collections.Generic;
using JesusTheChrist.Core.Models;
using JesusTheChrist.Data;
using Xunit;

namespace JesusTheChrist.Data.Tests;

public class ProgressServiceTests
{
    static Reference R(int v) => new("X", Volume.NewTestament, "john", "John", 3,
        new[] { v }, new List<ContextVerse>(), null);

    static TopicalGuide Guide() => new("Jesus Christ", "en", new[]
    {
        new SubTopic("advocate", "Jesus Christ, Advocate", "Advocate", new[] { R(1), R(2) }),
        new SubTopic("creator",  "Jesus Christ, Creator",  "Creator",  new[] { R(3) }),
    });

    [Fact]
    public void Overall_counts_read_over_total()
    {
        var g = Guide();
        var read = new HashSet<string> { R(1).Id("advocate") }; // 1 of 3
        var p = new ProgressService().Overall(g, read);
        Assert.Equal(1, p.Read);
        Assert.Equal(3, p.Total);
    }

    [Fact]
    public void PerSubTopic_keyed_by_subtopic_key()
    {
        var g = Guide();
        var read = new HashSet<string> { R(1).Id("advocate"), R(2).Id("advocate") };
        var map = new ProgressService().PerSubTopic(g, read);
        Assert.Equal(new Progress(2, 2), map["advocate"]);
        Assert.Equal(new Progress(0, 1), map["creator"]);
    }
}
