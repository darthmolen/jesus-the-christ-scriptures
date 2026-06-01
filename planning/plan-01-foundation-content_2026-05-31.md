# Plan 01 — Foundation & Content Layer — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Stand up the solution and a fully unit-tested, UI-free **content domain library** that loads the bundled Topical Guide JSON, models it, filters by scope (Bible Only vs Full), and exposes computed reference id / target text / gloss rule.

**Architecture:** A pure `.NET 10` class library (`JesusTheChrist.Core`) with no MAUI dependencies holds models + services, so it runs under plain `dotnet test`. The MAUI app (`JesusTheChrist.App`, `net10.0-android`, Material 3) references Core and bundles the JSON as a raw asset; it is scaffolded here but its screens are later plans. An xUnit project (`JesusTheChrist.Core.Tests`) drives everything TDD.

**Tech Stack:** C# / .NET 10, .NET MAUI (Material 3 via `<UseMaterial3>true</UseMaterial3>`), System.Text.Json, xUnit.

---

## Plan series (this is Plan 01 of 5)

1. **Foundation & Content Layer** ← this plan (Core models + JSON load + scope filter; fully tested)
2. **Local data layer** — sqlite-net-pcl: read-marks, notes, settings, progress (tested)
3. **App shell + Home/Challenge tracker** — Shell nav, topic list with rings, real data on screen
4. **Topic feed** — verse-focused cards, context expander, canonical order, gloss rule, read/note actions
5. **Settings, notes editor, progress detail, two-flavor packaging & polish**

Each plan is its own feature branch → PR. Implement them in order.

## Decisions baked in (from `planning/design-spec_2026-05-31.md`)

- `Bible` volumes = `oldtestament`, `newtestament`. Scope filter drops out-of-scope references and any sub-topic left empty.
- References are **already in canonical TG order** in the JSON (OT→NT→BoM→D&C→PGP, then chapter:verse), and the **Summary** sub-topic is in narrative order. The loader **preserves JSON order** — no re-sort (re-sorting would corrupt Summary). Task 8 adds an invariant test on the volume blocks only (book/chapter order within a volume is trusted from source, since Core does not model per-book canonical index in this plan).
- **Reference id** = `"{subtopicKey}:{vol}/{book}/{ch}/{vStart}-{vEnd}"` (vStart==vEnd → just the number). **`subtopicKey` is a language-invariant slug** of the English short — `Slug.Make(short_en ?? short)`, e.g. `atonement-through`. The `short` field is localized (`Advocate`/`Abogado`, `Summary`/`Resumen`), so basing the id on it would break read-marks/notes across EN/ES. ES JSON carries `short_en` (English); EN JSON's `short` is already English (no `short_en`), so `short_en ?? short` is the English short in both.
- **Gloss shown** only when a `note` exists and is **not** contained in the target verse text (normalized). EN notes are short TG highlights; most ES notes equal the verse and are suppressed.

## Conventions (this repo — see `CLAUDE.md`)

- **Branch/PR:** all work on `feature/implementation-plan` (already created) → PR into `main`; the owner merges. Never push to `main`.
- **Planning protocol:** Task 0 creates a `planning/in_progress/phase_*.md` doc; Task 9 moves it to `planning/completed/`. Phase-boundary commits use the `[PHASE]` message format with a `Phase:` footer. Per-task TDD commits stay small and descriptive (frequent commits are intentional).

## Prerequisites

- **.NET 10 SDK** installed (`dotnet --version` → `10.*`).
- **MAUI Android workload** (`dotnet workload install maui-android`) — installs the MAUI templates too; `dotnet new maui` fails without it.
- **Source content:** the Topical Guide extract produced by the `lds-nl-scriptures` pipeline. Its location is configurable via `LDS_SCRIPTURES_DIR` (default `$HOME/dev/lds-nl-scriptures`). Task 1 **vendors** the two JSON files into `src/JesusTheChrist.App/Resources/Raw/` (committed), so every later step — and CI — depends only on this repo, not the sibling checkout.

## File structure

```
JesusTheChrist.sln
src/JesusTheChrist.Core/
  JesusTheChrist.Core.csproj
  Models/Scope.cs            # Scope enum
  Models/Language.cs         # Language enum + Code()
  Models/Volume.cs           # Volume enum + parse + order + IsBible
  Models/Slug.cs             # language-invariant slug helper
  Models/ContextVerse.cs     # record
  Models/Reference.cs        # record + Id, TargetText, ShowGloss
  Models/SubTopic.cs         # record (+ language-invariant Key)
  Models/TopicalGuide.cs     # record
  Json/Dtos.cs               # System.Text.Json DTOs (wire shape, incl. short_en)
  Json/TopicalGuideLoader.cs # Stream -> TopicalGuide (preserves order, computes Key)
  Content/ScopeFilter.cs     # filter a TopicalGuide by Scope
  Content/ContentService.cs  # load + filter orchestration (Language-typed)
src/JesusTheChrist.App/      # MAUI app (scaffold only this plan)
  JesusTheChrist.App.csproj  # net10.0-android, UseMaterial3, refs Core
  Resources/Raw/jesus-christ.en.json
  Resources/Raw/jesus-christ.es.json
tests/JesusTheChrist.Core.Tests/
  JesusTheChrist.Core.Tests.csproj
  Fixtures/sample.json       # tiny deterministic fixture
  VolumeTests.cs
  LanguageTests.cs
  SlugTests.cs
  ReferenceIdTests.cs
  ReferenceGlossTests.cs
  TopicalGuideLoaderTests.cs
  ScopeFilterTests.cs
  ContentServiceTests.cs
  RealDataSmokeTests.cs
```

