using DKW.TransactionExtractor.Classification;
using DKW.TransactionExtractor.Models;
using Xunit;

namespace DKW.TransactionExtractor.Tests;

/// <summary>
/// Unit tests for the RegexMatcher class.
/// Verifies regex pattern matching with case sensitivity options.
/// </summary>
public class RegexMatcherTests
{
    [Fact]
    public void TryMatch_SimplePattern_MatchesCorrectly()
    {
        var matcher = new RegexMatcher("^WALMART", ignoreCase: true);

        Assert.True(matcher.TryMatch(new Transaction { Description = "WALMART SUPERCENTER", Amount = 0m }));
        Assert.True(matcher.TryMatch(new Transaction { Description = "walmart store", Amount = 0m }));
        Assert.False(matcher.TryMatch(new Transaction { Description = "THE WALMART", Amount = 0m }));
    }

    [Fact]
    public void TryMatch_WildcardPattern_MatchesCorrectly()
    {
        var matcher = new RegexMatcher("WALMART.*#\\d+", ignoreCase: true);

        Assert.True(matcher.TryMatch(new Transaction { Description = "WALMART SUPERCENTER #1234", Amount = 0m }));
        Assert.True(matcher.TryMatch(new Transaction { Description = "WALMART #5678", Amount = 0m }));
        Assert.False(matcher.TryMatch(new Transaction { Description = "WALMART STORE", Amount = 0m }));
    }

    [Fact]
    public void TryMatch_AlternationPattern_MatchesEither()
    {
        var matcher = new RegexMatcher("(WALMART|TARGET|COSTCO)", ignoreCase: true);

        Assert.True(matcher.TryMatch(new Transaction { Description = "WALMART #1234", Amount = 0m }));
        Assert.True(matcher.TryMatch(new Transaction { Description = "TARGET STORE", Amount = 0m }));
        Assert.True(matcher.TryMatch(new Transaction { Description = "COSTCO WHOLESALE", Amount = 0m }));
        Assert.False(matcher.TryMatch(new Transaction { Description = "SAFEWAY GROCERY", Amount = 0m }));
    }

    [Fact]
    public void TryMatch_CaseSensitive_OnlyMatchesCorrectCase()
    {
        var matcher = new RegexMatcher("^WALMART", ignoreCase: false);

        Assert.True(matcher.TryMatch(new Transaction { Description = "WALMART SUPERCENTER", Amount = 0m }));
        Assert.False(matcher.TryMatch(new Transaction { Description = "walmart supercenter", Amount = 0m }));
        Assert.False(matcher.TryMatch(new Transaction { Description = "Walmart Supercenter", Amount = 0m }));
    }

    [Fact]
    public void TryMatch_CaseInsensitive_MatchesAnyCase()
    {
        var matcher = new RegexMatcher("^walmart", ignoreCase: true);

        Assert.True(matcher.TryMatch(new Transaction { Description = "WALMART SUPERCENTER", Amount = 0m }));
        Assert.True(matcher.TryMatch(new Transaction { Description = "walmart supercenter", Amount = 0m }));
        Assert.True(matcher.TryMatch(new Transaction { Description = "Walmart Supercenter", Amount = 0m }));
    }

    [Fact]
    public void TryMatch_EmptyInput_ReturnsFalse()
    {
        var matcher = new RegexMatcher("WALMART", ignoreCase: true);

        Assert.False(matcher.TryMatch(new Transaction { Description = String.Empty, Amount = 0m }));
        Assert.False(matcher.TryMatch(new Transaction { Description = "   ", Amount = 0m }));
    }

    [Fact]
    public void TryMatch_NullInput_ReturnsFalse()
    {
        var matcher = new RegexMatcher("WALMART", ignoreCase: true);

        Assert.False(matcher.TryMatch(null!));
    }

