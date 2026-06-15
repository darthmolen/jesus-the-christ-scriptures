# How to develop

A practical guide to building, testing, and running **Scriptures: Jesus The Christ**.
For style rules see [CODING-STANDARDS.md](../CODING-STANDARDS.md); for shipping to the
Play Store see [android/HOW-TO-DEPLOY.md](android/HOW-TO-DEPLOY.md).

## Prerequisites

- **.NET SDK 10** — pinned in [global.json](../global.json) (`10.0.0`, `rollForward: latestFeature`).
- **.NET MAUI Android workload** — `dotnet workload install maui-android`.
- **Android SDK** (API 21+). The phone-side tools (`adb`) ship with it; on this machine
  `adb` lives at `C:\Program Files (x86)\Android\android-sdk\platform-tools\adb.exe`.
- A **physical Android phone** with USB debugging on. There is no emulator configured —
  device testing is done over USB (the simplest path is the **Run** button in Visual Studio).

> The app floats MAUI ahead of the installed workload baseline via `<MauiVersion>10.0.70</MauiVersion>`
> in [src/JesusTheChrist.App/JesusTheChrist.App.csproj](../src/JesusTheChrist.App/JesusTheChrist.App.csproj)
> to get the Material 3 control set. A NuGet restore pulls that version; no extra step needed.

## Solution layout

Four source projects plus their test projects:

| Project | TFM | Role |
|---|---|---|
| `JesusTheChrist.Core` | `net10.0` | Domain models, content loading (`ContentService`), JSON, scope filtering. No UI. |
| `JesusTheChrist.Data` | `net10.0` | SQLite stores (read marks, notes, settings, streak). |
| `JesusTheChrist.Presentation` | `net10.0` | View models, navigation seams, globalization, `.resx` string tables. No platform code. |
| `JesusTheChrist.App` | `net10.0-android` | MAUI host: XAML pages, DI wiring, platform services. |

The deliberate split keeps almost all logic in plain `net10.0` libraries that run on the
desktop test host — only `JesusTheChrist.App` is Android-only. **Put logic in
Core/Data/Presentation so it can be unit-tested; keep `App` thin** (XAML + glue).

## Build

```bash
# Whole solution
dotnet build

# Just the libraries (fast; no Android workload needed)
dotnet build src/JesusTheChrist.Presentation/JesusTheChrist.Presentation.csproj
```

The build is **strict** — see [src/Directory.Build.props](../src/Directory.Build.props):
`TreatWarningsAsErrors`, `CodeAnalysisTreatWarningsAsErrors`, `AnalysisMode=all`,
`EnforceCodeStyleInBuild`, `GenerateDocumentationFile`, and StyleCop via `stylecop.json`.
A warning fails the build, so a clean build is the real bar. Packages are centrally
versioned (`ManagePackageVersionsCentrally`) in `Directory.Packages.props` — add package
versions there, not in the `.csproj`.

## Test

```bash
# All tests (run on the desktop host, not the phone)
dotnet test

# One project
dotnet test tests/JesusTheChrist.Presentation.Tests/JesusTheChrist.Presentation.Tests.csproj
```

xUnit across `Core.Tests`, `Data.Tests`, and `Presentation.Tests`. The MAUI `App`
project has no unit tests by design — anything worth testing lives a layer down. Data
tests spin up a real temporary SQLite file (`TempDatabase`), so they exercise actual
queries.

## Run on a device

With the phone plugged in and USB debugging on:

```bash
# Deploy + launch the Debug build on the connected device
dotnet build src/JesusTheChrist.App/JesusTheChrist.App.csproj -t:Run -f net10.0-android
```

Visual Studio's **Run** button does the same and is the smoother loop (hot reload, the
debugger, device picker). If a Play-installed copy is already on the phone, uninstall it
first — a locally-signed Debug build and the Play-signed build share a package id but have
different signatures, so Android refuses to install one over the other.

## Before you commit

1. `dotnet build` clean (warnings are errors).
2. `dotnet test` green.
3. C# files are **CRLF**, end with a single newline, no trailing whitespace, 4-space indent
   (`.sh` files are LF). See [CODING-STANDARDS.md](../CODING-STANDARDS.md).
4. **Branch + PR — never commit to `main`.** `main` is protected; do work on a
   `feature/<name>` (or `docs/<name>`, `fix/<name>`) branch and open a PR. Only the repo
   owner merges. See the Branch & Merge Policy in [CLAUDE.md](../CLAUDE.md).
