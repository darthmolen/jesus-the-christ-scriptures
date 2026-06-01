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
