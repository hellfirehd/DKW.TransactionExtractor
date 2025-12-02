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
        var valuesArray = new[] { JsonSerializer.SerializeToElement(new { value = "WALMART" }), JsonSerializer.SerializeToElement(new { value = "TARGET" }) };

        var matcherConfig = new CategoryMatcher
        {
            Type = "ExactMatch",
            Parameters = new Dictionary<String, Object>
            {
                ["values"] = JsonSerializer.SerializeToElement(valuesArray)
            }
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
        var valuesArray = new[] { JsonSerializer.SerializeToElement(new { value = "COFFEE" }), JsonSerializer.SerializeToElement(new { value = "TEA" }) };

        var matcherConfig = new CategoryMatcher
        {
            Type = "Contains",
            Parameters = new Dictionary<String, Object>
            {
                ["values"] = JsonSerializer.SerializeToElement(valuesArray)
            }
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
            Parameters = new Dictionary<String, Object>
            {
                ["pattern"] = JsonSerializer.SerializeToElement("^WALMART #\\d+")
            }
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
            Parameters = new Dictionary<String, Object>()
        };

        var matcher = MatcherFactory.CreateMatcher(matcherConfig);

        Assert.Null(matcher);
    }

    [Fact]
    public void CreateMatcher_NullConfig_ReturnsNull()
    {
        var matcher = MatcherFactory.CreateMatcher(null!);

        Assert.Null(matcher);
    }

    [Fact]
    public void CreateMatcher_MissingValuesParameter_ReturnsNull()
    {
        var matcherConfig = new CategoryMatcher
        {
            Type = "ExactMatch",
            Parameters = new Dictionary<String, Object>
            {
                // Missing "values" parameter
            }
        };

        var matcher = MatcherFactory.CreateMatcher(matcherConfig);

        Assert.Null(matcher);
    }

    [Fact]
    public void CreateMatcher_MissingPatternParameter_ReturnsNull()
    {
        var matcherConfig = new CategoryMatcher
        {
            Type = "Regex",
            Parameters = new Dictionary<String, Object>
            {
                // Missing "pattern" parameter
            }
        };

        var matcher = MatcherFactory.CreateMatcher(matcherConfig);

        Assert.Null(matcher);
    }

    [Fact]
    public void CreateMatcher_EmptyValuesArray_ReturnsNull()
    {
        var matcherConfig = new CategoryMatcher
        {
            Type = "ExactMatch",
            Parameters = new Dictionary<String, Object>
            {
                ["values"] = JsonSerializer.SerializeToElement(Array.Empty<JsonElement>())
            }
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
            Parameters = new Dictionary<String, Object>
            {
                ["pattern"] = JsonSerializer.SerializeToElement("[invalid(regex")
            }
        };

        var matcher = MatcherFactory.CreateMatcher(matcherConfig);

        Assert.Null(matcher);
    }

    [Fact]
    public void CreateMatcher_DefaultCaseSensitivity_UsesInsensitive()
    {
        // caseSensitive parameter removed; behavior is always case-insensitive
        var valuesArray = new[] { JsonSerializer.SerializeToElement(new { value = "WALMART" }) };

        var matcherConfig = new CategoryMatcher
        {
            Type = "ExactMatch",
            Parameters = new Dictionary<String, Object>
            {
                ["values"] = JsonSerializer.SerializeToElement(valuesArray)
            }
        };

        var matcher = MatcherFactory.CreateMatcher(matcherConfig);

        Assert.NotNull(matcher);
        Assert.True(matcher.TryMatch(new Transaction { Description = "walmart", Amount = 0m })); // Case-insensitive enforced
    }

    [Fact]
    public void CreateMatcher_MultipleValues_AllWorkCorrectly()
    {
        var valuesArray = new[] { JsonSerializer.SerializeToElement(new { value = "WALMART" }), JsonSerializer.SerializeToElement(new { value = "TARGET" }), JsonSerializer.SerializeToElement(new { value = "COSTCO" }) };

        var matcherConfig = new CategoryMatcher
        {
            Type = "Contains",
            Parameters = new Dictionary<String, Object>
            {
                ["values"] = JsonSerializer.SerializeToElement(valuesArray)
            }
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
        var valuesArray = matcherType.Equals("regex", StringComparison.CurrentCultureIgnoreCase)
            ? new[] { JsonSerializer.SerializeToElement(new { value = "WALMART" }) }
            : [JsonSerializer.SerializeToElement(new { value = "WALMART" })];

        var matcherConfig = new CategoryMatcher
        {
            Type = matcherType,
            Parameters = matcherType.Equals("regex", StringComparison.CurrentCultureIgnoreCase)
                ? new Dictionary<String, Object>
                {
                    ["pattern"] = JsonSerializer.SerializeToElement("WALMART")
                }
                : new Dictionary<String, Object>
                {
                    ["values"] = JsonSerializer.SerializeToElement(valuesArray)
                }
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
                "Parameters": {
                    "values": ["MCDONALD'S", "WENDY'S", "BURGER KING"]
                }
            }
            """;

        var matcherConfig = JsonSerializer.Deserialize<CategoryMatcher>(json);
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
                "Type": "Contains",
                "Parameters": {
                    "values": ["COFFEE", "CAFE", "ESPRESSO"]
                }
            }
            """;

        var matcherConfig = JsonSerializer.Deserialize<CategoryMatcher>(json);
        var matcher = MatcherFactory.CreateMatcher(matcherConfig!);

        Assert.NotNull(matcher);
        Assert.IsType<ContainsMatcher>(matcher);
        Assert.True(matcher.TryMatch(new Transaction { Description = "STARBUCKS COFFEE", Amount = 0m }));
        Assert.True(matcher.TryMatch(new Transaction { Description = "LOCAL CAFE", Amount = 0m }));
        Assert.True(matcher.TryMatch(new Transaction { Description = "ESPRESSO BAR", Amount = 0m }));
    }

    [Fact]
    public void CreateMatcher_RealWorldRegexConfig_WorksCorrectly()
    {
        var json = """
            {
                "Type": "Regex",
                "Parameters": {
                    "pattern": "^WALMART #\\d{4,5}"
                }
            }
            """;

        var matcherConfig = JsonSerializer.Deserialize<CategoryMatcher>(json);
        var matcher = MatcherFactory.CreateMatcher(matcherConfig!);

        Assert.NotNull(matcher);
        Assert.IsType<RegexMatcher>(matcher);
        Assert.True(matcher.TryMatch(new Transaction { Description = "WALMART #1234", Amount = 0m }));
        Assert.True(matcher.TryMatch(new Transaction { Description = "WALMART #56789", Amount = 0m }));
        Assert.False(matcher.TryMatch(new Transaction { Description = "WALMART #123", Amount = 0m })); // Too few digits
        Assert.False(matcher.TryMatch(new Transaction { Description = "TARGET #1234", Amount = 0m })); // Wrong store
    }
}
