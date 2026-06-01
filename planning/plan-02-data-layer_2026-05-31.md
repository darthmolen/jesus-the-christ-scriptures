# Plan 02 — Local Data Layer — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add an on-device persistence layer (read-marks, notes, settings) plus derived **progress** and **streak** logic — pure .NET 10, fully unit-tested on Ubuntu, no MAUI.

**Architecture:** A new `JesusTheChrist.Data` class library wraps **sqlite-net-pcl** behind small focused stores (`ReadMarkStore`, `NoteStore`, `SettingsStore`) over an `AppDatabase`. `ProgressService` (pure) computes overall + per-sub-topic progress from a Core `TopicalGuide` + the set of read ids. `StreakService` (pure) advances streak state by date. Time is injected (`Func<DateTime>` / a passed-in `DateOnly`) so everything is deterministic. The MAUI app supplies the DB file path; tests use a temp-file DB. Native SQLite for the test run comes from `SQLitePCLRaw.bundle_e_sqlite3`.

**Tech Stack:** C# / .NET 10, sqlite-net-pcl, SQLitePCLRaw bundle, xUnit. Depends on Plan 01's `JesusTheChrist.Core`.

---

## Plan series (this is Plan 02 of 5)

1. Foundation & Content Layer ✅ (Plan 01)
2. **Local data layer** ← this plan
3. App shell + Home/Challenge tracker
4. Topic feed (cards, context, gloss, read/note)
5. Settings, notes editor, progress detail, two-flavor packaging & polish

## Prerequisites

- Plan 01 merged (`JesusTheChrist.Core` + `JesusTheChrist.slnx` present).
- .NET 10 SDK. No MAUI workload needed — this layer is pure .NET and tests run on Ubuntu via the
  `SQLitePCLRaw.bundle_e_sqlite3` native package.

## Decisions baked in (from `planning/design-spec_2026-05-31.md` §5.2)

- Storage = **sqlite-net-pcl** (decided). Three tables: `ReadMark`, `NoteEntry`, `Setting`.
- **Reference id** (from Core) is the key for read-marks and notes: `Reference.Id(subTopic.Key)`
  — language-invariant, so EN/ES share user data.
- **Progress** is derived, not stored: `Overall = read/total`; per sub-topic by its references.
- **Streak** (off by default in UI; logic still lives here): **any reference read in a day counts**;
  track `Current` + `Best`. Computed by a pure `Advance(prev, today)`; persisted via settings.
- **Notes**: empty/whitespace text deletes the note. Saving trims.
- Time is injected for determinism; no `DateTime.UtcNow`/`DateOnly.FromDateTime(DateTime.Now)`
  inside logic under test.

## File structure

```
src/JesusTheChrist.Data/
  JesusTheChrist.Data.csproj   # net10.0; ref Core; sqlite-net-pcl; SQLitePCLRaw.bundle_e_sqlite3
  Entities.cs                  # ReadMark, NoteEntry, Setting (sqlite-net rows)
  AppDatabase.cs               # connection + InitializeAsync (CreateTable x3)
  ReadMarkStore.cs             # mark/unmark/isRead/count/getReadIds
  NoteStore.cs                 # save(trim/delete-on-empty)/get/delete/hasNote
  Settings.cs                  # SettingKeys + SettingsStore (typed get/set)
  Progress.cs                  # Progress value + ProgressService (over a TopicalGuide)
  Streak.cs                    # StreakState + StreakService(Advance) + StreakStore(persist)
tests/JesusTheChrist.Data.Tests/
  JesusTheChrist.Data.Tests.csproj  # xunit; ref Data + Core
  TestDb.cs                    # temp-file AppDatabase helper (IAsyncDisposable)
  AppDatabaseTests.cs
  ReadMarkStoreTests.cs
  NoteStoreTests.cs
  SettingsStoreTests.cs
  ProgressServiceTests.cs
  StreakServiceTests.cs
```

