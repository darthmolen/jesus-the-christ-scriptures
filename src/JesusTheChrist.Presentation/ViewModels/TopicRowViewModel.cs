using System.Globalization;
using JesusTheChrist.Presentation.Resources;

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
    /// Gets a value indicating whether every reference in this sub-topic has been read.
    /// An empty sub-topic is never complete.
    /// </summary>
    public bool IsComplete => this.Total > 0 && this.Read >= this.Total;

    /// <summary>
    /// Gets the "read / total" progress label.
    /// </summary>
    public string ProgressLabel => $"{this.Read} / {this.Total}";

    /// <summary>
    /// Gets the screen-reader description of this row's progress. Completion is signalled
    /// visually by the gold ring alone, so it has to be spoken here as well.
    /// </summary>
    public string ProgressDescription
    {
        get
        {
            if (!this.IsComplete)
            {
                return this.ProgressLabel;
            }

            // Format with the same culture that resolves the string, matching HomeViewModel.
            var culture = AppResources.Culture ?? CultureInfo.CurrentUICulture;
#pragma warning disable CA1863 // Format string is culture-dependent (changes on language switch); a cached CompositeFormat cannot be used.
            return string.Format(culture, AppResources.HomeTopicCompleteFormat, this.Read, this.Total);
#pragma warning restore CA1863
        }
    }
}
