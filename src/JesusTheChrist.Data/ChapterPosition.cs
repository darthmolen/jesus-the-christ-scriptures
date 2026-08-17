using SQLite;

namespace JesusTheChrist.Data;

/// <summary>
/// The chapter a reader was last studying inside a long, multi-chapter reference. Only references
/// that span chapters and are too large to open all at once record one, so this table stays tiny.
/// </summary>
public class ChapterPosition
{
    /// <summary>
    /// Gets or sets the reference's language-invariant identifier (primary key).
    /// </summary>
    [PrimaryKey]
    public string RefId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the chapter the reader last opened within the reference.
    /// </summary>
    public int Ch { get; set; }

    /// <summary>
    /// Gets or sets the UTC time the position was last updated.
    /// </summary>
    public DateTime UpdatedAtUtc { get; set; }
}