---

### Task 0: Phase document (CLAUDE.md protocol)

**Files:** Create `planning/in_progress/phase_plan-02-data_2026-05-31.md`

- [ ] **Step 1: Create the phase doc**

```markdown
# Phase: Plan 02 — Local Data Layer

**Date:** 2026-05-31
**Plan:** planning/plan-02-data-layer_2026-05-31.md
**Branch:** feature/plan-02-data

## Objective
On-device SQLite persistence (read-marks, notes, settings) + derived progress + streak logic,
pure .NET, fully tested.

## Success criteria
- `dotnet test` green across Core + Data tests.
- Read-marks/notes keyed by language-invariant Reference.Id; progress derived; streak by date.

## Files expected to change
- src/JesusTheChrist.Data/**, tests/JesusTheChrist.Data.Tests/**, JesusTheChrist.slnx

## Outcomes
(to be filled at completion)
```

- [ ] **Step 2: Commit**

```bash
git add planning/in_progress/phase_plan-02-data_2026-05-31.md
git commit -m "[PHASE] Start Plan 02: local data layer

Phase: planning/in_progress/phase_plan-02-data_2026-05-31.md"
```

---

### Task 1: Scaffold the Data project + tests

**Files:** Create `src/JesusTheChrist.Data/*`, `tests/JesusTheChrist.Data.Tests/*`

- [ ] **Step 1: Create projects, wire references + packages**

```bash
dotnet new classlib -n JesusTheChrist.Data -o src/JesusTheChrist.Data -f net10.0
dotnet new xunit   -n JesusTheChrist.Data.Tests -o tests/JesusTheChrist.Data.Tests -f net10.0
rm -f src/JesusTheChrist.Data/Class1.cs tests/JesusTheChrist.Data.Tests/UnitTest1.cs
dotnet sln add src/JesusTheChrist.Data tests/JesusTheChrist.Data.Tests
dotnet add src/JesusTheChrist.Data reference src/JesusTheChrist.Core
dotnet add tests/JesusTheChrist.Data.Tests reference src/JesusTheChrist.Data src/JesusTheChrist.Core
dotnet add src/JesusTheChrist.Data package sqlite-net-pcl --version 1.9.172
dotnet add src/JesusTheChrist.Data package SQLitePCLRaw.bundle_e_sqlite3 --version 2.1.10
```

- [ ] **Step 2: Verify build**

Run: `dotnet build`
Expected: succeeds. (If a package version is unavailable, use the latest 1.9.x / 2.1.x.)

- [ ] **Step 3: Commit**

```bash
git add -A
git commit -m "chore: scaffold JesusTheChrist.Data + tests (sqlite-net-pcl)"
```

---

### Task 2: Entities + AppDatabase

**Files:** Create `src/JesusTheChrist.Data/Entities.cs`, `AppDatabase.cs`, `tests/.../TestDb.cs`, `AppDatabaseTests.cs`

- [ ] **Step 1: Write the failing test**

Create `tests/JesusTheChrist.Data.Tests/TestDb.cs`:

```csharp
using System;
using System.IO;
using System.Threading.Tasks;
using JesusTheChrist.Data;

namespace JesusTheChrist.Data.Tests;

// A throwaway temp-file database for a single test; deletes itself on dispose.
public sealed class TestDb : IAsyncDisposable
{
    public AppDatabase Db { get; }
    private readonly string _path;

    private TestDb(string path, AppDatabase db) { _path = path; Db = db; }

    public static async Task<TestDb> CreateAsync()
    {
        var path = Path.Combine(Path.GetTempPath(), $"jtc-test-{Guid.NewGuid():N}.db");
        var db = new AppDatabase(path);
        await db.InitializeAsync();
        return new TestDb(path, db);
    }

    public async ValueTask DisposeAsync()
    {
        await Db.Connection.CloseAsync();
        try { File.Delete(_path); } catch { /* best effort */ }
    }
}
```

