using JesusTheChrist.Presentation.Navigation;
using JesusTheChrist.Presentation.ViewModels;

namespace JesusTheChrist.App.Views;

/// <summary>
/// The per-reference note editor.
/// </summary>
public partial class NoteEditorPage : ContentPage, IQueryAttributable
{
    private readonly NoteEditorViewModel viewModel;
    private string? referenceId;
    private bool loaded;

    /// <summary>
    /// Initializes a new instance of the <see cref="NoteEditorPage"/> class.
    /// </summary>
    /// <param name="viewModel">The note editor view model.</param>
    public NoteEditorPage(NoteEditorViewModel viewModel)
    {
        this.InitializeComponent();
        this.viewModel = viewModel;
        this.BindingContext = viewModel;
    }

    /// <inheritdoc/>
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        ArgumentNullException.ThrowIfNull(query);

        // Always reset from the current query so a reused instance can't keep a stale id.
        this.referenceId = query.TryGetValue(NavigationRoutes.NoteRefIdParameter, out var value)
            ? value?.ToString()?.Trim()
            : null;
    }

    /// <inheritdoc/>
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Load once per navigation so resuming the app doesn't overwrite in-progress edits.
        if (!this.loaded && !string.IsNullOrEmpty(this.referenceId))
        {
            this.loaded = true;
            await this.viewModel.LoadAsync(this.referenceId);
        }
    }
}
