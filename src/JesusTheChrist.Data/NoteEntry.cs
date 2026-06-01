using SQLite;

namespace JesusTheChrist.Data;

/// <summary>
/// A persisted personal note for a reference.
/// </summary>
public class NoteEntry
{
    /// <summary>
    /// Gets or sets the reference identifier (primary key).
    /// </summary>
    [PrimaryKey]
    public string RefId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the note text.
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the UTC time the note was last updated.
    /// </summary>
    public DateTime UpdatedAtUtc { get; set; }
}
