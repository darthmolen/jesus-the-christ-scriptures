using JesusTheChrist.Core.Models;
using JesusTheChrist.Data;

namespace JesusTheChrist.Data.Tests;

public sealed class ProgressServiceTests
{
    [Fact]
    public void Overall_counts_read_over_total()
    {
        var g = Guide();
        var read = new HashSet<string> { R(1).Id("advocate") };
        var p = ProgressService.Overall(g, read);
        Assert.Equal(1, p.Read);
        Assert.Equal(3, p.Total);
    }

    [Fact]
    public void PerSubTopic_keyed_by_subtopic_key()
    {
        var g = Guide();
        var read = new HashSet<string> { R(1).Id("advocate"), R(2).Id("advocate") };
        var map = ProgressService.PerSubTopic(g, read);
        Assert.Equal(new Progress(2, 2), map["advocate"]);
        Assert.Equal(new Progress(0, 1), map["creator"]);
    }

    private static Reference R(int v) =>
        new("X", Volume.NewTestament, "john", "John", 3, [v], [], null);

    private static TopicalGuide Guide() =>
        new(
            "Jesus Christ",
            "en",
            [
                new SubTopic("advocate", "Jesus Christ, Advocate", "Advocate", [R(1), R(2)]),
                new SubTopic("creator", "Jesus Christ, Creator", "Creator", [R(3)]),
            ]);
}
