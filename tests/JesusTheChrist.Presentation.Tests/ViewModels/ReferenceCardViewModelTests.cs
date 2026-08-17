using JesusTheChrist.Presentation.Tests.Fakes;
using JesusTheChrist.Presentation.ViewModels;

namespace JesusTheChrist.Presentation.Tests.ViewModels;

public sealed class ReferenceCardViewModelTests
{
    private static readonly Uri CardStudyUri =
        new("https://www.churchofjesuschrist.org/study/scriptures/nt/heb/7?lang=eng&id=p25#p25");

    [Fact]
    public async Task OpenStudy_OpensTheReferenceStudyLink()
    {
        var opener = new FakeLinkOpener();
        var card = MakeSingleChapter(openLinkAsync: opener.OpenAsync);

        await card.OpenStudyCommand.ExecuteAsync(null);

        Assert.Equal(CardStudyUri.ToString(), opener.LastOpened);
    }

    [Fact]
    public void HasStudyLink_IsTrue_WhenAUriWasBuilt() =>
        Assert.True(MakeSingleChapter().HasStudyLink);

    /// <summary>
    /// A reference we cannot build a URL for shows no link rather than offering a dead one.
    /// </summary>
    [Fact]
    public async Task OpenStudy_WithoutAUri_IsANoOpAndHidesTheLink()
    {
        var opener = new FakeLinkOpener();
        var card = Make(
            "Heb. 7:25",
            [new ContextLineViewModel(25, "Wherefore he is able", isTarget: true)],
            [],
            copyAsync: null,
            delayAsync: null,
            studyUri: null,
            openLinkAsync: opener.OpenAsync);

        await card.OpenStudyCommand.ExecuteAsync(null);

        Assert.False(card.HasStudyLink);
        Assert.Empty(opener.Opened);
    }

    [Fact]
    public async Task ChapterMemory_ExpandingOneChapterCollapsesTheOthers()
    {
        var card = MakeCrossChapter(usesChapterMemory: true);
        Assert.True(card.Segments[0].IsExpanded);

        await card.Segments[1].ToggleExpandedCommand.ExecuteAsync(null);

        Assert.False(card.Segments[0].IsExpanded);
        Assert.True(card.Segments[1].IsExpanded);
    }

    [Fact]
    public async Task ChapterMemory_ExpandingAChapterPersistsIt()
    {
        var saved = new List<(string RefId, int Ch)>();
        var card = MakeCrossChapter(
            usesChapterMemory: true,
            saveChapterAsync: (refId, ch) =>
            {
                saved.Add((refId, ch));
                return Task.CompletedTask;
            });

        await card.Segments[1].ToggleExpandedCommand.ExecuteAsync(null);

        Assert.Equal(("advocate:newtestament/heb/7/25", 10), Assert.Single(saved));
    }

    /// <summary>
    /// Collapsing the only open chapter is allowed, and must not record anything — the reader is
    /// looking at the index, not moving to a different chapter.
    /// </summary>
    [Fact]
    public async Task ChapterMemory_CollapsingTheOpenChapterSavesNothing()
    {
        var saved = new List<(string RefId, int Ch)>();
        var card = MakeCrossChapter(
            usesChapterMemory: true,
            saveChapterAsync: (refId, ch) =>
            {
                saved.Add((refId, ch));
                return Task.CompletedTask;
            });

        await card.Segments[0].ToggleExpandedCommand.ExecuteAsync(null);

        Assert.False(card.Segments[0].IsExpanded);
        Assert.Empty(saved);
    }

    /// <summary>
    /// A short span opens every chapter at once and stays that way; an accordion there would take
    /// away a passage the reader can otherwise read straight through.
    /// </summary>
    [Fact]
    public async Task WithoutChapterMemory_ChaptersDoNotCollapseEachOther()
    {
        var saved = new List<(string RefId, int Ch)>();
        var card = MakeCrossChapter(
            usesChapterMemory: false,
            saveChapterAsync: (refId, ch) =>
            {
                saved.Add((refId, ch));
                return Task.CompletedTask;
            });

        await card.Segments[1].ToggleExpandedCommand.ExecuteAsync(null);
        await card.Segments[1].ToggleExpandedCommand.ExecuteAsync(null);

        Assert.True(card.Segments[0].IsExpanded);
        Assert.Empty(saved);
    }

    [Fact]
    public void CopyText_SingleChapterReference_IsLabelThenNumberedVerses()
    {
        var card = MakeSingleChapter();

        Assert.Equal(
            "Heb. 7:25\n25 Wherefore he is able also to save them to the uttermost",
            card.CopyText);
    }

    [Fact]
    public void CopyText_CrossChapterReference_IncludesChapterHeaders()
    {
        var card = MakeCrossChapter();

        Assert.Equal(
            "Matt. 9:35–11:1\nMatthew 9\n35 And Jesus went about all the cities\nMatthew 10\n1 And when he had called",
            card.CopyText);
    }

