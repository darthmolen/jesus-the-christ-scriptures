using SQLite;

namespace JesusTheChrist.Data;

/// <summary>
/// Persists and queries the last reading position (top-of-feed reference) per topic.
/// </summary>
public sealed class TopicPositionStore
{
    private readonly SQLiteAsyncConnection connection;
    private readonly Func<DateTime> utcNow;

    /// <summary>
    /// Initializes a new instance of the <see cref="TopicPositionStore"/> class.
    /// </summary>
    /// <param name="db">The application database.</param>
    /// <param name="utcNow">A clock for the update timestamp; defaults to <see cref="DateTime.UtcNow"/>.</param>
    public TopicPositionStore(AppDatabase db, Func<DateTime>? utcNow = null)
    {
        ArgumentNullException.ThrowIfNull(db);
        this.connection = db.Connection;
        this.utcNow = utcNow ?? (() => DateTime.UtcNow);
    }

    /// <summary>
    /// Saves the reference last seen at the top of a topic's feed, overwriting any previous one.
    /// </summary>
    /// <param name="topicKey">The sub-topic's language-invariant key.</param>
    /// <param name="refId">The reference identifier at the top of the feed.</param>
    /// <returns>A task that completes when the position is saved.</returns>
    public Task SaveAsync(string topicKey, string refId) =>
        this.connection.InsertOrReplaceAsync(new TopicPosition
        {
            TopicKey = topicKey,
            RefId = refId,
            UpdatedAtUtc = this.utcNow(),
        });

    /// <summary>
    /// Gets a topic's saved reference position, or <see langword="null"/> if none.
    /// </summary>
    /// <param name="topicKey">The sub-topic's language-invariant key.</param>
    /// <returns>The saved reference identifier, or <see langword="null"/>.</returns>
    public async Task<string?> GetAsync(string topicKey) =>
        (await this.connection.FindAsync<TopicPosition>(topicKey))?.RefId;
}
