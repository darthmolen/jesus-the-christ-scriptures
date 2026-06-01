# Design mockups — Jesus The Christ reading app

The option screens used during the **superpowers:brainstorming** session (2026-05-31) that
produced `planning/design-spec_2026-05-31.md`. Each was shown in the visual companion to choose
between A/B/C; the **chosen** option is noted below. Kept as the visual record of the design
rationale.

> These are visual-companion *content fragments* (no `<html>` wrapper) — they relied on the
> companion frame's CSS for the `.options`/`.letter` chrome, so opening them standalone renders
> the inline-styled phone mockups but not that outer framing. They are reference, not the app UI.

| File | Question | Chosen |
| --- | --- | --- |
| `reading-layout.html` | Scrolling feed vs. one-at-a-time pager | **A — scrolling card feed** |
| `card-density.html` | How much of each reference shows in the feed | **B — verse-focused, context on tap** |
| `navigation-ia.html` | Slice & dice: topic-first / axis tabs / filter chips | **A — topic-first** |
| `home-progress.html` | Headline progress metric | **A — "X / 2,196 read"** + optional streak toggle |

The live tooling dir (`.superpowers/`) stays gitignored — only these design artifacts are tracked.
