using JesusTheChrist.Presentation.Resources;
using JesusTheChrist.Presentation.ViewModels;

namespace JesusTheChrist.App.Views;

/// <summary>
/// The Home / Challenge page: the 53 sub-topics with per-topic progress rings
/// under the overall challenge header.
/// </summary>
public partial class HomePage : ContentPage
{
    private readonly HomeViewModel viewModel;
    private bool checkedFirstRun;

    /// <summary>
    /// Initializes a new instance of the <see cref="HomePage"/> class.
    /// </summary>
    /// <param name="viewModel">The Home view model.</param>
    public HomePage(HomeViewModel viewModel)
    {
        this.InitializeComponent();
        this.viewModel = viewModel;
        this.BindingContext = viewModel;
    }

    /// <inheritdoc/>
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Re-resolve localized chrome for the current culture. Home persists as the Shell root
        // across a live language switch, so its x:Static title/toolbar text don't update on their own.
        this.Title = AppResources.AppTitle;
        this.SettingsToolbarItem.Text = AppResources.SettingsTitle;

        await this.viewModel.LoadCommand.ExecuteAsync(null);

        // First launch only: show the invitation once, then never again.
        if (!this.checkedFirstRun)
        {
            this.checkedFirstRun = true;
            if (await this.viewModel.IsInvitationUnseenAsync())
            {
                await this.viewModel.OpenInvitationCommand.ExecuteAsync(null);
            }
        }
    }
}
