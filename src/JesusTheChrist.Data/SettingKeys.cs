namespace JesusTheChrist.Data;

/// <summary>
/// Well-known setting keys (centralized to avoid key drift).
/// </summary>
public static class SettingKeys
{
    /// <summary>The reading theme: <c>light</c>, <c>dark</c>, or <c>system</c>.</summary>
    public const string Theme = "theme";

    /// <summary>The reading font family.</summary>
    public const string FontFamily = "font_family";

    /// <summary>The reading font size (integer).</summary>
    public const string FontSize = "font_size";

    /// <summary>The content language code (<c>en</c> or <c>es</c>).</summary>
    public const string Language = "language";

    /// <summary>Whether the streak feature is enabled (boolean).</summary>
    public const string StreakEnabled = "streak_enabled";

    /// <summary>The current streak length (integer).</summary>
    public const string StreakCurrent = "streak_current";

    /// <summary>The best streak length (integer).</summary>
    public const string StreakBest = "streak_best";

    /// <summary>The last read date (<c>yyyy-MM-dd</c>) or empty.</summary>
    public const string StreakLastDate = "streak_last_date";
}
