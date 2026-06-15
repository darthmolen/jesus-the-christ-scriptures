using JesusTheChrist.Presentation.Appearance;
using JesusTheChrist.Presentation.Globalization;
using JesusTheChrist.Presentation.Navigation;
using JesusTheChrist.Presentation.ViewModels;

namespace JesusTheChrist.App.Views;

/// <summary>
/// The Settings screen: reading font size, theme, language, streak toggle, and attribution.
/// </summary>
public partial class SettingsPage : ContentPage
{
    private readonly SettingsViewModel viewModel;
    private bool initializing;
    private bool switchingLanguage;

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsPage"/> class.
    /// </summary>
    /// <param name="viewModel">The settings view model.</param>
    public SettingsPage(SettingsViewModel viewModel)
    {
        this.InitializeComponent();
        this.viewModel = viewModel;
        this.BindingContext = viewModel;
    }

    /// <inheritdoc/>
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await this.viewModel.LoadAsync();

        this.initializing = true;
        this.FontSlider.Value = this.viewModel.ReadingFontSize;
        this.ThemePicker.SelectedIndex = (int)this.viewModel.Theme;
        this.LanguagePicker.SelectedIndex = LanguageCatalog.IndexOf(this.viewModel.Language);
        this.StreakSwitch.IsToggled = this.viewModel.StreakEnabled;
        this.initializing = false;

        this.FontSlider.ValueChanged += this.OnFontValueChanged;
        this.FontSlider.DragCompleted += this.OnFontDragCompleted;
        this.ThemePicker.SelectedIndexChanged += this.OnThemeChanged;
        this.LanguagePicker.SelectedIndexChanged += this.OnLanguageChanged;
        this.StreakSwitch.Toggled += this.OnStreakToggled;
    }

    /// <inheritdoc/>
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        this.FontSlider.ValueChanged -= this.OnFontValueChanged;
        this.FontSlider.DragCompleted -= this.OnFontDragCompleted;
        this.ThemePicker.SelectedIndexChanged -= this.OnThemeChanged;
        this.LanguagePicker.SelectedIndexChanged -= this.OnLanguageChanged;
        this.StreakSwitch.Toggled -= this.OnStreakToggled;
    }

    private void OnFontValueChanged(object? sender, ValueChangedEventArgs e)
    {
        if (!this.initializing)
        {
            // Live preview as the slider moves; persistence happens on release.
            this.viewModel.PreviewReadingFontSize(e.NewValue);
        }
    }

    private async void OnFontDragCompleted(object? sender, EventArgs e)
    {
        if (!this.initializing)
        {
            await this.viewModel.SetReadingFontSizeAsync(this.FontSlider.Value);
        }
    }

    private async void OnThemeChanged(object? sender, EventArgs e)
    {
        if (!this.initializing && this.ThemePicker.SelectedIndex >= 0)
        {
            await this.viewModel.SetThemeAsync((ThemeOption)this.ThemePicker.SelectedIndex);
        }
    }

    private async void OnLanguageChanged(object? sender, EventArgs e)
    {
        // Ignore the programmatic selection during load, an empty selection, and any change
        // that arrives while the persist+rebuild navigation below is still in flight — a
        // second Shell navigation mid-flight throws, and this async void would crash the app.
        if (this.initializing || this.switchingLanguage || this.LanguagePicker.SelectedIndex < 0)
        {
            return;
        }

        this.switchingLanguage = true;
        try
        {
            await this.viewModel.SetLanguageAsync(LanguageCatalog.At(this.LanguagePicker.SelectedIndex));

            // UI text is resolved when a page is built, so reload Settings to re-render it in the
            // newly selected language. Pop then re-push the route (no animation) so the user stays
            // on Settings; other pages are transient and already rebuild on their next navigation.
            await Shell.Current.GoToAsync("..", animate: false);
            await Shell.Current.GoToAsync(NavigationRoutes.Settings, animate: false);
        }
        finally
        {
            this.switchingLanguage = false;
        }
    }

    private async void OnStreakToggled(object? sender, ToggledEventArgs e)
    {
        if (!this.initializing)
        {
            await this.viewModel.SetStreakEnabledAsync(e.Value);
        }
    }

    private async void OnReadInvitation(object? sender, EventArgs e) =>
        await Shell.Current.GoToAsync(NavigationRoutes.Invitation);
}
