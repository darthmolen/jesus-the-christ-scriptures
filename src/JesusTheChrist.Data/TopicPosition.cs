using SQLite;

namespace JesusTheChrist.Data;

/// <summary>
/// A persisted reading position for a topic: the reference last seen at the top of the feed.
/// </summary>
public class TopicPosition
{
    /// <summary>
    /// Gets or sets the sub-topic's language-invariant key (primary key).
    /// </summary>
    [PrimaryKey]
    public string TopicKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the reference identifier last seen at the top of the feed.
    /// </summary>
    public string RefId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the UTC time the position was last updated.
    /// </summary>
    public DateTime UpdatedAtUtc { get; set; }
}
