using JesusTheChrist.Core.Json;
using JesusTheChrist.Core.Models;

namespace JesusTheChrist.Core.Content;

/// <summary>
/// Loads the bundled Topical Guide for a language and applies the build scope.
/// </summary>
public sealed class ContentService
{
    private readonly IAssetSource assets;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContentService"/> class.
    /// </summary>
    /// <param name="assets">The source for bundled content assets.</param>
    public ContentService(IAssetSource assets) => this.assets = assets;

    /// <summary>
    /// Loads the Topical Guide for the given language, filtered to the given scope.
    /// </summary>
    /// <param name="lang">The content language.</param>
    /// <param name="scope">The build scope to apply.</param>
    /// <returns>The loaded, scope-filtered <see cref="TopicalGuide"/>.</returns>
    public async Task<TopicalGuide> LoadAsync(Language lang, Scope scope)
    {
        await using var stream = await this.assets.OpenAsync($"jesus-christ.{lang.Code()}.json");
        var guide = TopicalGuideLoader.Load(stream);
        return ScopeFilter.Apply(guide, scope);
    }
}
