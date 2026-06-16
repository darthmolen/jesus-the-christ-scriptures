namespace JesusTheChrist.Presentation.ViewModels;

/// <summary>
/// Carries the <see cref="ReferenceCardViewModel"/> a feed event concerns.
/// </summary>
public sealed class ReferenceCardEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ReferenceCardEventArgs"/> class.
    /// </summary>
    /// <param name="card">The reference card the event concerns.</param>
    public ReferenceCardEventArgs(ReferenceCardViewModel card) => this.Card = card;

    /// <summary>
    /// Gets the reference card the event concerns.
    /// </summary>
    public ReferenceCardViewModel Card { get; }
}
