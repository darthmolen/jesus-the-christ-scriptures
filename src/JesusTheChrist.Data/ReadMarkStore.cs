using SQLite;

namespace JesusTheChrist.Data;

/// <summary>
/// Persists and queries which references have been marked read.
/// </summary>
public sealed class ReadMarkStore
{
    private readonly SQLiteAsyncConnection connection;
    private readonly Func<DateTime> utcNow;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReadMarkStore"/> class.
    /// </summary>
    /// <param name="db">The application database.</param>
    /// <param name="utcNow">A clock for the read timestamp; defaults to <see cref="DateTime.UtcNow"/>.</param>
    public ReadMarkStore(AppDatabase db, Func<DateTime>? utcNow = null)
    {
        ArgumentNullException.ThrowIfNull(db);
        this.connection = db.Connection;
        this.utcNow = utcNow ?? (() => DateTime.UtcNow);
    }

    /// <summary>
    /// Marks a reference read (idempotent).
    /// </summary>
    /// <param name="refId">The reference identifier.</param>
    /// <returns>A task that completes when the mark is persisted.</returns>
    public Task MarkReadAsync(string refId) =>
        this.connection.InsertOrReplaceAsync(new ReadMark { RefId = refId, ReadAtUtc = this.utcNow() });

    /// <summary>
    /// Removes the read mark for a reference.
    /// </summary>
    /// <param name="refId">The reference identifier.</param>
    /// <returns>A task that completes when the mark is removed.</returns>
    public Task UnmarkAsync(string refId) => this.connection.DeleteAsync<ReadMark>(refId);

    /// <summary>
    /// Determines whether a reference is marked read.
    /// </summary>
    /// <param name="refId">The reference identifier.</param>
    /// <returns><see langword="true"/> if the reference is marked read.</returns>
    public async Task<bool> IsReadAsync(string refId) =>
        await this.connection.FindAsync<ReadMark>(refId) is not null;

    /// <summary>
    /// Counts the read references.
    /// </summary>
    /// <returns>The number of read marks.</returns>
    public Task<int> CountAsync() => this.connection.Table<ReadMark>().CountAsync();

    /// <summary>
    /// Gets the set of all read reference identifiers.
    /// </summary>
    /// <returns>The read reference identifiers.</returns>
    public async Task<HashSet<string>> GetReadIdsAsync() =>
        (await this.connection.Table<ReadMark>().ToListAsync()).Select(r => r.RefId).ToHashSet();
}
