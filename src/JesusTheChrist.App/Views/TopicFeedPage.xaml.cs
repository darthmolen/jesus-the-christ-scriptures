using JesusTheChrist.Presentation.Navigation;
using JesusTheChrist.Presentation.ViewModels;
using JesusTheChrist.Presentation.Views;

namespace JesusTheChrist.App.Views;

/// <summary>
/// A single sub-topic's reference feed: the verse cards a reader studies.
/// </summary>
public partial class TopicFeedPage : ContentPage, IQueryAttributable
{
    private readonly TopicFeedViewModel viewModel;
    private string? topicKey;
    private bool isVisible;
    private int? lastFirstVisible;
    private int? lastLastVisible;

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
            // Fresh topic: drop any cached scroll state so stale indices from a previously
            // shown topic can't misclassify the first collapse.
            this.lastFirstVisible = null;
            this.lastLastVisible = null;
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

    private void OnReferencesScrolled(object? sender, ItemsViewScrolledEventArgs e)
    {
        // Cache primitives, not the framework args object. These are read before we issue our
        // own ScrollTo below, so the Scrolled event that scroll raises can't feed back into a
        // later decision.
        this.lastFirstVisible = e.FirstVisibleItemIndex;
        this.lastLastVisible = e.LastVisibleItemIndex;
    }

    private void OnCardCollapsedAfterRead(object? sender, ReferenceCardEventArgs e)
    {
        // The card has just rolled up. Defer until its smaller layout settles, then re-anchor.
        // End rolls the collapsed heading down to the footer's old spot when the card had
        // filled the viewport (tall-card case); MakeVisible no-ops for a short, already-visible
        // card, leaving the next reference right below it. ScrollAnchor.Resolve owns that call.
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

                var index = this.viewModel.References.IndexOf(e.Card);
                var anchor = ScrollAnchor.Resolve(index, this.lastFirstVisible, this.lastLastVisible);
                var position = anchor switch
                {
                    ScrollAnchorPosition.End => ScrollToPosition.End,
                    _ => ScrollToPosition.MakeVisible,
                };

                this.ReferencesView.ScrollTo(e.Card, position: position, animate: true);
            });
    }
}
