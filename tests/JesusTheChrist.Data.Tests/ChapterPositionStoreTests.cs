using JesusTheChrist.Data;

namespace JesusTheChrist.Data.Tests;

public sealed class ChapterPositionStoreTests
{
    private static readonly DateTime Fixed = new(2026, 8, 16, 12, 0, 0, DateTimeKind.Utc);

    private const string ThirdNephi = "summary:bookofmormon/3-ne/11:1-26:14";
    private const string Matthew = "summary:newtestament/matt/5:1-7:29";

    [Fact]
    public async Task GetAsync_returns_null_when_nothing_saved()
    {
        await using var t = await TestDb.CreateAsync();
        var store = new ChapterPositionStore(t.Db, () => Fixed);

        Assert.Null(await store.GetAsync(ThirdNephi));
    }

    [Fact]
    public async Task Save_then_get_returns_the_saved_chapter()
    {
        await using var t = await TestDb.CreateAsync();
        var store = new ChapterPositionStore(t.Db, () => Fixed);

        await store.SaveAsync(ThirdNephi, 18);

        Assert.Equal(18, await store.GetAsync(ThirdNephi));
    }

    [Fact]
    public async Task Save_overwrites_previous_chapter_for_same_reference()
    {
        await using var t = await TestDb.CreateAsync();
        var store = new ChapterPositionStore(t.Db, () => Fixed);

        await store.SaveAsync(ThirdNephi, 18);
        await store.SaveAsync(ThirdNephi, 20);

        Assert.Equal(20, await store.GetAsync(ThirdNephi));
    }

    [Fact]
    public async Task Positions_are_independent_per_reference()
    {
        await using var t = await TestDb.CreateAsync();
        var store = new ChapterPositionStore(t.Db, () => Fixed);

        await store.SaveAsync(ThirdNephi, 18);
        await store.SaveAsync(Matthew, 6);

        Assert.Equal(18, await store.GetAsync(ThirdNephi));
        Assert.Equal(6, await store.GetAsync(Matthew));
    }

    [Fact]
    public async Task GetAllAsync_is_empty_when_nothing_saved()
    {
        await using var t = await TestDb.CreateAsync();
        var store = new ChapterPositionStore(t.Db, () => Fixed);

        Assert.Empty(await store.GetAllAsync());
    }

    [Fact]
    public async Task GetAllAsync_returns_every_saved_position_keyed_by_reference()
    {
        await using var t = await TestDb.CreateAsync();
        var store = new ChapterPositionStore(t.Db, () => Fixed);

        await store.SaveAsync(ThirdNephi, 18);
        await store.SaveAsync(Matthew, 6);

        var all = await store.GetAllAsync();

        Assert.Equal(2, all.Count);
        Assert.Equal(18, all[ThirdNephi]);
        Assert.Equal(6, all[Matthew]);
    }
}
