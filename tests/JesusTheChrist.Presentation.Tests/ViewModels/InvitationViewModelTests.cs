using JesusTheChrist.Core.Models;
using JesusTheChrist.Data;
using JesusTheChrist.Presentation;
using JesusTheChrist.Presentation.Tests.Fakes;
using JesusTheChrist.Presentation.ViewModels;

namespace JesusTheChrist.Presentation.Tests.ViewModels;

public class InvitationViewModelTests
{
    [Fact]
    public async Task Acknowledge_MarksSeenAndNavigatesBack()
    {
        await using var harness = await Harness.CreateAsync();

        await harness.ViewModel.AcknowledgeCommand.ExecuteAsync(null);

        Assert.True(await harness.Settings.GetBoolAsync(SettingKeys.InvitationSeen, false));
        Assert.Equal(1, harness.Navigation.BackCount);
    }

    [Fact]
    public async Task Load_ReadsMarkdownForCurrentLanguage()
    {
        await using var harness = await Harness.CreateAsync(defaultLanguage: Language.Es);

        await harness.ViewModel.LoadAsync();

        Assert.Equal("# La invitación", harness.ViewModel.BodyMarkdown);
    }

    [Fact]
    public async Task Load_FallsBackToEnglishWhenLanguageFileMissing()
    {
        await using var harness = await Harness.CreateAsync(defaultLanguage: Language.Es, includeSpanish: false);

        await harness.ViewModel.LoadAsync();

        Assert.Equal("# The Invitation", harness.ViewModel.BodyMarkdown);
    }

    private sealed class Harness : IAsyncDisposable
    {
        private readonly TempDatabase database;

        private Harness(TempDatabase database, InvitationViewModel viewModel, SettingsStore settings, RecordingNavigationService navigation)
        {
            this.database = database;
            this.ViewModel = viewModel;
            this.Settings = settings;
            this.Navigation = navigation;
        }

        public InvitationViewModel ViewModel { get; }

        public SettingsStore Settings { get; }

        public RecordingNavigationService Navigation { get; }

        public static async Task<Harness> CreateAsync(Language defaultLanguage = Language.En, bool includeSpanish = true)
        {
            var db = await TempDatabase.CreateAsync();
            var files = new Dictionary<string, string> { ["invitation.en.md"] = "# The Invitation" };
            if (includeSpanish)
            {
                files["invitation.es.md"] = "# La invitación";
            }

            var assets = new FakeAssetSource(files);
            var settings = new SettingsStore(db.Db);
            var navigation = new RecordingNavigationService();
            var env = new AppEnvironment(Scope.Full, defaultLanguage);
            var vm = new InvitationViewModel(assets, settings, db, navigation, env);
            return new Harness(db, vm, settings, navigation);
        }

        public async ValueTask DisposeAsync() => await this.database.DisposeAsync();
    }
}
