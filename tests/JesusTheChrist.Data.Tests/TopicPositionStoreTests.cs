using JesusTheChrist.Data;

namespace JesusTheChrist.Data.Tests;

public sealed class TopicPositionStoreTests
{
    private static readonly DateTime Fixed = new(2026, 6, 25, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task GetAsync_returns_null_when_nothing_saved()
    {
        await using var t = await TestDb.CreateAsync();
        var store = new TopicPositionStore(t.Db, () => Fixed);

        Assert.Null(await store.GetAsync("advocate"));
    }

    [Fact]
    public async Task Save_then_get_returns_the_saved_ref()
    {
        await using var t = await TestDb.CreateAsync();
        var store = new TopicPositionStore(t.Db, () => Fixed);

        await store.SaveAsync("advocate", "advocate:newtestament/heb/7/25");

        Assert.Equal("advocate:newtestament/heb/7/25", await store.GetAsync("advocate"));
    }

    [Fact]
    public async Task Save_overwrites_previous_position_for_same_topic()
    {
        await using var t = await TestDb.CreateAsync();
        var store = new TopicPositionStore(t.Db, () => Fixed);

        await store.SaveAsync("advocate", "advocate:newtestament/heb/7/25");
        await store.SaveAsync("advocate", "advocate:newtestament/1-jn/2/1");

        Assert.Equal("advocate:newtestament/1-jn/2/1", await store.GetAsync("advocate"));
    }

    [Fact]
    public async Task Positions_are_independent_per_topic()
    {
        await using var t = await TestDb.CreateAsync();
        var store = new TopicPositionStore(t.Db, () => Fixed);

        await store.SaveAsync("advocate", "advocate:newtestament/heb/7/25");
        await store.SaveAsync("creator", "creator:oldtestament/gen/1/1");

        Assert.Equal("advocate:newtestament/heb/7/25", await store.GetAsync("advocate"));
        Assert.Equal("creator:oldtestament/gen/1/1", await store.GetAsync("creator"));
    }
}
