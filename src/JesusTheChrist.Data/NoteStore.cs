using System;
using System.Threading.Tasks;
using SQLite;

namespace JesusTheChrist.Data;

public sealed class NoteStore
{
    private readonly SQLiteAsyncConnection _c;
    private readonly Func<DateTime> _utcNow;

    public NoteStore(AppDatabase db, Func<DateTime>? utcNow = null)
    {
        _c = db.Connection;
        _utcNow = utcNow ?? (() => DateTime.UtcNow);
    }

    public async Task SaveAsync(string refId, string text)
    {
        var trimmed = (text ?? "").Trim();
        if (trimmed.Length == 0) { await DeleteAsync(refId); return; }
        await _c.InsertOrReplaceAsync(new NoteEntry
        {
            RefId = refId, Text = trimmed, UpdatedAtUtc = _utcNow()
        });
    }

    public async Task<string?> GetAsync(string refId) =>
        (await _c.FindAsync<NoteEntry>(refId))?.Text;

    public Task DeleteAsync(string refId) => _c.DeleteAsync<NoteEntry>(refId);

    public async Task<bool> HasNoteAsync(string refId) =>
        await _c.FindAsync<NoteEntry>(refId) is not null;
}
