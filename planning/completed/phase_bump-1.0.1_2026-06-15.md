# Phase: Bump to 1.0.1 for Play release

**Date:** 2026-06-15
**Branch:** chore/bump-1.0.1

## Objective

Cut version 1.0.1 for upload to Google Play. The 1.0.1 build carries the merged
card inline-actions change (PR #32). Google Play orders/dedupes uploads by
`versionCode`, which must strictly increase; 1.0 went up as versionCode 1.

## Change

`src/JesusTheChrist.App/JesusTheChrist.App.csproj`:
- `ApplicationDisplayVersion` 1.0 → 1.0.1 (Android versionName, user-facing)
- `ApplicationVersion` 1 → 2 (Android versionCode, Play upload key)

## Release sequence

1. This PR merges to main.
2. Tag `v1.0.1` on the merge commit.
3. Build signed `.aab` locally (Release, keystore env vars set) and upload to the
   Play Console manually (CI auto-publish deferred — chosen "manual for now").

## Outcome

- (to fill after merge/tag/build)