---

### Task 0: Phase document (CLAUDE.md protocol)

**Files:**
- Create: `planning/in_progress/phase_plan-01-foundation_2026-05-31.md`

- [ ] **Step 1: Create the phase doc**

Create `planning/in_progress/phase_plan-01-foundation_2026-05-31.md`:

```markdown
# Phase: Plan 01 — Foundation & Content Layer

**Date:** 2026-05-31
**Plan:** planning/plan-01-foundation-content_2026-05-31.md

## Objective
Scaffold the solution and build the MAUI-free content domain library (models, JSON loader,
scope filter, content service) with full xUnit coverage.

## Approach
TDD task-by-task per the plan; pure Core library so tests run without MAUI/Android.

## Success criteria
- Solution builds; `dotnet test` green (fixtures + real-data smoke).
- Language-invariant reference ids; Bible-Only filter; order preserved.

## Files expected to change
- src/JesusTheChrist.Core/** , src/JesusTheChrist.App/** (scaffold), tests/JesusTheChrist.Core.Tests/**

## Outcomes
(to be filled at completion)
```

- [ ] **Step 2: Commit**

```bash
git add planning/in_progress/phase_plan-01-foundation_2026-05-31.md
git commit -m "[PHASE] Start Plan 01: foundation & content layer

- Create phase doc for the content-layer scaffold + domain library.

Phase: planning/in_progress/phase_plan-01-foundation_2026-05-31.md"
```

---

### Task 1: Scaffold solution, projects, and assets

**Files:**
- Create: `JesusTheChrist.sln`, the three `.csproj` files, `tests/.../Fixtures/sample.json`
- Vendor: the two content JSON files into `src/JesusTheChrist.App/Resources/Raw/`

- [ ] **Step 0: Preflight — verify the toolchain**

Run: `dotnet --version`  → expect `10.*`. If not, install the .NET 10 SDK first.
Run: `dotnet workload install maui-android`  → installs the MAUI Android workload + templates (idempotent).

- [ ] **Step 1: Create solution and projects**

Run (from repo root):

```bash
dotnet new sln -n JesusTheChrist
dotnet new classlib -n JesusTheChrist.Core -o src/JesusTheChrist.Core -f net10.0
dotnet new xunit  -n JesusTheChrist.Core.Tests -o tests/JesusTheChrist.Core.Tests -f net10.0
dotnet new maui   -n JesusTheChrist.App -o src/JesusTheChrist.App
rm src/JesusTheChrist.Core/Class1.cs tests/JesusTheChrist.Core.Tests/UnitTest1.cs
dotnet sln add src/JesusTheChrist.Core src/JesusTheChrist.App tests/JesusTheChrist.Core.Tests
dotnet add tests/JesusTheChrist.Core.Tests reference src/JesusTheChrist.Core
dotnet add src/JesusTheChrist.App reference src/JesusTheChrist.Core
```

- [ ] **Step 2: Pin the MAUI app to net10.0-android + Material 3**

Edit `src/JesusTheChrist.App/JesusTheChrist.App.csproj` so the first `<PropertyGroup>` contains exactly these (replace the default multi-TFM line):

```xml
<TargetFrameworks>net10.0-android</TargetFrameworks>
<UseMaterial3>true</UseMaterial3>
<OutputType>Exe</OutputType>
<UseMaui>true</UseMaui>
<SingleProject>true</SingleProject>
<ApplicationId>org.jesusthechrist.full</ApplicationId>
<ApplicationDisplayName>Scriptures: Jesus The Christ</ApplicationDisplayName>
```

Then ensure the controls package is current enough for Material 3:

```bash
dotnet add src/JesusTheChrist.App package Microsoft.Maui.Controls --version 10.0.60
```

- [ ] **Step 3: Vendor the content JSON as raw assets (with a guard)**

```bash
SRC="${LDS_SCRIPTURES_DIR:-$HOME/dev/lds-nl-scriptures}/content/processed/scriptures"
test -f "$SRC/en/topical-guide/jesus-christ.json" || { echo "ERROR: source JSON not found under $SRC — set LDS_SCRIPTURES_DIR to your lds-nl-scriptures checkout."; exit 1; }
mkdir -p src/JesusTheChrist.App/Resources/Raw
cp "$SRC/en/topical-guide/jesus-christ.json" src/JesusTheChrist.App/Resources/Raw/jesus-christ.en.json
cp "$SRC/es/topical-guide/jesus-christ.json" src/JesusTheChrist.App/Resources/Raw/jesus-christ.es.json
```

