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
    private const string SecondRefId = "advocate:newtestament/1-jn/2/1";

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
    public async Task Load_SingleChapterReference_HasOneHeaderlessExpandedSegment()
    {
        await using var harness = await Harness.CreateAsync();
        await harness.ViewModel.LoadAsync("advocate");

        var segment = Assert.Single(harness.ViewModel.References[0].Segments);
        Assert.False(segment.ShowHeader);
        Assert.True(segment.IsExpanded);
        Assert.Equal([25], segment.Verses.Select(v => v.Verse).ToArray());
    }

    [Fact]
    public async Task Load_CrossChapterReference_GroupsTargetVersesByChapterWithHeaders()
    {
        await using var harness = await Harness.CreateAsync();
        await harness.ViewModel.LoadAsync("summary");

        var card = harness.ViewModel.References[0];
        Assert.Equal(
            ["Matthew 9", "Matthew 10", "Matthew 11"],
            card.Segments.Select(s => s.ChapterLabel).ToArray());
        Assert.All(card.Segments, s => Assert.True(s.ShowHeader));
        Assert.Equal([35, 36], card.Segments[0].Verses.Select(v => v.Verse).ToArray());
        Assert.Equal([1, 2], card.Segments[1].Verses.Select(v => v.Verse).ToArray());
        Assert.Equal([1], card.Segments[2].Verses.Select(v => v.Verse).ToArray());
    }

    [Fact]
    public async Task Load_SmallCrossChapterReference_StartsEverySegmentExpanded()
    {
        await using var harness = await Harness.CreateAsync();
        await harness.ViewModel.LoadAsync("summary");

        var card = harness.ViewModel.References[0];
        Assert.All(card.Segments, s => Assert.True(s.IsExpanded));
        Assert.Equal(card.Segments[2].Verses, card.Segments[2].VisibleVerses);
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
    public async Task Load_CardsStartExpanded()
    {
        await using var harness = await Harness.CreateAsync();
        await harness.ViewModel.LoadAsync("advocate");

        Assert.True(harness.ViewModel.References[0].IsExpanded);
        Assert.Equal("▾", harness.ViewModel.References[0].ChevronGlyph);
    }

    [Fact]
    public async Task Load_ReadCardStartsCollapsed()
    {
        await using var harness = await Harness.CreateAsync();
        await harness.ReadMarks.MarkReadAsync(AdvocateRefId);

        await harness.ViewModel.LoadAsync("advocate");

        Assert.True(harness.ViewModel.References[0].IsRead);
        Assert.False(harness.ViewModel.References[0].IsExpanded);
    }

    [Fact]
    public async Task ToggleRead_CollapsesWhenMarkedAndExpandsWhenCleared()
    {
        await using var harness = await Harness.CreateAsync();
        await harness.ViewModel.LoadAsync("advocate");
        var card = harness.ViewModel.References[0];

        await card.ToggleReadCommand.ExecuteAsync(null);
        Assert.True(card.IsRead);
        Assert.False(card.IsExpanded);
        Assert.Equal("▸", card.ChevronGlyph);

        await card.ToggleReadCommand.ExecuteAsync(null);
        Assert.False(card.IsRead);
        Assert.True(card.IsExpanded);
    }

    [Fact]
    public async Task IsCollapsed_MirrorsInverseOfIsExpanded()
    {
        await using var harness = await Harness.CreateAsync();
        await harness.ViewModel.LoadAsync("advocate");
        var card = harness.ViewModel.References[0];

        Assert.True(card.IsExpanded);
        Assert.False(card.IsCollapsed);

        card.ToggleExpandedCommand.Execute(null);
        Assert.False(card.IsExpanded);
        Assert.True(card.IsCollapsed);
    }

    [Fact]
    public async Task ToggleRead_WhenMarkingRead_RaisesCardCollapsedAfterRead()
    {
        await using var harness = await Harness.CreateAsync();
        await harness.ViewModel.LoadAsync("advocate");
        var card = harness.ViewModel.References[0];

        ReferenceCardViewModel? collapsed = null;
        harness.ViewModel.CardCollapsedAfterRead += (_, e) => collapsed = e.Card;

        await card.ToggleReadCommand.ExecuteAsync(null);

        Assert.Same(card, collapsed);
    }

    [Fact]
    public async Task ToggleRead_WhenUnmarking_DoesNotRaiseCardCollapsedAfterRead()
    {
        await using var harness = await Harness.CreateAsync();
        await harness.ViewModel.LoadAsync("advocate");
        var card = harness.ViewModel.References[0];
        await card.ToggleReadCommand.ExecuteAsync(null);

        var raised = false;
        harness.ViewModel.CardCollapsedAfterRead += (_, _) => raised = true;
        await card.ToggleReadCommand.ExecuteAsync(null);

        Assert.False(raised);
    }

    [Fact]
    public async Task ToggleExpanded_FlipsBodyWithoutChangingReadState()
    {
        await using var harness = await Harness.CreateAsync();
        await harness.ViewModel.LoadAsync("advocate");
        var card = harness.ViewModel.References[0];

        card.ToggleExpandedCommand.Execute(null);
        Assert.False(card.IsExpanded);
        Assert.False(card.IsRead);

        card.ToggleExpandedCommand.Execute(null);
        Assert.True(card.IsExpanded);
        Assert.False(card.IsRead);
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
    public async Task OpenNote_NavigatesToNoteRouteWithRefIdLabelAndVerses()
    {
        await using var harness = await Harness.CreateAsync();
        await harness.ViewModel.LoadAsync("advocate");
        var card = harness.ViewModel.References[0];

        await card.OpenNoteCommand.ExecuteAsync(null);

        var call = Assert.Single(harness.Navigation.Calls);
        Assert.Equal(NavigationRoutes.Note, call.Route);
        Assert.Equal(AdvocateRefId, call.Parameters![NavigationRoutes.NoteRefIdParameter]);
        Assert.Equal(card.RefLabel, call.Parameters![NavigationRoutes.NoteRefLabelParameter]);
        Assert.Same(card.Verses, call.Parameters![NavigationRoutes.NoteVersesParameter]);
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

    [Fact]
    public async Task Resume_AfterSavedPosition_ReturnsMatchingCard()
    {
        await using var harness = await Harness.CreateAsync();
        await harness.Positions.SaveAsync("advocate", SecondRefId);

        await harness.ViewModel.LoadAsync("advocate");

        Assert.Same(harness.ViewModel.References[1], harness.ViewModel.ResumeCard());
    }

    [Fact]
    public async Task Resume_WithNoSavedPosition_ReturnsNull()
    {
        await using var harness = await Harness.CreateAsync();

        await harness.ViewModel.LoadAsync("advocate");

        Assert.Null(harness.ViewModel.ResumeCard());
    }

    [Fact]
    public async Task RecordVisible_ThenSave_PersistsPosition()
    {
        await using var harness = await Harness.CreateAsync();
        await harness.ViewModel.LoadAsync("advocate");

        harness.ViewModel.RecordVisible(SecondRefId);
        await harness.ViewModel.SavePositionAsync();

        Assert.Equal(SecondRefId, await harness.Positions.GetAsync("advocate"));
    }

    [Fact]
    public async Task SavePosition_WithoutRecord_LeavesStoredPosition()
    {
        await using var harness = await Harness.CreateAsync();
        await harness.Positions.SaveAsync("advocate", AdvocateRefId);
        await harness.ViewModel.LoadAsync("advocate");

        await harness.ViewModel.SavePositionAsync();

        Assert.Equal(AdvocateRefId, await harness.Positions.GetAsync("advocate"));
    }

    [Fact]
    public async Task SavePosition_OnFreshTopicWithoutRecord_PersistsNothing()
    {
        await using var harness = await Harness.CreateAsync();
        await harness.ViewModel.LoadAsync("advocate");

        await harness.ViewModel.SavePositionAsync();

        Assert.Null(await harness.Positions.GetAsync("advocate"));
    }

    private sealed class Harness : IAsyncDisposable
    {
        private readonly TempDatabase database;

        private Harness(TempDatabase database, TopicFeedViewModel viewModel, ReadMarkStore readMarks, NoteStore notes, TopicPositionStore positions, RecordingNavigationService navigation)
        {
            this.database = database;
            this.ViewModel = viewModel;
            this.ReadMarks = readMarks;
            this.Notes = notes;
            this.Positions = positions;
            this.Navigation = navigation;
        }

        public TopicFeedViewModel ViewModel { get; }

        public ReadMarkStore ReadMarks { get; }

        public NoteStore Notes { get; }

        public TopicPositionStore Positions { get; }

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
            var positions = new TopicPositionStore(db.Db);
            var settings = new SettingsStore(db.Db);
            var navigation = new RecordingNavigationService();
            var env = new AppEnvironment(Scope.Full, Language.En);
            var vm = new TopicFeedViewModel(content, readMarks, notes, positions, settings, db, navigation, env);
            return new Harness(db, vm, readMarks, notes, positions, navigation);
        }

        public async ValueTask DisposeAsync() => await this.database.DisposeAsync();

        private const string EnGuide = """
            {
              "topic": "Jesus Christ", "language": "en", "context_radius": 2,
              "subtopics": [
                {
                  "title": "Jesus Christ", "short": "Summary",
                  "references": [
                    { "ref": "Matt. 9:35–11:1", "vol": "newtestament", "book": "matt", "book_title": "Matthew",
                      "ch": 9, "end_ch": 11, "verses": [35, 36, 37, 38],
                      "context": [
                        { "vs": 35, "text": "and jesus went about all the cities", "target": true, "ch": 9 },
                        { "vs": 36, "text": "but when he saw the multitudes", "target": true, "ch": 9 },
                        { "vs": 1, "text": "and when he had called unto him his twelve", "target": true, "ch": 10 },
                        { "vs": 2, "text": "now the names of the twelve apostles", "target": true, "ch": 10 },
                        { "vs": 1, "text": "and it came to pass when jesus had made an end", "target": true, "ch": 11 }
                      ],
                      "note": "sends disciples forth by twos" }
                  ]
                },
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
