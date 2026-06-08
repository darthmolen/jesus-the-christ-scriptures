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
