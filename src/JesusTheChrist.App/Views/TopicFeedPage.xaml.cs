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
    private bool isVisible;

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

        // OnAppearing can fire without a matching OnDisappearing (e.g. app resume), so remove
        // before adding to keep the subscription single — otherwise the scroll handler stacks.
        this.viewModel.CardCollapsedAfterRead -= this.OnCardCollapsedAfterRead;
        this.viewModel.CardCollapsedAfterRead += this.OnCardCollapsedAfterRead;
        this.isVisible = true;

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
        this.isVisible = false;
        this.viewModel.CardCollapsedAfterRead -= this.OnCardCollapsedAfterRead;
    }

    private void OnCardCollapsedAfterRead(object? sender, ReferenceCardEventArgs e)
    {
        // The card just rolled up to its heading. Defer until the smaller layout settles, then
        // scroll that heading to the top of the viewport so the next reference is cued up right
        // below it — turning "done" into a smooth advance to the next scripture.
        this.Dispatcher.DispatchDelayed(
            TimeSpan.FromMilliseconds(100),
            () =>
            {
                // The reader may have navigated away during the delay; only scroll while this
                // page is still visible, or the deferred scroll could jump or hit a stale view.
                if (!this.isVisible)
                {
                    return;
                }

                this.ReferencesView.ScrollTo(e.Card, position: ScrollToPosition.Start, animate: true);
            });
    }
}
