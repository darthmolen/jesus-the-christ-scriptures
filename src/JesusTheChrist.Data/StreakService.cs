namespace JesusTheChrist.Data;

/// <summary>
/// Pure streak advancement: any reference read in a day counts as that day.
/// </summary>
public static class StreakService
{
    /// <summary>
    /// Advances the streak for a read on <paramref name="today"/>.
    /// </summary>
    /// <param name="prev">The previous streak state.</param>
    /// <param name="today">The date of the read.</param>
    /// <returns>The updated streak state; unchanged when <paramref name="today"/> is the same as,
    /// or earlier than, the last read date (clock skew).</returns>
    public static StreakState Advance(StreakState prev, DateOnly today)
    {
        if (prev.LastReadDate is { } prior && today <= prior)
        {
            return prev;
        }

        var current = prev.LastReadDate is { } last && last.AddDays(1) == today
            ? prev.Current + 1
            : 1;
        var best = Math.Max(prev.Best, current);
        return new StreakState(current, best, today);
    }
}
