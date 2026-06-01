using System.Threading.Tasks;
using JesusTheChrist.Data;
using Xunit;

namespace JesusTheChrist.Data.Tests;

public class AppDatabaseTests
{
    [Fact]
    public async Task Initialize_creates_all_three_tables_and_roundtrips()
    {
        await using var t = await TestDb.CreateAsync();
        var c = t.Db.Connection;

        await c.InsertAsync(new ReadMark { RefId = "r", ReadAtUtc = System.DateTime.UtcNow });
        await c.InsertAsync(new NoteEntry { RefId = "n", Text = "t", UpdatedAtUtc = System.DateTime.UtcNow });
        await c.InsertAsync(new Setting { Key = "k", Value = "v" });

        Assert.NotNull(await c.FindAsync<ReadMark>("r"));
        Assert.NotNull(await c.FindAsync<NoteEntry>("n"));
        Assert.Equal("v", (await c.FindAsync<Setting>("k"))!.Value);
    }
}
