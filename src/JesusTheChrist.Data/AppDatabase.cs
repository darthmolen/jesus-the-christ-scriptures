using SQLite;

namespace JesusTheChrist.Data;

/// <summary>
/// The on-device SQLite database for user data (read-marks, notes, settings).
/// </summary>
public sealed class AppDatabase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AppDatabase"/> class.
    /// </summary>
    /// <param name="dbPath">The SQLite database file path.</param>
    public AppDatabase(string dbPath)
    {
        SQLitePCL.Batteries_V2.Init();
        this.Connection = new SQLiteAsyncConnection(dbPath);
    }

    /// <summary>
    /// Gets the underlying asynchronous SQLite connection.
    /// </summary>
    public SQLiteAsyncConnection Connection { get; }

    /// <summary>
    /// Creates the database tables if they do not yet exist.
    /// </summary>
    /// <returns>A task that completes when the schema is ready.</returns>
    public async Task InitializeAsync()
    {
        await this.Connection.CreateTableAsync<ReadMark>();
        await this.Connection.CreateTableAsync<NoteEntry>();
        await this.Connection.CreateTableAsync<Setting>();
    }
}
