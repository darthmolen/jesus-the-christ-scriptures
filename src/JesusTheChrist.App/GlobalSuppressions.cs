using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage(
    "Naming",
    "CA1724:Type names should not match namespaces",
    Justification = "App is the conventional MAUI application class name; the clash with the Android.App namespace is unavoidable and harmless.",
    Scope = "type",
    Target = "~T:JesusTheChrist.App.App")]
