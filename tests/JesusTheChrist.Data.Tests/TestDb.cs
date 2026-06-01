using JesusTheChrist.Data;

namespace JesusTheChrist.Data.Tests;

/// <summary>
/// A throwaway temp-file database for a single test; deletes itself on dispose.
/// </summary>
public sealed class TestDb : IAsyncDisposable
{
    private readonly string path;

    private TestDb(string path, AppDatabase db)
    {
        this.path = path;
        this.Db = db;
    }

    /// <summary>
    /// Gets the application database under test.
    /// </summary>
    public AppDatabase Db { get; }

    /// <summary>
    /// Creates and initializes a fresh temp-file database.
    /// </summary>
    /// <returns>The created <see cref="TestDb"/>.</returns>
    public static async Task<TestDb> CreateAsync()
    {
        var path = Path.Combine(Path.GetTempPath(), $"jtc-test-{Guid.NewGuid():N}.db");
        var db = new AppDatabase(path);
        await db.InitializeAsync();
        return new TestDb(path, db);
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        await this.Db.Connection.CloseAsync();
        try
        {
            File.Delete(this.path);
        }
        catch (IOException)
        {
        }
    }
}
