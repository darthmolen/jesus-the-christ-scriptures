using System.Collections.Generic;
using System.Linq;
using JesusTheChrist.Core.Models;
using Xunit;

public class ReferenceGlossTests
{
    static Reference Ref(string? note, params (int vs, string text, bool target)[] ctx) => new(
        "X", Volume.NewTestament, "john", "John", 3,
        new[] { 16 },
        ctx.Select(c => new ContextVerse(c.vs, c.text, c.target)).ToList(),
        note);

    [Fact]
    public void TargetText_joins_only_target_verses()
    {
        var r = Ref(null, (15, "before", false), (16, "For God so loved", true), (17, "after", false));
        Assert.Equal("For God so loved", r.TargetText);
    }

    [Fact]
    public void ShowGloss_false_when_note_null()
        => Assert.False(Ref(null, (16, "x", true)).ShowGloss);

    [Fact]
    public void ShowGloss_false_when_note_contained_in_verse()
        => Assert.False(Ref("God so loved", (16, "For God so loved the world", true)).ShowGloss);

    [Fact]
    public void ShowGloss_true_when_note_adds_information()
        => Assert.True(Ref("His birth is foretold", (16, "For God so loved the world", true)).ShowGloss);
}
