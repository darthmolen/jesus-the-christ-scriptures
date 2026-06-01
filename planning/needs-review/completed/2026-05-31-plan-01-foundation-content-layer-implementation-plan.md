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
- References are **already in canonical TG order** in the JSON (OT→NT→BoM→D&C→PGP, then chapter:verse), and the **Summary** sub-topic is in narrative order. The loader **preserves JSON order** — no re-sort (re-sorting would corrupt Summary). Task 9 adds an invariant test.
- **Reference id** = `"{subtopicShort}:{vol}/{book}/{ch}/{vStart}-{vEnd}"` (vStart==vEnd → just the number). Same verse can appear under multiple sub-topics, so id is scoped by sub-topic.
- **Gloss shown** only when a `note` exists and is **not** contained in the target verse text (normalized). EN notes are short TG highlights; most ES notes equal the verse and are suppressed.

## File structure

```
JesusTheChrist.sln
src/JesusTheChrist.Core/
  JesusTheChrist.Core.csproj
  Models/Scope.cs            # Scope enum
  Models/Volume.cs           # Volume enum + parse + order + IsBible
  Models/ContextVerse.cs     # record
  Models/Reference.cs        # record + Id, TargetText, ShowGloss
  Models/SubTopic.cs         # record
  Models/TopicalGuide.cs     # record
  Json/Dtos.cs               # System.Text.Json DTOs (wire shape)
  Json/TopicalGuideLoader.cs # Stream -> TopicalGuide (preserves order)
  Content/ScopeFilter.cs     # filter a TopicalGuide by Scope
  Content/ContentService.cs  # load + filter orchestration
src/JesusTheChrist.App/      # MAUI app (scaffold only this plan)
  JesusTheChrist.App.csproj  # net10.0-android, UseMaterial3, refs Core
  Resources/Raw/jesus-christ.en.json
  Resources/Raw/jesus-christ.es.json
tests/JesusTheChrist.Core.Tests/
  JesusTheChrist.Core.Tests.csproj
  Fixtures/sample.json       # tiny deterministic fixture
  VolumeTests.cs
  ReferenceIdTests.cs
  ReferenceGlossTests.cs
  TopicalGuideLoaderTests.cs
  ScopeFilterTests.cs
  ContentServiceTests.cs
  RealDataSmokeTests.cs
```

---

### Task 1: Scaffold solution, projects, and assets

**Files:**
- Create: `JesusTheChrist.sln`, the three `.csproj` files, `tests/.../Fixtures/sample.json`
- Copy: the two content JSON files into `src/JesusTheChrist.App/Resources/Raw/`

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

- [ ] **Step 3: Bundle the content JSON as raw assets**

```bash
mkdir -p src/JesusTheChrist.App/Resources/Raw
cp ~/dev/lds-nl-scriptures/content/processed/scriptures/en/topical-guide/jesus-christ.json src/JesusTheChrist.App/Resources/Raw/jesus-christ.en.json
cp ~/dev/lds-nl-scriptures/content/processed/scriptures/es/topical-guide/jesus-christ.json src/JesusTheChrist.App/Resources/Raw/jesus-christ.es.json
```

(MAUI bundles everything under `Resources/Raw/` automatically; the app reads them via `FileSystem.OpenAppPackageFileAsync`.)

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

- [ ] **Step 5: Verify it builds and tests run (empty)**

Run: `dotnet build JesusTheChrist.sln`
Expected: build succeeds (Android workload must be installed; if not: `dotnet workload install maui-android`).

- [ ] **Step 6: Commit**

```bash
git add -A
git commit -m "chore: scaffold solution, Core/App/Tests projects, bundle content JSON"
```

---

### Task 2: Scope and Volume models

**Files:**
- Create: `src/JesusTheChrist.Core/Models/Scope.cs`, `src/JesusTheChrist.Core/Models/Volume.cs`
- Test: `tests/JesusTheChrist.Core.Tests/VolumeTests.cs`

