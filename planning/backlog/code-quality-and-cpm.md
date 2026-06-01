# Backlog: StyleCop + quality rules + Central Package Management

**Date:** 2026-05-31
**Origin:** owner note during Plan 02 execution

## Scope

Bring the solution up to the project's code-quality bar and centralize NuGet versions.

### StyleCop + quality rules
- Add StyleCop analyzers + the repo's quality ruleset (per `csharp-quality-developer` standards:
  SA rules, CRLF, `this.` prefix, no underscore fields, XML docs, LoggerMessage, brace placement).
- Wire an `.editorconfig` / ruleset and `Directory.Build.props` so rules apply solution-wide.
- Expect churn across the existing `Core` + `Data` source to satisfy the analyzers.

### Central Package Management (CPM)
- Add `Directory.Packages.props` with `<ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>`.
- **Two groupings:** one set of versions for **src** projects, another for **test** projects
  (owner's plan — likely via a shared props for src + a separate props/section for tests, or
  conditioned `ItemGroup`s). Migrate the current explicit `PackageReference` versions:
  - src: `sqlite-net-pcl`, `SQLitePCLRaw.bundle_e_sqlite3`
  - tests: `xunit`, `xunit.runner.visualstudio`, `Microsoft.NET.Test.Sdk`, `coverlet.collector`
- After migration, `PackageReference` entries drop their `Version=` attributes.

## Notes
- Today's projects use plain per-project `PackageReference` with explicit versions — straightforward
  to move to CPM (just lift versions into `Directory.Packages.props`).
- Do this as its own phase/PR before the layer count grows (App + future libs).
