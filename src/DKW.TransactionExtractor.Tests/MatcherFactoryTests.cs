using DKW.TransactionExtractor.Classification;
using DKW.TransactionExtractor.Models;
using System.Text.Json;
using Xunit;

namespace DKW.TransactionExtractor.Tests;

/// <summary>
/// Unit tests for the MatcherFactory class.
/// Verifies creation of matchers from JSON configuration.
/// </summary>
public class MatcherFactoryTests
{
    [Fact]
    public void CreateMatcher_ExactMatchType_CreatesExactMatcher()
    {
        var matcherConfig = new CategoryMatcher
        {
            Type = "ExactMatch",
            Parameters = [
                new MatcherValue("WALMART", null),
                new MatcherValue("TARGET", null)
            ]
        };

        var matcher = MatcherFactory.CreateMatcher(matcherConfig);

        Assert.NotNull(matcher);
        Assert.IsType<ExactMatcher>(matcher);
        Assert.True(matcher.TryMatch(new Transaction { Description = "WALMART", Amount = 0m }));
        Assert.True(matcher.TryMatch(new Transaction { Description = "walmart", Amount = 0m })); // Case insensitive enforced
    }

    [Fact]
    public void CreateMatcher_ContainsType_CreatesContainsMatcher()
    {
        var matcherConfig = new CategoryMatcher
        {
            Type = "Contains",
            Parameters = [
                new MatcherValue("COFFEE", null),
                new MatcherValue("TEA", null)
            ]
        };

        var matcher = MatcherFactory.CreateMatcher(matcherConfig);

        Assert.NotNull(matcher);
        Assert.IsType<ContainsMatcher>(matcher);
        Assert.True(matcher.TryMatch(new Transaction { Description = "STARBUCKS COFFEE", Amount = 0m }));
        Assert.True(matcher.TryMatch(new Transaction { Description = "TEA SHOP", Amount = 0m }));
    }

    [Fact]
    public void CreateMatcher_RegexType_CreatesRegexMatcher()
    {
        var matcherConfig = new CategoryMatcher
        {
            Type = "Regex",
            Parameters = [
                new MatcherValue("^WALMART #\\d+", null)
            ]
        };

        var matcher = MatcherFactory.CreateMatcher(matcherConfig);

        Assert.NotNull(matcher);
        Assert.IsType<RegexMatcher>(matcher);
        Assert.True(matcher.TryMatch(new Transaction { Description = "WALMART #1234", Amount = 0m }));
        Assert.False(matcher.TryMatch(new Transaction { Description = "TARGET #1234", Amount = 0m }));
    }

    [Fact]
    public void CreateMatcher_UnknownType_ReturnsNull()
    {
        var matcherConfig = new CategoryMatcher
        {
            Type = "UnknownMatcher",
            Parameters = []
        };

        var matcher = MatcherFactory.CreateMatcher(matcherConfig);

        Assert.Null(matcher);
    }

