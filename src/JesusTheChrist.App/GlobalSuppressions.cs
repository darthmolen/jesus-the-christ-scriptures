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

[assembly: SuppressMessage(
    "Design",
    "CA1031:Do not catch general exception types",
    Justification = "Copying a held verse is best-effort; a clipboard failure on device must never crash the reader's page, and the call is unawaited so an escaping exception would go unobserved.",
    Scope = "member",
    Target = "~M:JesusTheChrist.App.Views.TopicFeedPage.CopyVerseAsync(JesusTheChrist.Presentation.ViewModels.ReferenceCardViewModel,JesusTheChrist.Presentation.ViewModels.ContextLineViewModel)")]

[assembly: SuppressMessage(
    "Design",
    "CA1031:Do not catch general exception types",
    Justification = "Opening a study link is best-effort; the platform launcher can fail in ways we cannot enumerate (no handler, no activity, a malformed intent), and a reader who taps the link is better served by nothing happening than by a crash or an error they cannot act on.",
    Scope = "member",
    Target = "~M:JesusTheChrist.App.Services.MauiLinkOpener.OpenAsync(System.Uri)")]
