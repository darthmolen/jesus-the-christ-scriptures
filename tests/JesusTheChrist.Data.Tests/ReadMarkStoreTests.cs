using JesusTheChrist.Data;

namespace JesusTheChrist.Data.Tests;

public sealed class ReadMarkStoreTests
{
    private static readonly DateTime Fixed = new(2026, 5, 31, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Mark_then_isread_count_and_ids()
    {
        await using var t = await TestDb.CreateAsync();
        var store = new ReadMarkStore(t.Db, () => Fixed);

        Assert.False(await store.IsReadAsync("a:nt/john/3/16"));
        await store.MarkReadAsync("a:nt/john/3/16");
        await store.MarkReadAsync("b:nt/john/1/1");

        Assert.True(await store.IsReadAsync("a:nt/john/3/16"));
        Assert.Equal(2, await store.CountAsync());
        Assert.Contains("b:nt/john/1/1", await store.GetReadIdsAsync());
    }

    [Fact]
    public async Task Mark_is_idempotent_and_unmark_removes()
    {
        await using var t = await TestDb.CreateAsync();
        var store = new ReadMarkStore(t.Db, () => Fixed);

        await store.MarkReadAsync("x");
        await store.MarkReadAsync("x");
        Assert.Equal(1, await store.CountAsync());

        await store.UnmarkAsync("x");
        Assert.False(await store.IsReadAsync("x"));
        Assert.Equal(0, await store.CountAsync());
    }
}
