using JesusTheChrist.Presentation.Navigation;

namespace JesusTheChrist.App.Views;

/// <summary>
/// Placeholder destination for a tapped sub-topic. The verse-focused reference
/// feed is built in Plan 04; this confirms navigation end-to-end.
/// </summary>
public partial class TopicStubPage : ContentPage, IQueryAttributable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TopicStubPage"/> class.
    /// </summary>
    public TopicStubPage()
    {
        this.InitializeComponent();
    }

    /// <inheritdoc/>
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        ArgumentNullException.ThrowIfNull(query);

        var key = query.TryGetValue(NavigationRoutes.TopicKeyParameter, out var value)
            ? value?.ToString()
            : null;

        this.TopicLabel.Text = $"Topic: {key}";
    }
}
