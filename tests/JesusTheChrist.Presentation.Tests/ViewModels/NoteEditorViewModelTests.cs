using JesusTheChrist.Data;
using JesusTheChrist.Presentation.Tests.Fakes;
using JesusTheChrist.Presentation.ViewModels;

namespace JesusTheChrist.Presentation.Tests.ViewModels;

public class NoteEditorViewModelTests
{
    private const string RefId = "advocate:newtestament/heb/7/25";

    [Fact]
    public async Task Load_ExistingNote_PopulatesText()
    {
        await using var harness = await Harness.CreateAsync();
        await harness.Notes.SaveAsync(RefId, "He pleads my cause.");

        await harness.ViewModel.LoadAsync(RefId);

        Assert.Equal("He pleads my cause.", harness.ViewModel.Text);
        Assert.True(harness.ViewModel.HasExistingNote);
    }

    [Fact]
    public async Task Load_PopulatesScripturePaneFromVerseContext()
    {
        await using var harness = await Harness.CreateAsync();
        var verses = new[]
        {
            new ContextLineViewModel(25, "Wherefore he is able also to save them to the uttermost.", true),
        };

        await harness.ViewModel.LoadAsync(RefId, "Heb. 7:25", verses);

        Assert.Equal("Heb. 7:25", harness.ViewModel.ReferenceLabel);
        Assert.Single(harness.ViewModel.Verses);
        Assert.Equal(25, harness.ViewModel.Verses[0].Verse);
        Assert.Contains("uttermost", harness.ViewModel.Verses[0].Text);
        Assert.True(harness.ViewModel.HasVerse);
    }

    [Fact]
    public async Task Load_WithoutVerseContext_HasNoVerse()
    {
        await using var harness = await Harness.CreateAsync();

        await harness.ViewModel.LoadAsync(RefId);

        Assert.Empty(harness.ViewModel.Verses);
        Assert.False(harness.ViewModel.HasVerse);
    }

    [Fact]
    public async Task Load_NoNote_IsEmpty()
    {
        await using var harness = await Harness.CreateAsync();

        await harness.ViewModel.LoadAsync(RefId);

        Assert.Equal(string.Empty, harness.ViewModel.Text);
        Assert.False(harness.ViewModel.HasExistingNote);
    }

    [Fact]
    public async Task Save_PersistsAndNavigatesBack()
    {
        await using var harness = await Harness.CreateAsync();
        await harness.ViewModel.LoadAsync(RefId);
        harness.ViewModel.Text = "My note.";

        await harness.ViewModel.SaveCommand.ExecuteAsync(null);

        Assert.Equal("My note.", await harness.Notes.GetAsync(RefId));
        Assert.Equal(1, harness.Navigation.BackCount);
    }

    [Fact]
    public async Task Save_EmptyText_DeletesNote()
    {
        await using var harness = await Harness.CreateAsync();
        await harness.Notes.SaveAsync(RefId, "old");
        await harness.ViewModel.LoadAsync(RefId);
        harness.ViewModel.Text = "   ";

        await harness.ViewModel.SaveCommand.ExecuteAsync(null);

        Assert.False(await harness.Notes.HasNoteAsync(RefId));
    }

    [Fact]
    public async Task Delete_RemovesNoteAndNavigatesBack()
    {
        await using var harness = await Harness.CreateAsync();
        await harness.Notes.SaveAsync(RefId, "old");
        await harness.ViewModel.LoadAsync(RefId);

        await harness.ViewModel.DeleteCommand.ExecuteAsync(null);

        Assert.False(await harness.Notes.HasNoteAsync(RefId));
        Assert.Equal(1, harness.Navigation.BackCount);
    }

    [Fact]
    public async Task Load_WithBlankRefId_StaysEmptyWithoutQuerying()
    {
        await using var harness = await Harness.CreateAsync();

        await harness.ViewModel.LoadAsync("   ");

        Assert.Equal(string.Empty, harness.ViewModel.Text);
        Assert.False(harness.ViewModel.HasExistingNote);
    }

    [Fact]
    public async Task Save_WithoutValidReference_DoesNotPersistButNavigatesBack()
    {
        await using var harness = await Harness.CreateAsync();

        // LoadAsync was never called, so there is no reference id.
        harness.ViewModel.Text = "orphan note";
        await harness.ViewModel.SaveCommand.ExecuteAsync(null);

        Assert.Empty(await harness.Notes.GetNoteIdsAsync());
        Assert.Equal(1, harness.Navigation.BackCount);
    }

    private sealed class Harness : IAsyncDisposable
    {
        private readonly TempDatabase database;

        private Harness(TempDatabase database, NoteEditorViewModel viewModel, NoteStore notes, RecordingNavigationService navigation)
        {
            this.database = database;
            this.ViewModel = viewModel;
            this.Notes = notes;
            this.Navigation = navigation;
        }

        public NoteEditorViewModel ViewModel { get; }

        public NoteStore Notes { get; }

        public RecordingNavigationService Navigation { get; }

        public static async Task<Harness> CreateAsync()
        {
            var db = await TempDatabase.CreateAsync();
            var notes = new NoteStore(db.Db);
            var navigation = new RecordingNavigationService();
            var vm = new NoteEditorViewModel(notes, db, navigation);
            return new Harness(db, vm, notes, navigation);
        }

        public async ValueTask DisposeAsync() => await this.database.DisposeAsync();
    }
}
