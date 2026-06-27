#!/usr/bin/env python3
"""Rewrite the three cross-chapter "Summary" references with their full, faithful span.

The bundled Topical Guide JSON (vendored from the `lds-nl-scriptures` pipeline) captured each
multi-chapter reference as only its *start chapter*, mislabeled. This one-off tool pulls the true
verse text for the whole span from the same local corpus and rewrites the three entries in both
languages, tagging every verse with its chapter (`ch`) and recording the reference's `end_ch`.

It edits by (subtopic, index) — never by label — and preserves every other key (including the
Spanish note_en / note_source fields) in place, so the only diff is the three entries.

Run from the repo root:

    LDS_SCRIPTURES_DIR=../lds-nl-scriptures python tools/cross-chapter-fix/build_spanning_refs.py

Idempotent: re-running reproduces the same output.
"""
from __future__ import annotations

import json
import os
import sys
from pathlib import Path

REPO = Path(__file__).resolve().parents[2]
RAW = REPO / "src" / "JesusTheChrist.App" / "Resources" / "Raw"
CORPUS = Path(os.environ.get("LDS_SCRIPTURES_DIR", REPO.parent / "lds-nl-scriptures"))
SCRIPTURES = CORPUS / "content" / "processed" / "scriptures"

# Corpus book keys differ per language; the bundled file uses church codes for both.
BOOK_KEY = {
    ("matt", "en"): "matthew", ("matt", "es"): "matt",
    ("3-ne", "en"): "3nephi", ("3-ne", "es"): "3-ne",
}
VOLUME_FILE = {"newtestament": "newtestament.json", "bookofmormon": "bookofmormon.json"}


def full(ch_start, ch_end):
    """All chapters in [ch_start, ch_end], each marked 'all verses'."""
    return [(c, None) for c in range(ch_start, ch_end + 1)]


# Each target: the Summary/Resumen index, plus the new labels, span, and chapter/verse selection.
# `chapters` is a list of (chapter, verses-or-None); None means the whole chapter.
TARGETS = [
    {
        "index": 27,
        "ref_en": "Matt. 5–7", "ref_es": "Mateo 5–7",
        "ch": 5, "end_ch": 7,
        "chapters": full(5, 7),
    },
    {
        "index": 28,
        "ref_en": "Matt. 9:35–11:1", "ref_es": "Mateo 9:35–11:1",
        "ch": 9, "end_ch": 11,
        "chapters": [(9, [35, 36, 37, 38]), (10, None), (11, [1])],
    },
    {
        "index": 74,
        "ref_en": "3 Ne. 11–26", "ref_es": "3 Nefi 11–26",
        "ch": 11, "end_ch": 26,
        "chapters": full(11, 26),
    },
]

SUBTOPIC = {"en": "Summary", "es": "Resumen"}


def load_corpus(lang, volume):
    path = SCRIPTURES / lang / VOLUME_FILE[volume]
    return json.loads(path.read_text(encoding="utf-8"))["books"]


def verse_text(books, book_key, lang, chapter, verse):
    chapters = books[book_key]["chapters"]
    verses = chapters[str(chapter)]["verses"]
    if lang == "en":
        return verses[verse - 1]["text"]  # English verses are positional, 1-based.
    for v in verses:                       # Spanish verses carry an explicit number.
        if v["verse"] == verse:
            return v["text"]
    raise KeyError(f"{book_key} {chapter}:{verse} not found ({lang})")


def chapter_verse_numbers(books, book_key, chapter):
    return len(books[book_key]["chapters"][str(chapter)]["verses"])


def build_context(books, book_key, lang, chapters):
    """Build the full target context across the span, every verse tagged with its chapter."""
    context = []
    for chapter, verses in chapters:
        if verses is None:
            verses = range(1, chapter_verse_numbers(books, book_key, chapter) + 1)
        for n in verses:
            context.append({
                "vs": n,
                "text": verse_text(books, book_key, lang, chapter, n),
                "target": True,
                "ch": chapter,
            })
    return context


def rewrite_entry(entry, *, ref, end_ch, verses, context):
    """Return a new entry dict: same key order, with end_ch inserted after ch, verses/context/ref replaced."""
    out = {}
    for key, value in entry.items():
        if key == "ref":
            out["ref"] = ref
        elif key == "ch":
            out["ch"] = value
            out["end_ch"] = end_ch       # insert immediately after ch
        elif key == "verses":
            out["verses"] = verses
        elif key == "context":
            out["context"] = context
        else:
            out[key] = value
    return out


def verify_overlap(old_context, new_context, label, lang):
    """The old (start-chapter) context must match the new span verse-for-verse where they overlap."""
    start_ch = new_context[0]["ch"]
    new_by_vs = {c["vs"]: c["text"] for c in new_context if c["ch"] == start_ch}
    for c in old_context:
        if c["vs"] in new_by_vs and c["text"] != new_by_vs[c["vs"]]:
            raise AssertionError(
                f"[{lang}] {label} v{c['vs']}: corpus text differs from bundled text\n"
                f"  bundled: {c['text']!r}\n  corpus:  {new_by_vs[c['vs']]!r}")


def main():
    changed = 0
    for lang in ("en", "es"):
        bundled_path = RAW / f"jesus-christ.{lang}.json"
        data = json.loads(bundled_path.read_text(encoding="utf-8"))
        subtopic = next(s for s in data["subtopics"] if s["short"] == SUBTOPIC[lang])
        refs = subtopic["references"]

        corpora = {}
        for t in TARGETS:
            entry = refs[t["index"]]
            book = entry["book"]
            volume = entry["vol"]
            book_key = BOOK_KEY[(book, lang)]
            corpora.setdefault(volume, load_corpus(lang, volume))
            books = corpora[volume]

            context = build_context(books, book_key, lang, t["chapters"])
            verify_overlap(entry["context"], context, t["ref_en"], lang)

            start_ch = t["chapters"][0][0]
            start_verses = [c["vs"] for c in context if c["ch"] == start_ch]
            new_entry = rewrite_entry(
                entry,
                ref=t[f"ref_{lang}"],
                end_ch=t["end_ch"],
                verses=start_verses,
                context=context,
            )
            refs[t["index"]] = new_entry
            print(f"[{lang}] idx {t['index']}: {entry['ref']!r} -> {new_entry['ref']!r} "
                  f"({len(context)} verses across {start_ch}-{t['end_ch']})")
            changed += 1

        bundled_path.write_text(
            json.dumps(data, ensure_ascii=False, indent=2), encoding="utf-8")
        print(f"wrote {bundled_path.name}")

    print(f"done: {changed} entries rewritten")


if __name__ == "__main__":
    if not SCRIPTURES.exists():
        sys.exit(f"corpus not found at {SCRIPTURES}; set LDS_SCRIPTURES_DIR")
    main()
