using JesusTheChrist.App.Views;
using JesusTheChrist.Presentation.Navigation;

namespace JesusTheChrist.App;

/// <summary>
/// The application Shell hosting the navigation hierarchy.
/// </summary>
public partial class AppShell : Shell
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AppShell"/> class.
    /// </summary>
    public AppShell()
    {
        this.InitializeComponent();
        Routing.RegisterRoute(NavigationRoutes.Topic, typeof(TopicFeedPage));
    }
}
