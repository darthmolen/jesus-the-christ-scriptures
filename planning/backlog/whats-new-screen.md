# "What's New" — a curated, version-gated changelog screen

> Backlog item (captured 2026-06-29). Approach decided; not yet started. When picked up, move to
> `planning/in_progress/` per the planning protocol.

## Context

When we publish a release we have no way to tell users in-app what changed — they only see Google
Play's release notes, which our first-launch and Spanish users may never read. We want a **curated
"What's New" screen** that:

1. **Shows once after an update** — the first time the app runs on a newer version, surface what
   changed, then never again until the next version.
2. **Is reachable from Settings** any time, as a rolling history of recent versions.
3. Stays **simple**: a hand-curated, bundled, localized markdown file (newest version on top) — no
   remote fetch, no per-version data plumbing. (Chosen over remote-hosted / structured-JSON
   alternatives — see below.)

This is a near-clone of the existing **first-run Invitation** feature, which already does "load a
bundled localized markdown via `MarkdownView`, show once gated by a `SettingsStore` flag, and expose
it from Settings." We reuse that pattern wholesale and gate on **app version** instead of a bool.

### Alternatives considered (for the record)
- **Content source:** *Bundled markdown* (chosen) vs *remote-hosted on the existing GitHub Pages*
  (editable without an app release, but adds network + fallback + a hosting spot) vs *native Play
  "What's new" only* (zero in-app work, but no first-launch reveal and no Spanish control).
- **Format:** *Curated markdown rolling list* (chosen — reuses `MarkdownView` 1:1) vs *structured
  per-version JSON* (could highlight only new entries, but needs a list renderer + diffing).

## Key existing pieces to reuse

- **Content + render:** [InvitationViewModel.cs](../../src/JesusTheChrist.Presentation/ViewModels/InvitationViewModel.cs)
  loads `invitation.{lang}.md` through `IAssetSource` (EN fallback) and
  [InvitationPage.xaml](../../src/JesusTheChrist.App/Views/InvitationPage.xaml) renders it with the
  `controls:MarkdownView`. Clone both.
