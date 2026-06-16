# Backlog: reference title links to the verse on churchofjesuschrist.org

**Date:** 2026-06-16
**Origin:** Owner request (2026-06-16 session)

## Idea

Make the reference label on a card a **tappable link** that opens the verse on
[churchofjesuschrist.org](https://www.churchofjesuschrist.org/). On **Android**, an
`https://www.churchofjesuschrist.org/...` link opens the **Gospel Library app** (via Android
App Links) when it's installed — which is a genuinely nice escape hatch for a reader who wants
to deep-dive the footnotes or keep their own notes over there. Falls back to the browser
otherwise. (Already observed: the invitation-page links in `invitation.en.md` behave this way.)

## Work required

- **Port the URL builder to C#** from [docs/DEEP-LINKING-TO-LDS-WEBSITE.md](../../docs/DEEP-LINKING-TO-LDS-WEBSITE.md)
  (that doc has the URL formula + volume/book code tables and a **Python** reference
  implementation). Add a `ScriptureUrlBuilder` to Core. URL shape:
  `…/study/scriptures/{volume}/{book}/{chapter}?lang={eng|spa}&id=p{verse}#p{verse}`.
- **Reuse our existing reference fields** rather than the Python's friendly-name map: the
  content already carries `vol`, `book`, `ch`, and `verses` (e.g. `vol":"newtestament"`,
  `"book":"heb"`, `"ch":7`, `"verses":[25]`). So the port is mainly:
  - a **`vol` → volume-code** map (`newtestament→nt`, `oldtestament→ot`, `bookofmormon→bofm`,
    `doctrineandcovenants→dc-testament`, `pearlofgreatprice→pgp`);
  - **verify our `book` values already equal the church book codes** in the doc (`heb`,
    `1-jn`, `1-ne`, `w-of-m`, `moro`, …). If any differ, add a small override map. **D&C is
    section-based** (`dc-testament/dc/{section}`) — handle that special case.
  - **verses → `id`/anchor:** single `p25`, range `p7-p8`, list `p7,p9,p11`; anchor `#p{first}`.
  - **language:** map `Language.En→eng`, `Language.Es→spa`.
- **Open the link:** `Launcher.Default.OpenAsync(uri)` from the card (a `OpenReferenceCommand`),
  wired to a tap on the `RefLabel` in `TopicFeedPage.xaml`. Give the label a link affordance
  (color/underline) so it reads as tappable.
- **Tests:** unit-test `ScriptureUrlBuilder` against the worked examples in the doc (John 3:16,
  Alma 32:21 spa, D&C 121:7–8 range, Moses 1:39, etc.).

## Notes

- Link **construction only** — no API call, no scraping; the study-site URL scheme is stable.
- Companion to [[copy-verse-button]] (both add per-reference actions).
- The book-code parity check is the main unknown; do it first, since it decides whether the
  port is a pure `vol` map or also needs a `book` override table.
