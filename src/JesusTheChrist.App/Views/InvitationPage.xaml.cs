using JesusTheChrist.Presentation.ViewModels;

namespace JesusTheChrist.App.Views;

/// <summary>
/// The first-run invitation screen, also reachable from Settings. Renders the
/// bundled invitation markdown for the current language.
/// </summary>
public partial class InvitationPage : ContentPage
{
    private readonly InvitationViewModel viewModel;
    private bool loaded;

    /// <summary>
    /// Initializes a new instance of the <see cref="InvitationPage"/> class.
    /// </summary>
    /// <param name="viewModel">The invitation view model.</param>
    public InvitationPage(InvitationViewModel viewModel)
    {
        this.InitializeComponent();
        this.viewModel = viewModel;
        this.BindingContext = viewModel;
    }

    /// <inheritdoc/>
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (!this.loaded)
        {
            this.loaded = true;
            await this.viewModel.LoadAsync();
        }
    }
}
