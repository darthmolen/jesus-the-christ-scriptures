using JesusTheChrist.Core.Content;
using JesusTheChrist.Core.Models;
using JesusTheChrist.Data;
using JesusTheChrist.Presentation;
using JesusTheChrist.Presentation.Navigation;
using JesusTheChrist.Presentation.Tests.Fakes;
using JesusTheChrist.Presentation.ViewModels;

namespace JesusTheChrist.Presentation.Tests.ViewModels;

public class TopicFeedViewModelTests
{
    private const string AdvocateRefId = "advocate:newtestament/heb/7/25";

    [Fact]
    public async Task Load_SetsTitleFromSubTopic()
    {
        await using var harness = await Harness.CreateAsync();
        await harness.ViewModel.LoadAsync("advocate");
        Assert.Equal("Jesus Christ, Advocate", harness.ViewModel.Title);
    }

    [Fact]
    public async Task Load_PopulatesReferenceCardsWithVerseText()
    {
        await using var harness = await Harness.CreateAsync();
        await harness.ViewModel.LoadAsync("advocate");

        Assert.Equal(2, harness.ViewModel.References.Count);
        Assert.Equal("Heb. 7:25", harness.ViewModel.References[0].RefLabel);
        Assert.Contains("intercession", harness.ViewModel.References[0].VerseText);
    }

    [Fact]
    public async Task Load_ShowsGlossOnlyWhenItAddsInformation()
    {
        await using var harness = await Harness.CreateAsync();
        await harness.ViewModel.LoadAsync("advocate");

        Assert.True(harness.ViewModel.References[0].HasGloss);
        Assert.Equal("he advocateth the cause of the children of men", harness.ViewModel.References[0].Gloss);
        Assert.False(harness.ViewModel.References[1].HasGloss);
    }

    [Fact]
    public async Task Load_ReflectsExistingReadState()
    {
        await using var harness = await Harness.CreateAsync();
        await harness.ReadMarks.MarkReadAsync(AdvocateRefId);

        await harness.ViewModel.LoadAsync("advocate");

        Assert.True(harness.ViewModel.References[0].IsRead);
        Assert.False(harness.ViewModel.References[1].IsRead);
    }

    [Fact]
    public async Task ToggleRead_PersistsAndUpdatesCard()
    {
        await using var harness = await Harness.CreateAsync();
        await harness.ViewModel.LoadAsync("advocate");
        var card = harness.ViewModel.References[0];

        await card.ToggleReadCommand.ExecuteAsync(null);
        Assert.True(card.IsRead);
        Assert.True(await harness.ReadMarks.IsReadAsync(AdvocateRefId));

        await card.ToggleReadCommand.ExecuteAsync(null);
        Assert.False(card.IsRead);
        Assert.False(await harness.ReadMarks.IsReadAsync(AdvocateRefId));
    }

    [Fact]
    public async Task Load_ExposesTargetVersesAsNumberedLines()
    {
        await using var harness = await Harness.CreateAsync();
        await harness.ViewModel.LoadAsync("advocate");

        var verses = harness.ViewModel.References[0].Verses;
        Assert.Single(verses);
        Assert.Equal(25, verses[0].Verse);
        Assert.Contains("intercession", verses[0].Text);
    }

    [Fact]
    public async Task Load_BuildsContextLinesWithTargetFlag()
    {
        await using var harness = await Harness.CreateAsync();
        await harness.ViewModel.LoadAsync("advocate");

        var context = harness.ViewModel.References[0].Context;
        Assert.Equal(3, context.Count);
        Assert.True(context.Single(c => c.Verse == 25).IsTarget);
        Assert.False(context.Single(c => c.Verse == 24).IsTarget);
    }

    [Fact]
    public async Task ToggleContext_FlipsVisibility()
    {
        await using var harness = await Harness.CreateAsync();
        await harness.ViewModel.LoadAsync("advocate");
        var card = harness.ViewModel.References[0];

        Assert.False(card.IsContextVisible);
        card.ToggleContextCommand.Execute(null);
        Assert.True(card.IsContextVisible);
    }

    [Fact]
    public async Task Load_ReflectsExistingNote()
    {
        await using var harness = await Harness.CreateAsync();
        await harness.Notes.SaveAsync(AdvocateRefId, "a thought");

        await harness.ViewModel.LoadAsync("advocate");

        Assert.True(harness.ViewModel.References[0].HasNote);
        Assert.False(harness.ViewModel.References[1].HasNote);
    }

    [Fact]
    public async Task OpenNote_NavigatesToNoteRouteWithRefId()
    {
        await using var harness = await Harness.CreateAsync();
        await harness.ViewModel.LoadAsync("advocate");

        await harness.ViewModel.References[0].OpenNoteCommand.ExecuteAsync(null);

        var call = Assert.Single(harness.Navigation.Calls);
        Assert.Equal(NavigationRoutes.Note, call.Route);
        Assert.Equal(AdvocateRefId, call.Parameters![NavigationRoutes.NoteRefIdParameter]);
    }

    [Fact]
    public async Task RefreshNotes_UpdatesIndicators()
    {
        await using var harness = await Harness.CreateAsync();
        await harness.ViewModel.LoadAsync("advocate");
        Assert.False(harness.ViewModel.References[0].HasNote);

        await harness.Notes.SaveAsync(AdvocateRefId, "added later");
        await harness.ViewModel.RefreshNotesAsync();

        Assert.True(harness.ViewModel.References[0].HasNote);
    }

    private sealed class Harness : IAsyncDisposable
    {
        private readonly TempDatabase database;

        private Harness(TempDatabase database, TopicFeedViewModel viewModel, ReadMarkStore readMarks, NoteStore notes, RecordingNavigationService navigation)
        {
            this.database = database;
            this.ViewModel = viewModel;
            this.ReadMarks = readMarks;
            this.Notes = notes;
            this.Navigation = navigation;
        }

        public TopicFeedViewModel ViewModel { get; }

        public ReadMarkStore ReadMarks { get; }

        public NoteStore Notes { get; }

        public RecordingNavigationService Navigation { get; }

        public static async Task<Harness> CreateAsync()
        {
            var db = await TempDatabase.CreateAsync();
            var assets = new FakeAssetSource(new Dictionary<string, string>
            {
                ["jesus-christ.en.json"] = EnGuide,
                ["jesus-christ.es.json"] = EnGuide,
            });
            var content = new ContentService(assets);
            var readMarks = new ReadMarkStore(db.Db);
            var notes = new NoteStore(db.Db);
            var settings = new SettingsStore(db.Db);
            var navigation = new RecordingNavigationService();
            var env = new AppEnvironment(Scope.Full, Language.En);
            var vm = new TopicFeedViewModel(content, readMarks, notes, settings, db, navigation, env);
            return new Harness(db, vm, readMarks, notes, navigation);
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
                      "context": [
                        { "vs": 24, "text": "but this man, because he continueth ever", "target": false },
                        { "vs": 25, "text": "he ever liveth to make intercession for them", "target": true },
                        { "vs": 26, "text": "for such an high priest became us", "target": false }
                      ],
                      "note": "he advocateth the cause of the children of men" },
                    { "ref": "1 Jn. 2:1", "vol": "newtestament", "book": "1-jn", "book_title": "1 John",
                      "ch": 2, "verses": [1],
                      "context": [
                        { "vs": 1, "text": "we have an advocate with the Father", "target": true }
                      ],
                      "note": "we have an advocate with the Father" }
                  ]
                }
              ]
            }
            """;
    }
}