- [ ] **Step 1: Write the failing test**

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

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test --filter VolumeTests`
Expected: FAIL — `Volume` / `VolumeExtensions` do not exist.

- [ ] **Step 3: Write minimal implementation**

Create `src/JesusTheChrist.Core/Models/Scope.cs`:

```csharp
namespace JesusTheChrist.Core.Models;

public enum Scope { BibleOnly, Full }
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

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test --filter VolumeTests`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add src/JesusTheChrist.Core/Models/Scope.cs src/JesusTheChrist.Core/Models/Volume.cs tests/JesusTheChrist.Core.Tests/VolumeTests.cs
git commit -m "feat(core): Scope and Volume models with parse/order/IsBible"
```

---

### Task 3: Domain records + Reference.Id

**Files:**
- Create: `src/JesusTheChrist.Core/Models/ContextVerse.cs`, `Reference.cs`, `SubTopic.cs`, `TopicalGuide.cs`
- Test: `tests/JesusTheChrist.Core.Tests/ReferenceIdTests.cs`

- [ ] **Step 1: Write the failing test**

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
    public void Id_single_verse()
        => Assert.Equal("Advocate:newtestament/john/3/16", Ref(new[] { 16 }).Id("Advocate"));

    [Fact]
    public void Id_verse_range_uses_first_and_last()
        => Assert.Equal("Summary:newtestament/john/3/16-18", Ref(new[] { 16, 17, 18 }).Id("Summary"));
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test --filter ReferenceIdTests`
Expected: FAIL — types not defined.

- [ ] **Step 3: Write minimal implementation**

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
    public string Id(string subtopicShort)
    {
        var lo = Verses.Count > 0 ? Verses[0] : 0;
        var hi = Verses.Count > 0 ? Verses[^1] : 0;
        var span = lo == hi ? $"{lo}" : $"{lo}-{hi}";
        return $"{subtopicShort}:{Vol.ToString().ToLowerInvariant()}/{Book}/{Ch}/{span}";
    }
}
```

Create `src/JesusTheChrist.Core/Models/SubTopic.cs`:

```csharp
using System.Collections.Generic;

namespace JesusTheChrist.Core.Models;

public record SubTopic(string Title, string Short, IReadOnlyList<Reference> References);
```

Create `src/JesusTheChrist.Core/Models/TopicalGuide.cs`:

```csharp
using System.Collections.Generic;

namespace JesusTheChrist.Core.Models;

public record TopicalGuide(string Topic, string Language, IReadOnlyList<SubTopic> SubTopics);
```

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test --filter ReferenceIdTests`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add src/JesusTheChrist.Core/Models tests/JesusTheChrist.Core.Tests/ReferenceIdTests.cs
git commit -m "feat(core): domain records and Reference.Id"
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

Add `using System.Linq;` at the top of the test file.

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
    public void Loads_subtopics_in_order()
    {
        var g = Load();
        Assert.Equal("Jesus Christ", g.Topic);
        Assert.Equal(new[] { "Summary", "Advocate" }, g.SubTopics.Select(t => t.Short).ToArray());
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
}
```

