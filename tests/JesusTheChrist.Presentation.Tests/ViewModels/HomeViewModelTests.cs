using System.ComponentModel;
using JesusTheChrist.Core.Content;
using JesusTheChrist.Core.Models;
using JesusTheChrist.Data;
using JesusTheChrist.Presentation;
using JesusTheChrist.Presentation.Navigation;
using JesusTheChrist.Presentation.Tests.Fakes;
using JesusTheChrist.Presentation.ViewModels;

namespace JesusTheChrist.Presentation.Tests.ViewModels;

public class HomeViewModelTests
{
    [Fact]
    public async Task DefaultState_IsEmptyAndNotLoading()
    {
        await using var harness = await Harness.CreateAsync();

        Assert.Empty(harness.ViewModel.Topics);
        Assert.Equal(string.Empty, harness.ViewModel.HeaderText);
        Assert.False(harness.ViewModel.IsLoading);
    }

    [Fact]
    public async Task OpenTopic_NavigatesToTopicRouteWithKey()
    {
        await using var harness = await Harness.CreateAsync();
        var row = new TopicRowViewModel("advocate", "Jesus Christ, Advocate", "Advocate", 0, 2);

        await harness.ViewModel.OpenTopicCommand.ExecuteAsync(row);

        var call = Assert.Single(harness.Navigation.Calls);
        Assert.Equal(NavigationRoutes.Topic, call.Route);
        Assert.NotNull(call.Parameters);
        Assert.Equal("advocate", call.Parameters![NavigationRoutes.TopicKeyParameter]);
    }

    [Fact]
    public async Task IsInvitationUnseen_TrueByDefaultThenFalseOnceSeen()
    {
        await using var harness = await Harness.CreateAsync();

        Assert.True(await harness.ViewModel.IsInvitationUnseenAsync());

        await harness.Settings.SetBoolAsync(SettingKeys.InvitationSeen, true);
        Assert.False(await harness.ViewModel.IsInvitationUnseenAsync());
    }

    [Fact]
    public async Task OpenInvitation_NavigatesToInvitationRoute()
    {
        await using var harness = await Harness.CreateAsync();

        await harness.ViewModel.OpenInvitationCommand.ExecuteAsync(null);

        var call = Assert.Single(harness.Navigation.Calls);
        Assert.Equal(NavigationRoutes.Invitation, call.Route);
    }

    [Fact]
    public async Task OpenSettings_NavigatesToSettingsRoute()
    {
        await using var harness = await Harness.CreateAsync();

        await harness.ViewModel.OpenSettingsCommand.ExecuteAsync(null);

        var call = Assert.Single(harness.Navigation.Calls);
        Assert.Equal(NavigationRoutes.Settings, call.Route);
    }

    [Fact]
    public async Task Load_PopulatesTopicsInGuideOrder()
    {
        await using var harness = await Harness.CreateAsync();

        await harness.ViewModel.LoadCommand.ExecuteAsync(null);

        Assert.Collection(
            harness.ViewModel.Topics,
            row => Assert.Equal("advocate", row.Key),
            row => Assert.Equal("creator", row.Key));
    }

    [Fact]
    public async Task Load_RowProgressReflectsReadMarks()
    {
        await using var harness = await Harness.CreateAsync();
        await harness.ReadMarks.MarkReadAsync("advocate:newtestament/heb/7/25");

        await harness.ViewModel.LoadCommand.ExecuteAsync(null);

        var advocate = harness.ViewModel.Topics.Single(r => r.Key == "advocate");
        var creator = harness.ViewModel.Topics.Single(r => r.Key == "creator");
        Assert.Equal(1, advocate.Read);
        Assert.Equal(1, advocate.Total);
        Assert.Equal(0, creator.Read);
        Assert.Equal(1, creator.Total);
    }

    [Fact]
    public async Task Load_HeaderShowsOverallProgress()
    {
        await using var harness = await Harness.CreateAsync();
        await harness.ReadMarks.MarkReadAsync("advocate:newtestament/heb/7/25");

        await harness.ViewModel.LoadCommand.ExecuteAsync(null);

        Assert.Equal("1 / 2 references read", harness.ViewModel.HeaderText);
    }

    [Fact]
    public async Task Load_SetsOverallFraction()
    {
        await using var harness = await Harness.CreateAsync();
        await harness.ReadMarks.MarkReadAsync("advocate:newtestament/heb/7/25");

        await harness.ViewModel.LoadCommand.ExecuteAsync(null);

        Assert.Equal(0.5, harness.ViewModel.OverallFraction);
    }

