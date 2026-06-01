using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SQLite;

namespace JesusTheChrist.Data;

public sealed class ReadMarkStore
{
    private readonly SQLiteAsyncConnection _c;
    private readonly Func<DateTime> _utcNow;

    public ReadMarkStore(AppDatabase db, Func<DateTime>? utcNow = null)
    {
        _c = db.Connection;
        _utcNow = utcNow ?? (() => DateTime.UtcNow);
    }

    public Task MarkReadAsync(string refId) =>
        _c.InsertOrReplaceAsync(new ReadMark { RefId = refId, ReadAtUtc = _utcNow() });

    public Task UnmarkAsync(string refId) => _c.DeleteAsync<ReadMark>(refId);

    public async Task<bool> IsReadAsync(string refId) =>
        await _c.FindAsync<ReadMark>(refId) is not null;

    public Task<int> CountAsync() => _c.Table<ReadMark>().CountAsync();

    public async Task<HashSet<string>> GetReadIdsAsync() =>
        (await _c.Table<ReadMark>().ToListAsync()).Select(r => r.RefId).ToHashSet();
}
