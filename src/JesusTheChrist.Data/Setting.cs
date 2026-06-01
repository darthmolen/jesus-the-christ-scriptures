using SQLite;

namespace JesusTheChrist.Data;

/// <summary>
/// A persisted key/value application setting.
/// </summary>
public class Setting
{
    /// <summary>
    /// Gets or sets the setting key (primary key).
    /// </summary>
    [PrimaryKey]
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the setting value.
    /// </summary>
    public string Value { get; set; } = string.Empty;
}
