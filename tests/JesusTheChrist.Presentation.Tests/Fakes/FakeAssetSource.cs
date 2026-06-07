using System.Text;
using JesusTheChrist.Core.Content;

namespace JesusTheChrist.Presentation.Tests.Fakes;

/// <summary>
/// An in-memory <see cref="IAssetSource"/> backed by named JSON strings.
/// </summary>
public sealed class FakeAssetSource : IAssetSource
{
    private readonly IReadOnlyDictionary<string, string> assets;

    /// <summary>
    /// Initializes a new instance of the <see cref="FakeAssetSource"/> class.
    /// </summary>
    /// <param name="assets">A map of asset name to JSON content.</param>
    public FakeAssetSource(IReadOnlyDictionary<string, string> assets)
    {
        this.assets = assets;
    }

    /// <inheritdoc/>
    public Task<Stream> OpenAsync(string name)
    {
        if (!this.assets.TryGetValue(name, out var json))
        {
            throw new FileNotFoundException($"No fake asset named '{name}'.");
        }

        Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        return Task.FromResult(stream);
    }
}
