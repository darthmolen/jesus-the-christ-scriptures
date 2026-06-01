# Design Spec — Scriptures: Jesus The Christ (v1)

**Date:** 2026-05-31
**Status:** 🟡 Draft for owner review (brainstorm output; not yet implementation)
**Companion:** `planning/reading-app-plan_2026-05-31.md` (vision + stack + Material 3 notes)

---

## 1. Purpose

Let anyone read — without friction — the scriptures President Russell M. Nelson challenged the
Church to study: the ~2,200 Topical Guide references under "Jesus Christ" (Elder Andersen,
Oct 2020, *"We Talk of Christ"*). The printed TG forces you to flip to every reference by hand.
**This app brings the words to you.** That friction-removal is the core value.

## 2. Product & scope

- **One MAUI codebase, two Android flavors / store listings:**
  - **Bible Only** — references where `vol ∈ {ot, nt}`.
  - **Full** — all standard works.
  The split is pastoral/outreach (invite those wary of the LDS faith), not a code fork.
- **Android first** (iOS later, no rewrite). **EN + ES** content. **Offline-first, no backend, no accounts.**

## 3. Experience design (decided in brainstorm)

### 3.1 Navigation — Topic-first
- **Home = the 53 sub-topics**, each a row with a **progress ring**, under a **challenge header**.
  Home doubles as the tracker. This honors the TG's own structure and the "challenge" framing.
- Tapping a sub-topic opens its **reference feed directly** — no intermediate screen, no "go find
  the verse." (The TG's structure is the spine; book is a secondary re-slice — see 3.3.)

### 3.2 Reference feed — scrolling, verse-focused
- A vertical **scrolling feed of reference cards** (chosen over a one-at-a-time pager).
- **Card = demarcated block, reference title on top** (e.g. *"Mosíah 3:7"*), then the **full target
  verse(s) inline** — so people read immediately, no tap required to see scripture.
- **"Show context (±2)"** expander reveals the surrounding verses inline when a verse pulls you in.
- **Per-card actions:** a **read checkmark** and a **note** affordance.
- **Order:** references sorted in **canonical book + chapter:verse order** (OT→NT→BoM→D&C→PGP),
  matching the TG — **except the "Summary"** sub-topic, which keeps its **narrative life-of-Christ
  order** (foretold → born → … → resurrected → ascended).

### 3.3 Secondary re-slice (book / book+topic)
- Inside a topic feed, a **"⇄ by book"** control groups the same references by book.
- **Book + topic** is reachable as a filter once viewing a book. (No separate top-level "by book"
  destination in v1; Topic-first is the primary spine.)

### 3.4 The note gloss — conditional
- Show the `note` gloss only **when it adds information**: the short TG "highlight" phrase (EN) and
  the ~26 narrative labels (e.g. *"Se anuncia su nacimiento"*). **Suppress it when it equals the
  verse** — in ES, 1,416 notes were replaced with the official verse text, so showing them would
  duplicate the verse. Rule: render the gloss only if it is not a substring-match of the displayed
  target verse text.

### 3.5 Reading comfort ("Kindle-like")
- Adjustable **font family** and **size**; **light / dark** theme. Settings persist on device.
- Material 3 (`<UseMaterial3>true</UseMaterial3>`) themes the controls; reference-card chrome is
  custom-styled (Border/CollectionView are not auto-restyled in M3 — by design we want custom cards).

### 3.6 Notes
- A **note per reference** (free text), saved on device, editable/deletable. A card shows whether a
  note exists; tapping opens the editor. Notes are private; export/backup is **out of v1**.

### 3.7 Progress / the challenge
- **Headline (default): "X / 2,196 references read"** with an overall bar — echoes the prophet's
  challenge; advances with every verse.
