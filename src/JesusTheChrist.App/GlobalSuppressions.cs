using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage(
    "Naming",
    "CA1724:Type names should not match namespaces",
    Justification = "App is the conventional MAUI application class name; the clash with the Android.App namespace is unavoidable and harmless.",
    Scope = "type",
    Target = "~T:JesusTheChrist.App.App")]

[assembly: SuppressMessage(
    "Design",
    "CA1031:Do not catch general exception types",
    Justification = "Persisting the reading position is best-effort; a storage failure must never crash the UI thread during scroll or page teardown.",
    Scope = "member",
    Target = "~M:JesusTheChrist.App.Views.TopicFeedPage.FlushPositionAsync")]
