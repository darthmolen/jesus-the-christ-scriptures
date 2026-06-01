using System.IO;
using System.Threading.Tasks;
using JesusTheChrist.Core.Content;
using JesusTheChrist.Core.Models;
using Xunit;

public class ContentServiceTests
{
    sealed class FileAssets : IAssetSource
    {
        public string? LastRequested;
        public Task<Stream> OpenAsync(string name)
        {
            LastRequested = name;
            return Task.FromResult<Stream>(File.OpenRead(Path.Combine("Fixtures", "sample.json")));
        }
    }

    [Fact]
    public async Task LoadAsync_uses_language_filename_and_applies_scope()
    {
        var assets = new FileAssets();
        var svc = new ContentService(assets);
        var guide = await svc.LoadAsync(Language.En, Scope.BibleOnly);

        Assert.Equal("jesus-christ.en.json", assets.LastRequested);
        Assert.All(guide.SubTopics, t => Assert.All(t.References, r => Assert.True(r.Vol.IsBible())));
    }
}