    [Fact]
    public void CopyText_IncludesVersesOfCollapsedSegments()
    {
        var card = MakeCrossChapter();
        var secondChapter = card.Segments[1];
        secondChapter.IsExpanded = false;

        // A copy must carry the whole passage, not just what happens to be realized on screen.
        Assert.Empty(secondChapter.VisibleVerses);
        Assert.Contains("Matthew 10\n1 And when he had called", card.CopyText, StringComparison.Ordinal);
    }

    [Fact]
    public void CopyText_ExcludesContextVerses()
    {
        var card = MakeSingleChapter();

        Assert.Contains("before the target", card.Context.Select(c => c.Text));
        Assert.DoesNotContain("before the target", card.CopyText, StringComparison.Ordinal);
    }

    [Fact]
    public void CopyText_IsBuiltOnceAndCached()
    {
        var card = MakeSingleChapter();

        // Cards are all constructed up front by LoadAsync, so the join must never happen
        // at load - and never twice for a reader who copies the same card again.
        Assert.Same(card.CopyText, card.CopyText);
    }

    [Fact]
    public async Task Copy_WritesCopyTextToClipboard()
    {
        var writes = new List<string>();
        var card = MakeSingleChapter(copyAsync: text =>
        {
            writes.Add(text);
            return Task.CompletedTask;
        });

        await card.CopyCommand.ExecuteAsync(null);

        Assert.Equal([card.CopyText], writes);
    }

    [Fact]
    public async Task Copy_SetsJustCopiedWhileConfirming_ThenClears()
    {
        bool? whileWaiting = null;
        ReferenceCardViewModel? card = null;
        card = MakeSingleChapter(delayAsync: _ =>
        {
            whileWaiting = card!.JustCopied;
            return Task.CompletedTask;
        });

        Assert.False(card.JustCopied);

        await card.CopyCommand.ExecuteAsync(null);

        Assert.True(whileWaiting);
        Assert.False(card.JustCopied);
    }

    [Fact]
    public async Task Copy_WaitsTheConfirmationDuration()
    {
        var waits = new List<TimeSpan>();
        var card = MakeSingleChapter(delayAsync: duration =>
        {
            waits.Add(duration);
            return Task.CompletedTask;
        });

        await card.CopyCommand.ExecuteAsync(null);

        Assert.Equal([ReferenceCardViewModel.CopiedFeedbackDuration], waits);
    }

    [Fact]
    public async Task Copy_RaisesPropertyChangedForJustCopied()
    {
        var card = MakeSingleChapter();
        var changed = new List<string?>();
        card.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        await card.CopyCommand.ExecuteAsync(null);

        // Once on set, once on clear - the button label rides this both ways.
        Assert.Equal(2, changed.Count(name => name == nameof(ReferenceCardViewModel.JustCopied)));
    }

    [Fact]
    public async Task Copy_WhileConfirming_CannotBeInvokedAgain()
    {
        var gate = new TaskCompletionSource();
        var card = MakeSingleChapter(delayAsync: _ => gate.Task);

        Assert.True(card.CopyCommand.CanExecute(null));

        var first = card.CopyCommand.ExecuteAsync(null);

        // The generated AsyncRelayCommand reports CanExecute false while it is still running,
        // so the button is disabled for as long as it reads "Copied".
        Assert.False(card.CopyCommand.CanExecute(null));

        gate.SetResult();
        await first;

        Assert.True(card.CopyCommand.CanExecute(null));
    }

    [Fact]
    public async Task CopyVerse_BuildsHeaderFromChapterLabelAndVerseNumber()
    {
        var writes = new List<string>();
        var card = MakeCrossChapter(copyAsync: text =>
        {
            writes.Add(text);
            return Task.CompletedTask;
        });

        await card.CopyVerseCommand.ExecuteAsync(card.Segments[0].Verses[0]);

        Assert.Equal(["Matthew 9:35\nAnd Jesus went about all the cities"], writes);
    }

    [Fact]
    public async Task CopyVerse_OmitsTheVerseNumberFromTheBody()
    {
        var writes = new List<string>();
        var card = MakeSingleChapter(copyAsync: text =>
        {
            writes.Add(text);
            return Task.CompletedTask;
        });

        await card.CopyVerseCommand.ExecuteAsync(card.Segments[0].Verses[0]);

        // The number belongs in the header, not repeated at the head of the prose.
        Assert.Equal("Hebrews 7:25\nWherefore he is able also to save them to the uttermost", writes[0]);
    }

    [Fact]
    public async Task CopyVerse_CrossChapterCard_UsesTheOwningChaptersLabel()
    {
        var writes = new List<string>();
        var card = MakeCrossChapter(copyAsync: text =>
        {
            writes.Add(text);
            return Task.CompletedTask;
        });

        await card.CopyVerseCommand.ExecuteAsync(card.Segments[1].Verses[0]);

        // Not the first chapter's label - the verse belongs to Matthew 10.
        Assert.StartsWith("Matthew 10:1\n", writes[0], StringComparison.Ordinal);
    }

