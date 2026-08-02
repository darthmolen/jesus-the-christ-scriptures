# Backlog: verify every bundled verse against the churchofjesuschrist.org source

**Date:** 2026-08-02
**Origin:** Owner request (2026-08-02 session), item #2 — "check all verses in the
application against the lds.org api."

## Idea

Programmatically confirm that every verse string we ship in
`src/JesusTheChrist.App/Resources/Raw/jesus-christ.{en,es}.json` matches the canonical text on
churchofjesuschrist.org, so we can catch any *genuinely* truncated or mistranscribed content in
the data. (This is separate from the on-screen truncation bug tracked in
[[verse-truncation-android-formattedtext]], which is a rendering defect, not a data defect.)

## Why it's a backlog item, not done inline

The app — and the web/sandbox dev environment — **cannot reach the internet**. This check has to
run as an **offline dev/CI tool** on a machine (or CI job) with outbound network access, not from
inside the app. It produces a report; it does not change app behavior.

## Approach

There is no official public JSON "API"; the **study site is the source of truth** and its URL
scheme is stable (same scheme we already document for deep-linking). Reuse the formula and
volume/book code tables in [docs/DEEP-LINKING-TO-LDS-WEBSITE.md](../../docs/DEEP-LINKING-TO-LDS-WEBSITE.md).

1. **Enumerate our verses.** Load both JSON files. For every `Reference`, walk its `context`
   array → `(vol, book, ch, vs, text)` tuples. This is the ground truth of what we ship.
2. **Build the source URL per chapter** with the existing formula:
   `…/study/scriptures/{volume}/{book}/{chapter}?lang={eng|spa}` — fetch **once per chapter**
   and cache, not once per verse (be polite: throttle + cache to disk).
3. **Extract canonical verse text.** The study pages expose verse-anchored spans
   (`id="p{verse}"`). Parse the fetched HTML, pull the text for each `p{verse}`, and normalize
   whitespace/typographic quotes/footnote markers on both sides before comparing.
4. **Diff** our stored `text` vs the canonical text per verse. Emit a report of mismatches
   (missing tail like "the Lord.", extra footnote letters, punctuation drift, wrong verse).
5. **Output** a `verse-verification-report.md` (or CSV): reference, language, our text, source
   text, diff. Non-zero exit on any mismatch so it can gate CI later.

## Scope / cautions

- **Fetch per chapter, cache aggressively**, throttle requests — this is a read-only politeness
  concern against a public site, run occasionally, not on every build.
- **Normalization is the hard part**: the study site carries footnote superscripts, small-caps
  "LORD", and typographic punctuation. Expect to iterate on the normalizer to avoid false
  positives. Start with a handful of known references (incl. 3 Nephi 20:41) as fixtures.
- **Spanish** (`lang=spa`) uses the Reina-Valera / church Spanish edition — verify the `es.json`
  against `lang=spa`, not a re-translation.
- Home for the tool: `tools/verify-verses/` (Python is fine, mirroring the reference impl in the
  deep-linking doc; or a small C# console project consistent with the repo).

## Definition of done

- A runnable tool that, given the two JSON files, fetches + diffs and reports mismatches.
- Report reviewed once by the owner; any real data defects filed/fixed.
- Optional stretch: wire as a scheduled/manual CI job (needs network egress) so drift is caught.

## Related

- [[verse-truncation-android-formattedtext]] — the *rendering* bug that prompted this; the data
  itself was found complete for the reported verse, so this check is about proving that holds
  everywhere.
- [[title-deeplink-to-lds]] — same URL formula / code tables; a `ScriptureUrlBuilder` port would
  give this tool its URLs for free (build one, both features use it).
