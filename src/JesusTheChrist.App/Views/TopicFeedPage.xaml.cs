using JesusTheChrist.Presentation.Navigation;
using JesusTheChrist.Presentation.ViewModels;

namespace JesusTheChrist.App.Views;

/// <summary>
/// A single sub-topic's reference feed: the verse cards a reader studies.
/// </summary>
public partial class TopicFeedPage : ContentPage, IQueryAttributable
{
    private readonly TopicFeedViewModel viewModel;
    private string? topicKey;

    /// <summary>
    /// Initializes a new instance of the <see cref="TopicFeedPage"/> class.
    /// </summary>
    /// <param name="viewModel">The topic feed view model.</param>
    public TopicFeedPage(TopicFeedViewModel viewModel)
    {
        this.InitializeComponent();
        this.viewModel = viewModel;
        this.BindingContext = viewModel;
    }

    /// <inheritdoc/>
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (query.TryGetValue(NavigationRoutes.TopicKeyParameter, out var value))
        {
            this.topicKey = value?.ToString();
        }
    }

    /// <inheritdoc/>
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (!string.IsNullOrEmpty(this.topicKey) && this.viewModel.References.Count == 0)
        {
            await this.viewModel.LoadAsync(this.topicKey);
        }
    }
}