    [Fact]
    public async Task CopyVerse_ContextWindowVerse_FallsBackToTheReferenceChapter()
    {
        var writes = new List<string>();
        var card = MakeSingleChapter(copyAsync: text =>
        {
            writes.Add(text);
            return Task.CompletedTask;
        });

        // A context line is a different instance than its twin inside a segment, so it is
        // never found by the segment scan and takes the reference's own chapter.
        var contextOnly = card.Context.First(c => !c.IsTarget);
        await card.CopyVerseCommand.ExecuteAsync(contextOnly);

        Assert.Equal("Hebrews 7:24\nbefore the target", writes[0]);
    }

    [Fact]
    public async Task CopyVerse_WithNullParameter_DoesNothing()
    {
        var writes = new List<string>();
        var card = MakeSingleChapter(copyAsync: text =>
        {
            writes.Add(text);
            return Task.CompletedTask;
        });

        await card.CopyVerseCommand.ExecuteAsync(null);

        Assert.Empty(writes);
    }

    [Fact]
    public async Task CopyVerse_DoesNotDisturbTheCardLevelCopyConfirmation()
    {
        var card = MakeSingleChapter();

        await card.CopyVerseCommand.ExecuteAsync(card.Segments[0].Verses[0]);

        // The verse gesture confirms with a toast, not by flipping the card's copy button.
        Assert.False(card.JustCopied);
    }

    private static ReferenceCardViewModel MakeSingleChapter(
        Func<string, Task>? copyAsync = null,
        Func<TimeSpan, Task>? delayAsync = null,
        Func<Uri, Task>? openLinkAsync = null)
    {
        List<ContextLineViewModel> target =
            [new ContextLineViewModel(25, "Wherefore he is able also to save them to the uttermost", isTarget: true)];

        List<ContextLineViewModel> context =
            [new ContextLineViewModel(24, "before the target", isTarget: false), target[0]];

        return Make(
            "Heb. 7:25",
            context,
            [Segment(7, "Hebrews 7", showHeader: false, target)],
            copyAsync,
            delayAsync,
            CardStudyUri,
            openLinkAsync);
    }

    private static ReferenceCardViewModel MakeCrossChapter(
        Func<string, Task>? copyAsync = null,
        Func<TimeSpan, Task>? delayAsync = null,
        bool usesChapterMemory = false,
        Func<string, int, Task>? saveChapterAsync = null)
    {
        List<ContextLineViewModel> ninth =
            [new ContextLineViewModel(35, "And Jesus went about all the cities", isTarget: true)];

        List<ContextLineViewModel> tenth =
            [new ContextLineViewModel(1, "And when he had called", isTarget: true)];

        return Make(
            "Matt. 9:35–11:1",
            [.. ninth, .. tenth],
            [
                // With chapter memory the feed opens only the first chapter, which is the state the
                // accordion has to move from.
                Segment(9, "Matthew 9", showHeader: true, ninth),
                Segment(10, "Matthew 10", showHeader: true, tenth, isExpanded: !usesChapterMemory),
            ],
            copyAsync,
            delayAsync,
            CardStudyUri,
            usesChapterMemory: usesChapterMemory,
            saveChapterAsync: saveChapterAsync);
    }

    private static ChapterSegmentViewModel Segment(
        int ch,
        string chapterLabel,
        bool showHeader,
        IReadOnlyList<ContextLineViewModel> verses,
        bool isExpanded = true) =>
        new(
            ch,
            chapterLabel,
            showHeader,
            verses,
            isExpanded,
            new Uri($"https://www.churchofjesuschrist.org/study/scriptures/nt/matt/{ch}?lang=eng"),
            _ => Task.CompletedTask);

    private static ReferenceCardViewModel Make(
        string refLabel,
        IReadOnlyList<ContextLineViewModel> context,
        IReadOnlyList<ChapterSegmentViewModel> segments,
        Func<string, Task>? copyAsync,
        Func<TimeSpan, Task>? delayAsync,
        Uri? studyUri,
        Func<Uri, Task>? openLinkAsync = null,
        bool usesChapterMemory = false,
        Func<string, int, Task>? saveChapterAsync = null) =>
        new(
            "advocate:newtestament/heb/7/25",
            refLabel,
            string.Join(' ', context.Where(c => c.IsTarget).Select(c => c.Text)),
            gloss: null,
            context,
            segments,
            isRead: false,
            hasNote: false,
            setReadAsync: (_, _) => Task.CompletedTask,
            openNoteAsync: _ => Task.CompletedTask,
            onReadCollapsed: _ => { },
            copyAsync: copyAsync ?? (_ => Task.CompletedTask),
            copyVerseAsync: copyAsync ?? (_ => Task.CompletedTask),
            studyUri: studyUri,
            openLinkAsync: openLinkAsync ?? (_ => Task.CompletedTask),
            usesChapterMemory: usesChapterMemory,
            saveChapterAsync: saveChapterAsync ?? ((_, _) => Task.CompletedTask),
            delayAsync: delayAsync ?? (_ => Task.CompletedTask));
}