(MAUI bundles everything under `Resources/Raw/` automatically; the app reads them via `FileSystem.OpenAppPackageFileAsync`. The files are committed, so the sibling repo is needed only this once.)

- [ ] **Step 4: Add a tiny deterministic test fixture**

Create `tests/JesusTheChrist.Core.Tests/Fixtures/sample.json`:

```json
{
  "topic": "Jesus Christ",
  "language": "en",
  "context_radius": 2,
  "subtopics": [
    {
      "title": "Jesus Christ",
      "short": "Summary",
      "references": [
        { "ref": "Luke 1:26-38", "vol": "newtestament", "book": "luke", "book_title": "Luke",
          "ch": 1, "verses": [26],
          "context": [ { "vs": 26, "text": "And in the sixth month the angel Gabriel was sent", "target": true } ],
          "note": "His birth is foretold" }
      ]
    },
    {
      "title": "Jesus Christ, Advocate",
      "short": "Advocate",
      "references": [
        { "ref": "Heb. 7:25", "vol": "newtestament", "book": "heb", "book_title": "Hebrews",
          "ch": 7, "verses": [25],
          "context": [ { "vs": 25, "text": "he ever liveth to make intercession for them", "target": true } ],
          "note": "he ever liveth to make intercession" },
        { "ref": "Moro. 7:28", "vol": "bookofmormon", "book": "moro", "book_title": "Moroni",
          "ch": 7, "verses": [28],
          "context": [ { "vs": 28, "text": "he hath ... pleadeth the cause of the children of men", "target": true } ],
          "note": "he advocateth the cause of the children of men" }
      ]
    }
  ]
}
```

Mark it copy-to-output by adding to `tests/JesusTheChrist.Core.Tests/JesusTheChrist.Core.Tests.csproj` inside `<Project>`:

```xml
<ItemGroup>
  <None Update="Fixtures/sample.json"><CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory></None>
</ItemGroup>
```

- [ ] **Step 5: Verify it builds**

Run: `dotnet build JesusTheChrist.sln`
Expected: build succeeds.

- [ ] **Step 6: Commit**

```bash
git add -A
git commit -m "chore: scaffold solution, Core/App/Tests projects, vendor content JSON"
```

---

### Task 2: Scope, Language, and Volume models

**Files:**
- Create: `src/JesusTheChrist.Core/Models/Scope.cs`, `Language.cs`, `Volume.cs`
- Test: `tests/JesusTheChrist.Core.Tests/VolumeTests.cs`, `LanguageTests.cs`

- [ ] **Step 1: Write the failing tests**

Create `tests/JesusTheChrist.Core.Tests/VolumeTests.cs`:

```csharp
using JesusTheChrist.Core.Models;
using Xunit;

public class VolumeTests
{
    [Theory]
    [InlineData("oldtestament", Volume.OldTestament)]
    [InlineData("newtestament", Volume.NewTestament)]
    [InlineData("bookofmormon", Volume.BookOfMormon)]
    [InlineData("doctrineandcovenants", Volume.DoctrineAndCovenants)]
    [InlineData("pearlofgreatprice", Volume.PearlOfGreatPrice)]
    public void Parse_maps_known_vol_strings(string raw, Volume expected)
        => Assert.Equal(expected, VolumeExtensions.Parse(raw));

    [Fact]
    public void Parse_unknown_throws()
        => Assert.Throws<System.ArgumentException>(() => VolumeExtensions.Parse("nope"));

    [Theory]
    [InlineData(Volume.OldTestament, true)]
    [InlineData(Volume.NewTestament, true)]
    [InlineData(Volume.BookOfMormon, false)]
    public void IsBible_is_true_only_for_ot_and_nt(Volume v, bool expected)
        => Assert.Equal(expected, v.IsBible());

    [Fact]
    public void Order_is_canonical()
        => Assert.True(Volume.OldTestament.Order() < Volume.NewTestament.Order()
                    && Volume.NewTestament.Order() < Volume.BookOfMormon.Order());
}
```

Create `tests/JesusTheChrist.Core.Tests/LanguageTests.cs`:

```csharp
using JesusTheChrist.Core.Models;
using Xunit;

public class LanguageTests
{
    [Theory]
    [InlineData(Language.En, "en")]
    [InlineData(Language.Es, "es")]
    public void Code_maps_language(Language l, string expected)
        => Assert.Equal(expected, l.Code());
}
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test --filter "VolumeTests|LanguageTests"`
Expected: FAIL — types do not exist.

- [ ] **Step 3: Write minimal implementation**

Create `src/JesusTheChrist.Core/Models/Scope.cs`:

```csharp
namespace JesusTheChrist.Core.Models;

public enum Scope { BibleOnly, Full }
```

Create `src/JesusTheChrist.Core/Models/Language.cs`:

```csharp
using System;

namespace JesusTheChrist.Core.Models;

public enum Language { En, Es }

public static class LanguageExtensions
{
    public static string Code(this Language l) => l switch
    {
        Language.En => "en",
        Language.Es => "es",
        _ => throw new ArgumentOutOfRangeException(nameof(l))
    };
}
```

