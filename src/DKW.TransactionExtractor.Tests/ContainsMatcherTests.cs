using System;
using DKW.TransactionExtractor.Classification;
using DKW.TransactionExtractor.Models;
using Xunit;

namespace DKW.TransactionExtractor.Tests;

/// <summary>
/// Unit tests for the ContainsMatcher class.
/// Verifies substring matching (case-insensitive only).
/// </summary>
public class ContainsMatcherTests
{
    private static Transaction CreateTransaction(String description, Decimal amount)
    {
        return TestHelpers.CreateTransaction(description, amount);
    }

    [Fact]
    public void TryMatch_SubstringMatchCaseInsensitive_ReturnsTrue()
    {
        var matcher = new ContainsMatcher(new[] { new MatcherValue("WALMART", null) });

        Assert.True(matcher.TryMatch(CreateTransaction("WALMART SUPERCENTER #1234", 0m)));
        Assert.True(matcher.TryMatch(CreateTransaction("walmart", 0m)));
        Assert.True(matcher.TryMatch(CreateTransaction("Shopping at WALMART today", 0m)));
    }

    [Fact]
    public void TryMatch_NoMatch_ReturnsFalse()
    {
        var matcher = new ContainsMatcher(new[] { new MatcherValue("WALMART", null) });

        Assert.False(matcher.TryMatch(CreateTransaction("TARGET STORE", 0m)));
        Assert.False(matcher.TryMatch(CreateTransaction("COSTCO WHOLESALE", 0m)));
    }

    [Fact]
    public void TryMatch_MultipleValues_MatchesAny()
    {
        var matcher = new ContainsMatcher(new[] { new MatcherValue("WALMART", null), new MatcherValue("TARGET", null), new MatcherValue("COSTCO", null) });

        Assert.True(matcher.TryMatch(CreateTransaction("WALMART #1234", 0m)));
        Assert.True(matcher.TryMatch(CreateTransaction("Shopping at TARGET", 0m)));
        Assert.True(matcher.TryMatch(CreateTransaction("COSTCO WHOLESALE", 0m)));
        Assert.False(matcher.TryMatch(CreateTransaction("SAFEWAY GROCERY", 0m)));
    }

    [Fact]
    public void TryMatch_EmptyInput_ReturnsFalse()
    {
        var matcher = new ContainsMatcher(new[] { new MatcherValue("WALMART", null) });

        Assert.False(matcher.TryMatch(CreateTransaction(String.Empty, 0m)));
        Assert.False(matcher.TryMatch(CreateTransaction("   ", 0m)));
    }

    [Fact]
    public void TryMatch_NullInput_ReturnsFalse()
    {
        var matcher = new ContainsMatcher(new[] { new MatcherValue("WALMART", null) });

        Assert.False(matcher.TryMatch(null!));
    }

    [Fact]
    public void TryMatch_PartialWordMatch_ReturnsTrue()
    {
        var matcher = new ContainsMatcher(new[] { new MatcherValue("MART", null) });

        Assert.True(matcher.TryMatch(CreateTransaction("WALMART", 0m)));
        Assert.True(matcher.TryMatch(CreateTransaction("KMART", 0m)));
        Assert.True(matcher.TryMatch(CreateTransaction("SMART & FINAL", 0m)));
    }

    [Fact]
    public void Constructor_EmptyValues_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new ContainsMatcher(Array.Empty<MatcherValue>()));
    }

    [Fact]
    public void Constructor_NullValues_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new ContainsMatcher(null!));
    }

    [Theory]
    [InlineData("MCDONALD", "MCDONALD'S #40610", true)]
    [InlineData("COFFEE", "COFFEE SHOP DOWNTOWN", true)]
    [InlineData("PIZZA", "DOMINO'S PIZZA", true)]
    [InlineData("BURGER", "FIVE GUYS BURGERS", true)]
    [InlineData("SUSHI", "PIZZA HUT", false)]
    public void TryMatch_CommonScenarios_WorksCorrectly(String pattern, String input, Boolean expected)
    {
        var matcher = new ContainsMatcher(new[] { new MatcherValue(pattern, null) });

        var result = matcher.TryMatch(CreateTransaction(input, 0m));

        Assert.Equal(expected, result);
    }

    [Fact]
    public void TryMatch_SpecialCharactersInPattern_MatchesLiterally()
    {
        var matcher = new ContainsMatcher(new[] { new MatcherValue("MCDONALD'S", null) });

        Assert.True(matcher.TryMatch(CreateTransaction("MCDONALD'S #40610", 0m)));
        Assert.False(matcher.TryMatch(CreateTransaction("MCDONALDS", 0m))); // No apostrophe
    }

    [Fact]
    public void TryMatch_NumbersInPattern_MatchesCorrectly()
    {
        var matcher = new ContainsMatcher(new[] { new MatcherValue("7-ELEVEN", null), new MatcherValue("7-11", null) });

        Assert.True(matcher.TryMatch(CreateTransaction("7-ELEVEN STORE #1234", 0m)));
        Assert.True(matcher.TryMatch(CreateTransaction("7-11", 0m)));
        Assert.False(matcher.TryMatch(CreateTransaction("SEVEN ELEVEN", 0m)));
    }

    [Fact]
    public void TryMatch_WhitespaceInPattern_MatchesExactWhitespace()
    {
        var matcher = new ContainsMatcher(new[] { new MatcherValue("COFFEE  SHOP", null) });

        Assert.True(matcher.TryMatch(CreateTransaction("THE COFFEE  SHOP DOWNTOWN", 0m)));
        Assert.False(matcher.TryMatch(CreateTransaction("THE COFFEE SHOP DOWNTOWN", 0m))); // Single space
    }

    [Fact]
    public void TryMatch_BeginningOfString_MatchesCorrectly()
    {
        var matcher = new ContainsMatcher(new[] { new MatcherValue("WALMART", null) });

        Assert.True(matcher.TryMatch(CreateTransaction("WALMART", 0m)));
        Assert.True(matcher.TryMatch(CreateTransaction("WALMART SUPERCENTER", 0m)));
    }

    [Fact]
    public void TryMatch_EndOfString_MatchesCorrectly()
    {
        var matcher = new ContainsMatcher(new[] { new MatcherValue("WALMART", null) });

        Assert.True(matcher.TryMatch(CreateTransaction("SHOPPING AT WALMART", 0m)));
        Assert.True(matcher.TryMatch(CreateTransaction("WALMART", 0m)));
    }

    [Fact]
    public void TryMatch_MiddleOfString_MatchesCorrectly()
    {
        var matcher = new ContainsMatcher(new[] { new MatcherValue("WALMART", null) });

        Assert.True(matcher.TryMatch(CreateTransaction("THE WALMART STORE", 0m)));
    }
}
