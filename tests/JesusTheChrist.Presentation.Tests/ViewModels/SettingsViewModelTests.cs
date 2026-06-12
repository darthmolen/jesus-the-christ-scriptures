using JesusTheChrist.Core.Models;
using JesusTheChrist.Data;
using JesusTheChrist.Presentation.Appearance;
using JesusTheChrist.Presentation.Tests.Fakes;
using JesusTheChrist.Presentation.ViewModels;

namespace JesusTheChrist.Presentation.Tests.ViewModels;

public class SettingsViewModelTests
{
    [Fact]
    public async Task Load_AppliesPersistedThemeAndFontSize()
    {
        await using var harness = await Harness.CreateAsync();
        await harness.Settings.SetAsync(SettingKeys.Theme, "dark");
        await harness.Settings.SetIntAsync(SettingKeys.FontSize, 22);

        await harness.ViewModel.LoadAsync();

        Assert.Equal(ThemeOption.Dark, harness.ViewModel.Theme);
        Assert.Equal(22, harness.ViewModel.ReadingFontSize);
        Assert.Equal(ThemeOption.Dark, harness.Appearance.LastTheme);
        Assert.Equal(22, harness.Appearance.LastFontSize);
    }

    [Fact]
    public async Task Load_UsesDefaultsWhenUnset()
    {
        await using var harness = await Harness.CreateAsync();

        await harness.ViewModel.LoadAsync();

        Assert.Equal(ThemeOption.System, harness.ViewModel.Theme);
        Assert.Equal(SettingsViewModel.DefaultReadingFontSize, harness.ViewModel.ReadingFontSize);
        Assert.Equal(Language.En, harness.ViewModel.Language);
        Assert.False(harness.ViewModel.StreakEnabled);
    }

    [Fact]
    public async Task SetReadingFontSize_PersistsAndApplies()
    {
        await using var harness = await Harness.CreateAsync();
        await harness.ViewModel.LoadAsync();

        await harness.ViewModel.SetReadingFontSizeAsync(20);

        Assert.Equal(20, harness.ViewModel.ReadingFontSize);
        Assert.Equal(20, await harness.Settings.GetIntAsync(SettingKeys.FontSize, 0));
        Assert.Equal(20, harness.Appearance.LastFontSize);
    }

    [Fact]
    public async Task SetReadingFontSize_RoundsConsistently()
    {
        await using var harness = await Harness.CreateAsync();
        await harness.ViewModel.LoadAsync();

        await harness.ViewModel.SetReadingFontSizeAsync(20.6);

        // In-memory, persisted, and applied values all match the rounded integer.
        Assert.Equal(21, harness.ViewModel.ReadingFontSize);
        Assert.Equal(21, await harness.Settings.GetIntAsync(SettingKeys.FontSize, 0));
        Assert.Equal(21, harness.Appearance.LastFontSize);
    }

    [Fact]
    public async Task PreviewReadingFontSize_AppliesWithoutPersisting()
    {
        await using var harness = await Harness.CreateAsync();
        await harness.ViewModel.LoadAsync();

        harness.ViewModel.PreviewReadingFontSize(24);

        Assert.Equal(24, harness.ViewModel.ReadingFontSize);
        Assert.Equal(24, harness.Appearance.LastFontSize);

        // Not persisted: a fresh load still sees the default.
        Assert.Equal((int)SettingsViewModel.DefaultReadingFontSize, await harness.Settings.GetIntAsync(SettingKeys.FontSize, (int)SettingsViewModel.DefaultReadingFontSize));
    }

    [Fact]
    public async Task SetTheme_PersistsAndApplies()
    {
        await using var harness = await Harness.CreateAsync();
        await harness.ViewModel.LoadAsync();

        await harness.ViewModel.SetThemeAsync(ThemeOption.Light);

        Assert.Equal(ThemeOption.Light, harness.ViewModel.Theme);
        Assert.Equal("light", await harness.Settings.GetAsync(SettingKeys.Theme));
        Assert.Equal(ThemeOption.Light, harness.Appearance.LastTheme);
    }

    [Fact]
    public async Task SetLanguage_Persists()
    {
        await using var harness = await Harness.CreateAsync();
        await harness.ViewModel.LoadAsync();

        await harness.ViewModel.SetLanguageAsync(Language.Es);

        Assert.Equal(Language.Es, harness.ViewModel.Language);
        Assert.Equal("es", await harness.Settings.GetAsync(SettingKeys.Language));
    }

    [Fact]
    public async Task SetLanguage_MirrorsToStartupPreference()
    {
        await using var harness = await Harness.CreateAsync();
        await harness.ViewModel.LoadAsync();

        await harness.ViewModel.SetLanguageAsync(Language.Es);

        // The startup preference is what App reads to apply culture before the first page builds.
        Assert.Equal("es", harness.LanguagePreference.GetCode());
    }

    [Fact]
    public async Task Load_MirrorsSavedLanguageToStartupPreference()
    {
        await using var harness = await Harness.CreateAsync();
        await harness.Settings.SetAsync(SettingKeys.Language, "es");

        await harness.ViewModel.LoadAsync();

        Assert.Equal("es", harness.LanguagePreference.GetCode());
    }

    [Fact]
    public async Task SetStreakEnabled_Persists()
    {
        await using var harness = await Harness.CreateAsync();
        await harness.ViewModel.LoadAsync();

        await harness.ViewModel.SetStreakEnabledAsync(true);

        Assert.True(harness.ViewModel.StreakEnabled);
        Assert.True(await harness.Settings.GetBoolAsync(SettingKeys.StreakEnabled, false));
    }

    private sealed class Harness : IAsyncDisposable
    {
        private readonly TempDatabase database;

        private Harness(TempDatabase database, SettingsViewModel viewModel, SettingsStore settings, FakeAppearanceApplier appearance, FakeLanguagePreference languagePreference)
        {
            this.database = database;
            this.ViewModel = viewModel;
            this.Settings = settings;
            this.Appearance = appearance;
            this.LanguagePreference = languagePreference;
        }

        public SettingsViewModel ViewModel { get; }

        public SettingsStore Settings { get; }

        public FakeAppearanceApplier Appearance { get; }

        public FakeLanguagePreference LanguagePreference { get; }

        public static async Task<Harness> CreateAsync()
        {
            var db = await TempDatabase.CreateAsync();
            var settings = new SettingsStore(db.Db);
            var appearance = new FakeAppearanceApplier();
            var languagePreference = new FakeLanguagePreference();
            var vm = new SettingsViewModel(settings, db, appearance, languagePreference);
            return new Harness(db, vm, settings, appearance, languagePreference);
        }

        public async ValueTask DisposeAsync() => await this.database.DisposeAsync();
    }
}
