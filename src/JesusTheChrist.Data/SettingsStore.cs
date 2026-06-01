using System.Globalization;
using SQLite;

namespace JesusTheChrist.Data;

/// <summary>
/// Reads and writes key/value application settings, with typed helpers.
/// </summary>
public sealed class SettingsStore
{
    private readonly SQLiteAsyncConnection connection;

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsStore"/> class.
    /// </summary>
    /// <param name="db">The application database.</param>
    public SettingsStore(AppDatabase db)
    {
        ArgumentNullException.ThrowIfNull(db);
        this.connection = db.Connection;
    }

    /// <summary>
    /// Gets a setting value, or <see langword="null"/> if unset.
    /// </summary>
    /// <param name="key">The setting key.</param>
    /// <returns>The value, or <see langword="null"/>.</returns>
    public async Task<string?> GetAsync(string key) =>
        (await this.connection.FindAsync<Setting>(key))?.Value;

    /// <summary>
    /// Sets a setting value.
    /// </summary>
    /// <param name="key">The setting key.</param>
    /// <param name="value">The value to store.</param>
    /// <returns>A task that completes when the value is persisted.</returns>
    public Task SetAsync(string key, string value) =>
        this.connection.InsertOrReplaceAsync(new Setting { Key = key, Value = value });

    /// <summary>
    /// Gets an integer setting, or a fallback if unset or unparsable.
    /// </summary>
    /// <param name="key">The setting key.</param>
    /// <param name="fallback">The value to return when unset or unparsable.</param>
    /// <returns>The parsed value or the fallback.</returns>
    public async Task<int> GetIntAsync(string key, int fallback) =>
        int.TryParse(await this.GetAsync(key), NumberStyles.Integer, CultureInfo.InvariantCulture, out var v)
            ? v
            : fallback;

    /// <summary>
    /// Sets an integer setting.
    /// </summary>
    /// <param name="key">The setting key.</param>
    /// <param name="value">The value to store.</param>
    /// <returns>A task that completes when the value is persisted.</returns>
    public Task SetIntAsync(string key, int value) =>
        this.SetAsync(key, value.ToString(CultureInfo.InvariantCulture));

    /// <summary>
    /// Gets a boolean setting, or a fallback if unset or unparsable.
    /// </summary>
    /// <param name="key">The setting key.</param>
    /// <param name="fallback">The value to return when unset or unparsable.</param>
    /// <returns>The parsed value or the fallback.</returns>
    public async Task<bool> GetBoolAsync(string key, bool fallback) =>
        bool.TryParse(await this.GetAsync(key), out var v) ? v : fallback;

    /// <summary>
    /// Sets a boolean setting.
    /// </summary>
    /// <param name="key">The setting key.</param>
    /// <param name="value">The value to store.</param>
    /// <returns>A task that completes when the value is persisted.</returns>
    public Task SetBoolAsync(string key, bool value) =>
        this.SetAsync(key, value ? "true" : "false");
}
