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
        this.viewModel.CardCollapsedAfterRead += this.OnCardCollapsedAfterRead;

        if (string.IsNullOrEmpty(this.topicKey))
        {
            return;
        }

        if (this.viewModel.References.Count == 0)
        {
            await this.viewModel.LoadAsync(this.topicKey);
        }
        else
        {
            // Returning from the note editor — refresh the note indicators.
            await this.viewModel.RefreshNotesAsync();
        }
    }

    /// <inheritdoc/>
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        this.viewModel.CardCollapsedAfterRead -= this.OnCardCollapsedAfterRead;
    }

    private void OnCardCollapsedAfterRead(object? sender, ReferenceCardEventArgs e)
    {
        // The card has just rolled up. Defer until its smaller layout settles, then bring it
        // back into view: MakeVisible scrolls up to the collapsed card when it sits above the
        // viewport (the tall-card case) and leaves a short, already-visible card untouched, so
        // the next reference lands right below it instead of off the bottom of the screen.
        this.Dispatcher.DispatchDelayed(
            TimeSpan.FromMilliseconds(100),
            () => this.ReferencesView.ScrollTo(e.Card, position: ScrollToPosition.MakeVisible, animate: true));
    }
}
