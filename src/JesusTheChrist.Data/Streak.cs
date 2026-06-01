using System;
using System.Globalization;
using System.Threading.Tasks;

namespace JesusTheChrist.Data;

public readonly record struct StreakState(int Current, int Best, DateOnly? LastReadDate);

public sealed class StreakService
{
    // "Any reference read in a day counts." today is supplied by the caller (injected clock).
    public StreakState Advance(StreakState prev, DateOnly today)
    {
        // Same day or a backwards date (clock skew): ignore, keep prior state.
        if (prev.LastReadDate is { } prior && today <= prior) return prev;

        int current = prev.LastReadDate is { } last && last.AddDays(1) == today
            ? prev.Current + 1
            : 1;
        int best = Math.Max(prev.Best, current);
        return new StreakState(current, best, today);
    }
}

// Persists StreakState via SettingsStore (keys centralized in SettingKeys).
public sealed class StreakStore
{
    private readonly SettingsStore _settings;
    public StreakStore(SettingsStore settings) => _settings = settings;

    public async Task<StreakState> LoadAsync()
    {
        var current = await _settings.GetIntAsync(SettingKeys.StreakCurrent, 0);
        var best = await _settings.GetIntAsync(SettingKeys.StreakBest, 0);
        var lastRaw = await _settings.GetAsync(SettingKeys.StreakLastDate);
        DateOnly? last = DateOnly.TryParseExact(lastRaw, "yyyy-MM-dd",
            CultureInfo.InvariantCulture, DateTimeStyles.None, out var d) ? d : null;
        return new StreakState(current, best, last);
    }

    public async Task SaveAsync(StreakState s)
    {
        await _settings.SetIntAsync(SettingKeys.StreakCurrent, s.Current);
        await _settings.SetIntAsync(SettingKeys.StreakBest, s.Best);
        await _settings.SetAsync(SettingKeys.StreakLastDate,
            s.LastReadDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "");
    }
}
