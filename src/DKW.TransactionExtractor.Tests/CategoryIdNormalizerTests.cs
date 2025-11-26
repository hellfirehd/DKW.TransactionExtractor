using DKW.TransactionExtractor.Classification;
using Xunit;

namespace DKW.TransactionExtractor.Tests;

/// <summary>
/// Tests for CategoryIdNormalizer to ensure consistent category ID formatting.
/// </summary>
public class CategoryIdNormalizerTests
{
    [Theory]
    [InlineData("Vacation & Travel", "vacation-travel")]
    [InlineData("vacation-&-travel", "vacation-travel")]
    [InlineData("UPPERCASE", "uppercase")]
    [InlineData("Mixed Case Name", "mixed-case-name")]
    [InlineData("Special!@#$%Characters", "special-characters")]
    [InlineData("multiple   spaces", "multiple-spaces")]
    [InlineData("--leading-trailing--", "leading-trailing")]
    [InlineData("consecutive---hyphens", "consecutive-hyphens")]
    [InlineData("unicode-\u0026-ampersand", "unicode-ampersand")]
    [InlineData("café-montréal", "caf-montr-al")]
    [InlineData("123-numbers-456", "123-numbers-456")]
    [InlineData("mixed123ABC", "mixed123abc")]
    public void Normalize_VariousInputs_ReturnsExpectedOutput(String input, String expected)
    {
        var result = CategoryIdNormalizer.Normalize(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Normalize_EmptyOrWhitespace_ReturnsEmpty(String input)
    {
        var result = CategoryIdNormalizer.Normalize(input);
        Assert.Equal(String.Empty, result);
    }

    [Theory]
    [InlineData("simple", "simple")]
    [InlineData("already-normalized", "already-normalized")]
    [InlineData("test123", "test123")]
    public void Normalize_AlreadyNormalizedInput_RemainsUnchanged(String input, String expected)
    {
        var result = CategoryIdNormalizer.Normalize(input);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Normalize_OnlySpecialCharacters_ReturnsEmpty()
    {
        var result = CategoryIdNormalizer.Normalize("!@#$%^&*()");
        Assert.Equal(String.Empty, result);
    }

    [Fact]
    public void Normalize_OnlyHyphens_ReturnsEmpty()
    {
        var result = CategoryIdNormalizer.Normalize("---");
        Assert.Equal(String.Empty, result);
    }

    [Theory]
    [InlineData("Test_With_Underscores", "test-with-underscores")]
    [InlineData("Period.Separated.Words", "period-separated-words")]
    [InlineData("Slash/Separated/Words", "slash-separated-words")]
    [InlineData("Plus+Sign+Words", "plus-sign-words")]
    public void Normalize_VariousSeparators_ConvertedToHyphens(String input, String expected)
    {
        var result = CategoryIdNormalizer.Normalize(input);
        Assert.Equal(expected, result);
    }
}
