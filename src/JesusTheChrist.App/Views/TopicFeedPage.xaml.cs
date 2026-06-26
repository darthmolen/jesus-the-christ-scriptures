using JesusTheChrist.Presentation.Navigation;
using JesusTheChrist.Presentation.ViewModels;

namespace JesusTheChrist.App.Views;

/// <summary>
/// A single sub-topic's reference feed: the verse cards a reader studies.
/// </summary>
public partial class TopicFeedPage : ContentPage, IQueryAttributable
{
    private const int PositionSaveDebounceMs = 750;
    private readonly TopicFeedViewModel viewModel;
    private string? topicKey;
    private bool isVisible;
    private int saveGeneration;

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
            this.ScrollToResumePosition();
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

        // Final flush so leaving the topic always persists the latest reading position.
        // Fire-and-forget: the helper contains its own failures so teardown can't be torn down.
        _ = this.FlushPositionAsync();
    }

    private void OnReferencesScrolled(object? sender, ItemsViewScrolledEventArgs e)
    {
        var index = e.FirstVisibleItemIndex;
        if (index < 0 || index >= this.viewModel.References.Count)
        {
            return;
        }

        this.viewModel.RecordVisible(this.viewModel.References[index].Id);

        // Persist shortly after scrolling settles (debounced) so an OS kill of a backgrounded
        // app never loses more than the last moment of position, without writing on every tick.
        var generation = ++this.saveGeneration;
        this.Dispatcher.DispatchDelayed(
            TimeSpan.FromMilliseconds(PositionSaveDebounceMs),
            () =>
            {
                // Skip if a newer scroll superseded this one or the reader already left.
                if (generation == this.saveGeneration && this.isVisible)
                {
                    _ = this.FlushPositionAsync();
                }
            });
    }

    private async Task FlushPositionAsync()
    {
        try
        {
            await this.viewModel.SavePositionAsync();
        }
        catch (Exception ex)
        {
            // Persisting the reading position is best-effort; a storage failure must never
            // crash the UI thread during scroll or page teardown.
            System.Diagnostics.Debug.WriteLine($"Failed to save reading position: {ex}");
        }
    }

    private void ScrollToResumePosition()
    {
        var resume = this.viewModel.ResumeCard();
        if (resume is null)
        {
            return;
        }

        // Defer until layout settles, then park the last-viewed reference at the top so the
        // reader resumes where they left off. Restore without animation — they're arriving.
        this.Dispatcher.DispatchDelayed(
            TimeSpan.FromMilliseconds(100),
            () =>
            {
                if (!this.isVisible)
                {
                    return;
                }

                this.ReferencesView.ScrollTo(resume, position: ScrollToPosition.Start, animate: false);
            });
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
