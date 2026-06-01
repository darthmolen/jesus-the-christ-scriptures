using System.Collections.Generic;
using JesusTheChrist.Core.Models;
using Xunit;

public class ReferenceIdTests
{
    static Reference Ref(int[] verses) => new(
        RefLabel: "X", Vol: Volume.NewTestament, Book: "john", BookTitle: "John",
        Ch: 3, Verses: verses, Context: new List<ContextVerse>(), Note: null);

    [Fact]
    public void Id_single_verse_uses_language_invariant_key()
        => Assert.Equal("advocate:newtestament/john/3/16", Ref(new[] { 16 }).Id("advocate"));

    [Fact]
    public void Id_verse_range_uses_first_and_last()
        => Assert.Equal("summary:newtestament/john/3/16-18", Ref(new[] { 16, 17, 18 }).Id("summary"));
}
