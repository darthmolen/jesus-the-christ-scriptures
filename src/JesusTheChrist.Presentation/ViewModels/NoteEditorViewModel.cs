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
    /// Loads the saved note for the given reference.
    /// </summary>
    /// <param name="refId">The reference id whose note is edited.</param>
    /// <returns>A task that completes when the note is loaded.</returns>
    public async Task LoadAsync(string refId)
    {
        await this.databaseInitializer.EnsureInitializedAsync();
        this.referenceId = refId;
        var existing = await this.notes.GetAsync(refId);
        this.Text = existing ?? string.Empty;
        this.HasExistingNote = !string.IsNullOrWhiteSpace(existing);
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        await this.notes.SaveAsync(this.referenceId, this.Text);
        await this.navigation.GoBackAsync();
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        await this.notes.DeleteAsync(this.referenceId);
        await this.navigation.GoBackAsync();
    }
}
