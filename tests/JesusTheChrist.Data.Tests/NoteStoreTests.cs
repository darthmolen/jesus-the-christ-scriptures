using System;
using System.Threading.Tasks;
using JesusTheChrist.Data;
using Xunit;

namespace JesusTheChrist.Data.Tests;

public class NoteStoreTests
{
    static readonly DateTime Fixed = new(2026, 5, 31, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Save_get_and_overwrite()
    {
        await using var t = await TestDb.CreateAsync();
        var store = new NoteStore(t.Db, () => Fixed);

        Assert.Null(await store.GetAsync("r"));
        await store.SaveAsync("r", "  first thought  ");
        Assert.Equal("first thought", await store.GetAsync("r")); // trimmed
        Assert.True(await store.HasNoteAsync("r"));

        await store.SaveAsync("r", "second");
        Assert.Equal("second", await store.GetAsync("r"));
    }

    [Fact]
    public async Task Empty_text_deletes_the_note()
    {
        await using var t = await TestDb.CreateAsync();
        var store = new NoteStore(t.Db, () => Fixed);

        await store.SaveAsync("r", "something");
        await store.SaveAsync("r", "   "); // whitespace clears
        Assert.False(await store.HasNoteAsync("r"));
        Assert.Null(await store.GetAsync("r"));
    }
}
