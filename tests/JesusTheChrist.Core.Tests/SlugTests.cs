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
