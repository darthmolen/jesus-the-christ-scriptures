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