Create `tests/JesusTheChrist.Data.Tests/AppDatabaseTests.cs`:

```csharp
using System.Threading.Tasks;
using JesusTheChrist.Data;
using Xunit;

namespace JesusTheChrist.Data.Tests;

public class AppDatabaseTests
{
    [Fact]
    public async Task Initialize_creates_tables_and_roundtrips_a_row()
    {
        await using var t = await TestDb.CreateAsync();
        await t.Db.Connection.InsertAsync(new Setting { Key = "k", Value = "v" });
        var row = await t.Db.Connection.FindAsync<Setting>("k");
        Assert.NotNull(row);
        Assert.Equal("v", row!.Value);
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test --filter AppDatabaseTests`
Expected: FAIL — `AppDatabase` / `Setting` not defined.

- [ ] **Step 3: Write minimal implementation**

Create `src/JesusTheChrist.Data/Entities.cs`:

```csharp
using System;
using SQLite;

namespace JesusTheChrist.Data;

public class ReadMark
{
    [PrimaryKey] public string RefId { get; set; } = "";
    public DateTime ReadAtUtc { get; set; }
}

public class NoteEntry
{
    [PrimaryKey] public string RefId { get; set; } = "";
    public string Text { get; set; } = "";
    public DateTime UpdatedAtUtc { get; set; }
}

public class Setting
{
    [PrimaryKey] public string Key { get; set; } = "";
    public string Value { get; set; } = "";
}
```

Create `src/JesusTheChrist.Data/AppDatabase.cs`:

```csharp
using System.Threading.Tasks;
using SQLite;

namespace JesusTheChrist.Data;

public sealed class AppDatabase
{
    public SQLiteAsyncConnection Connection { get; }

    public AppDatabase(string dbPath)
    {
        SQLitePCL.Batteries_V2.Init(); // ensure the native provider is registered
        Connection = new SQLiteAsyncConnection(dbPath);
    }

    public async Task InitializeAsync()
    {
        await Connection.CreateTableAsync<ReadMark>();
        await Connection.CreateTableAsync<NoteEntry>();
        await Connection.CreateTableAsync<Setting>();
    }
}
```

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test --filter AppDatabaseTests`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add src/JesusTheChrist.Data/Entities.cs src/JesusTheChrist.Data/AppDatabase.cs tests/JesusTheChrist.Data.Tests/TestDb.cs tests/JesusTheChrist.Data.Tests/AppDatabaseTests.cs
git commit -m "feat(data): entities + AppDatabase (create tables)"
```

---

### Task 3: ReadMarkStore

**Files:** Create `src/JesusTheChrist.Data/ReadMarkStore.cs`, `tests/.../ReadMarkStoreTests.cs`

- [ ] **Step 1: Write the failing test**

Create `tests/JesusTheChrist.Data.Tests/ReadMarkStoreTests.cs`:

```csharp
using System;
using System.Threading.Tasks;
using JesusTheChrist.Data;
using Xunit;

namespace JesusTheChrist.Data.Tests;

public class ReadMarkStoreTests
{
    static readonly DateTime Fixed = new(2026, 5, 31, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Mark_then_isread_count_and_ids()
    {
        await using var t = await TestDb.CreateAsync();
        var store = new ReadMarkStore(t.Db, () => Fixed);

        Assert.False(await store.IsReadAsync("a:nt/john/3/16"));
        await store.MarkReadAsync("a:nt/john/3/16");
        await store.MarkReadAsync("b:nt/john/1/1");

        Assert.True(await store.IsReadAsync("a:nt/john/3/16"));
        Assert.Equal(2, await store.CountAsync());
        Assert.Contains("b:nt/john/1/1", await store.GetReadIdsAsync());
    }

    [Fact]
    public async Task Mark_is_idempotent_and_unmark_removes()
    {
        await using var t = await TestDb.CreateAsync();
        var store = new ReadMarkStore(t.Db, () => Fixed);

        await store.MarkReadAsync("x");
        await store.MarkReadAsync("x"); // no duplicate
        Assert.Equal(1, await store.CountAsync());

        await store.UnmarkAsync("x");
        Assert.False(await store.IsReadAsync("x"));
        Assert.Equal(0, await store.CountAsync());
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test --filter ReadMarkStoreTests`
Expected: FAIL — `ReadMarkStore` not defined.

