using SQLite;

namespace JesusTheChrist.Data;

/// <summary>
/// A persisted mark recording that a reference has been read.
/// </summary>
public class ReadMark
{
    /// <summary>
    /// Gets or sets the reference identifier (primary key).
    /// </summary>
    [PrimaryKey]
    public string RefId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the UTC time the reference was marked read.
    /// </summary>
    public DateTime ReadAtUtc { get; set; }
}