    [Fact]
    public void CreateMatcher_NullConfig_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => MatcherFactory.CreateMatcher(null!));
    }

    [Fact]
    public void CreateMatcher_MissingPatternParameter_ReturnsNull()
    {
        var matcherConfig = new CategoryMatcher
        {
            Type = "Regex",
            Parameters = []
        };

        var matcher = MatcherFactory.CreateMatcher(matcherConfig);

        Assert.Null(matcher);
    }

    [Fact]
    public void CreateMatcher_EmptyParametersArray_ReturnsNull()
    {
        var matcherConfig = new CategoryMatcher
        {
            Type = "ExactMatch",
            Parameters = []
        };

        var matcher = MatcherFactory.CreateMatcher(matcherConfig);

        Assert.Null(matcher);
    }

    [Fact]
    public void CreateMatcher_InvalidRegexPattern_ReturnsNull()
    {
        var matcherConfig = new CategoryMatcher
        {
            Type = "Regex",
            Parameters = [new MatcherValue("[invalid(regex", null)]
        };

        var matcher = MatcherFactory.CreateMatcher(matcherConfig);

        Assert.Null(matcher);
    }

    [Fact]
    public void CreateMatcher_DefaultCaseSensitivity_UsesInsensitive()
    {
        var matcherConfig = new CategoryMatcher
        {
            Type = "ExactMatch",
            Parameters = [
                new MatcherValue("WALMART", null)
            ]
        };

        var matcher = MatcherFactory.CreateMatcher(matcherConfig);

        Assert.NotNull(matcher);
        Assert.True(matcher.TryMatch(new Transaction { Description = "walmart", Amount = 0m })); // Case-insensitive enforced
    }

    [Fact]
    public void CreateMatcher_MultipleValues_AllWorkCorrectly()
    {
        var matcherConfig = new CategoryMatcher
        {
            Type = "Contains",
            Parameters = [
                new MatcherValue("WALMART", null),
                new MatcherValue("TARGET", null),
                new MatcherValue("COSTCO", null)
            ]
        };

        var matcher = MatcherFactory.CreateMatcher(matcherConfig);

        Assert.NotNull(matcher);
        Assert.True(matcher.TryMatch(new Transaction { Description = "WALMART SUPERCENTER", Amount = 0m }));
        Assert.True(matcher.TryMatch(new Transaction { Description = "Shopping at TARGET", Amount = 0m }));
        Assert.True(matcher.TryMatch(new Transaction { Description = "COSTCO WHOLESALE", Amount = 0m }));
        Assert.False(matcher.TryMatch(new Transaction { Description = "SAFEWAY", Amount = 0m }));
    }

    [Theory]
    [InlineData("ExactMatch")]
    [InlineData("exactmatch")]
    [InlineData("EXACTMATCH")]
    [InlineData("Contains")]
    [InlineData("contains")]
    [InlineData("Regex")]
    [InlineData("regex")]
    public void CreateMatcher_CaseInsensitiveTypeNames_WorksCorrectly(String matcherType)
    {
        var matcherConfig = new CategoryMatcher
        {
            Type = matcherType,
            Parameters = [
                new MatcherValue("WALMART", null),
                new MatcherValue("TARGET", null),
                new MatcherValue("COSTCO", null)
            ]
        };

        var matcher = MatcherFactory.CreateMatcher(matcherConfig);

        Assert.NotNull(matcher);
        Assert.True(matcher.TryMatch(new Transaction { Description = "WALMART", Amount = 0m }));
    }

    [Fact]
    public void CreateMatcher_RealWorldExactMatchConfig_WorksCorrectly()
    {
        // Simulate configuration loaded from JSON file
        var json = """
            {
                "Type": "ExactMatch",
                "Parameters": [
                    { "value":"MCDONALD'S" },
                    { "value": "WENDY'S" },
                    { "value":"BURGER KING" }
                ]
            }
            """;

        var matcherConfig = JsonSerializer.Deserialize<CategoryMatcher>(json, SerializationHelper.JSO);
        var matcher = MatcherFactory.CreateMatcher(matcherConfig!);

        Assert.NotNull(matcher);
        Assert.IsType<ExactMatcher>(matcher);
        Assert.True(matcher.TryMatch(new Transaction { Description = "MCDONALD'S", Amount = 0m }));
        Assert.True(matcher.TryMatch(new Transaction { Description = "mcdonald's", Amount = 0m }));
        Assert.True(matcher.TryMatch(new Transaction { Description = "WENDY'S", Amount = 0m }));
        Assert.False(matcher.TryMatch(new Transaction { Description = "MCDONALD'S #1234", Amount = 0m })); // Exact match only
    }

    [Fact]
    public void CreateMatcher_RealWorldContainsConfig_WorksCorrectly()
    {
        var json = """
        {
            "type": "Contains",
            "parameters": [
                {
                    "value": "GIRL GUIDES"
                },
                {
                    "value": "THUNDERBIRD EMAIL"
                }
            ]
        }
        """;

        var matcherConfig = JsonSerializer.Deserialize<CategoryMatcher>(json, SerializationHelper.JSO);
        var matcher = MatcherFactory.CreateMatcher(matcherConfig!);

        Assert.NotNull(matcher);
        Assert.IsType<ContainsMatcher>(matcher);
        Assert.True(matcher.TryMatch(new Transaction { Description = "GIRL GUIDES", Amount = 0m }));
        Assert.True(matcher.TryMatch(new Transaction { Description = "THUNDERBIRD EMAIL", Amount = 0m }));
    }

    [Fact]
    public void CreateMatcher_RealWorldRegexConfig_WorksCorrectly()
    {
        var json = """
            {
                "Type": "Regex",
                "Parameters": [
                    {"value": "^WALMART #\\d{4,5}"}
                ]
            }
            """;

        var matcherConfig = JsonSerializer.Deserialize<CategoryMatcher>(json, SerializationHelper.JSO);
        var matcher = MatcherFactory.CreateMatcher(matcherConfig!);

        Assert.NotNull(matcher);
        Assert.IsType<RegexMatcher>(matcher);
        Assert.True(matcher.TryMatch(new Transaction { Description = "WALMART #1234", Amount = 0m }));
        Assert.True(matcher.TryMatch(new Transaction { Description = "WALMART #56789", Amount = 0m }));
        Assert.False(matcher.TryMatch(new Transaction { Description = "WALMART #123", Amount = 0m })); // Too few digits
        Assert.False(matcher.TryMatch(new Transaction { Description = "TARGET #1234", Amount = 0m })); // Wrong store
    }
}
