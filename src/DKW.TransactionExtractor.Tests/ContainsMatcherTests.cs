using DKW.TransactionExtractor.Classification;
using Xunit;

namespace DKW.TransactionExtractor.Tests;

/// <summary>
/// Unit tests for the ContainsMatcher class.
/// Verifies substring matching with case sensitivity options.
/// </summary>
public class ContainsMatcherTests
{
    [Fact]
    public void TryMatch_SubstringMatchCaseInsensitive_ReturnsTrue()
    {
        var matcher = new ContainsMatcher(["WALMART"], caseSensitive: false);

        Assert.True(matcher.TryMatch("WALMART SUPERCENTER #1234"));
        Assert.True(matcher.TryMatch("walmart"));
        Assert.True(matcher.TryMatch("Shopping at WALMART today"));
    }

    [Fact]
    public void TryMatch_SubstringMatchCaseSensitive_OnlyMatchesCorrectCase()
    {
        var matcher = new ContainsMatcher(["WALMART"], caseSensitive: true);

        Assert.True(matcher.TryMatch("WALMART SUPERCENTER"));
        Assert.False(matcher.TryMatch("walmart supercenter"));
        Assert.False(matcher.TryMatch("Walmart Supercenter"));
    }

    [Fact]
    public void TryMatch_NoMatch_ReturnsFalse()
    {
        var matcher = new ContainsMatcher(["WALMART"], caseSensitive: false);

        Assert.False(matcher.TryMatch("TARGET STORE"));
        Assert.False(matcher.TryMatch("COSTCO WHOLESALE"));
    }

    [Fact]
    public void TryMatch_MultipleValues_MatchesAny()
    {
        var matcher = new ContainsMatcher(["WALMART", "TARGET", "COSTCO"], caseSensitive: false);

        Assert.True(matcher.TryMatch("WALMART #1234"));
        Assert.True(matcher.TryMatch("Shopping at TARGET"));
        Assert.True(matcher.TryMatch("COSTCO WHOLESALE"));
        Assert.False(matcher.TryMatch("SAFEWAY GROCERY"));
    }

    [Fact]
    public void TryMatch_EmptyInput_ReturnsFalse()
    {
        var matcher = new ContainsMatcher(["WALMART"], caseSensitive: false);

        Assert.False(matcher.TryMatch(String.Empty));
        Assert.False(matcher.TryMatch("   "));
    }

    [Fact]
    public void TryMatch_NullInput_ReturnsFalse()
    {
        var matcher = new ContainsMatcher(["WALMART"], caseSensitive: false);

        Assert.False(matcher.TryMatch(null!));
    }

    [Fact]
    public void TryMatch_PartialWordMatch_ReturnsTrue()
    {
        var matcher = new ContainsMatcher(["MART"], caseSensitive: false);

        Assert.True(matcher.TryMatch("WALMART"));
        Assert.True(matcher.TryMatch("KMART"));
        Assert.True(matcher.TryMatch("SMART & FINAL"));
    }

    [Fact]
    public void Constructor_EmptyValues_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new ContainsMatcher([], caseSensitive: false));
    }

    [Fact]
    public void Constructor_NullValues_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new ContainsMatcher(null!, caseSensitive: false));
    }

    [Theory]
    [InlineData("MCDONALD", "MCDONALD'S #40610", true)]
    [InlineData("COFFEE", "COFFEE SHOP DOWNTOWN", true)]
    [InlineData("PIZZA", "DOMINO'S PIZZA", true)]
    [InlineData("BURGER", "FIVE GUYS BURGERS", true)]
    [InlineData("SUSHI", "PIZZA HUT", false)]
    public void TryMatch_CommonScenarios_WorksCorrectly(String pattern, String input, Boolean expected)
    {
        var matcher = new ContainsMatcher([pattern], caseSensitive: false);

        var result = matcher.TryMatch(input);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void TryMatch_SpecialCharactersInPattern_MatchesLiterally()
    {
        var matcher = new ContainsMatcher(["MCDONALD'S"], caseSensitive: false);

        Assert.True(matcher.TryMatch("MCDONALD'S #40610"));
        Assert.False(matcher.TryMatch("MCDONALDS")); // No apostrophe
    }

    [Fact]
    public void TryMatch_NumbersInPattern_MatchesCorrectly()
    {
        var matcher = new ContainsMatcher(["7-ELEVEN", "7-11"], caseSensitive: false);

        Assert.True(matcher.TryMatch("7-ELEVEN STORE #1234"));
        Assert.True(matcher.TryMatch("7-11"));
        Assert.False(matcher.TryMatch("SEVEN ELEVEN"));
    }

    [Fact]
    public void TryMatch_WhitespaceInPattern_MatchesExactWhitespace()
    {
        var matcher = new ContainsMatcher(["COFFEE  SHOP"], caseSensitive: false);

        Assert.True(matcher.TryMatch("THE COFFEE  SHOP DOWNTOWN"));
        Assert.False(matcher.TryMatch("THE COFFEE SHOP DOWNTOWN")); // Single space
    }

    [Fact]
    public void TryMatch_BeginningOfString_MatchesCorrectly()
    {
        var matcher = new ContainsMatcher(["WALMART"], caseSensitive: false);

        Assert.True(matcher.TryMatch("WALMART"));
        Assert.True(matcher.TryMatch("WALMART SUPERCENTER"));
    }

    [Fact]
    public void TryMatch_EndOfString_MatchesCorrectly()
    {
        var matcher = new ContainsMatcher(["WALMART"], caseSensitive: false);

        Assert.True(matcher.TryMatch("SHOPPING AT WALMART"));
        Assert.True(matcher.TryMatch("WALMART"));
    }

    [Fact]
    public void TryMatch_MiddleOfString_MatchesCorrectly()
    {
        var matcher = new ContainsMatcher(["WALMART"], caseSensitive: false);

        Assert.True(matcher.TryMatch("THE WALMART STORE"));
    }
}
