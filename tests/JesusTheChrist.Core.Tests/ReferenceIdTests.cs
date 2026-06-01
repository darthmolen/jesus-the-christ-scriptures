using JesusTheChrist.Core.Models;

namespace JesusTheChrist.Core.Tests;

public sealed class ReferenceIdTests
{
    [Fact]
    public void Id_single_verse_uses_language_invariant_key() =>
        Assert.Equal("advocate:newtestament/john/3/16", Ref([16]).Id("advocate"));

    [Fact]
    public void Id_verse_range_uses_first_and_last() =>
        Assert.Equal("summary:newtestament/john/3/16-18", Ref([16, 17, 18]).Id("summary"));

    private static Reference Ref(int[] verses) =>
        new(
            RefLabel: "X",
            Vol: Volume.NewTestament,
            Book: "john",
            BookTitle: "John",
            Ch: 3,
            Verses: verses,
            Context: [],
            Note: null);
}
