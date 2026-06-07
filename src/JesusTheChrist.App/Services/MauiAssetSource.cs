using JesusTheChrist.Core.Content;

namespace JesusTheChrist.App.Services;

/// <summary>
/// Reads bundled content assets from the MAUI app package.
/// </summary>
public sealed class MauiAssetSource : IAssetSource
{
    /// <inheritdoc/>
    public Task<Stream> OpenAsync(string name) => FileSystem.OpenAppPackageFileAsync(name);
}
