using DKW.TransactionExtractor.Classification;
using DKW.TransactionExtractor.Models;
using Xunit;

namespace DKW.TransactionExtractor.Tests;

/// <summary>
/// Unit tests for the ExactMatcher class.
/// Verifies exact string matching behavior (case-insensitive only).
/// </summary>
public class ExactMatcherTests
{
    [Fact]
    public void TryMatch_ExactMatch_ReturnsTrue()
    {
        var matcher = new ExactMatcher([new MatcherValue("WALMART", null)]);

        var result = matcher.TryMatch(TestHelpers.CreateTransaction("WALMART", 0m));

        Assert.True(result);
    }

    [Fact]
    public void TryMatch_ExactMatchCaseInsensitive_ReturnsTrue()
    {
        var matcher = new ExactMatcher([new MatcherValue("walmart", null)]);

        var result = matcher.TryMatch(TestHelpers.CreateTransaction("WALMART", 0m));

        Assert.True(result);
    }

    [Fact]
    public void TryMatch_NoMatchDifferentString_ReturnsFalse()
    {
        var matcher = new ExactMatcher([new MatcherValue("WALMART", null)]);

        var result = matcher.TryMatch(TestHelpers.CreateTransaction("TARGET", 0m));

        Assert.False(result);
    }

    [Fact]
    public void TryMatch_MultipleValues_MatchesAny()
    {
        var matcher = new ExactMatcher([new MatcherValue("WALMART", null), new MatcherValue("TARGET", null), new MatcherValue("COSTCO", null)]);

        Assert.True(matcher.TryMatch(TestHelpers.CreateTransaction("WALMART", 0m)));
        Assert.True(matcher.TryMatch(TestHelpers.CreateTransaction("TARGET", 0m)));
        Assert.True(matcher.TryMatch(TestHelpers.CreateTransaction("COSTCO", 0m)));
        Assert.False(matcher.TryMatch(TestHelpers.CreateTransaction("SAFEWAY", 0m)));
    }

    [Fact]
    public void TryMatch_EmptyInput_ReturnsFalse()
    {
        var matcher = new ExactMatcher([new MatcherValue("WALMART", null)]);

        Assert.False(matcher.TryMatch(TestHelpers.CreateTransaction(String.Empty, 0m)));
        Assert.False(matcher.TryMatch(TestHelpers.CreateTransaction("   ", 0m)));
    }

    [Fact]
    public void TryMatch_NullInput_ReturnsFalse()
    {
        var matcher = new ExactMatcher([new MatcherValue("WALMART", null)]);

        Assert.False(matcher.TryMatch(null!));
    }

    [Fact]
    public void TryMatch_PartialMatch_ReturnsFalse()
    {
        var matcher = new ExactMatcher([new MatcherValue("WALMART", null)]);

        Assert.False(matcher.TryMatch(TestHelpers.CreateTransaction("WALMART SUPERCENTER", 0m)));
        Assert.False(matcher.TryMatch(TestHelpers.CreateTransaction("THE WALMART", 0m)));
    }

    [Fact]
    public void Constructor_EmptyValues_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new ExactMatcher([]));
    }

    [Fact]
    public void Constructor_NullValues_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new ExactMatcher(null!));
    }

    [Theory]
    [InlineData("MCDONALD'S", "MCDONALD'S", true)]
    [InlineData("MCDONALD'S", "mcdonald's", true)]
    [InlineData("MCDONALD'S", "McDonald's", true)]
    [InlineData("MCDONALD'S", "MCDONALDS", false)]
    public void TryMatch_SpecialCharacters_HandledCorrectly(String pattern, String input, Boolean expected)
    {
        var matcher = new ExactMatcher([new MatcherValue(pattern, null)]);

        var result = matcher.TryMatch(TestHelpers.CreateTransaction(input, 0m));

        Assert.Equal(expected, result);
    }

    [Fact]
    public void TryMatch_WhitespaceInValues_MatchesExactly()
    {
        var matcher = new ExactMatcher([new MatcherValue("COFFEE  SHOP", null)]);

        Assert.True(matcher.TryMatch(TestHelpers.CreateTransaction("COFFEE  SHOP", 0m)));
        Assert.False(matcher.TryMatch(TestHelpers.CreateTransaction("COFFEE SHOP", 0m))); // Single space
    }
}
