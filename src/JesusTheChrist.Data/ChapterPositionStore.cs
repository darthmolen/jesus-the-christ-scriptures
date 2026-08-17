using SQLite;

namespace JesusTheChrist.Data;

/// <summary>
/// Persists and queries the chapter a reader was last studying inside a multi-chapter reference,
/// so a long span such as 3 Nephi 11–26 reopens where they left off rather than at its first chapter.
/// </summary>
public sealed class ChapterPositionStore
{
    private readonly SQLiteAsyncConnection connection;
    private readonly Func<DateTime> utcNow;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChapterPositionStore"/> class.
    /// </summary>
    /// <param name="db">The application database.</param>
    /// <param name="utcNow">A clock for the update timestamp; defaults to <see cref="DateTime.UtcNow"/>.</param>
    public ChapterPositionStore(AppDatabase db, Func<DateTime>? utcNow = null)
    {
        ArgumentNullException.ThrowIfNull(db);
        this.connection = db.Connection;
        this.utcNow = utcNow ?? (() => DateTime.UtcNow);
    }

    /// <summary>
    /// Saves the chapter a reader opened within a reference, overwriting any previous one.
    /// </summary>
    /// <param name="refId">The reference's language-invariant identifier.</param>
    /// <param name="ch">The chapter the reader opened.</param>
    /// <returns>A task that completes when the position is saved.</returns>
    public Task SaveAsync(string refId, int ch) =>
        this.connection.InsertOrReplaceAsync(new ChapterPosition
        {
            RefId = refId,
            Ch = ch,
            UpdatedAtUtc = this.utcNow(),
        });

    /// <summary>
    /// Gets a reference's saved chapter, or <see langword="null"/> if none.
    /// </summary>
    /// <param name="refId">The reference's language-invariant identifier.</param>
    /// <returns>The saved chapter, or <see langword="null"/>.</returns>
    public async Task<int?> GetAsync(string refId) =>
        (await this.connection.FindAsync<ChapterPosition>(refId))?.Ch;

    /// <summary>
    /// Gets every saved chapter position, keyed by reference identifier. Read once per feed load so
    /// a topic costs one query rather than one per card.
    /// </summary>
    /// <returns>The saved chapter for each reference that has one.</returns>
    public async Task<Dictionary<string, int>> GetAllAsync()
    {
        var rows = await this.connection.Table<ChapterPosition>().ToListAsync();
        return rows.ToDictionary(r => r.RefId, r => r.Ch, StringComparer.Ordinal);
    }
}