- [ ] **Step 3: Write minimal implementation**

Create `src/JesusTheChrist.Data/ReadMarkStore.cs`:

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SQLite;

namespace JesusTheChrist.Data;

public sealed class ReadMarkStore
{
    private readonly SQLiteAsyncConnection _c;
    private readonly Func<DateTime> _utcNow;

    public ReadMarkStore(AppDatabase db, Func<DateTime>? utcNow = null)
    {
        _c = db.Connection;
        _utcNow = utcNow ?? (() => DateTime.UtcNow);
    }

    public Task MarkReadAsync(string refId) =>
        _c.InsertOrReplaceAsync(new ReadMark { RefId = refId, ReadAtUtc = _utcNow() });

    public Task UnmarkAsync(string refId) => _c.DeleteAsync<ReadMark>(refId);

    public async Task<bool> IsReadAsync(string refId) =>
        await _c.FindAsync<ReadMark>(refId) is not null;

    public Task<int> CountAsync() => _c.Table<ReadMark>().CountAsync();

    public async Task<HashSet<string>> GetReadIdsAsync() =>
        (await _c.Table<ReadMark>().ToListAsync()).Select(r => r.RefId).ToHashSet();
}
```

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test --filter ReadMarkStoreTests`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add src/JesusTheChrist.Data/ReadMarkStore.cs tests/JesusTheChrist.Data.Tests/ReadMarkStoreTests.cs
git commit -m "feat(data): ReadMarkStore (mark/unmark/isRead/count/ids)"
```

---

### Task 4: NoteStore

**Files:** Create `src/JesusTheChrist.Data/NoteStore.cs`, `tests/.../NoteStoreTests.cs`

- [ ] **Step 1: Write the failing test**

Create `tests/JesusTheChrist.Data.Tests/NoteStoreTests.cs`:

```csharp
using System;
using System.Threading.Tasks;
using JesusTheChrist.Data;
using Xunit;

namespace JesusTheChrist.Data.Tests;

