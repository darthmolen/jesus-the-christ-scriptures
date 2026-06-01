# Windows / Visual Studio session — kickoff prompt

Use this to start a fresh Claude Code session on **Windows in Visual Studio** to create the MAUI
App project and continue with Plan 03. **Before starting:** ensure the latest `main` is pulled
(it includes Core + Data + the StyleCop/CPM tooling).

Paste the block below as the first message:

```text
We're continuing "Scriptures: Jesus The Christ" — a .NET 10 MAUI Android reading app —
now on Windows in Visual Studio. Read CLAUDE.md, CODING-STANDARDS.md, and
planning/design-spec_2026-05-31.md first. Use the csharp-quality-developer skill for all
C# work, and superpowers:brainstorming / writing-plans for design/plan work.

WHAT EXISTS ON `main` (pull latest):
- src/JesusTheChrist.Core — content domain (models, JSON loader, ScopeFilter, ContentService).
- src/JesusTheChrist.Data — sqlite-net-pcl stores (read-marks, notes, settings) + ProgressService
  (static) + StreakService (static) + StreakStore. 47 tests pass (tests/*.Tests).
- Quality tooling: StyleCop + AnalysisMode=all + TreatWarningsAsErrors + Central Package
  Management. Separate Directory.Build.props + Directory.Packages.props in src/ and tests/.
  Every project references StyleCop.Analyzers.Unstable (version via CPM). global.json, stylecop.json,
  .editorconfig (root + tests/), .gitattributes (CRLF for code).
- Content already vendored: src/JesusTheChrist.App/Resources/Raw/jesus-christ.{en,es}.json
  (53 sub-topics / 2,196 references each; verse text + ±2 context + note).

STACK (locked): .NET 10 MAUI + Material 3 — <UseMaterial3>true</UseMaterial3>, target
net10.0-android, Microsoft.Maui.Controls >= 10.0.60. Offline-first, no backend, no accounts.
Bible Only flavor = filter references where vol in {oldtestament, newtestament}.

IMMEDIATE TASK — create the MAUI App project (this is why we moved to Windows; the Linux box
had no MAUI workload):
1. Create src/JesusTheChrist.App as a .NET MAUI App; add it to JesusTheChrist.slnx; reference
   JesusTheChrist.Core and JesusTheChrist.Data.
2. Reconcile its .csproj with the existing tooling — EXPECT these gotchas:
   - src/Directory.Build.props sets <TargetFramework>net10.0</TargetFramework>; the App must
     OVERRIDE to <TargetFramework>net10.0-android</TargetFramework> (+ UseMaui, SingleProject,
     UseMaterial3, ApplicationId org.jesusthechrist.full, ApplicationDisplayName).
   - Central Package Management is ON: move every MAUI package VERSION into
     src/Directory.Packages.props as <PackageVersion .../> (add Microsoft.Maui.Controls 10.0.60
     etc.); the .csproj uses versionless <PackageReference>. CentralPackageTransitivePinningEnabled
     is true, so you may need to pin a few transitive MAUI packages.
   - Add the StyleCop.Analyzers.Unstable <PackageReference> (PrivateAssets=all) to the App, like
     the other projects.
   - The MAUI template + generated code (App.xaml.cs, MauiProgram.cs, Platforms/**) will trip
     StyleCop/analyzers. Clean them to standard where reasonable; for generated/platform code use
     a per-project GlobalSuppressions.cs or scoped .editorconfig — do NOT weaken the global rules.
3. Goal: `dotnet build` clean under warnings-as-errors and the app launches to a stub page.

THEN: brainstorm/execute Plan 03 — App shell + Home/Challenge tracker (Shell nav; the 53
sub-topics as a list with per-topic progress rings; header "X / 2,196 references read";
load content via an IAssetSource backed by MAUI FileSystem.OpenAppPackageFileAsync; wire the
Data layer for read-marks/progress). Topic-first IA, verse-focused feed comes in Plan 04.
Reference order is the TG's THEMATIC order — preserve it (canonical-sort toggle is backlogged).

POLICY: nothing goes directly to main — feature branch -> PR -> owner (darthmolen) merges; the
agent stops at "PR opened." main is protected.
```

## Quick reference (paths)

- Standards: `CLAUDE.md`, `CODING-STANDARDS.md`, `csharp-quality-developer` skill.
- Design: `planning/design-spec_2026-05-31.md`; plans/phases under `planning/`.
- Tooling: `src/` and `tests/` `Directory.Build.props` + `Directory.Packages.props`,
  `stylecop.json`, `.editorconfig`.
- Backlog: `planning/backlog/` (canonical-sort toggle; code-quality/CPM notes).
