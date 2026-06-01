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
