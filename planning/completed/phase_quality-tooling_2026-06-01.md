# Phase: Code Quality Tooling + StyleCop + Central Package Management

**Date:** 2026-06-01
**Branch:** feature/quality-tooling (stacked on feature/plan-02-data / PR #5)
**Backlog:** planning/backlog/code-quality-and-cpm.md

## Objective

Bring the solution to the project's high-quality C# bar (StyleCop + Roslyn analyzers,
warnings-as-errors) and centralize NuGet versions (CPM), with **separate** package/build props
for `src/` and `tests/`. Then fix all resulting style/analyzer violations until `dotnet build`
(warnings-as-errors) and `dotnet test` are green.

## Source of standards

Copied/adapted from the reference project `/mnt/c/dev/other/ai-agents/server-agent`:
global.json, stylecop.json, .gitattributes, root .editorconfig, tests/.editorconfig,
src+tests `Directory.Build.props` + `Directory.Packages.props`, CODING-STANDARDS.md.
Per-project `StyleCop.Analyzers.Unstable` reference (PrivateAssets=all), version via CPM.

## Approach

1. Install tooling files; convert the 4 projects to CPM (drop inline `Version=`).
2. `dotnet build` → enumerate diagnostics (expect many: SA1101 this., SA1309 underscore fields,
   SA1402 one-type-per-file, CS1591/SA1600 XML docs, ordering, CA rules).
3. Fix systematically, src first (tests get a relaxed .editorconfig). Keep `dotnet test` green.
4. Iterate to a clean build. Per csharp-quality-developer: after ~3 manual passes on a stubborn
   set, surface it (VS quick-fixes are faster) rather than looping.

## Files expected to change

- New: global.json, stylecop.json, .gitattributes, .editorconfig, tests/.editorconfig,
  CODING-STANDARDS.md, src/ & tests/ `Directory.Build.props` + `Directory.Packages.props`,
  per-project GlobalSuppressions.cs as needed.
- Modified: all `*.csproj` (CPM + StyleCop ref); most `src/**/*.cs` (this./docs/field names/splits).

## Outcomes

- **Full solution builds clean under StyleCop + AnalysisMode=all + warnings-as-errors**, and all
  **47 tests pass** (Core 32, Data 15). Started at 446 src violations → 0.
- Tooling installed: global.json, stylecop.json, .gitattributes (CRLF for code), root + tests
  `.editorconfig`, CODING-STANDARDS.md, and **Central Package Management** with separate
  `Directory.Build.props` + `Directory.Packages.props` for `src/` and `tests/`. Each project
  references `StyleCop.Analyzers.Unstable` (version via CPM).
- src rewritten clean: one type per file, XML docs on public members, `this.` qualifiers,
  `string.Empty`, minimal usings (ImplicitUsings on), null-guards (CA1062).
- **Decisions:**
  - `ProgressService` / `StreakService` made **static** (CA1822 — pure, stateless).
  - `SubTopic.Short` → **`ShortTitle`** (CA1720 — `Short` is a type-name alias).
  - Test conventions live in `tests/.editorconfig` (per owner): **CA1707** off (underscore test
    names), CA1515 off (public test types), CA1861/CA1031 off, **CS1591** off (tests are
    self-documenting); StyleCop doc/ordering/layout categories already off for tests. Chose this
    over per-assembly `GlobalSuppressions.cs`.
- **Follow-up for the owner's next session:** bring real StyleCop/quality rules to the **MAUI App**
  project (created in VS on Windows) the same way — add the StyleCop ref + it inherits
  `src/Directory.Build.props`.