Create `src/JesusTheChrist.Core/Models/Volume.cs`:

```csharp
using System;

namespace JesusTheChrist.Core.Models;

public enum Volume { OldTestament, NewTestament, BookOfMormon, DoctrineAndCovenants, PearlOfGreatPrice }

public static class VolumeExtensions
{
    public static Volume Parse(string raw) => raw switch
    {
        "oldtestament" => Volume.OldTestament,
        "newtestament" => Volume.NewTestament,
        "bookofmormon" => Volume.BookOfMormon,
        "doctrineandcovenants" => Volume.DoctrineAndCovenants,
        "pearlofgreatprice" => Volume.PearlOfGreatPrice,
        _ => throw new ArgumentException($"Unknown volume '{raw}'", nameof(raw))
    };

    public static bool IsBible(this Volume v) => v is Volume.OldTestament or Volume.NewTestament;

    public static int Order(this Volume v) => (int)v; // enum declaration order is canonical
}
```

- [ ] **Step 4: Run tests to verify they pass**

Run: `dotnet test --filter "VolumeTests|LanguageTests"`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add src/JesusTheChrist.Core/Models/Scope.cs src/JesusTheChrist.Core/Models/Language.cs src/JesusTheChrist.Core/Models/Volume.cs tests/JesusTheChrist.Core.Tests/VolumeTests.cs tests/JesusTheChrist.Core.Tests/LanguageTests.cs
git commit -m "feat(core): Scope, Language, Volume models"
```

---

### Task 3: Slug, domain records + Reference.Id

**Files:**
- Create: `src/JesusTheChrist.Core/Models/Slug.cs`, `ContextVerse.cs`, `Reference.cs`, `SubTopic.cs`, `TopicalGuide.cs`
- Test: `tests/JesusTheChrist.Core.Tests/SlugTests.cs`, `ReferenceIdTests.cs`

- [ ] **Step 1: Write the failing tests**

Create `tests/JesusTheChrist.Core.Tests/SlugTests.cs`:

```csharp
using JesusTheChrist.Core.Models;
using Xunit;

public class SlugTests
{
    [Theory]
    [InlineData("Advocate", "advocate")]
    [InlineData("Atonement through", "atonement-through")]
    [InlineData("Types of, in Anticipation", "types-of-in-anticipation")]
    [InlineData("Summary", "summary")]
    public void Make_kebab_cases(string input, string expected)
        => Assert.Equal(expected, Slug.Make(input));
}
```

Create `tests/JesusTheChrist.Core.Tests/ReferenceIdTests.cs`:

```csharp
using System.Collections.Generic;
using JesusTheChrist.Core.Models;
using Xunit;

public class ReferenceIdTests
{
    static Reference Ref(int[] verses) => new(
        RefLabel: "X", Vol: Volume.NewTestament, Book: "john", BookTitle: "John",
        Ch: 3, Verses: verses, Context: new List<ContextVerse>(), Note: null);

    [Fact]
    public void Id_single_verse_uses_language_invariant_key()
        => Assert.Equal("advocate:newtestament/john/3/16", Ref(new[] { 16 }).Id("advocate"));

    [Fact]
    public void Id_verse_range_uses_first_and_last()
        => Assert.Equal("summary:newtestament/john/3/16-18", Ref(new[] { 16, 17, 18 }).Id("summary"));
}
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test --filter "SlugTests|ReferenceIdTests"`
Expected: FAIL — types not defined.

- [ ] **Step 3: Write minimal implementation**

Create `src/JesusTheChrist.Core/Models/Slug.cs`:

```csharp
using System.Text;

namespace JesusTheChrist.Core.Models;

public static class Slug
{
    public static string Make(string text)
    {
        var sb = new StringBuilder(text.Length);
        var lastDash = false;
        foreach (var ch in text.ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(ch)) { sb.Append(ch); lastDash = false; }
            else if (sb.Length > 0 && !lastDash) { sb.Append('-'); lastDash = true; }
        }
        return sb.ToString().Trim('-');
    }
}
```

Create `src/JesusTheChrist.Core/Models/ContextVerse.cs`:

```csharp
namespace JesusTheChrist.Core.Models;

public record ContextVerse(int Vs, string Text, bool Target);
```

Create `src/JesusTheChrist.Core/Models/Reference.cs`:

```csharp
using System.Collections.Generic;
using System.Linq;

namespace JesusTheChrist.Core.Models;

public record Reference(
    string RefLabel,
    Volume Vol,
    string Book,
    string BookTitle,
    int Ch,
    IReadOnlyList<int> Verses,
    IReadOnlyList<ContextVerse> Context,
    string? Note)
{
    // subtopicKey is the language-invariant slug from SubTopic.Key.
    public string Id(string subtopicKey)
    {
        var lo = Verses.Count > 0 ? Verses[0] : 0;
        var hi = Verses.Count > 0 ? Verses[^1] : 0;
        var span = lo == hi ? $"{lo}" : $"{lo}-{hi}";
        return $"{subtopicKey}:{Vol.ToString().ToLowerInvariant()}/{Book}/{Ch}/{span}";
    }
}
```

Create `src/JesusTheChrist.Core/Models/SubTopic.cs`:

```csharp
using System.Collections.Generic;

