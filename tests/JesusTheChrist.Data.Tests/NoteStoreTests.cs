using JesusTheChrist.Data;

namespace JesusTheChrist.Data.Tests;

public sealed class NoteStoreTests
{
    private static readonly DateTime Fixed = new(2026, 5, 31, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Save_get_and_overwrite()
    {
        await using var t = await TestDb.CreateAsync();
        var store = new NoteStore(t.Db, () => Fixed);

        Assert.Null(await store.GetAsync("r"));
        await store.SaveAsync("r", "  first thought  ");
        Assert.Equal("first thought", await store.GetAsync("r"));
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
        await store.SaveAsync("r", "   ");
        Assert.False(await store.HasNoteAsync("r"));
        Assert.Null(await store.GetAsync("r"));
    }

    [Fact]
    public async Task GetNoteIds_returns_only_referenced_with_notes()
    {
        await using var t = await TestDb.CreateAsync();
        var store = new NoteStore(t.Db, () => Fixed);

        await store.SaveAsync("a", "note a");
        await store.SaveAsync("b", "note b");
        await store.SaveAsync("b", "   "); // deletes b

        var ids = await store.GetNoteIdsAsync();

        Assert.Equal(new HashSet<string> { "a" }, ids);
    }
}
