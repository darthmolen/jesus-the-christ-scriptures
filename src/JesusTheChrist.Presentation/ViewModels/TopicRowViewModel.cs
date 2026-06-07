namespace JesusTheChrist.Presentation.ViewModels;

/// <summary>
/// An immutable snapshot of one sub-topic row on the Home screen.
/// </summary>
public sealed class TopicRowViewModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TopicRowViewModel"/> class.
    /// </summary>
    /// <param name="key">The sub-topic's language-invariant key.</param>
    /// <param name="title">The full sub-topic title.</param>
    /// <param name="shortTitle">The short sub-topic title.</param>
    /// <param name="read">The number of references read in this sub-topic.</param>
    /// <param name="total">The total number of references in this sub-topic.</param>
    public TopicRowViewModel(string key, string title, string shortTitle, int read, int total)
    {
        this.Key = key;
        this.Title = title;
        this.ShortTitle = shortTitle;
        this.Read = read;
        this.Total = total;
    }

    /// <summary>
    /// Gets the sub-topic's language-invariant key.
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// Gets the full sub-topic title.
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// Gets the short sub-topic title.
    /// </summary>
    public string ShortTitle { get; }

    /// <summary>
    /// Gets the number of references read in this sub-topic.
    /// </summary>
    public int Read { get; }

    /// <summary>
    /// Gets the total number of references in this sub-topic.
    /// </summary>
    public int Total { get; }

    /// <summary>
    /// Gets the read fraction in the range [0, 1]; zero when the sub-topic has no references.
    /// </summary>
    public double Fraction => this.Total == 0 ? 0.0 : (double)this.Read / this.Total;

    /// <summary>
    /// Gets the "read / total" progress label.
    /// </summary>
    public string ProgressLabel => $"{this.Read} / {this.Total}";
}
