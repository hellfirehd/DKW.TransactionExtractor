using DKW.TransactionExtractor.Classification;
using DKW.TransactionExtractor.Models;
using System.Text.Json;
using Xunit;

namespace DKW.TransactionExtractor.Tests;

public class MatcherFactoryRegexAmountTests
{
    [Fact]
    public void CreateMatcher_Regex_WithAmountParameter_CreatesRegexMatcherWithAmount()
    {
        var matcherConfig = new CategoryMatcher
        {
            Type = "Regex",
            Parameters = new Dictionary<String, Object>
            {
                ["pattern"] = JsonSerializer.SerializeToElement("^WALMART #\\d+"),
                ["amount"] = JsonSerializer.SerializeToElement(50.00m)
            }
        };

        var matcher = MatcherFactory.CreateMatcher(matcherConfig);

        Assert.NotNull(matcher);
        Assert.IsType<RegexMatcher>(matcher);
        Assert.True(matcher.TryMatch(new Transaction { Description = "WALMART #1234", Amount = 50.00m }));
        Assert.False(matcher.TryMatch(new Transaction { Description = "WALMART #1234", Amount = 49.99m }));
    }

    [Fact]
    public void CreateMatcher_Regex_WithAmountAsString_CreatesRegexMatcherWithAmount()
    {
        var matcherConfig = new CategoryMatcher
        {
            Type = "Regex",
            Parameters = new Dictionary<String, Object>
            {
                ["pattern"] = JsonSerializer.SerializeToElement("^WALMART #\\d+"),
                ["amount"] = "12.34"
            }
        };

        var matcher = MatcherFactory.CreateMatcher(matcherConfig);

        Assert.NotNull(matcher);
        Assert.IsType<RegexMatcher>(matcher);
        Assert.True(matcher.TryMatch(new Transaction { Description = "WALMART #1", Amount = 12.34m }));
        Assert.False(matcher.TryMatch(new Transaction { Description = "WALMART #1", Amount = 12.33m }));
    }
}
