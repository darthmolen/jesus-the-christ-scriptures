using JesusTheChrist.Core.Models;
using JesusTheChrist.Presentation.Appearance;
using JesusTheChrist.Presentation.ViewModels;

namespace JesusTheChrist.App.Views;

/// <summary>
/// The Settings screen: reading font size, theme, language, streak toggle, and attribution.
/// </summary>
public partial class SettingsPage : ContentPage
{
    private readonly SettingsViewModel viewModel;
    private bool initializing;

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
        this.LanguagePicker.SelectedIndex = this.viewModel.Language == Language.Es ? 1 : 0;
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
        if (!this.initializing)
        {
            await this.viewModel.SetLanguageAsync(this.LanguagePicker.SelectedIndex == 1 ? Language.Es : Language.En);
        }
    }

    private async void OnStreakToggled(object? sender, ToggledEventArgs e)
    {
        if (!this.initializing)
        {
            await this.viewModel.SetStreakEnabledAsync(e.Value);
        }
    }
}
