namespace JesusTheChrist.Presentation.Views;

/// <summary>
/// Where the feed should re-anchor a card after it collapses on "done". Mirrors the subset
/// of MAUI's <c>ScrollToPosition</c> the feed uses; mapped to it in the view code-behind so
/// this decision stays free of any view dependency and remains unit-testable.
/// </summary>
public enum ScrollAnchorPosition
{
    /// <summary>
    /// Roll the collapsed heading down to the bottom of the viewport — the footer ✓'s old
    /// spot — for a tall card whose heading had scrolled above the top edge.
    /// </summary>
    End,

    /// <summary>
    /// Bring the card into view with the least scrolling, which no-ops for a short card that
    /// is already fully visible, leaving the next reference right below it.
    /// </summary>
    MakeVisible,
}