public class NoteStoreTests
{
    static readonly DateTime Fixed = new(2026, 5, 31, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Save_get_and_overwrite()
    {
        await using var t = await TestDb.CreateAsync();
        var store = new NoteStore(t.Db, () => Fixed);

        Assert.Null(await store.GetAsync("r"));
        await store.SaveAsync("r", "  first thought  ");
        Assert.Equal("first thought", await store.GetAsync("r")); // trimmed
        Assert.True(await store.HasNoteAsync("r"));

        await store.SaveAsync("r", "second");
        Assert.Equal("second", await store.GetAsync("r"));
    }

    [Fact]
    public async Task Empty_text_deletes_the_note()
    {
        await using var t = await TestDb.CreateAsync();
        var store = new NoteStore(t.Db, () => Fixed);

        await store.SaveAsync("r", "something");
        await store.SaveAsync("r", "   "); // whitespace clears
        Assert.False(await store.HasNoteAsync("r"));
        Assert.Null(await store.GetAsync("r"));
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test --filter NoteStoreTests`
Expected: FAIL — `NoteStore` not defined.

- [ ] **Step 3: Write minimal implementation**

Create `src/JesusTheChrist.Data/NoteStore.cs`:

```csharp
using System;
using System.Threading.Tasks;
using SQLite;

namespace JesusTheChrist.Data;

public sealed class NoteStore
{
    private readonly SQLiteAsyncConnection _c;
    private readonly Func<DateTime> _utcNow;

    public NoteStore(AppDatabase db, Func<DateTime>? utcNow = null)
    {
        _c = db.Connection;
        _utcNow = utcNow ?? (() => DateTime.UtcNow);
    }

    public async Task SaveAsync(string refId, string text)
    {
        var trimmed = (text ?? "").Trim();
        if (trimmed.Length == 0) { await DeleteAsync(refId); return; }
        await _c.InsertOrReplaceAsync(new NoteEntry
        {
            RefId = refId, Text = trimmed, UpdatedAtUtc = _utcNow()
        });
    }

    public async Task<string?> GetAsync(string refId) =>
        (await _c.FindAsync<NoteEntry>(refId))?.Text;

    public Task DeleteAsync(string refId) => _c.DeleteAsync<NoteEntry>(refId);

    public async Task<bool> HasNoteAsync(string refId) =>
        await _c.FindAsync<NoteEntry>(refId) is not null;
}
```

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test --filter NoteStoreTests`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add src/JesusTheChrist.Data/NoteStore.cs tests/JesusTheChrist.Data.Tests/NoteStoreTests.cs
git commit -m "feat(data): NoteStore (save/trim/delete-on-empty/get/hasNote)"
```

---

### Task 5: SettingsStore (+ keys, typed helpers)

**Files:** Create `src/JesusTheChrist.Data/Settings.cs`, `tests/.../SettingsStoreTests.cs`

- [ ] **Step 1: Write the failing test**

Create `tests/JesusTheChrist.Data.Tests/SettingsStoreTests.cs`:

```csharp
using System.Threading.Tasks;
using JesusTheChrist.Data;
using Xunit;

namespace JesusTheChrist.Data.Tests;

public class SettingsStoreTests
{
    [Fact]
    public async Task String_get_set_with_fallback()
    {
        await using var t = await TestDb.CreateAsync();
        var s = new SettingsStore(t.Db);

        Assert.Null(await s.GetAsync(SettingKeys.Theme));
        await s.SetAsync(SettingKeys.Theme, "dark");
        Assert.Equal("dark", await s.GetAsync(SettingKeys.Theme));
    }

    [Fact]
    public async Task Typed_int_and_bool_helpers()
    {
        await using var t = await TestDb.CreateAsync();
        var s = new SettingsStore(t.Db);

        Assert.Equal(18, await s.GetIntAsync(SettingKeys.FontSize, 18)); // fallback
        await s.SetIntAsync(SettingKeys.FontSize, 22);
        Assert.Equal(22, await s.GetIntAsync(SettingKeys.FontSize, 18));

        Assert.False(await s.GetBoolAsync(SettingKeys.StreakEnabled, false));
        await s.SetBoolAsync(SettingKeys.StreakEnabled, true);
        Assert.True(await s.GetBoolAsync(SettingKeys.StreakEnabled, false));
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test --filter SettingsStoreTests`
Expected: FAIL — `SettingsStore` / `SettingKeys` not defined.

- [ ] **Step 3: Write minimal implementation**

Create `src/JesusTheChrist.Data/Settings.cs`:

```csharp
using System.Globalization;
using System.Threading.Tasks;
using SQLite;

namespace JesusTheChrist.Data;

public static class SettingKeys
{
    public const string Theme = "theme";              // "light" | "dark" | "system"
    public const string FontFamily = "font_family";
    public const string FontSize = "font_size";       // int
    public const string Language = "language";        // "en" | "es"
    public const string StreakEnabled = "streak_enabled"; // bool
}

public sealed class SettingsStore
{
    private readonly SQLiteAsyncConnection _c;
    public SettingsStore(AppDatabase db) => _c = db.Connection;

    public async Task<string?> GetAsync(string key) =>
        (await _c.FindAsync<Setting>(key))?.Value;

    public Task SetAsync(string key, string value) =>
        _c.InsertOrReplaceAsync(new Setting { Key = key, Value = value });

    public async Task<int> GetIntAsync(string key, int fallback) =>
        int.TryParse(await GetAsync(key), NumberStyles.Integer, CultureInfo.InvariantCulture, out var v)
            ? v : fallback;

    public Task SetIntAsync(string key, int value) =>
        SetAsync(key, value.ToString(CultureInfo.InvariantCulture));

    public async Task<bool> GetBoolAsync(string key, bool fallback) =>
        bool.TryParse(await GetAsync(key), out var v) ? v : fallback;

    public Task SetBoolAsync(string key, bool value) =>
        SetAsync(key, value ? "true" : "false");
}
```

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test --filter SettingsStoreTests`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add src/JesusTheChrist.Data/Settings.cs tests/JesusTheChrist.Data.Tests/SettingsStoreTests.cs
git commit -m "feat(data): SettingsStore with keys + typed helpers"
```

---

### Task 6: ProgressService (derived from Core's TopicalGuide)

**Files:** Create `src/JesusTheChrist.Data/Progress.cs`, `tests/.../ProgressServiceTests.cs`

- [ ] **Step 1: Write the failing test**

Create `tests/JesusTheChrist.Data.Tests/ProgressServiceTests.cs`:

```csharp
using System.Collections.Generic;
using JesusTheChrist.Core.Models;
using JesusTheChrist.Data;
using Xunit;

namespace JesusTheChrist.Data.Tests;

public class ProgressServiceTests
{
    static Reference R(int v) => new("X", Volume.NewTestament, "john", "John", 3,
        new[] { v }, new List<ContextVerse>(), null);

    static TopicalGuide Guide() => new("Jesus Christ", "en", new[]
    {
        new SubTopic("advocate", "Jesus Christ, Advocate", "Advocate", new[] { R(1), R(2) }),
        new SubTopic("creator",  "Jesus Christ, Creator",  "Creator",  new[] { R(3) }),
    });

    [Fact]
    public void Overall_counts_read_over_total()
    {
        var g = Guide();
        var read = new HashSet<string> { R(1).Id("advocate") }; // 1 of 3
        var p = new ProgressService().Overall(g, read);
        Assert.Equal(1, p.Read);
        Assert.Equal(3, p.Total);
    }

    [Fact]
    public void PerSubTopic_keyed_by_subtopic_key()
    {
        var g = Guide();
        var read = new HashSet<string> { R(1).Id("advocate"), R(2).Id("advocate") };
        var map = new ProgressService().PerSubTopic(g, read);
        Assert.Equal(new Progress(2, 2), map["advocate"]);
        Assert.Equal(new Progress(0, 1), map["creator"]);
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test --filter ProgressServiceTests`
Expected: FAIL — `Progress` / `ProgressService` not defined.

- [ ] **Step 3: Write minimal implementation**

Create `src/JesusTheChrist.Data/Progress.cs`:

```csharp
using System.Collections.Generic;
using JesusTheChrist.Core.Models;

namespace JesusTheChrist.Data;

public readonly record struct Progress(int Read, int Total)
{
    public double Fraction => Total == 0 ? 0.0 : (double)Read / Total;
}

public sealed class ProgressService
{
    public Progress Overall(TopicalGuide guide, IReadOnlySet<string> readIds)
    {
        int read = 0, total = 0;
        foreach (var st in guide.SubTopics)
            foreach (var r in st.References)
            {
                total++;
                if (readIds.Contains(r.Id(st.Key))) read++;
            }
        return new Progress(read, total);
    }

    public IReadOnlyDictionary<string, Progress> PerSubTopic(
        TopicalGuide guide, IReadOnlySet<string> readIds)
    {
        var map = new Dictionary<string, Progress>();
        foreach (var st in guide.SubTopics)
        {
            int read = 0, total = 0;
            foreach (var r in st.References)
            {
                total++;
                if (readIds.Contains(r.Id(st.Key))) read++;
            }
            map[st.Key] = new Progress(read, total);
        }
        return map;
    }
}
```

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test --filter ProgressServiceTests`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add src/JesusTheChrist.Data/Progress.cs tests/JesusTheChrist.Data.Tests/ProgressServiceTests.cs
git commit -m "feat(data): ProgressService (overall + per-subtopic over TopicalGuide)"
```

---

### Task 7: StreakService (+ persistence)

**Files:** Create `src/JesusTheChrist.Data/Streak.cs`, `tests/.../StreakServiceTests.cs`

- [ ] **Step 1: Write the failing test**

Create `tests/JesusTheChrist.Data.Tests/StreakServiceTests.cs`:

```csharp
using System;
using System.Threading.Tasks;
using JesusTheChrist.Data;
using Xunit;

namespace JesusTheChrist.Data.Tests;

public class StreakServiceTests
{
    static readonly DateOnly Day1 = new(2026, 5, 31);
    static readonly DateOnly Day2 = new(2026, 6, 1);
    static readonly DateOnly Day4 = new(2026, 6, 3);

    [Fact]
    public void First_read_starts_streak_at_one()
    {
        var s = new StreakService().Advance(default, Day1);
        Assert.Equal(new StreakState(1, 1, Day1), s);
    }

    [Fact]
    public void Same_day_does_not_change()
    {
        var svc = new StreakService();
        var s1 = svc.Advance(default, Day1);
        var s2 = svc.Advance(s1, Day1);
        Assert.Equal(s1, s2);
    }

    [Fact]
    public void Consecutive_day_increments_and_tracks_best()
    {
        var svc = new StreakService();
        var s = svc.Advance(svc.Advance(default, Day1), Day2);
        Assert.Equal(new StreakState(2, 2, Day2), s);
    }

    [Fact]
    public void Gap_resets_current_but_keeps_best()
    {
        var svc = new StreakService();
        var twoDay = svc.Advance(svc.Advance(default, Day1), Day2); // best 2
        var afterGap = svc.Advance(twoDay, Day4);                   // skipped Day3
        Assert.Equal(new StreakState(1, 2, Day4), afterGap);
    }

    [Fact]
    public async Task Store_roundtrips_state_via_settings()
    {
        await using var t = await TestDb.CreateAsync();
        var store = new StreakStore(new SettingsStore(t.Db));

        Assert.Equal(default, await store.LoadAsync());
        await store.SaveAsync(new StreakState(3, 5, Day2));
        Assert.Equal(new StreakState(3, 5, Day2), await store.LoadAsync());
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test --filter StreakServiceTests`
Expected: FAIL — `StreakService` / `StreakState` / `StreakStore` not defined.

- [ ] **Step 3: Write minimal implementation**

Create `src/JesusTheChrist.Data/Streak.cs`:

```csharp
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace JesusTheChrist.Data;

public readonly record struct StreakState(int Current, int Best, DateOnly? LastReadDate);

public sealed class StreakService
{
    // "Any reference read in a day counts." today is supplied by the caller (injected clock).
    public StreakState Advance(StreakState prev, DateOnly today)
    {
        if (prev.LastReadDate == today) return prev; // already counted today

        int current = prev.LastReadDate is { } last && last.AddDays(1) == today
            ? prev.Current + 1
            : 1;
        int best = Math.Max(prev.Best, current);
        return new StreakState(current, best, today);
    }
}

// Persists StreakState across the three settings keys.
public sealed class StreakStore
{
    private const string CurrentKey = "streak_current";
    private const string BestKey = "streak_best";
    private const string LastKey = "streak_last_date"; // yyyy-MM-dd or empty

    private readonly SettingsStore _settings;
    public StreakStore(SettingsStore settings) => _settings = settings;

    public async Task<StreakState> LoadAsync()
    {
        var current = await _settings.GetIntAsync(CurrentKey, 0);
        var best = await _settings.GetIntAsync(BestKey, 0);
        var lastRaw = await _settings.GetAsync(LastKey);
        DateOnly? last = DateOnly.TryParseExact(lastRaw, "yyyy-MM-dd",
            CultureInfo.InvariantCulture, DateTimeStyles.None, out var d) ? d : null;
        return new StreakState(current, best, last);
    }

    public async Task SaveAsync(StreakState s)
    {
        await _settings.SetIntAsync(CurrentKey, s.Current);
        await _settings.SetIntAsync(BestKey, s.Best);
        await _settings.SetAsync(LastKey,
            s.LastReadDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "");
    }
}
```

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test --filter StreakServiceTests`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add src/JesusTheChrist.Data/Streak.cs tests/JesusTheChrist.Data.Tests/StreakServiceTests.cs
git commit -m "feat(data): StreakService (pure Advance) + StreakStore persistence"
```

---

### Task 8: Full green run + close the phase

- [ ] **Step 1: Run the whole suite**

Run: `dotnet test`
Expected: all tests PASS (Core + Data).

- [ ] **Step 2: Record outcomes and move the phase doc**

Fill the `## Outcomes` of `planning/in_progress/phase_plan-02-data_2026-05-31.md`, then:

```bash
mkdir -p planning/completed
git mv planning/in_progress/phase_plan-02-data_2026-05-31.md planning/completed/phase_plan-02-data_2026-05-31.md
```

(If `git mv` errors on this WSL2 box, use `mv` + `git add -A`.)

- [ ] **Step 3: Commit (phase close)**

```bash
git add -A
git commit -m "[PHASE] Complete Plan 02: local data layer

- AppDatabase + ReadMarkStore + NoteStore + SettingsStore (sqlite-net-pcl).
- ProgressService (over Core TopicalGuide) + StreakService/Store. All tested.

Phase: planning/completed/phase_plan-02-data_2026-05-31.md"
```

---

## Self-review

- **Spec coverage (§5.2):** SQLite via sqlite-net-pcl ✓ (T1–T2), `ReadMark`/`NoteEntry`/`Setting`
  tables ✓ (T2), read-marks keyed by language-invariant `Reference.Id` ✓ (T3, T6), notes with
  trim/delete-on-empty ✓ (T4), settings for theme/font/language/streak ✓ (T5), derived progress
  overall + per-sub-topic ✓ (T6), streak "any reference that day" + current/best ✓ (T7). Out of
  scope (later plans): the MAUI `IAssetSource`/DB-path wiring, screens, and the canonical-sort
  toggle (backlog).
- **Placeholder scan:** none — every step has runnable code/commands.
- **Type consistency:** `AppDatabase.Connection/InitializeAsync`, `ReadMarkStore(AppDatabase, Func<DateTime>?)`
  with `MarkReadAsync/UnmarkAsync/IsReadAsync/CountAsync/GetReadIdsAsync`, `NoteStore.SaveAsync/GetAsync/DeleteAsync/HasNoteAsync`,
  `SettingsStore.GetAsync/SetAsync/GetIntAsync/SetIntAsync/GetBoolAsync/SetBoolAsync` + `SettingKeys`,
  `Progress(Read,Total)` + `ProgressService.Overall/PerSubTopic`, `StreakState(Current,Best,LastReadDate)`
  + `StreakService.Advance` + `StreakStore.LoadAsync/SaveAsync` are used consistently across tasks.
- **Risks:** package versions (`sqlite-net-pcl` 1.9.x / `SQLitePCLRaw.bundle_e_sqlite3` 2.1.x) may
  need bumping for net10.0 — use the latest if the pinned version fails to restore. The bundle
  supplies the native sqlite3 for the Ubuntu test run.