Add `using System.Linq;` at the top.

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
using System;
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
git commit -m "feat(core): JSON DTOs and TopicalGuideLoader (order-preserving)"
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
        // Advocate loses the Book of Mormon (Moroni) reference, keeps Hebrews.
        var advocate = g.SubTopics.Single(t => t.Short == "Advocate");
        Assert.Single(advocate.References);
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
        public Task<Stream> OpenAsync(string name) =>
            Task.FromResult<Stream>(File.OpenRead(Path.Combine("Fixtures", "sample.json")));
    }

    [Fact]
    public async Task LoadAsync_applies_scope()
    {
        var svc = new ContentService(new FileAssets());
        var guide = await svc.LoadAsync("en", Scope.BibleOnly);
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

    public async Task<TopicalGuide> LoadAsync(string lang, Scope scope)
    {
        await using var stream = await _assets.OpenAsync($"jesus-christ.{lang}.json");
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
git commit -m "feat(core): ContentService load+filter with IAssetSource abstraction"
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

- [ ] **Step 2: Write the failing test**

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
        Assert.Equal(53, g.SubTopics.Count);
        Assert.Equal(2196, g.SubTopics.Sum(t => t.References.Count));
    }

    [Fact]
    public void Non_summary_subtopics_are_in_canonical_order()
    {
        var g = Load();
        foreach (var t in g.SubTopics.Where(t => t.Short != "Summary"))
        {
            var keys = t.References
                .Select(r => (r.Vol.Order(), r.Ch, r.Verses.Count > 0 ? r.Verses[0] : 0))
                .ToList();
            var sorted = keys.OrderBy(k => k.Item1).ThenBy(k => k.Ch).ThenBy(k => k.Item3).ToList();
            // Same volume blocks must appear in canonical volume order (book order within a
            // volume is preserved from source, not re-derived here).
            Assert.Equal(
                keys.Select(k => k.Item1).Distinct().ToList(),
                keys.Select(k => k.Item1).Distinct().OrderBy(x => x).ToList());
        }
    }
}
```

- [ ] **Step 3: Run test to verify it fails, then passes**

Run: `dotnet test --filter RealDataSmokeTests`
Expected: PASS if counts match. If `Has_expected_topic_and_counts` fails on the numbers, update the asserted counts to the real values printed in the failure and re-run (the data is authoritative). Volume-order test should pass.

- [ ] **Step 4: Commit**

```bash
git add tests/JesusTheChrist.Core.Tests/RealDataSmokeTests.cs tests/JesusTheChrist.Core.Tests/JesusTheChrist.Core.Tests.csproj
git commit -m "test(core): real-data smoke counts and canonical volume-order invariant"
```

---

### Task 9: Full green run

- [ ] **Step 1: Run the whole suite**

Run: `dotnet test JesusTheChrist.sln`
Expected: all tests PASS.

- [ ] **Step 2: Commit any final cleanup**

```bash
git add -A
git commit -m "chore: Plan 01 foundation complete — content layer green" --allow-empty
```

---

## Self-review

- **Spec coverage (Plan 01 slice):** bundled JSON load ✓ (T5), reference id ✓ (T3), gloss rule ✓ (T4), Bible-Only filter + empty-subtopic drop ✓ (T6), order preserved incl. Summary exception ✓ (T5/T8), Core stays MAUI-free + testable ✓ (IAssetSource). Out of this plan by design: SQLite, screens, progress, settings, two-flavor packaging (Plans 02–05).
- **Placeholder scan:** none — every step has runnable code/commands.
- **Type consistency:** `Reference.Id(string)`, `Reference.TargetText`, `Reference.ShowGloss`, `VolumeExtensions.Parse/IsBible/Order`, `TopicalGuideLoader.Load(Stream)`, `ScopeFilter.Apply(TopicalGuide, Scope)`, `ContentService.LoadAsync(string, Scope)`, `IAssetSource.OpenAsync(string)` are used consistently across tasks.
- **Risk:** the `2196` count in T8 is asserted from the current data; if the extract is regenerated the test self-documents the new number (update on first run). Android workload may need `dotnet workload install maui-android` before T1 builds.

---

## Plan Review

**Reviewed:** 2026-05-31 19:51
**Reviewer:** Claude Code (plan-review-intake)

### Strengths
- **"Decisions baked in" + Tasks 2–7:** The plan has a clear architecture: MAUI-free `JesusTheChrist.Core`, isolated content/domain logic, and a thin app boundary via `IAssetSource`. That fits the design spec's layered approach well.
- **Task structure throughout:** Each task is concrete, ordered, TDD-oriented, and includes explicit verification commands and expected fail/pass states.
- **Task 8:** Using both fixture-based tests and a real-data smoke test is a strong quality gate for a greenfield content layer.
- **Repo fit:** The repo is effectively greenfield today (no solution, projects, or source files yet), and the plan correctly treats most referenced files as new creations.

### Issues

#### Critical (Must Address Before Implementation)

**Task 3 / "Decisions baked in" (`Reference.Id`)**
- **What's wrong:** The plan defines `Reference.Id` using `subtopicShort`. The real JSON's `short` field is localized (`Summary` in EN vs `Resumen` in ES, `Advocate` vs `Abogado`).
- **Why it matters:** IDs become language-dependent, which will break stable keys for read marks/notes when EN/ES are both supported. Conflicts with the design spec's stable slug-based ID intent.
- **Suggested fix:** Define a language-invariant subtopic key (canonical slug, stable mapping file, or index-based stable key) and base `Reference.Id` on that instead of localized `short`.

**Task 1 Step 3 (copying JSON assets)**
- **What's wrong:** The plan depends on `~/dev/lds-nl-scriptures/...`, a machine-specific external repo path.
- **Why it matters:** Another implementer cannot execute the plan as written unless they happen to have that exact checkout/path. This is a hidden prerequisite.
- **Suggested fix:** Add an explicit prerequisites section with required source repo/commit and path, or vendor the source JSON into this repo/workflow in a reproducible way.

#### Important (Should Address)

**Task 8 Step 2 ("Non_summary_subtopics_are_in_canonical_order")**
- **What's wrong:** The test computes `sorted` but never uses it; only checks distinct volume block order.
- **Why it matters:** References could be out of order within a volume/book/chapter and still pass, so the stated ordering requirement isn't actually guarded.
- **Suggested fix:** Compare the full sequence against a real canonical key (volume + book index + chapter + first verse), or rename the test to match the weaker invariant it checks.

**Task 1 Step 1 / Step 5 (MAUI workload prerequisite)**
- **What's wrong:** The MAUI workload prerequisite is mentioned only after scaffolding/build, but `dotnet new maui` itself may fail before that.
- **Suggested fix:** Add a preflight step before scaffolding: verify .NET 10 SDK + MAUI templates/workload, then proceed.

**All tasks vs `CLAUDE.md` Planning/Commit Protocol**
- **What's wrong:** The plan does not require creating/updating/moving a `planning/in_progress/phase_*.md` document, and its commit messages do not follow the required `[PHASE] ...` format with the phase-file reference.
- **Why it matters:** Violates the repository's explicit execution conventions.
- **Suggested fix:** Add a phase-document step at the start/end of the plan and replace commit examples with CLAUDE-compliant messages.

**Task 5 / Task 7 (error handling)**
- **What's wrong:** Error handling behavior is underdefined for invalid JSON, missing assets, and unsupported language values; `ContentService` also uses a raw `string lang`.
- **Why it matters:** Key failure modes are unspecified and untested.
- **Suggested fix:** Add negative tests and define contract behavior; consider a `Language` enum or validated constants instead of free-form strings.

#### Minor (Consider)

**Task 8 Step 3 (hard-coded counts)**
- Telling the implementer to update counts to whatever current data returns weakens regression value unless input data version is pinned.
- **Suggested fix:** Record the source JSON revision/commit, or make the smoke test about nonzero structure/invariants rather than mutable counts.

**Plan self-review section**
- "Placeholder scan: none" overstates confidence given the external asset dependency and ID-stability issue.
- **Suggested fix:** Update the self-review to call out those assumptions explicitly.

### Recommendations
- Keep the overall architecture; it is sound for this repo.
- Fix the **stable ID strategy** before any implementation starts — downstream data/storage plans will depend on it.
- Add a short **Prerequisites** section (SDK/workloads, external asset source, source revision).
- Bring execution/commit steps into alignment with **`CLAUDE.md`** so the plan is usable in this repository.

### Assessment
**Implementable as written?** With fixes
**Reasoning:** The decomposition and architecture are strong, but two issues should be corrected first: the localized `subtopicShort`-based ID design and the undocumented machine-specific JSON source dependency. Without those fixes, implementation would likely create unstable identifiers and a non-reproducible setup.
