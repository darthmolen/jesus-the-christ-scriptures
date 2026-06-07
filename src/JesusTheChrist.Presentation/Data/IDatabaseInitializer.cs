namespace JesusTheChrist.Presentation.Data;

/// <summary>
/// Ensures the on-device database schema exists before any data is read.
/// </summary>
public interface IDatabaseInitializer
{
    /// <summary>
    /// Creates the database schema if needed; safe to call repeatedly.
    /// </summary>
    /// <returns>A task that completes when the schema is ready.</returns>
    public Task EnsureInitializedAsync();
}