- **Show-once hook:** [HomePage.xaml.cs](../../src/JesusTheChrist.App/Views/HomePage.xaml.cs#L38-L46)
  `OnAppearing` already calls `IsInvitationUnseenAsync()` → `OpenInvitationCommand`. Extend it.
- **Persistence:** `SettingsStore.GetAsync/SetAsync` (string) +
  [SettingKeys.cs](../../src/JesusTheChrist.Data/SettingKeys.cs); the invitation records its flag in
  `AcknowledgeAsync`.
- **Settings entry point:** [SettingsPage.xaml.cs](../../src/JesusTheChrist.App/Views/SettingsPage.xaml.cs#L119)
  `OnReadInvitation` → `Shell.Current.GoToAsync(NavigationRoutes.Invitation)`; mirror it.
- **Routes/DI:** `NavigationRoutes`, `AppShell.xaml.cs` route registration, `MauiProgram.cs`
  transient page+VM registration.

## Approach

### 1. App-version abstraction (Presentation has no MAUI reference)
`Presentation` references only Core/Data/MVVM, so `AppInfo` isn't available there — mirror the
`ILanguagePreference` pattern:
- New `IAppVersion { string Current { get; } }` in `Presentation/Globalization/`.
- `MauiAppVersion : IAppVersion => AppInfo.Current.VersionString` in `App/Services/`.
- Register `services.AddSingleton<IAppVersion, MauiAppVersion>()` in
  [MauiProgram.cs](../../src/JesusTheChrist.App/MauiProgram.cs).

### 2. Content
- `Resources/Raw/whats-new.en.md` and `whats-new.es.md`, curated, **newest version first** (one
  `##`-section per version with date + bullet highlights). Seed with 1.0.5 (cross-chapter passages)
  and 1.0.4 (resume position + verse numbers in notes).
- **Per-release curation step** (document in the phase doc): before tagging a release, prepend the
  new version's section to both files. The show-once logic keys off the app version automatically.

### 3. Screen (clone of Invitation)
- `WhatsNewViewModel` (Presentation): same shape as `InvitationViewModel` — load
  `whats-new.{lang}.md` (EN fallback) into `BodyMarkdown`; a `CloseCommand` that records the seen
  version and navigates back. Inject `IAssetSource`, `SettingsStore`, `IDatabaseInitializer`,
  `INavigationService`, `AppEnvironment`, **`IAppVersion`**.
  - `CloseAsync`: `await settings.SetAsync(SettingKeys.WhatsNewLastSeenVersion, appVersion.Current)`
    then `navigation.GoBackAsync()`.
- `WhatsNewPage.xaml`/`.cs` (App): clone `InvitationPage` — `ScrollView` + `controls:MarkdownView`
  bound to `BodyMarkdown` + a "Done" button bound to `CloseCommand`. Title = `AppResources.WhatsNewTitle`.

### 4. Version-gated show-once (HomeViewModel)
Add to [HomeViewModel.cs](../../src/JesusTheChrist.Presentation/ViewModels/HomeViewModel.cs) (inject `IAppVersion`):
- `IsWhatsNewUnseenAsync()`:
  ```
  lastSeen = await settings.GetAsync(WhatsNewLastSeenVersion)
  if (lastSeen == appVersion.Current) return false        // already seen this version
  return await settings.GetBoolAsync(InvitationSeen, false) // only for users past onboarding
  ```
- `SeedWhatsNewBaselineAsync()`: if `WhatsNewLastSeenVersion` is unset, set it to the current
  version. Called when we show the Invitation, so a brand-new user is **not** shown What's New for
  the version they installed on next launch.
- `OpenWhatsNewCommand` → `navigation.GoToAsync(NavigationRoutes.WhatsNew)`.

Extend `HomePage.OnAppearing`'s first-run block:
```
if (await vm.IsInvitationUnseenAsync()) {
    await vm.SeedWhatsNewBaselineAsync();          // baseline new users at current version
    await vm.OpenInvitationCommand.ExecuteAsync(null);
} else if (await vm.IsWhatsNewUnseenAsync()) {
    await vm.OpenWhatsNewCommand.ExecuteAsync(null);
}
```
This covers all cases: fresh install → Invitation only (baselined, so no What's New for the install
version); existing user upgrading into the feature (`lastSeen` null, invitation already seen) → sees
What's New; thereafter only on a genuine version change.

### 5. Settings entry point + version label
In [SettingsPage.xaml](../../src/JesusTheChrist.App/Views/SettingsPage.xaml) About section, add a
"What's New" link button (mirror the Invitation button) → `OnWhatsNew` →
`GoToAsync(NavigationRoutes.WhatsNew)`. Also show a small `Version X.Y.Z` label (the app shows its
version nowhere today) via `IAppVersion` exposed on `SettingsViewModel`.

### 6. Wiring
- `SettingKeys.WhatsNewLastSeenVersion = "whatsnew_last_version"`.
- `NavigationRoutes.WhatsNew = "whatsnew"`; register in `AppShell.xaml.cs`.
- `MauiProgram`: `AddTransient<WhatsNewViewModel>()` + `AddTransient<WhatsNewPage>()`;
  `AddSingleton<IAppVersion, MauiAppVersion>()`.
- Localized strings (both `AppResources.resx` + `AppResources.es.resx` **and** the hand-maintained
  `AppResources.Designer.cs`): `WhatsNewTitle`, `SettingsWhatsNew`, `WhatsNewClose`,
  `SettingsVersionFormat` ("Version {0}").

## Files to create / change
- New: `Presentation/Globalization/IAppVersion.cs`, `App/Services/MauiAppVersion.cs`,
  `Presentation/ViewModels/WhatsNewViewModel.cs`, `App/Views/WhatsNewPage.xaml(.cs)`,
  `App/Resources/Raw/whats-new.{en,es}.md`.
- Edit: `SettingKeys.cs`, `NavigationRoutes.cs`, `AppShell.xaml.cs`, `MauiProgram.cs`,
  `HomeViewModel.cs`, `HomePage.xaml.cs`, `SettingsViewModel.cs`, `SettingsPage.xaml(.cs)`,
  `AppResources.resx` + `.es.resx` + `.Designer.cs`.

## Verification
- **Unit tests (Presentation):**
  - `WhatsNewViewModel`: loads `whats-new.{lang}.md` for the chosen language and falls back to EN
    when the localized asset is missing; `CloseCommand` writes the current version to
    `WhatsNewLastSeenVersion` and navigates back. (Reuse `FakeAssetSource`; add a fake `IAppVersion`.)
  - `HomeViewModel`: `IsWhatsNewUnseenAsync` truth table — equal version → false; `lastSeen` null +
    invitation seen → true; `lastSeen` null + invitation unseen → false; older version + invitation
    seen → true. `SeedWhatsNewBaselineAsync` seeds only when unset (doesn't overwrite).
- **Build:** full solution clean under warnings-as-errors; App XAML compiles.
- **Manual on device (no emulator):** fresh install shows the Invitation only; simulate an upgrade
  (clear `whatsnew_last_version`, or run a higher version) → What's New appears once, then not again;
  the Settings "What's New" button opens it any time; the version label reads `1.0.5`; toggle to
  Spanish renders `whats-new.es.md`.
