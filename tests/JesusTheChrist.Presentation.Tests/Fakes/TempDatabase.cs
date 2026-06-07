using JesusTheChrist.Data;
using JesusTheChrist.Presentation.Data;

namespace JesusTheChrist.Presentation.Tests.Fakes;

/// <summary>
/// A throwaway temp-file database for a single test; deletes itself on dispose.
/// Doubles as an <see cref="IDatabaseInitializer"/> for the view model under test.
/// </summary>
public sealed class TempDatabase : IDatabaseInitializer, IAsyncDisposable
{
    private readonly string path;

    private TempDatabase(string path, AppDatabase db)
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
    /// <returns>The created <see cref="TempDatabase"/>.</returns>
    public static async Task<TempDatabase> CreateAsync()
    {
        var path = Path.Combine(Path.GetTempPath(), $"jtc-vm-{Guid.NewGuid():N}.db");
        var db = new AppDatabase(path);
        await db.InitializeAsync();
        return new TempDatabase(path, db);
    }

    /// <inheritdoc/>
    public Task EnsureInitializedAsync() => this.Db.InitializeAsync();

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
