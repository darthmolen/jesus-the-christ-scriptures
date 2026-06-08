using SQLite;

namespace JesusTheChrist.Data;

/// <summary>
/// Persists and queries personal notes keyed by reference identifier.
/// </summary>
public sealed class NoteStore
{
    private readonly SQLiteAsyncConnection connection;
    private readonly Func<DateTime> utcNow;

    /// <summary>
    /// Initializes a new instance of the <see cref="NoteStore"/> class.
    /// </summary>
    /// <param name="db">The application database.</param>
    /// <param name="utcNow">A clock for the update timestamp; defaults to <see cref="DateTime.UtcNow"/>.</param>
    public NoteStore(AppDatabase db, Func<DateTime>? utcNow = null)
    {
        ArgumentNullException.ThrowIfNull(db);
        this.connection = db.Connection;
        this.utcNow = utcNow ?? (() => DateTime.UtcNow);
    }

    /// <summary>
    /// Saves a note, trimming whitespace; empty text deletes the note.
    /// </summary>
    /// <param name="refId">The reference identifier.</param>
    /// <param name="text">The note text.</param>
    /// <returns>A task that completes when the note is saved or removed.</returns>
    public async Task SaveAsync(string refId, string text)
    {
        var trimmed = (text ?? string.Empty).Trim();
        if (trimmed.Length == 0)
        {
            await this.DeleteAsync(refId);
            return;
        }

        await this.connection.InsertOrReplaceAsync(new NoteEntry
        {
            RefId = refId,
            Text = trimmed,
            UpdatedAtUtc = this.utcNow(),
        });
    }

    /// <summary>
    /// Gets a reference's note text, or <see langword="null"/> if none.
    /// </summary>
    /// <param name="refId">The reference identifier.</param>
    /// <returns>The note text, or <see langword="null"/>.</returns>
    public async Task<string?> GetAsync(string refId) =>
        (await this.connection.FindAsync<NoteEntry>(refId))?.Text;

    /// <summary>
    /// Deletes a reference's note.
    /// </summary>
    /// <param name="refId">The reference identifier.</param>
    /// <returns>A task that completes when the note is removed.</returns>
    public Task DeleteAsync(string refId) => this.connection.DeleteAsync<NoteEntry>(refId);

    /// <summary>
    /// Determines whether a reference has a saved note.
    /// </summary>
    /// <param name="refId">The reference identifier.</param>
    /// <returns><see langword="true"/> if a note exists.</returns>
    public async Task<bool> HasNoteAsync(string refId) =>
        await this.connection.FindAsync<NoteEntry>(refId) is not null;

    /// <summary>
    /// Gets the set of reference identifiers that have a saved note, in a single query.
    /// </summary>
    /// <returns>The reference identifiers with notes.</returns>
    public async Task<HashSet<string>> GetNoteIdsAsync() =>
        (await this.connection.Table<NoteEntry>().ToListAsync()).Select(n => n.RefId).ToHashSet();
}