namespace JesusTheChrist.Core.Models;

// Key is the language-invariant slug (set by the loader from short_en ?? short).
public record SubTopic(string Key, string Title, string Short, IReadOnlyList<Reference> References);
```

Create `src/JesusTheChrist.Core/Models/TopicalGuide.cs`:

```csharp
using System.Collections.Generic;

namespace JesusTheChrist.Core.Models;

public record TopicalGuide(string Topic, string Language, IReadOnlyList<SubTopic> SubTopics);
```

- [ ] **Step 4: Run tests to verify they pass**

Run: `dotnet test --filter "SlugTests|ReferenceIdTests"`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add src/JesusTheChrist.Core/Models tests/JesusTheChrist.Core.Tests/SlugTests.cs tests/JesusTheChrist.Core.Tests/ReferenceIdTests.cs
git commit -m "feat(core): Slug, domain records, language-invariant Reference.Id"
```

---

### Task 4: Reference.TargetText and ShowGloss (gloss rule)

**Files:**
- Modify: `src/JesusTheChrist.Core/Models/Reference.cs`
- Test: `tests/JesusTheChrist.Core.Tests/ReferenceGlossTests.cs`

- [ ] **Step 1: Write the failing test**

Create `tests/JesusTheChrist.Core.Tests/ReferenceGlossTests.cs`:

```csharp
using System.Collections.Generic;
using System.Linq;
using JesusTheChrist.Core.Models;
using Xunit;

public class ReferenceGlossTests
{
    static Reference Ref(string? note, params (int vs, string text, bool target)[] ctx) => new(
        "X", Volume.NewTestament, "john", "John", 3,
        new[] { 16 },
        ctx.Select(c => new ContextVerse(c.vs, c.text, c.target)).ToList(),
        note);

    [Fact]
    public void TargetText_joins_only_target_verses()
    {
        var r = Ref(null, (15, "before", false), (16, "For God so loved", true), (17, "after", false));
        Assert.Equal("For God so loved", r.TargetText);
    }

    [Fact]
    public void ShowGloss_false_when_note_null()
        => Assert.False(Ref(null, (16, "x", true)).ShowGloss);

    [Fact]
    public void ShowGloss_false_when_note_contained_in_verse()
        => Assert.False(Ref("God so loved", (16, "For God so loved the world", true)).ShowGloss);

    [Fact]
    public void ShowGloss_true_when_note_adds_information()
        => Assert.True(Ref("His birth is foretold", (16, "For God so loved the world", true)).ShowGloss);
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test --filter ReferenceGlossTests`
Expected: FAIL — `TargetText` / `ShowGloss` not defined.

- [ ] **Step 3: Write minimal implementation**

Add these members inside the `Reference` record body in `src/JesusTheChrist.Core/Models/Reference.cs` (after the `Id` method):

```csharp
    public string TargetText =>
        string.Join(" ", Context.Where(c => c.Target).Select(c => c.Text));

    public bool ShowGloss =>
        !string.IsNullOrWhiteSpace(Note) && !Normalize(TargetText).Contains(Normalize(Note!));

    private static string Normalize(string s)
    {
        var sb = new System.Text.StringBuilder(s.Length);
        foreach (var ch in s.ToLowerInvariant())
            if (char.IsLetterOrDigit(ch)) sb.Append(ch);
            else if (char.IsWhiteSpace(ch)) sb.Append(' ');
        return string.Join(' ', sb.ToString().Split(' ', System.StringSplitOptions.RemoveEmptyEntries));
    }
```

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test --filter ReferenceGlossTests`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add src/JesusTheChrist.Core/Models/Reference.cs tests/JesusTheChrist.Core.Tests/ReferenceGlossTests.cs
git commit -m "feat(core): TargetText and conditional gloss rule"
```

---

### Task 5: JSON DTOs + TopicalGuideLoader

**Files:**
- Create: `src/JesusTheChrist.Core/Json/Dtos.cs`, `src/JesusTheChrist.Core/Json/TopicalGuideLoader.cs`
- Test: `tests/JesusTheChrist.Core.Tests/TopicalGuideLoaderTests.cs`

- [ ] **Step 1: Write the failing test**

Create `tests/JesusTheChrist.Core.Tests/TopicalGuideLoaderTests.cs`:

