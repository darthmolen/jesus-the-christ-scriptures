using JesusTheChrist.Core.Content;
using JesusTheChrist.Core.Models;

namespace JesusTheChrist.Core.Tests;

public sealed class ContentServiceTests
{
    [Fact]
    public async Task LoadAsync_uses_language_filename_and_applies_scope()
    {
        var assets = new FileAssets();
        var svc = new ContentService(assets);
        var guide = await svc.LoadAsync(Language.En, Scope.BibleOnly);

        Assert.Equal("jesus-christ.en.json", assets.LastRequested);
        Assert.All(guide.SubTopics, t => Assert.All(t.References, r => Assert.True(r.Vol.IsBible())));
    }

    private sealed class FileAssets : IAssetSource
    {
        public string? LastRequested { get; private set; }

        public Task<Stream> OpenAsync(string name)
        {
            this.LastRequested = name;
            return Task.FromResult<Stream>(File.OpenRead(Path.Combine("Fixtures", "sample.json")));
        }
    }
}
