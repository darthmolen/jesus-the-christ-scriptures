# Backlog: "Show achievements" toggle — readers view vs. achievements view

**Date:** 2026-06-29
**Origin:** Owner request — let readers opt into a "pure content" view without progress noise.

## Context

The app always shows reading-progress indicators: per-topic progress **wheels** and an
overall **progress bar** on Home, "X / Y" read **counts**, and a **checkmark** on each
reference card (which doubles as the tap-to-mark-read button). Some readers want a clean
view of the scripture content without this gamification.

Add a single boolean setting, **Show achievements** (ON by default). When OFF, hide *all*
progress UI and remove the mark-read affordance. Read state is never lost — preserved in
the database, simply not shown; turning the setting back on restores every indicator.

### Decisions confirmed with owner
- **Hide scope = all progress UI**: per-topic wheels, the Home overall progress bar, the
  "X / Y" counts, and the card checkmarks.
- **Mark-read = remove the button entirely** in readers view (read state persists underneath).

## Approach

Follow the existing `StreakEnabled` setting end-to-end for persistence + Settings UI, then
gate each indicator's `IsVisible` on a `ShowAchievements` flag read at view-model load time.

**Why a view-model flag, not a DynamicResource:** both `HomeViewModel.LoadAsync` and
`TopicFeedViewModel.LoadAsync` already read `SettingsStore` and rebuild their child
view-models on every page appearance (`HomePage.OnAppearing` → `LoadCommand`). A flag read
at load time updates correctly when the user toggles the setting and navigates back — no
app-wide messaging needed. The heading checkmark must also combine with the existing
`IsCollapsed` condition, which a computed VM property handles cleanly and a
resource/`MultiBinding` would not.

## Changes

### 1. Persistence + setting key
- `SettingKeys.cs` — add `public const string ShowAchievements = "show_achievements";`
- Reuse `SettingsStore.GetBoolAsync` / `SetBoolAsync` — no store changes.

### 2. Settings UI (mirror the Streak toggle)
- `SettingsViewModel.cs` — add `[ObservableProperty] bool ShowAchievements`; load in
  `LoadAsync` with fallback **`true`**; add `SetShowAchievementsAsync(bool)` (copy
  `SetStreakEnabledAsync`).
- `SettingsPage.xaml` — add a `Grid`/`Switch` row named `AchievementsSwitch`, copying the
  `StreakSwitch` block.
- `SettingsPage.xaml.cs` — set `AchievementsSwitch.IsToggled` in `OnAppearing` inside the
  `initializing` guard, wire `Toggled`; unwire in `OnDisappearing`; handler calls
  `SetShowAchievementsAsync(e.Value)`.

### 3. Localization (app is fully localized — add to BOTH resx)
- `AppResources.resx` + `AppResources.es.resx` — add `SettingsShowAchievements` and
  `SettingsShowAchievementsHint`, mirroring `SettingsTrackStreak` / `SettingsStreakHint`.
  - en: "Show achievements" / "Show progress wheels, counts, and read checkmarks."
  - es: "Mostrar logros" / "Muestra los círculos de progreso, los recuentos y las marcas de lectura."

### 4. Home page — wheels, overall bar, counts
- `HomeViewModel.cs` — add `[ObservableProperty] bool ShowAchievements`; set in `LoadAsync`
  from `GetBoolAsync(SettingKeys.ShowAchievements, true)`; pass into each `TopicRowViewModel`.
- `TopicRowViewModel.cs` — add a `bool showAchievements` ctor param + `ShowAchievements`
  getter (immutable snapshot, consistent with the class).
- `HomePage.xaml`:
  - Header `ProgressBar` (line 27) → add `IsVisible="{Binding ShowAchievements}"`.
  - `ProgressRingView` (line 54) → add `IsVisible="{Binding ShowAchievements}"`.
  - "X / Y" count `Label` (Grid.Column 2, line 73) → add `IsVisible="{Binding ShowAchievements}"`.
  - **Layout fix:** change the row `Grid` `ColumnDefinitions="44,*,Auto"` (line 53) to
    `ColumnDefinitions="Auto,*,Auto"` so the wheel's column collapses to 0 when hidden
    (ring keeps `WidthRequest="44"`). Verify the leading `ColumnSpacing` leaves no awkward
    indent; adjust if it does.

### 5. Reference cards — checkmarks (and their tap-to-read button)
- `ReferenceCardViewModel.cs` — add a `bool showAchievements` ctor param/field; add:
  - `public bool ShowReadMark => this.showAchievements;` (footer)
  - `public bool ShowHeadingReadMark => this.IsCollapsed && this.showAchievements;` (heading)
  - add `[NotifyPropertyChangedFor(nameof(ShowHeadingReadMark))]` to the `IsExpanded`
    property (line 127) so it tracks collapse changes.
- `TopicFeedViewModel.cs` — in `LoadAsync` read
  `var showAchievements = await settings.GetBoolAsync(SettingKeys.ShowAchievements, true);`
  and pass into each `new ReferenceCardViewModel(...)` (line 136).
- `TopicFeedPage.xaml`:
  - Heading checkmark `Border` (line 85) — change `IsVisible="{Binding IsCollapsed}"` →
    `IsVisible="{Binding ShowHeadingReadMark}"`.
  - Footer checkmark `Border` (line 217) — add `IsVisible="{Binding ShowReadMark}"`.
  - Both live in `Auto` columns (`ColumnDefinitions="*,Auto,Auto"`), so they collapse
    cleanly when hidden — no layout change needed.

## Tests (extend existing suites, don't add files)
- `SettingsViewModelTests.cs` — defaults to `true` unset; load reflects stored value;
  `SetShowAchievementsAsync` persists.
- `HomeViewModelTests.cs` — after load, `ShowAchievements` matches the setting and
  propagates to each `TopicRowViewModel`.
- `TopicRowViewModelTests.cs` — `ShowAchievements` reflects the ctor arg.
- `TopicFeedViewModelTests.cs` — cards built with the flag; `ShowHeadingReadMark` is
  `false` when achievements off (even while collapsed), `true` when on + collapsed.

## Verification
1. `dotnet build` then `dotnet test` (CPM + warnings-as-errors — keep it clean).
2. Manual (physical phone via VS): with the setting **ON** (default), confirm wheels,
   bar, counts, and card checkmarks show and mark-read works.
3. Toggle **OFF**, back to Home → wheels/bar/counts gone, rows align with no orphaned
   indent; open a topic → no checkmarks and no tap-to-mark-read control.
4. Mark items read with the setting ON, toggle OFF then ON → read state intact (data
   preserved, only hidden).
5. Switch to Spanish → confirm the new Settings label/hint are localized.

## Notes
- This is a *view* concern. Read-marks and notes are untouched; toggling the setting never
  mutates progress data.
- Pure additive feature — no migration, no schema change (the key-value `Setting` table
  already exists).
</content>