    [Fact]
    public async Task Load_WithSavedSpanishPreference_LoadsSpanishGuide()
    {
        await using var harness = await Harness.CreateAsync(defaultLanguage: Language.En);
        await harness.Settings.SetAsync(SettingKeys.Language, "es");

        await harness.ViewModel.LoadCommand.ExecuteAsync(null);

        Assert.Equal("Jesucristo, Abogado", harness.ViewModel.Topics[0].Title);
    }

    [Fact]
    public async Task Load_WithNoPreference_FallsBackToDefaultLanguage()
    {
        await using var harness = await Harness.CreateAsync(defaultLanguage: Language.Es);

        await harness.ViewModel.LoadCommand.ExecuteAsync(null);

        Assert.Equal("Jesucristo, Abogado", harness.ViewModel.Topics[0].Title);
    }

    [Fact]
    public async Task Load_TogglesIsLoadingTrueThenFalse()
    {
        await using var harness = await Harness.CreateAsync();
        var sawLoadingTrue = false;
        harness.ViewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(HomeViewModel.IsLoading) && harness.ViewModel.IsLoading)
            {
                sawLoadingTrue = true;
            }
        };

        await harness.ViewModel.LoadCommand.ExecuteAsync(null);

        Assert.True(sawLoadingTrue);
        Assert.False(harness.ViewModel.IsLoading);
    }

    /// <summary>
    /// Builds a HomeViewModel over a real temp database and a fake asset source.
    /// </summary>
    private sealed class Harness : IAsyncDisposable
    {
        private readonly TempDatabase database;

        private Harness(
            TempDatabase database,
            HomeViewModel viewModel,
            RecordingNavigationService navigation,
            ReadMarkStore readMarks,
            SettingsStore settings)
        {
            this.database = database;
            this.ViewModel = viewModel;
            this.Navigation = navigation;
            this.ReadMarks = readMarks;
            this.Settings = settings;
        }

        public HomeViewModel ViewModel { get; }

        public RecordingNavigationService Navigation { get; }

        public ReadMarkStore ReadMarks { get; }

        public SettingsStore Settings { get; }

        public static async Task<Harness> CreateAsync(Language defaultLanguage = Language.En)
        {
            var db = await TempDatabase.CreateAsync();
            var assets = new FakeAssetSource(new Dictionary<string, string>
            {
                ["jesus-christ.en.json"] = EnGuide,
                ["jesus-christ.es.json"] = EsGuide,
            });
            var content = new ContentService(assets);
            var readMarks = new ReadMarkStore(db.Db);
            var settings = new SettingsStore(db.Db);
            var navigation = new RecordingNavigationService();
            var env = new AppEnvironment(Scope.Full, defaultLanguage);
            var vm = new HomeViewModel(content, readMarks, settings, db, navigation, env);
            return new Harness(db, vm, navigation, readMarks, settings);
        }

        public async ValueTask DisposeAsync() => await this.database.DisposeAsync();

        private const string EnGuide = """
            {
              "topic": "Jesus Christ", "language": "en", "context_radius": 2,
              "subtopics": [
                {
                  "title": "Jesus Christ, Advocate", "short": "Advocate",
                  "references": [
                    { "ref": "Heb. 7:25", "vol": "newtestament", "book": "heb", "book_title": "Hebrews",
                      "ch": 7, "verses": [25],
                      "context": [ { "vs": 25, "text": "he ever liveth", "target": true } ], "note": null }
                  ]
                },
                {
                  "title": "Jesus Christ, Creator", "short": "Creator",
                  "references": [
                    { "ref": "John 1:3", "vol": "newtestament", "book": "john", "book_title": "John",
                      "ch": 1, "verses": [3],
                      "context": [ { "vs": 3, "text": "All things were made by him", "target": true } ], "note": null }
                  ]
                }
              ]
            }
            """;

        private const string EsGuide = """
            {
              "topic": "Jesucristo", "language": "es", "context_radius": 2,
              "subtopics": [
                {
                  "title": "Jesucristo, Abogado", "short": "Abogado", "short_en": "Advocate",
                  "references": [
                    { "ref": "Heb. 7:25", "vol": "newtestament", "book": "heb", "book_title": "Hebreos",
                      "ch": 7, "verses": [25],
                      "context": [ { "vs": 25, "text": "viviendo siempre", "target": true } ], "note": null }
                  ]
                },
                {
                  "title": "Jesucristo, Creador", "short": "Creador", "short_en": "Creator",
                  "references": [
                    { "ref": "Juan 1:3", "vol": "newtestament", "book": "john", "book_title": "Juan",
                      "ch": 1, "verses": [3],
                      "context": [ { "vs": 3, "text": "Todas las cosas por el fueron hechas", "target": true } ], "note": null }
                  ]
                }
              ]
            }
            """;
    }
}
