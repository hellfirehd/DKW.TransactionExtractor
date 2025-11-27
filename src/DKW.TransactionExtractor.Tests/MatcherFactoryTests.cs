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
            Parameters = new Dictionary<String, Object>
            {
                ["values"] = JsonSerializer.SerializeToElement(new[] { "WALMART", "TARGET" }),
                ["caseSensitive"] = JsonSerializer.SerializeToElement(false)
            }
        };

        var matcher = MatcherFactory.CreateMatcher(matcherConfig);

        Assert.NotNull(matcher);
        Assert.IsType<ExactMatcher>(matcher);
        Assert.True(matcher.TryMatch("WALMART"));
        Assert.True(matcher.TryMatch("walmart")); // Case insensitive
    }

    [Fact]
    public void CreateMatcher_ContainsType_CreatesContainsMatcher()
    {
        var matcherConfig = new CategoryMatcher
        {
            Type = "Contains",
            Parameters = new Dictionary<String, Object>
            {
                ["values"] = JsonSerializer.SerializeToElement(new[] { "COFFEE", "TEA" }),
                ["caseSensitive"] = JsonSerializer.SerializeToElement(false)
            }
        };

        var matcher = MatcherFactory.CreateMatcher(matcherConfig);

        Assert.NotNull(matcher);
        Assert.IsType<ContainsMatcher>(matcher);
        Assert.True(matcher.TryMatch("STARBUCKS COFFEE"));
        Assert.True(matcher.TryMatch("TEA SHOP"));
    }

    [Fact]
    public void CreateMatcher_RegexType_CreatesRegexMatcher()
    {
        var matcherConfig = new CategoryMatcher
        {
            Type = "Regex",
            Parameters = new Dictionary<String, Object>
            {
                ["pattern"] = JsonSerializer.SerializeToElement("^WALMART #\\d+"),
                ["caseSensitive"] = JsonSerializer.SerializeToElement(false)
            }
        };

        var matcher = MatcherFactory.CreateMatcher(matcherConfig);

        Assert.NotNull(matcher);
        Assert.IsType<RegexMatcher>(matcher);
        Assert.True(matcher.TryMatch("WALMART #1234"));
        Assert.False(matcher.TryMatch("TARGET #1234"));
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
    public void CreateMatcher_NullConfig_ReturnsNull()
    {
        var matcher = MatcherFactory.CreateMatcher(null!);

        Assert.Null(matcher);
    }

    [Fact]
    public void CreateMatcher_ExactMatchCaseSensitiveTrue_CreatesCaseSensitiveMatcher()
    {
        var matcherConfig = new CategoryMatcher
        {
            Type = "ExactMatch",
            Parameters = new Dictionary<String, Object>
            {
                ["values"] = JsonSerializer.SerializeToElement(new[] { "WALMART" }),
                ["caseSensitive"] = JsonSerializer.SerializeToElement(true)
            }
        };

        var matcher = MatcherFactory.CreateMatcher(matcherConfig);

        Assert.NotNull(matcher);
        Assert.True(matcher.TryMatch("WALMART"));
        Assert.False(matcher.TryMatch("walmart")); // Case sensitive
    }

    [Fact]
    public void CreateMatcher_MissingValuesParameter_ReturnsNull()
    {
        var matcherConfig = new CategoryMatcher
        {
            Type = "ExactMatch",
            Parameters = new Dictionary<String, Object>
            {
                ["caseSensitive"] = JsonSerializer.SerializeToElement(false)
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
                ["caseSensitive"] = JsonSerializer.SerializeToElement(false)
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
                ["values"] = JsonSerializer.SerializeToElement(Array.Empty<String>()),
                ["caseSensitive"] = JsonSerializer.SerializeToElement(false)
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
                ["pattern"] = JsonSerializer.SerializeToElement("[invalid(regex"),
                ["caseSensitive"] = JsonSerializer.SerializeToElement(false)
            }
        };

        var matcher = MatcherFactory.CreateMatcher(matcherConfig);

        Assert.Null(matcher);
    }

    [Fact]
    public void CreateMatcher_DefaultCaseSensitivity_UsesFalse()
    {
        // Missing caseSensitive parameter should default to false
        var matcherConfig = new CategoryMatcher
        {
            Type = "ExactMatch",
            Parameters = new Dictionary<String, Object>
            {
                ["values"] = JsonSerializer.SerializeToElement(new[] { "WALMART" })
                // Missing "caseSensitive" parameter
            }
        };

        var matcher = MatcherFactory.CreateMatcher(matcherConfig);

        Assert.NotNull(matcher);
        Assert.True(matcher.TryMatch("walmart")); // Should be case insensitive by default
    }

    [Fact]
    public void CreateMatcher_MultipleValues_AllWorkCorrectly()
    {
        var matcherConfig = new CategoryMatcher
        {
            Type = "Contains",
            Parameters = new Dictionary<String, Object>
            {
                ["values"] = JsonSerializer.SerializeToElement(new[] { "WALMART", "TARGET", "COSTCO" }),
                ["caseSensitive"] = JsonSerializer.SerializeToElement(false)
            }
        };

        var matcher = MatcherFactory.CreateMatcher(matcherConfig);

        Assert.NotNull(matcher);
        Assert.True(matcher.TryMatch("WALMART SUPERCENTER"));
        Assert.True(matcher.TryMatch("Shopping at TARGET"));
        Assert.True(matcher.TryMatch("COSTCO WHOLESALE"));
        Assert.False(matcher.TryMatch("SAFEWAY"));
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
            Parameters = matcherType.Equals("regex", StringComparison.CurrentCultureIgnoreCase)
                ? new Dictionary<String, Object>
                {
                    ["pattern"] = JsonSerializer.SerializeToElement("WALMART"),
                    ["caseSensitive"] = JsonSerializer.SerializeToElement(false)
                }
                : new Dictionary<String, Object>
                {
                    ["values"] = JsonSerializer.SerializeToElement(new[] { "WALMART" }),
                    ["caseSensitive"] = JsonSerializer.SerializeToElement(false)
                }
        };

        var matcher = MatcherFactory.CreateMatcher(matcherConfig);

        Assert.NotNull(matcher);
        Assert.True(matcher.TryMatch("WALMART"));
    }

    [Fact]
    public void CreateMatcher_RealWorldExactMatchConfig_WorksCorrectly()
    {
        // Simulate configuration loaded from JSON file
        var json = """
            {
                "Type": "ExactMatch",
                "Parameters": {
                    "values": ["MCDONALD'S", "WENDY'S", "BURGER KING"],
                    "caseSensitive": false
                }
            }
            """;

        var matcherConfig = JsonSerializer.Deserialize<CategoryMatcher>(json);
        var matcher = MatcherFactory.CreateMatcher(matcherConfig!);

        Assert.NotNull(matcher);
        Assert.IsType<ExactMatcher>(matcher);
        Assert.True(matcher.TryMatch("MCDONALD'S"));
        Assert.True(matcher.TryMatch("mcdonald's"));
        Assert.True(matcher.TryMatch("WENDY'S"));
        Assert.False(matcher.TryMatch("MCDONALD'S #1234")); // Exact match only
    }

    [Fact]
    public void CreateMatcher_RealWorldContainsConfig_WorksCorrectly()
    {
        var json = """
            {
                "Type": "Contains",
                "Parameters": {
                    "values": ["COFFEE", "CAFE", "ESPRESSO"],
                    "caseSensitive": false
                }
            }
            """;

        var matcherConfig = JsonSerializer.Deserialize<CategoryMatcher>(json);
        var matcher = MatcherFactory.CreateMatcher(matcherConfig!);

        Assert.NotNull(matcher);
        Assert.IsType<ContainsMatcher>(matcher);
        Assert.True(matcher.TryMatch("STARBUCKS COFFEE"));
        Assert.True(matcher.TryMatch("LOCAL CAFE"));
        Assert.True(matcher.TryMatch("ESPRESSO BAR"));
    }

    [Fact]
    public void CreateMatcher_RealWorldRegexConfig_WorksCorrectly()
    {
        var json = """
            {
                "Type": "Regex",
                "Parameters": {
                    "pattern": "^WALMART #\\d{4,5}",
                    "caseSensitive": false
                }
            }
            """;

        var matcherConfig = JsonSerializer.Deserialize<CategoryMatcher>(json);
        var matcher = MatcherFactory.CreateMatcher(matcherConfig!);

        Assert.NotNull(matcher);
        Assert.IsType<RegexMatcher>(matcher);
        Assert.True(matcher.TryMatch("WALMART #1234"));
        Assert.True(matcher.TryMatch("WALMART #56789"));
        Assert.False(matcher.TryMatch("WALMART #123")); // Too few digits
        Assert.False(matcher.TryMatch("TARGET #1234")); // Wrong store
    }
}
