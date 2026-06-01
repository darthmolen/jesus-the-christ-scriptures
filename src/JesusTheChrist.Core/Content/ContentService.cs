using System.IO;
using System.Threading.Tasks;
using JesusTheChrist.Core.Json;
using JesusTheChrist.Core.Models;

namespace JesusTheChrist.Core.Content;

// Abstraction so Core stays MAUI-free; the app supplies a MAUI-backed source,
// tests supply a file-backed one.
public interface IAssetSource
{
    Task<Stream> OpenAsync(string name);
}

public sealed class ContentService
{
    private readonly IAssetSource _assets;
    public ContentService(IAssetSource assets) => _assets = assets;

    public async Task<TopicalGuide> LoadAsync(Language lang, Scope scope)
    {
        await using var stream = await _assets.OpenAsync($"jesus-christ.{lang.Code()}.json");
        var guide = TopicalGuideLoader.Load(stream);
        return ScopeFilter.Apply(guide, scope);
    }
}
