# Scriptures: Jesus The Christ — Plan

**Date:** 2026-05-31
**Status:** 🌱 Rough plan — actively brainstorming (do NOT implement yet)
**Repo:** `darthmolen/jesus-the-christ-scriptures`

## Why

Come Follow Me, May 25–31 2026 ("The Lord Raised Up a Deliverer," Judges 2–4; 6–8; 13–16)
points to Elder Neil L. Andersen's Oct 2020 talk *"We Talk of Christ,"* which recounts
President Russell M. Nelson's invitation (that talk's footnote 8 for the source) to study
**all ~2,200 Topical Guide references under "Jesus Christ."** President Nelson said he took the
challenge himself and **it changed him** — an apostle of 50+ years, changed by it. (He has
since passed away.)

The invitation was originally given in President Nelson's 2017 young-adult devotional — see
`planning/the-invitation.md` for the quote and source links:
- [President Russell M. Nelson, *"Prophets, Leadership, and Divine Law"* (Jan. 8, 2017)](https://www.churchofjesuschrist.org/study/broadcasts/worldwide-devotional-for-young-adults-an-evening-with-president-nelson/2017/01/prophets-leadership-and-divine-law?lang=eng)
- [Neil L. Andersen, *"We Talk of Christ"* (Oct. 2020)](https://www.churchofjesuschrist.org/study/general-conference/2020/10/45andersen?lang=eng)

Goal: let **anyone** read the words of and about Christ in that curated set and have their lives
changed — and lower the barrier for Christians who may be wary of the LDS faith.

## Product: one codebase, two scopes

- **Scriptures: Jesus The Christ (Bible Only)** — references whose verses are in the Old/New Testament.
- **Scriptures: Jesus The Christ (Full)** — all standard works.

The split is pastoral/outreach, not technical: **Bible Only is a filter** (`vol ∈ {oldtestament, newtestament}`) on
the same data. Ship as **one app, two flavors / store listings**. EN + ES content. **Android first**, iOS later.

## Tech stack (decided)

- **.NET 10 + Visual Studio + .NET MAUI** — one C# codebase, Android now, iOS later with no rewrite.
- **Material 3 (Material You)** via the built-in opt-in:
  ```xml
  <PropertyGroup>
    <UseMaterial3>true</UseMaterial3>
  </PropertyGroup>
  ```
  Requires `net10.0-android` and **Microsoft.Maui.Controls ≥ 10.0.60** (full control set).
  Refs: [.NET Blog](https://devblogs.microsoft.com/dotnet/dotnet-maui-material-3/),
  [MS Learn](https://learn.microsoft.com/en-us/dotnet/maui/user-interface/material-design?view=net-maui-10.0).
- **Offline-first, no backend, no accounts.** All reading data bundled; user data stays on-device.

### Material 3 specifics that shape the design
- **Android-only** (iOS/Mac/Windows keep native styling) — fine, we're Android-first.
- M3 themes these for free: **CheckBox** (read-tracking), **ProgressBar** (challenge progress),
  **Editor/Entry** (notes), **Switch** (dark mode), **Slider** (font size), **Shell** (navigation),
  Button, SearchBar, Pickers.
- **Caveats:** it's an **app-wide switch** (no per-control toggle), there's **no .NET API for M3
  color roles** yet (theme via `AppThemeBinding` + Android theme), and **CollectionView and
  Border/Frame are NOT auto-restyled** in this release — so our **reference cards** (Border +
  CollectionView) get M3 *colors* but we style the card chrome ourselves (we want that anyway).
  Explicit styles always win.

## Content (already built — input artifact)

From the `lds-nl-scriptures` pipeline (now in its `main`):

- `content/processed/scriptures/en/topical-guide/jesus-christ.json`
- `content/processed/scriptures/es/topical-guide/jesus-christ.json` (Spanish, verified)

Each reference carries: `vol` (the volume id —
`oldtestament|newtestament|bookofmormon|doctrineandcovenants|pearlofgreatprice`), `book` (the
church short code, e.g. `matt`, `john`, `1-ne`, `dc`), `ch`, `verses`, full
**verse text + ±2 verse context**, a `note` gloss, and sub-topic grouping (53 sub-topics,
2,196 references). → Ship the JSON as a **bundled asset** (`Resources/Raw/`); Bible Only =
filter `vol ∈ {oldtestament, newtestament}` at load.

## Features (v1 — keep it simple, "Kindle-like")

| Feature | Notes | MAUI/M3 |
|---|---|---|
| Reading comfort | adjustable font family + size; light/dark | Slider, Switch, `AppThemeBinding` |
| Reference cards | own demarcated block, **reference title on top**, verse text + ±2 context, target verses emphasized | Border + CollectionView (custom-styled) |
| Slice & dice | all references, or by **book**, by **topic**, or **book + topic** | Shell nav / flyout |
| Per-reference note | write + save personal thoughts | Editor (M3) |
| Per-reference checkmark | mark read/studied | CheckBox (M3) |
| Progress tracker | progress toward the full set ("the challenge") | ProgressBar (M3) + summary view |

## On-device data (user content)

Notes + checkmarks + progress stored locally (no login). Storage tech is an open question
(SQLite via EF Core / sqlite-net, vs. files/Preferences). Keyed by stable reference id
(`vol/book/ch/verses`). Export/backup is a *later* nicety.

## Out of scope for v1 (YAGNI)

Accounts, cloud sync, social features, notes export/backup, iOS, audio, full-text search,
study-plan scheduling. Revisit after Android v1.

## Open questions (for brainstorm)

1. Reference **id scheme** + the notes/checkmark/progress data model (and storage tech).
2. Information architecture: how slice-and-dice (book / topic / book+topic) maps to Shell, and
   how a single reference is reached from each path.
3. **Reading screen** layout: single scrolling list of cards vs. one-reference-at-a-time pager;
   how ±2 context and the `note` gloss are shown; how target verses are emphasized.
4. **Progress model**: per-reference %, per-sub-topic, or total — and what the tracker screen shows.
5. **Bible Only vs Full** delivery: build flavors / product flavors vs. a single app + toggle.
6. **Licensing/attribution** per translation (KJV public domain; Spanish text + TG structure are
   Church-copyright) — resolve before public distribution, esp. the outreach "Bible Only" app.
7. EN/ES language handling (device locale vs in-app switch; bundling both).
8. Branding/naming, app icons, store presence for the two flavors.

## Brainstorm kickoff prompt (paste to resume in a fresh session)

> Resume the design brainstorm for the **"Scriptures: Jesus The Christ"** .NET MAUI Android app.
> Read `planning/reading-app-plan_2026-05-31.md` for full context. Stack is locked: **.NET 10 +
> MAUI + Material 3 (`<UseMaterial3>true</UseMaterial3>`, net10.0-android, Controls ≥10.0.60),
> Android-first, offline, no backend**; content is the bundled `jesus-christ.json` Topical Guide
> extract (EN+ES; Bible Only = `vol ∈ {oldtestament,newtestament}`). Use the **superpowers:brainstorming** skill and
> the **visual companion** (browser mockups). Work through the Open Questions one at a time,
> starting with the **reading-screen layout** (cards list vs. pager) and the **information
> architecture** for slice-and-dice. Produce a design spec in `planning/`, then hand to
> writing-plans. Do not implement yet.

## Next step

Continue the **superpowers:brainstorming** session with the visual companion (mockups persist in
`.superpowers/brainstorm/`). First topics: reading-screen layout and slice-and-dice IA.
