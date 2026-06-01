using JesusTheChrist.Core.Models;

namespace JesusTheChrist.Core.Tests;

public sealed class ReferenceGlossTests
{
    [Fact]
    public void TargetText_joins_only_target_verses()
    {
        var r = Ref(null, (15, "before", false), (16, "For God so loved", true), (17, "after", false));
        Assert.Equal("For God so loved", r.TargetText);
    }

    [Fact]
    public void ShowGloss_false_when_note_null() =>
        Assert.False(Ref(null, (16, "x", true)).ShowGloss);

    [Fact]
    public void ShowGloss_false_when_note_contained_in_verse() =>
        Assert.False(Ref("God so loved", (16, "For God so loved the world", true)).ShowGloss);

    [Fact]
    public void ShowGloss_true_when_note_adds_information() =>
        Assert.True(Ref("His birth is foretold", (16, "For God so loved the world", true)).ShowGloss);

    private static Reference Ref(string? note, params (int Vs, string Text, bool Target)[] ctx) =>
        new(
            "X",
            Volume.NewTestament,
            "john",
            "John",
            3,
            [16],
            [.. ctx.Select(c => new ContextVerse(c.Vs, c.Text, c.Target))],
            note);
}
