namespace JesusTheChrist.Core.Content;

/// <summary>
/// Opens bundled content assets by name. Keeps the content layer free of any MAUI dependency:
/// the app supplies a platform-backed source, tests a file-backed one.
/// </summary>
public interface IAssetSource
{
    /// <summary>
    /// Opens the named bundled asset for reading.
    /// </summary>
    /// <param name="name">The asset file name, for example <c>jesus-christ.en.json</c>.</param>
    /// <returns>A readable stream over the asset.</returns>
    public Task<Stream> OpenAsync(string name);
}