    [Fact]
    public void Constructor_InvalidRegexPattern_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new RegexMatcher("[invalid(regex", ignoreCase: false));
    }

    [Fact]
    public void Constructor_NullPattern_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new RegexMatcher(null!, ignoreCase: false));
    }

    [Fact]
    public void Constructor_EmptyPattern_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new RegexMatcher(String.Empty, ignoreCase: false));
    }

    [Theory]
    [InlineData("^MCDONALD'S #\\d+", "MCDONALD'S #40610", true)]
    [InlineData("^MCDONALD'S #\\d+", "WENDY'S #1234", false)]
    [InlineData("COFFEE.*SHOP", "COFFEE BEAN & TEA LEAF SHOP", true)]
    [InlineData("COFFEE.*SHOP", "STARBUCKS COFFEE", false)]
    [InlineData("\\d{3}-\\d{4}", "PHONE: 555-1234", true)]
    [InlineData("\\d{3}-\\d{4}", "PHONE: 5551234", false)]
    public void TryMatch_VariousPatterns_WorkCorrectly(String pattern, String input, Boolean expected)
    {
        var matcher = new RegexMatcher(pattern, ignoreCase: true);

        var result = matcher.TryMatch(new Transaction { Description = input, Amount = 0m });

        Assert.Equal(expected, result);
    }

    [Fact]
    public void TryMatch_StoreNumberPattern_ExtractsCorrectly()
    {
        var matcher = new RegexMatcher("#\\d{4,5}", ignoreCase: true);

        Assert.True(matcher.TryMatch(new Transaction { Description = "WALMART #1234", Amount = 0m }));
        Assert.True(matcher.TryMatch(new Transaction { Description = "TARGET #56789", Amount = 0m }));
        Assert.False(matcher.TryMatch(new Transaction { Description = "COSTCO #123", Amount = 0m })); // Only 3 digits
        Assert.False(matcher.TryMatch(new Transaction { Description = "STORE NUMBER 1234", Amount = 0m })); // No # symbol
    }

    [Fact]
    public void TryMatch_DatePattern_MatchesCorrectly()
    {
        var matcher = new RegexMatcher("\\d{2}/\\d{2}/\\d{4}", ignoreCase: true);

        Assert.True(matcher.TryMatch(new Transaction { Description = "PAYMENT ON 10/15/2024", Amount = 0m }));
        Assert.True(matcher.TryMatch(new Transaction { Description = "12/31/2024", Amount = 0m }));
        Assert.False(matcher.TryMatch(new Transaction { Description = "2024-10-15", Amount = 0m })); // Wrong format
    }

    [Fact]
    public void TryMatch_EmailPattern_MatchesCorrectly()
    {
        var matcher = new RegexMatcher("[a-z0-9._%+-]+@[a-z0-9.-]+\\.[a-z]{2,}", ignoreCase: true);

        Assert.True(matcher.TryMatch(new Transaction { Description = "Contact: user@example.com", Amount = 0m }));
        Assert.True(matcher.TryMatch(new Transaction { Description = "test.email+tag@domain.co.uk", Amount = 0m }));
        Assert.False(matcher.TryMatch(new Transaction { Description = "not-an-email", Amount = 0m }));
    }

    [Fact]
    public void TryMatch_WordBoundaries_RespectBoundaries()
    {
        var matcher = new RegexMatcher("\\bWALMART\\b", ignoreCase: true);

        Assert.True(matcher.TryMatch(new Transaction { Description = "WALMART", Amount = 0m }));
        Assert.True(matcher.TryMatch(new Transaction { Description = "THE WALMART STORE", Amount = 0m }));
        Assert.False(matcher.TryMatch(new Transaction { Description = "WALMARTSTORE", Amount = 0m })); // No boundary
    }

    [Fact]
    public void TryMatch_AnchoredPattern_MatchesCompleteString()
    {
        var matcher = new RegexMatcher("^WALMART #\\d+$", ignoreCase: true);

        Assert.True(matcher.TryMatch(new Transaction { Description = "WALMART #1234", Amount = 0m }));
        Assert.False(matcher.TryMatch(new Transaction { Description = "WALMART #1234 SUPERCENTER", Amount = 0m })); // Extra text
        Assert.False(matcher.TryMatch(new Transaction { Description = "THE WALMART #1234", Amount = 0m })); // Prefix text
    }

    [Fact]
    public void TryMatch_NegativeLookahead_ExcludesPattern()
    {
        // Match WALMART but not WALMART NEIGHBORHOOD MARKET
        var matcher = new RegexMatcher("WALMART(?! NEIGHBORHOOD)", ignoreCase: true);

        Assert.True(matcher.TryMatch(new Transaction { Description = "WALMART SUPERCENTER", Amount = 0m }));
        Assert.True(matcher.TryMatch(new Transaction { Description = "WALMART", Amount = 0m }));
        Assert.False(matcher.TryMatch(new Transaction { Description = "WALMART NEIGHBORHOOD MARKET", Amount = 0m }));
    }

    [Fact]
    public void TryMatch_GroupCapture_MatchesCorrectly()
    {
        var matcher = new RegexMatcher("(VISA|MASTERCARD|AMEX) \\d{4}", ignoreCase: true);

        Assert.True(matcher.TryMatch(new Transaction { Description = "VISA 1234", Amount = 0m }));
        Assert.True(matcher.TryMatch(new Transaction { Description = "MASTERCARD 5678", Amount = 0m }));
        Assert.True(matcher.TryMatch(new Transaction { Description = "AMEX 9012", Amount = 0m }));
        Assert.False(matcher.TryMatch(new Transaction { Description = "DISCOVER 1234", Amount = 0m }));
    }

    [Fact]
    public void TryMatch_SpecialRegexCharacters_EscapedCorrectly()
    {
        // Pattern with literal special characters
        var matcher = new RegexMatcher("PRICE: \\$\\d+\\.\\d{2}", ignoreCase: true);

        Assert.True(matcher.TryMatch(new Transaction { Description = "PRICE: $25.99", Amount = 0m }));
        Assert.True(matcher.TryMatch(new Transaction { Description = "TOTAL PRICE: $100.00", Amount = 0m }));
        Assert.False(matcher.TryMatch(new Transaction { Description = "PRICE: 25.99", Amount = 0m })); // No dollar sign
    }

    [Fact]
    public void TryMatch_OptionalGroups_MatchesWithOrWithout()
    {
        var matcher = new RegexMatcher("MCDONALD'?S", ignoreCase: true);

        Assert.True(matcher.TryMatch(new Transaction { Description = "MCDONALD'S", Amount = 0m }));
        Assert.True(matcher.TryMatch(new Transaction { Description = "MCDONALDS", Amount = 0m })); // No apostrophe
    }

    [Fact]
    public void TryMatch_WithAmount_MatchesOnlyWhenAmountAndPatternMatch()
    {
        var matcher = new RegexMatcher("^WALMART #\\d+", ignoreCase: true, amount: 50.00m);

        // Matching description and exact amount
        Assert.True(matcher.TryMatch(new Transaction { Description = "WALMART #1234", Amount = 50.00m }));

        // Matching description but different amount -> should not match
        Assert.False(matcher.TryMatch(new Transaction { Description = "WALMART #1234", Amount = 49.99m }));

        // Non-matching description even if amount matches -> should not match
        Assert.False(matcher.TryMatch(new Transaction { Description = "TARGET #1234", Amount = 50.00m }));
    }

    [Fact]
    public void TryMatch_WithAmount_RoundsAmountsToTwoDecimals()
    {
        var matcher = new RegexMatcher("^WALMART #\\d+", ignoreCase: true, amount: 12.3456m);

        // Matcher amount rounds to 12.35; transaction amount 12.3456 rounds to 12.35 -> match
        Assert.True(matcher.TryMatch(new Transaction { Description = "WALMART #1", Amount = 12.3456m }));

        // Different rounded amount -> no match
        Assert.False(matcher.TryMatch(new Transaction { Description = "WALMART #1", Amount = 12.344m }));
    }
}
