using System;
using System.Threading.Tasks;
using JesusTheChrist.Data;
using Xunit;

namespace JesusTheChrist.Data.Tests;

public class StreakServiceTests
{
    static readonly DateOnly Day1 = new(2026, 5, 31);
    static readonly DateOnly Day2 = new(2026, 6, 1);
    static readonly DateOnly Day4 = new(2026, 6, 3);

    [Fact]
    public void First_read_starts_streak_at_one()
    {
        var s = new StreakService().Advance(default, Day1);
        Assert.Equal(new StreakState(1, 1, Day1), s);
    }

    [Fact]
    public void Same_day_does_not_change()
    {
        var svc = new StreakService();
        var s1 = svc.Advance(default, Day1);
        var s2 = svc.Advance(s1, Day1);
        Assert.Equal(s1, s2);
    }

    [Fact]
    public void Consecutive_day_increments_and_tracks_best()
    {
        var svc = new StreakService();
        var s = svc.Advance(svc.Advance(default, Day1), Day2);
        Assert.Equal(new StreakState(2, 2, Day2), s);
    }

    [Fact]
    public void Gap_resets_current_but_keeps_best()
    {
        var svc = new StreakService();
        var twoDay = svc.Advance(svc.Advance(default, Day1), Day2); // best 2
        var afterGap = svc.Advance(twoDay, Day4);                   // skipped Day3
        Assert.Equal(new StreakState(1, 2, Day4), afterGap);
    }

    [Fact]
    public void Backwards_date_is_ignored_clock_skew()
    {
        var svc = new StreakService();
        var atDay2 = svc.Advance(svc.Advance(default, Day1), Day2);
        var backwards = svc.Advance(atDay2, Day1); // today < last -> no-op
        Assert.Equal(atDay2, backwards);
    }

    [Fact]
    public async Task Store_roundtrips_state_via_settings()
    {
        await using var t = await TestDb.CreateAsync();
        var store = new StreakStore(new SettingsStore(t.Db));

        Assert.Equal(default, await store.LoadAsync());
        await store.SaveAsync(new StreakState(3, 5, Day2));
        Assert.Equal(new StreakState(3, 5, Day2), await store.LoadAsync());
    }
}
