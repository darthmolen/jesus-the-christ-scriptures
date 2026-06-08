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

        if (query.TryGetValue(NavigationRoutes.NoteRefIdParameter, out var value))
        {
            this.referenceId = value?.ToString();
        }
    }

    /// <inheritdoc/>
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (!string.IsNullOrEmpty(this.referenceId))
        {
            await this.viewModel.LoadAsync(this.referenceId);
        }
    }
}
