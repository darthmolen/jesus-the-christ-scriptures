using JesusTheChrist.Data;

namespace JesusTheChrist.App.Services;

/// <summary>
/// Ensures the SQLite database schema is created exactly once, regardless of how
/// many callers request initialization.
/// </summary>
public sealed class DatabaseInitializer
{
    private readonly AppDatabase database;
    private readonly Lock gate = new();
    private Task? initialization;

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseInitializer"/> class.
    /// </summary>
    /// <param name="database">The application database to initialize.</param>
    public DatabaseInitializer(AppDatabase database)
    {
        this.database = database;
    }

    /// <summary>
    /// Creates the database schema on first call and returns the same task thereafter.
    /// </summary>
    /// <returns>A task that completes when the schema has been created.</returns>
    public Task EnsureInitializedAsync()
    {
        lock (this.gate)
        {
            return this.initialization ??= this.database.InitializeAsync();
        }
    }
}