```csharp
using System.IO;
using System.Linq;
using System.Text;
using JesusTheChrist.Core.Json;
using JesusTheChrist.Core.Models;
using Xunit;

public class TopicalGuideLoaderTests
{
    static TopicalGuide Load()
    {
        using var s = File.OpenRead(Path.Combine("Fixtures", "sample.json"));
        return TopicalGuideLoader.Load(s);
    }

    [Fact]
    public void Loads_subtopics_in_order_with_invariant_keys()
    {
        var g = Load();
        Assert.Equal("Jesus Christ", g.Topic);
        Assert.Equal(new[] { "Summary", "Advocate" }, g.SubTopics.Select(t => t.Short).ToArray());
        Assert.Equal(new[] { "summary", "advocate" }, g.SubTopics.Select(t => t.Key).ToArray());
    }

    [Fact]
    public void Maps_reference_fields()
    {
        var r = Load().SubTopics[1].References[0];
        Assert.Equal("Heb. 7:25", r.RefLabel);
        Assert.Equal(Volume.NewTestament, r.Vol);
        Assert.Equal("heb", r.Book);
        Assert.Equal(7, r.Ch);
        Assert.Equal(new[] { 25 }, r.Verses.ToArray());
        Assert.Single(r.Context);
        Assert.True(r.Context[0].Target);
    }

    [Fact]
    public void Load_null_json_throws_invalid_data()
    {
        using var s = new MemoryStream(Encoding.UTF8.GetBytes("null"));
        Assert.Throws<InvalidDataException>(() => TopicalGuideLoader.Load(s));
    }

    [Fact]
    public void Load_malformed_json_throws()
    {
        using var s = new MemoryStream(Encoding.UTF8.GetBytes("{ this is not json"));
        Assert.ThrowsAny<System.Exception>(() => TopicalGuideLoader.Load(s));
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test --filter TopicalGuideLoaderTests`
Expected: FAIL — loader/DTOs not defined.

- [ ] **Step 3: Write minimal implementation**

Create `src/JesusTheChrist.Core/Json/Dtos.cs`:

```csharp
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace JesusTheChrist.Core.Json;

internal sealed class GuideDto
{
    [JsonPropertyName("topic")] public string Topic { get; set; } = "";
    [JsonPropertyName("language")] public string Language { get; set; } = "";
    [JsonPropertyName("subtopics")] public List<SubTopicDto> SubTopics { get; set; } = new();
}

internal sealed class SubTopicDto
{
    [JsonPropertyName("title")] public string Title { get; set; } = "";
    [JsonPropertyName("short")] public string Short { get; set; } = "";
    [JsonPropertyName("short_en")] public string? ShortEn { get; set; }
    [JsonPropertyName("references")] public List<ReferenceDto> References { get; set; } = new();
}

internal sealed class ReferenceDto
{
    [JsonPropertyName("ref")] public string Ref { get; set; } = "";
    [JsonPropertyName("vol")] public string Vol { get; set; } = "";
    [JsonPropertyName("book")] public string Book { get; set; } = "";
    [JsonPropertyName("book_title")] public string BookTitle { get; set; } = "";
    [JsonPropertyName("ch")] public int Ch { get; set; }
    [JsonPropertyName("verses")] public List<int> Verses { get; set; } = new();
    [JsonPropertyName("context")] public List<ContextDto> Context { get; set; } = new();
    [JsonPropertyName("note")] public string? Note { get; set; }
}

internal sealed class ContextDto
{
    [JsonPropertyName("vs")] public int Vs { get; set; }
    [JsonPropertyName("text")] public string Text { get; set; } = "";
    [JsonPropertyName("target")] public bool Target { get; set; }
}
```

Create `src/JesusTheChrist.Core/Json/TopicalGuideLoader.cs`:

```csharp
using System.IO;
using System.Linq;
using System.Text.Json;
using JesusTheChrist.Core.Models;

namespace JesusTheChrist.Core.Json;

public static class TopicalGuideLoader
{
    public static TopicalGuide Load(Stream json)
    {
        var dto = JsonSerializer.Deserialize<GuideDto>(json)
                  ?? throw new InvalidDataException("Empty Topical Guide JSON.");

        var subtopics = dto.SubTopics.Select(s => new SubTopic(
            Slug.Make(s.ShortEn ?? s.Short), // language-invariant key from the English short
            s.Title, s.Short,
            s.References.Select(r => new Reference(
                r.Ref, VolumeExtensions.Parse(r.Vol), r.Book, r.BookTitle, r.Ch,
                r.Verses,
                r.Context.Select(c => new ContextVerse(c.Vs, c.Text, c.Target)).ToList(),
                r.Note)).ToList()
        )).ToList();

        return new TopicalGuide(dto.Topic, dto.Language, subtopics);
    }
}
```

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test --filter TopicalGuideLoaderTests`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add src/JesusTheChrist.Core/Json tests/JesusTheChrist.Core.Tests/TopicalGuideLoaderTests.cs
git commit -m "feat(core): JSON DTOs and TopicalGuideLoader (order-preserving, invariant keys)"
```

---

### Task 6: ScopeFilter (Bible Only vs Full)

**Files:**
- Create: `src/JesusTheChrist.Core/Content/ScopeFilter.cs`
- Test: `tests/JesusTheChrist.Core.Tests/ScopeFilterTests.cs`

- [ ] **Step 1: Write the failing test**

Create `tests/JesusTheChrist.Core.Tests/ScopeFilterTests.cs`:

