namespace JesusTheChrist.Presentation.Views;

/// <summary>
/// Decides how the feed re-anchors a card that has just collapsed on "done", isolated from
/// the view so it can be unit-tested. Kept dependency-free; the view maps the result onto
/// MAUI's <c>ScrollToPosition</c>.
/// </summary>
public static class ScrollAnchor
{
    /// <summary>
    /// Resolves the scroll anchor for a just-collapsed card from the feed's last known
    /// visible-item range.
    /// </summary>
    /// <param name="cardIndex">The index of the collapsed card in the feed.</param>
    /// <param name="firstVisible">The first visible item index at the last scroll, or
    /// <see langword="null"/> if the feed has not scrolled.</param>
    /// <param name="lastVisible">The last visible item index at the last scroll, or
    /// <see langword="null"/> if the feed has not scrolled.</param>
    /// <returns>
    /// <see cref="ScrollAnchorPosition.End"/> when the card filled the viewport (its heading
    /// had scrolled above the top edge), otherwise <see cref="ScrollAnchorPosition.MakeVisible"/>.
    /// </returns>
    public static ScrollAnchorPosition Resolve(int cardIndex, int? firstVisible, int? lastVisible)
    {
        // The card filled the viewport when it was the first visible item and at most the
        // next item peeked in below it — so its heading had scrolled above the top edge.
        // The "<= cardIndex + 1" allowance is the known weak spot to confirm on-device.
        var filledViewport = firstVisible == cardIndex
            && lastVisible is { } last
            && last <= cardIndex + 1;

        return filledViewport ? ScrollAnchorPosition.End : ScrollAnchorPosition.MakeVisible;
    }
}
