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
(to be filled at completion)