```csharp
using System.IO;
using System.Linq;
using JesusTheChrist.Core.Content;
using JesusTheChrist.Core.Json;
using JesusTheChrist.Core.Models;
using Xunit;

public class ScopeFilterTests
{
    static TopicalGuide Load()
    {
        using var s = File.OpenRead(Path.Combine("Fixtures", "sample.json"));
        return TopicalGuideLoader.Load(s);
    }

    [Fact]
    public void Full_keeps_everything()
    {
        var g = ScopeFilter.Apply(Load(), Scope.Full);
        Assert.Equal(2, g.SubTopics.Count);
        Assert.Equal(2, g.SubTopics[1].References.Count); // Advocate: Heb + Moroni
    }

    [Fact]
    public void BibleOnly_drops_non_bible_refs_and_empty_subtopics()
    {
        var g = ScopeFilter.Apply(Load(), Scope.BibleOnly);
        var advocate = g.SubTopics.Single(t => t.Short == "Advocate");
        Assert.Single(advocate.References); // Moroni (Book of Mormon) dropped
        Assert.True(advocate.References.All(r => r.Vol.IsBible()));
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test --filter ScopeFilterTests`
Expected: FAIL — `ScopeFilter` not defined.

- [ ] **Step 3: Write minimal implementation**

Create `src/JesusTheChrist.Core/Content/ScopeFilter.cs`:

```csharp
using System.Linq;
using JesusTheChrist.Core.Models;

namespace JesusTheChrist.Core.Content;

public static class ScopeFilter
{
    public static TopicalGuide Apply(TopicalGuide guide, Scope scope)
    {
        if (scope == Scope.Full) return guide;

        var kept = guide.SubTopics
            .Select(t => t with { References = t.References.Where(r => r.Vol.IsBible()).ToList() })
            .Where(t => t.References.Count > 0)
            .ToList();

        return guide with { SubTopics = kept };
    }
}
```

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test --filter ScopeFilterTests`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add src/JesusTheChrist.Core/Content/ScopeFilter.cs tests/JesusTheChrist.Core.Tests/ScopeFilterTests.cs
git commit -m "feat(core): ScopeFilter for Bible Only vs Full"
```

---

### Task 7: ContentService (load + filter orchestration)

**Files:**
- Create: `src/JesusTheChrist.Core/Content/ContentService.cs`
- Test: `tests/JesusTheChrist.Core.Tests/ContentServiceTests.cs`

- [ ] **Step 1: Write the failing test**

Create `tests/JesusTheChrist.Core.Tests/ContentServiceTests.cs`:

```csharp
using System.IO;
using System.Threading.Tasks;
using JesusTheChrist.Core.Content;
using JesusTheChrist.Core.Models;
using Xunit;

public class ContentServiceTests
{
    sealed class FileAssets : IAssetSource
    {
        public string? LastRequested;
        public Task<Stream> OpenAsync(string name)
        {
            LastRequested = name;
            return Task.FromResult<Stream>(File.OpenRead(Path.Combine("Fixtures", "sample.json")));
        }
    }

    [Fact]
    public async Task LoadAsync_uses_language_filename_and_applies_scope()
    {
        var assets = new FileAssets();
        var svc = new ContentService(assets);
        var guide = await svc.LoadAsync(Language.En, Scope.BibleOnly);

        Assert.Equal("jesus-christ.en.json", assets.LastRequested);
        Assert.All(guide.SubTopics, t => Assert.All(t.References, r => Assert.True(r.Vol.IsBible())));
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test --filter ContentServiceTests`
Expected: FAIL — `ContentService` / `IAssetSource` not defined.

- [ ] **Step 3: Write minimal implementation**

Create `src/JesusTheChrist.Core/Content/ContentService.cs`:

```csharp
using System.IO;
using System.Threading.Tasks;
using JesusTheChrist.Core.Json;
using JesusTheChrist.Core.Models;

namespace JesusTheChrist.Core.Content;

// Abstraction so Core stays MAUI-free; the app supplies a MAUI-backed source,
// tests supply a file-backed one.
public interface IAssetSource
{
    Task<Stream> OpenAsync(string name);
}

public sealed class ContentService
{
    private readonly IAssetSource _assets;
    public ContentService(IAssetSource assets) => _assets = assets;

    public async Task<TopicalGuide> LoadAsync(Language lang, Scope scope)
    {
        await using var stream = await _assets.OpenAsync($"jesus-christ.{lang.Code()}.json");
        var guide = TopicalGuideLoader.Load(stream);
        return ScopeFilter.Apply(guide, scope);
    }
}
```

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test --filter ContentServiceTests`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add src/JesusTheChrist.Core/Content/ContentService.cs tests/JesusTheChrist.Core.Tests/ContentServiceTests.cs
git commit -m "feat(core): ContentService (Language-typed) with IAssetSource seam"
```

---

### Task 8: Real-data smoke test + canonical-order invariant

**Files:**
- Create: `tests/JesusTheChrist.Core.Tests/RealDataSmokeTests.cs`
- Modify: `tests/JesusTheChrist.Core.Tests/JesusTheChrist.Core.Tests.csproj` (link the real EN JSON for tests)

- [ ] **Step 1: Link the bundled EN JSON into the test output**

Add to `tests/JesusTheChrist.Core.Tests/JesusTheChrist.Core.Tests.csproj` inside `<Project>`:

