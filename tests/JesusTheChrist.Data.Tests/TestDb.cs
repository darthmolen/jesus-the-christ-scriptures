using System;
using System.IO;
using System.Threading.Tasks;
using JesusTheChrist.Data;

namespace JesusTheChrist.Data.Tests;

// A throwaway temp-file database for a single test; deletes itself on dispose.
public sealed class TestDb : IAsyncDisposable
{
    public AppDatabase Db { get; }
    private readonly string _path;

    private TestDb(string path, AppDatabase db) { _path = path; Db = db; }

    public static async Task<TestDb> CreateAsync()
    {
        var path = Path.Combine(Path.GetTempPath(), $"jtc-test-{Guid.NewGuid():N}.db");
        var db = new AppDatabase(path);
        await db.InitializeAsync();
        return new TestDb(path, db);
    }

    public async ValueTask DisposeAsync()
    {
        await Db.Connection.CloseAsync();
        try { File.Delete(_path); } catch { /* best effort */ }
    }
}
