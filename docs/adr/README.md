# Architecture Decision Records

An ADR records a decision that was **expensive to reach and will be expensive to reverse** —
usually one where the obvious answer was rejected for a reason that is not visible in the code.

Code comments explain what the code does. Phase documents in `planning/` explain what a piece of
work set out to do. Neither survives well as an answer to *"why is it built this way, and when
should we change it?"* six months later, when the constraint that forced the decision has been
forgotten and the workaround just looks like sloppiness.

## When to write one

Write an ADR when a decision:

- **constrains future work** — a later feature will hit the same wall;
- **rejected a more obvious option** for a non-obvious reason;
- **has a foreseeable expiry** — you can name the change in scope that would overturn it.

Do *not* write one for a routine implementation choice, a naming convention, or anything a phase
document already covers adequately.

## Format

One file per decision, numbered sequentially, never renumbered:

```
docs/adr/NNNN-short-slug.md
```

Each carries: **Status**, **Context** (the forces, including what was tried), **Decision**,
**Consequences** (good and bad, honestly), **Alternatives considered** (and why each lost), and
**Revisit when** — the concrete trigger that should reopen the question.

## Status values

| Status | Meaning |
|--------|---------|
| `Proposed` | Under discussion, not yet acted on |
| `Accepted` | In force; the code reflects it |
| `Superseded by NNNN` | Replaced — leave the original in place, do not edit its decision |
| `Deprecated` | No longer applies, with nothing replacing it |

**Never rewrite an accepted ADR's decision.** Supersede it with a new one and link both ways. The
value is the trail, not the tidiness.

## Index

| # | Title | Status |
|---|-------|--------|
| [0001](0001-chapter-navigation-inside-a-reference-card.md) | Chapter navigation inside a reference card | Accepted |
