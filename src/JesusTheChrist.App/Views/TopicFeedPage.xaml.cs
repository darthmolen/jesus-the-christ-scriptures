using JesusTheChrist.Presentation.Navigation;
using JesusTheChrist.Presentation.Resources;
using JesusTheChrist.Presentation.ViewModels;

namespace JesusTheChrist.App.Views;

/// <summary>
/// A single sub-topic's reference feed: the verse cards a reader studies.
/// </summary>
public partial class TopicFeedPage : ContentPage, IQueryAttributable
{
    private const int PositionSaveDebounceMs = 750;

    /// <summary>
    /// How long a verse must be held before releasing copies it. Just past Android's ~500ms
    /// long-press threshold, so a quick tap or a scroll flick never arms it.
    /// </summary>
    private const int VerseHoldMs = 600;

    /// <summary>
    /// How far a pointer may drift during a hold before it is read as a scroll and abandoned.
    /// Without this, every scroll that begins on a verse would start arming a copy.
    /// </summary>
    private const double VerseHoldSlopUnits = 12;

    /// <summary>
    /// Peak opacity of the hold highlight, so the ramp tints the line without drowning the text.
    /// </summary>
    private const float VerseHoldPeakAlpha = 0.28f;

    private const string VerseHoldAnimation = "verseHold";
    private const int ToastFadeInMs = 150;
    private const int ToastFadeOutMs = 250;
    private const int ToastDwellMs = 1600;

    private readonly TopicFeedViewModel viewModel;
    private string? topicKey;
    private bool isVisible;
    private int saveGeneration;
    private int toastGeneration;
    private Label? heldVerse;
    private Point heldOrigin;

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
        this.viewModel.VerseCopied -= this.OnVerseCopied;
        this.viewModel.VerseCopied += this.OnVerseCopied;
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
        this.viewModel.VerseCopied -= this.OnVerseCopied;

        // A hold in flight when the reader navigates away must not leave a tinted line behind.
        this.CancelVerseHold();

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

    private void OnVersePointerPressed(object? sender, PointerEventArgs e)
    {
        if (sender is not Label verse || e is null)
        {
            return;
        }

        this.CancelVerseHold();

        this.heldVerse = verse;
        this.heldOrigin = e.GetPosition(this) ?? Point.Zero;

        // Ramp the line's background so the reader can watch the gesture arm. Linear, because
        // the ramp doubles as a progress bar — an eased one would misreport how much is left.
        // The ramp is also the clock: reaching the end *is* the copy, so the reader never has to
        // let go to commit. Every way of abandoning the hold aborts the animation, which arrives
        // here as cancelled — so the gesture is strictly either/or.
        var tint = VerseHoldTint();
        verse.Animate(
            VerseHoldAnimation,
            value => verse.BackgroundColor = tint.WithAlpha((float)value),
            start: 0,
            end: VerseHoldPeakAlpha,
            length: VerseHoldMs,
            easing: Easing.Linear,
            finished: (_, cancelled) =>
            {
                if (!cancelled)
                {
                    CopyVerse(verse);
                }
            });
    }

    private static void CopyVerse(Label verse)
    {
        // The card owns the command; the verse rides in as its parameter, so no per-verse
        // command is ever allocated for the hundreds of lines a feed can hold.
        if (verse.BindingContext is ContextLineViewModel line
            && CardFor(verse) is { } card
            && card.CopyVerseCommand.CanExecute(line))
        {
            card.CopyVerseCommand.Execute(line);
        }
    }

    private void OnVersePointerMoved(object? sender, PointerEventArgs e)
    {
        if (this.heldVerse is null || !ReferenceEquals(sender, this.heldVerse) || e is null)
        {
            return;
        }

        var current = e.GetPosition(this);
        if (current is null)
        {
            return;
        }

        // The feed scrolls, and a drag that begins on a verse arrives here as a press followed by
        // movement. Past the slop radius this is a scroll, not a hold — let go of it entirely.
        var dx = current.Value.X - this.heldOrigin.X;
        var dy = current.Value.Y - this.heldOrigin.Y;
        if ((dx * dx) + (dy * dy) > VerseHoldSlopUnits * VerseHoldSlopUnits)
        {
            this.CancelVerseHold();
        }
    }

    /// <summary>
    /// Ends a hold, whether the reader lifted their finger or dragged off the line. Lifting early
    /// aborts the ramp and copies nothing; lifting after it completed simply clears the highlight,
    /// because the copy already fired when the ramp reached its end.
    /// </summary>
    /// <param name="sender">The verse label the pointer left.</param>
    /// <param name="e">The pointer event.</param>
    private void OnVerseHoldEnded(object? sender, PointerEventArgs e)
    {
        if (this.heldVerse is not null && ReferenceEquals(sender, this.heldVerse))
        {
            this.CancelVerseHold();
        }
    }

    /// <summary>
    /// Walks up from a verse line to the reference card that owns it. Verse labels live several
    /// layouts deep inside the card's data template, so the card is found by binding context
    /// rather than by a fixed number of parent hops.
    /// </summary>
    /// <param name="verse">The verse label the reader held.</param>
    /// <returns>The owning card, or null if the visual tree has already been torn down.</returns>
    private static ReferenceCardViewModel? CardFor(Element verse)
    {
        var node = (Element?)verse;
        while (node is not null)
        {
            if (node.BindingContext is ReferenceCardViewModel card)
            {
                return card;
            }

            node = node.Parent;
        }

        return null;
    }

    private void CancelVerseHold()
    {
        var verse = this.heldVerse;
        this.heldVerse = null;
        if (verse is null)
        {
            return;
        }

        verse.AbortAnimation(VerseHoldAnimation);
        verse.BackgroundColor = Colors.Transparent;
    }

    private static Color VerseHoldTint()
    {
        var key = Application.Current?.RequestedTheme == AppTheme.Dark
            ? "VerseHoldHighlightDark"
            : "VerseHoldHighlightLight";

        return Application.Current?.Resources.TryGetValue(key, out var value) == true && value is Color color
            ? color
            : Colors.CornflowerBlue;
    }

    private void OnVerseCopied(object? sender, EventArgs e) =>
        this.ShowToast(AppResources.ToastVerseCopied);

    private void ShowToast(string message)
    {
        this.ToastLabel.Text = message;
        this.ToastHost.IsVisible = true;

        // Generation guard, same idea as the position-save debounce: a second copy while the
        // first toast is still up restarts the dwell instead of letting the older one hide it.
        var generation = ++this.toastGeneration;
        _ = this.ToastHost.FadeToAsync(1, ToastFadeInMs);

        this.Dispatcher.DispatchDelayed(
            TimeSpan.FromMilliseconds(ToastDwellMs),
            () =>
            {
                if (generation == this.toastGeneration && this.isVisible)
                {
                    _ = this.HideToastAsync(generation);
                }
            });
    }

    private async Task HideToastAsync(int generation)
    {
        await this.ToastHost.FadeToAsync(0, ToastFadeOutMs);

        // A newer toast may have claimed the host mid-fade; only the current owner hides it.
        if (generation == this.toastGeneration)
        {
            this.ToastHost.IsVisible = false;
        }
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
