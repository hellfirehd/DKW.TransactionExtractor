using DKW.TransactionExtractor.Classification;
using Xunit;

namespace DKW.TransactionExtractor.Tests;

/// <summary>
/// Unit tests for the ExactMatcher class.
/// Verifies exact string matching with case sensitivity options.
/// </summary>
public class ExactMatcherTests
{
    [Fact]
    public void TryMatch_ExactMatchCaseSensitive_ReturnsTrue()
    {
        var matcher = new ExactMatcher(["WALMART"], caseSensitive: true);

        var result = matcher.TryMatch("WALMART");

        Assert.True(result);
    }

    [Fact]
    public void TryMatch_ExactMatchCaseInsensitive_ReturnsTrue()
    {
        var matcher = new ExactMatcher(["walmart"], caseSensitive: false);

        var result = matcher.TryMatch("WALMART");

        Assert.True(result);
    }

    [Fact]
    public void TryMatch_NoMatchCaseSensitive_ReturnsFalse()
    {
        var matcher = new ExactMatcher(["WALMART"], caseSensitive: true);

        var result = matcher.TryMatch("walmart");

        Assert.False(result);
    }

    [Fact]
    public void TryMatch_NoMatchDifferentString_ReturnsFalse()
    {
        var matcher = new ExactMatcher(["WALMART"], caseSensitive: false);

        var result = matcher.TryMatch("TARGET");

        Assert.False(result);
    }

    [Fact]
    public void TryMatch_MultipleValues_MatchesAny()
    {
        var matcher = new ExactMatcher(["WALMART", "TARGET", "COSTCO"], caseSensitive: false);

        Assert.True(matcher.TryMatch("WALMART"));
        Assert.True(matcher.TryMatch("TARGET"));
        Assert.True(matcher.TryMatch("COSTCO"));
        Assert.False(matcher.TryMatch("SAFEWAY"));
    }

    [Fact]
    public void TryMatch_EmptyInput_ReturnsFalse()
    {
        var matcher = new ExactMatcher(["WALMART"], caseSensitive: false);

        Assert.False(matcher.TryMatch(String.Empty));
        Assert.False(matcher.TryMatch("   "));
    }

    [Fact]
    public void TryMatch_NullInput_ReturnsFalse()
    {
        var matcher = new ExactMatcher(["WALMART"], caseSensitive: false);

        Assert.False(matcher.TryMatch(null!));
    }

    [Fact]
    public void TryMatch_PartialMatch_ReturnsFalse()
    {
        var matcher = new ExactMatcher(["WALMART"], caseSensitive: false);

        Assert.False(matcher.TryMatch("WALMART SUPERCENTER"));
        Assert.False(matcher.TryMatch("THE WALMART"));
    }

    [Fact]
    public void Constructor_EmptyValues_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new ExactMatcher([], caseSensitive: false));
    }

    [Fact]
    public void Constructor_NullValues_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new ExactMatcher(null!, caseSensitive: false));
    }

    [Theory]
    [InlineData("MCDONALD'S", "MCDONALD'S", true)]
    [InlineData("MCDONALD'S", "mcdonald's", true)]
    [InlineData("MCDONALD'S", "McDonald's", true)]
    [InlineData("MCDONALD'S", "MCDONALDS", false)]
    public void TryMatch_SpecialCharacters_HandledCorrectly(String pattern, String input, Boolean expected)
    {
        var matcher = new ExactMatcher([pattern], caseSensitive: false);

        var result = matcher.TryMatch(input);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void TryMatch_WhitespaceInValues_MatchesExactly()
    {
        var matcher = new ExactMatcher(["COFFEE  SHOP"], caseSensitive: false);

        Assert.True(matcher.TryMatch("COFFEE  SHOP"));
        Assert.False(matcher.TryMatch("COFFEE SHOP")); // Single space
    }

    [Fact]
    public void TryMatch_CaseSensitiveWithMixedCase_OnlyMatchesExactCase()
    {
        var matcher = new ExactMatcher(["McDonald's"], caseSensitive: true);

        Assert.True(matcher.TryMatch("McDonald's"));
        Assert.False(matcher.TryMatch("MCDONALD'S"));
        Assert.False(matcher.TryMatch("mcdonald's"));
    }
}
