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