```xml
<ItemGroup>
  <None Include="../../src/JesusTheChrist.App/Resources/Raw/jesus-christ.en.json"
        Link="RealData/jesus-christ.en.json">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </None>
</ItemGroup>
```

- [ ] **Step 2: Write the test**

The counts below are **pinned by the vendored (committed) asset** — they are not expected to drift. If they ever fail, the bundled JSON was changed deliberately; update the numbers as a conscious decision, not to "make it pass."

Create `tests/JesusTheChrist.Core.Tests/RealDataSmokeTests.cs`:

```csharp
using System.IO;
using System.Linq;
using JesusTheChrist.Core.Json;
using JesusTheChrist.Core.Models;
using Xunit;

public class RealDataSmokeTests
{
    static TopicalGuide Load()
    {
        using var s = File.OpenRead(Path.Combine("RealData", "jesus-christ.en.json"));
        return TopicalGuideLoader.Load(s);
    }

    [Fact]
    public void Has_expected_topic_and_counts()
    {
        var g = Load();
        Assert.Equal("Jesus Christ", g.Topic);
        Assert.Equal(53, g.SubTopics.Count);               // pinned by committed asset
        Assert.Equal(2196, g.SubTopics.Sum(t => t.References.Count)); // pinned by committed asset
    }

    [Fact]
    public void Subtopic_keys_are_unique_and_non_empty()
    {
        var keys = Load().SubTopics.Select(t => t.Key).ToList();
        Assert.DoesNotContain("", keys);
        Assert.Equal(keys.Count, keys.Distinct().Count());
    }

    [Fact]
    public void Volume_blocks_appear_in_canonical_order()
    {
        var g = Load();
        foreach (var t in g.SubTopics.Where(t => t.Short != "Summary"))
        {
            // The distinct volumes, in first-appearance order, must be non-decreasing
            // (OT→NT→BoM→D&C→PGP). Book/chapter order *within* a volume is preserved from
            // source (the TG page order), not re-derived here — Core has no per-book index.
            var distinctVols = t.References.Select(r => r.Vol.Order()).Distinct().ToList();
            Assert.Equal(distinctVols.OrderBy(x => x).ToList(), distinctVols);
        }
    }
}
```

- [ ] **Step 3: Run test to verify it passes**

Run: `dotnet test --filter RealDataSmokeTests`
Expected: PASS. (Task 8 tests already-implemented code, so there is no red→green here — it is a guardrail over the loader + vendored data.)

- [ ] **Step 4: Commit**

```bash
git add tests/JesusTheChrist.Core.Tests/RealDataSmokeTests.cs tests/JesusTheChrist.Core.Tests/JesusTheChrist.Core.Tests.csproj
git commit -m "test(core): real-data smoke counts, unique keys, canonical volume-order"
```

---

### Task 9: Full green run + close the phase

- [ ] **Step 1: Run the whole suite**

Run: `dotnet test JesusTheChrist.sln`
Expected: all tests PASS.

- [ ] **Step 2: Record outcomes and move the phase doc**

Fill in the `## Outcomes` section of `planning/in_progress/phase_plan-01-foundation_2026-05-31.md`, then:

```bash
git mv planning/in_progress/phase_plan-01-foundation_2026-05-31.md planning/completed/phase_plan-01-foundation_2026-05-31.md
```

- [ ] **Step 3: Commit (phase close)**

```bash
git add -A
git commit -m "[PHASE] Complete Plan 01: foundation & content layer

- Solution + Core/App/Tests scaffolded; content layer green.
- Language-invariant ids, Bible-Only filter, order preserved, real-data smoke.

Phase: planning/completed/phase_plan-01-foundation_2026-05-31.md"
```

---

## Self-review

- **Spec coverage (Plan 01 slice):** bundled JSON load ✓ (T5), **language-invariant** reference id ✓ (T3, T5), gloss rule ✓ (T4), Bible-Only filter + empty-subtopic drop ✓ (T6), order preserved incl. Summary exception ✓ (T5/T8), Core stays MAUI-free + testable ✓ (IAssetSource), negative-path behavior ✓ (T5). Out of this plan by design: SQLite, screens, progress, settings, two-flavor packaging (Plans 02–05).
- **Placeholder scan:** none. Two prior assumptions were resolved by review and are now explicit: (a) the content source is a documented **prerequisite** + vendored asset (not a hard-coded home path); (b) ids derive from a **language-invariant slug**, not the localized `short`.
- **Type consistency:** `Reference.Id(string subtopicKey)`, `Reference.TargetText`, `Reference.ShowGloss`, `Slug.Make(string)`, `SubTopic(Key, Title, Short, References)`, `VolumeExtensions.Parse/IsBible/Order`, `LanguageExtensions.Code`, `TopicalGuideLoader.Load(Stream)`, `ScopeFilter.Apply(TopicalGuide, Scope)`, `ContentService.LoadAsync(Language, Scope)`, `IAssetSource.OpenAsync(string)` are used consistently across tasks.
- **Known setup gotcha:** the Android workload must be installed (Task 1 Step 0) before `dotnet new maui`.
