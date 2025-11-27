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

        Assert.True(matcher.TryMatch("WALMART SUPERCENTER"));
        Assert.True(matcher.TryMatch("walmart store"));
        Assert.False(matcher.TryMatch("THE WALMART"));
    }

    [Fact]
    public void TryMatch_WildcardPattern_MatchesCorrectly()
    {
        var matcher = new RegexMatcher("WALMART.*#\\d+", ignoreCase: true);

        Assert.True(matcher.TryMatch("WALMART SUPERCENTER #1234"));
        Assert.True(matcher.TryMatch("WALMART #5678"));
        Assert.False(matcher.TryMatch("WALMART STORE"));
    }

    [Fact]
    public void TryMatch_AlternationPattern_MatchesEither()
    {
        var matcher = new RegexMatcher("(WALMART|TARGET|COSTCO)", ignoreCase: true);

        Assert.True(matcher.TryMatch("WALMART #1234"));
        Assert.True(matcher.TryMatch("TARGET STORE"));
        Assert.True(matcher.TryMatch("COSTCO WHOLESALE"));
        Assert.False(matcher.TryMatch("SAFEWAY GROCERY"));
    }

    [Fact]
    public void TryMatch_CaseSensitive_OnlyMatchesCorrectCase()
    {
        var matcher = new RegexMatcher("^WALMART", ignoreCase: false);

        Assert.True(matcher.TryMatch("WALMART SUPERCENTER"));
        Assert.False(matcher.TryMatch("walmart supercenter"));
        Assert.False(matcher.TryMatch("Walmart Supercenter"));
    }

    [Fact]
    public void TryMatch_CaseInsensitive_MatchesAnyCase()
    {
        var matcher = new RegexMatcher("^walmart", ignoreCase: true);

        Assert.True(matcher.TryMatch("WALMART SUPERCENTER"));
        Assert.True(matcher.TryMatch("walmart supercenter"));
        Assert.True(matcher.TryMatch("Walmart Supercenter"));
    }

    [Fact]
    public void TryMatch_EmptyInput_ReturnsFalse()
    {
        var matcher = new RegexMatcher("WALMART", ignoreCase: true);

        Assert.False(matcher.TryMatch(String.Empty));
        Assert.False(matcher.TryMatch("   "));
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

        var result = matcher.TryMatch(input);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void TryMatch_StoreNumberPattern_ExtractsCorrectly()
    {
        var matcher = new RegexMatcher("#\\d{4,5}", ignoreCase: true);

        Assert.True(matcher.TryMatch("WALMART #1234"));
        Assert.True(matcher.TryMatch("TARGET #56789"));
        Assert.False(matcher.TryMatch("COSTCO #123")); // Only 3 digits
        Assert.False(matcher.TryMatch("STORE NUMBER 1234")); // No # symbol
    }

    [Fact]
    public void TryMatch_DatePattern_MatchesCorrectly()
    {
        var matcher = new RegexMatcher("\\d{2}/\\d{2}/\\d{4}", ignoreCase: true);

        Assert.True(matcher.TryMatch("PAYMENT ON 10/15/2024"));
        Assert.True(matcher.TryMatch("12/31/2024"));
        Assert.False(matcher.TryMatch("2024-10-15")); // Wrong format
    }

    [Fact]
    public void TryMatch_EmailPattern_MatchesCorrectly()
    {
        var matcher = new RegexMatcher("[a-z0-9._%+-]+@[a-z0-9.-]+\\.[a-z]{2,}", ignoreCase: true);

        Assert.True(matcher.TryMatch("Contact: user@example.com"));
        Assert.True(matcher.TryMatch("test.email+tag@domain.co.uk"));
        Assert.False(matcher.TryMatch("not-an-email"));
    }

    [Fact]
    public void TryMatch_WordBoundaries_RespectBoundaries()
    {
        var matcher = new RegexMatcher("\\bWALMART\\b", ignoreCase: true);

        Assert.True(matcher.TryMatch("WALMART"));
        Assert.True(matcher.TryMatch("THE WALMART STORE"));
        Assert.False(matcher.TryMatch("WALMARTSTORE")); // No boundary
    }

    [Fact]
    public void TryMatch_AnchoredPattern_MatchesCompleteString()
    {
        var matcher = new RegexMatcher("^WALMART #\\d+$", ignoreCase: true);

        Assert.True(matcher.TryMatch("WALMART #1234"));
        Assert.False(matcher.TryMatch("WALMART #1234 SUPERCENTER")); // Extra text
        Assert.False(matcher.TryMatch("THE WALMART #1234")); // Prefix text
    }

    [Fact]
    public void TryMatch_NegativeLookahead_ExcludesPattern()
    {
        // Match WALMART but not WALMART NEIGHBORHOOD MARKET
        var matcher = new RegexMatcher("WALMART(?! NEIGHBORHOOD)", ignoreCase: true);

        Assert.True(matcher.TryMatch("WALMART SUPERCENTER"));
        Assert.True(matcher.TryMatch("WALMART"));
        Assert.False(matcher.TryMatch("WALMART NEIGHBORHOOD MARKET"));
    }

    [Fact]
    public void TryMatch_GroupCapture_MatchesCorrectly()
    {
        var matcher = new RegexMatcher("(VISA|MASTERCARD|AMEX) \\d{4}", ignoreCase: true);

        Assert.True(matcher.TryMatch("VISA 1234"));
        Assert.True(matcher.TryMatch("MASTERCARD 5678"));
        Assert.True(matcher.TryMatch("AMEX 9012"));
        Assert.False(matcher.TryMatch("DISCOVER 1234"));
    }

    [Fact]
    public void TryMatch_SpecialRegexCharacters_EscapedCorrectly()
    {
        // Pattern with literal special characters
        var matcher = new RegexMatcher("PRICE: \\$\\d+\\.\\d{2}", ignoreCase: true);

        Assert.True(matcher.TryMatch("PRICE: $25.99"));
        Assert.True(matcher.TryMatch("TOTAL PRICE: $100.00"));
        Assert.False(matcher.TryMatch("PRICE: 25.99")); // No dollar sign
    }

    [Fact]
    public void TryMatch_OptionalGroups_MatchesWithOrWithout()
    {
        var matcher = new RegexMatcher("MCDONALD'?S", ignoreCase: true);

        Assert.True(matcher.TryMatch("MCDONALD'S"));
        Assert.True(matcher.TryMatch("MCDONALDS")); // No apostrophe
    }
}