- **Per-topic rings** on the home rows give milestone satisfaction.
- **Streaks: optional, off by default** — a settings toggle (devotional study shouldn't guilt-trip).
  When enabled, **a day counts if any reference is read that day** (no quota); track current + best streak.
- A reference's **read mark is per (sub-topic, reference)**: the same verse can appear under several
  sub-topics, and the challenge counts those ~2,196 entries (this matches "study all ~2,200").

## 4. Screens (information architecture)

1. **Home / Challenge** — header counter + bar; scrollable list of 53 sub-topics with rings.
2. **Topic feed** — the reference cards (3.2); top bar has the topic title, "⇄ by book", and a
   read/unread filter.
3. **Note editor** — modal/sheet for a reference's note.
4. **Settings** — font family/size, theme, language, streak toggle, about/attribution.
   (Navigation via MAUI **Shell**, which M3 themes.)

## 5. Data

### 5.1 Content (read-only, bundled)
- Ship the TG extract `jesus-christ.json` (EN + ES) from the `lds-nl-scriptures` pipeline as a
  **bundled asset** (`Resources/Raw/`). 53 sub-topics, 2,196 references; each reference carries
  `vol, book, ch, verses`, full **verse text + ±2 context**, a `note`, and its sub-topic.
- **Reference id:** `"{subtopicSlug}:{vol}/{book}/{ch}/{versesStart-versesEnd}"` — stable, unique
  per entry, used to key user data.
- **Bible Only flavor:** filter `vol ∈ {ot, nt}` at load (compile-time flavor constant).
- **Canonical sort key:** (volume order, book index, chapter, first verse); Summary bypasses sort.

### 5.2 User data (on device)
- **SQLite** local DB via **sqlite-net-pcl** (decided — lean, fast cold start; EF Core considered and
  rejected as over-complex for a 3-table, essentially-frozen schema).
- Tables: `ReadMark(refId, readAt)`, `Note(refId, text, updatedAt)`, plus `Setting(key, value)`
  for font/theme/language/streak and streak bookkeeping (last-read date, current/best streak).
- Progress is derived: `count(ReadMark) / totalReferences(flavor)`, and per sub-topic by prefix.

## 6. Architecture

- **.NET 10 MAUI**, **MVVM**, **Shell** navigation, **Material 3** enabled.
  `net10.0-android`, **Microsoft.Maui.Controls ≥ 10.0.60**.
- **Layers:** `Content` (load + parse bundled JSON, filter, sort) → `Data` (SQLite repo for
  read-marks/notes/settings) → `ViewModels` → `Views` (Shell pages, CollectionView feed).
- **Offline always:** no network calls in v1.
- Keep units small and testable: JSON loader, flavor filter, canonical sorter, gloss rule, and
  progress calculator are each pure/isolated and unit-tested.

## 7. Delivery (two flavors)

- One project, **two build flavors** differing by: application id, app name/icon, and a
  `Scope` constant (`BibleOnly` vs `Full`). MSBuild configurations / a flavor property drive the
  scope filter and branding. **Two separate store listings** (decided), per the outreach goal.

## 8. Localization

- Both EN and ES bundled. Default to **device locale**; **in-app language toggle** in Settings.
- UI strings via `.resx`; scripture text/titles/notes come from the chosen-language JSON.

## 9. Licensing & attribution  ⚠️ (owner action before public release)

- **English Bible text (KJV): public domain** — clean for the "Bible Only" outreach app.
- **The Spanish scripture text and the Topical Guide structure/notes are © Intellectual Reserve,
  Inc.** Distributing them publicly needs permission or must follow the Church's terms of use.
- **Decided path:** **build the app first** (personal use + a working demo to show), then **pursue
  permission from Intellectual Reserve before any public release.** If permission is declined, the
  app stays **personal-use** (unpublished). An attribution screen is built in regardless. Public
  release is gated on rights; development is not.

## 10. Out of scope (v1 — YAGNI)

Accounts, cloud sync, notes export/backup, iOS, audio, full-text search, study-plan scheduling,
social/sharing. Revisit after Android v1 ships.

## 11. Testing

- Unit tests (xUnit) for: JSON load/parse, Bible-Only filter, canonical sort (incl. Summary
  exception), gloss-suppression rule, reference-id generation, progress/streak math.
- Manual UI smoke pass on a device/emulator for the feed, notes, theme, and language toggle.

## 12. Decisions (resolved)

1. **Delivery:** two separate store listings / build flavors. ✓
2. **Storage:** `sqlite-net-pcl`. ✓
3. **Licensing:** build first (personal/demo) → pursue Intellectual Reserve permission before any
   public release → fallback is personal use (see §9). ✓
4. **Streak rule:** off by default; when on, any reference read that day counts (see §3.7). ✓

Remaining release-gate (not a code task): securing rights for the Spanish text + TG structure.

## 13. Next step

On approval: invoke **writing-plans** to produce the implementation plan (project scaffold,
data layer, screens, tests), tracked under `planning/` per this repo's protocol. **No code yet.**
