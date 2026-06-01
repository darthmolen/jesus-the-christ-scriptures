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

    public static int Order(this Volume v) => (int)v;
}
