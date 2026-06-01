using System.Globalization;

namespace JesusTheChrist.Data;

/// <summary>
/// Persists <see cref="StreakState"/> using <see cref="SettingsStore"/>.
/// </summary>
public sealed class StreakStore
{
    private readonly SettingsStore settings;

    /// <summary>
    /// Initializes a new instance of the <see cref="StreakStore"/> class.
    /// </summary>
    /// <param name="settings">The settings store used for persistence.</param>
    public StreakStore(SettingsStore settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        this.settings = settings;
    }

    /// <summary>
    /// Loads the persisted streak state (defaults when unset).
    /// </summary>
    /// <returns>The stored streak state.</returns>
    public async Task<StreakState> LoadAsync()
    {
        var current = await this.settings.GetIntAsync(SettingKeys.StreakCurrent, 0);
        var best = await this.settings.GetIntAsync(SettingKeys.StreakBest, 0);
        var lastRaw = await this.settings.GetAsync(SettingKeys.StreakLastDate);
        DateOnly? last = DateOnly.TryParseExact(
            lastRaw, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d)
            ? d
            : null;
        return new StreakState(current, best, last);
    }

    /// <summary>
    /// Saves the streak state.
    /// </summary>
    /// <param name="state">The state to persist.</param>
    /// <returns>A task that completes when the state is persisted.</returns>
    public async Task SaveAsync(StreakState state)
    {
        await this.settings.SetIntAsync(SettingKeys.StreakCurrent, state.Current);
        await this.settings.SetIntAsync(SettingKeys.StreakBest, state.Best);
        await this.settings.SetAsync(
            SettingKeys.StreakLastDate,
            state.LastReadDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? string.Empty);
    }
}
