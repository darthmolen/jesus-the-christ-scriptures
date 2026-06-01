using System.Globalization;
using System.Threading.Tasks;
using SQLite;

namespace JesusTheChrist.Data;

public static class SettingKeys
{
    public const string Theme = "theme";              // "light" | "dark" | "system"
    public const string FontFamily = "font_family";
    public const string FontSize = "font_size";       // int
    public const string Language = "language";        // "en" | "es"
    public const string StreakEnabled = "streak_enabled"; // bool

    // Streak persistence (written by StreakStore; centralized here to avoid key drift).
    public const string StreakCurrent = "streak_current";    // int
    public const string StreakBest = "streak_best";          // int
    public const string StreakLastDate = "streak_last_date"; // yyyy-MM-dd or empty
}

public sealed class SettingsStore
{
    private readonly SQLiteAsyncConnection _c;
    public SettingsStore(AppDatabase db) => _c = db.Connection;

    public async Task<string?> GetAsync(string key) =>
        (await _c.FindAsync<Setting>(key))?.Value;

    public Task SetAsync(string key, string value) =>
        _c.InsertOrReplaceAsync(new Setting { Key = key, Value = value });

    public async Task<int> GetIntAsync(string key, int fallback) =>
        int.TryParse(await GetAsync(key), NumberStyles.Integer, CultureInfo.InvariantCulture, out var v)
            ? v : fallback;

    public Task SetIntAsync(string key, int value) =>
        SetAsync(key, value.ToString(CultureInfo.InvariantCulture));

    public async Task<bool> GetBoolAsync(string key, bool fallback) =>
        bool.TryParse(await GetAsync(key), out var v) ? v : fallback;

    public Task SetBoolAsync(string key, bool value) =>
        SetAsync(key, value ? "true" : "false");
}
