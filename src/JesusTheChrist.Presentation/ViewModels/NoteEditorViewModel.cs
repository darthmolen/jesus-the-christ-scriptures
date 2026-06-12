using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JesusTheChrist.Data;
using JesusTheChrist.Presentation.Data;
using JesusTheChrist.Presentation.Navigation;

namespace JesusTheChrist.Presentation.ViewModels;

/// <summary>
/// Backs the per-reference note editor: loads the saved note, persists edits
/// (empty text deletes), and navigates back when done.
/// </summary>
public partial class NoteEditorViewModel : ObservableObject
{
    private readonly NoteStore notes;
    private readonly IDatabaseInitializer databaseInitializer;
    private readonly INavigationService navigation;
    private string referenceId = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="NoteEditorViewModel"/> class.
    /// </summary>
    /// <param name="notes">The note store.</param>
    /// <param name="databaseInitializer">Ensures the database schema before reads.</param>
    /// <param name="navigation">The navigation service.</param>
    public NoteEditorViewModel(NoteStore notes, IDatabaseInitializer databaseInitializer, INavigationService navigation)
    {
        this.notes = notes;
        this.databaseInitializer = databaseInitializer;
        this.navigation = navigation;
    }

    /// <summary>
    /// Gets or sets the note text being edited.
    /// </summary>
    [ObservableProperty]
    public partial string Text { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether a saved note already exists.
    /// </summary>
    [ObservableProperty]
    public partial bool HasExistingNote { get; set; }

    /// <summary>
    /// Gets or sets the display label of the reference being annotated (for example "Heb. 7:25").
    /// </summary>
    [ObservableProperty]
    public partial string ReferenceLabel { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the target verse text shown in the scripture pane.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasVerse))]
    public partial string VerseText { get; set; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether there is verse text to show in the scripture pane.
    /// </summary>
    public bool HasVerse => !string.IsNullOrWhiteSpace(this.VerseText);

    /// <summary>
    /// Loads the saved note for the given reference and the verse context shown alongside it.
    /// </summary>
    /// <param name="refId">The reference id whose note is edited.</param>
    /// <param name="referenceLabel">The reference display label for the scripture pane.</param>
    /// <param name="verseText">The target verse text for the scripture pane.</param>
    /// <returns>A task that completes when the note is loaded.</returns>
    public async Task LoadAsync(string refId, string referenceLabel = "", string verseText = "")
    {
        this.referenceId = (refId ?? string.Empty).Trim();
        this.ReferenceLabel = (referenceLabel ?? string.Empty).Trim();
        this.VerseText = (verseText ?? string.Empty).Trim();
        this.Text = string.Empty;
        this.HasExistingNote = false;
        if (this.referenceId.Length == 0)
        {
            return;
        }

        await this.databaseInitializer.EnsureInitializedAsync();
        var existing = await this.notes.GetAsync(this.referenceId);
        this.Text = existing ?? string.Empty;
        this.HasExistingNote = !string.IsNullOrWhiteSpace(existing);
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (this.referenceId.Length > 0)
        {
            await this.databaseInitializer.EnsureInitializedAsync();
            await this.notes.SaveAsync(this.referenceId, this.Text);
        }

        await this.navigation.GoBackAsync();
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (this.referenceId.Length > 0)
        {
            await this.databaseInitializer.EnsureInitializedAsync();
            await this.notes.DeleteAsync(this.referenceId);
        }

        await this.navigation.GoBackAsync();
    }
}
