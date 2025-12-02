using DKW.TransactionExtractor.Classification;
using DKW.TransactionExtractor.Models;
using System.Text.Json;
using Xunit;

namespace DKW.TransactionExtractor.Tests;

public class AmountMatcherTests
{
    [Fact]
    public void ExactMatcher_WithAmount_MatchesOnlyWhenAmountAndDescriptionMatch()
    {
        var matcher = new ExactMatcher([new MatcherValue("FOO TRANSACTION", 12.34m)]);

        Assert.True(matcher.TryMatch(TestHelpers.CreateTransaction("FOO TRANSACTION", 12.34m)));
        Assert.False(matcher.TryMatch(TestHelpers.CreateTransaction("FOO TRANSACTION", 12.35m)));
        Assert.False(matcher.TryMatch(TestHelpers.CreateTransaction("FOO TRANSACTION", 0m)));
        Assert.False(matcher.TryMatch(TestHelpers.CreateTransaction("OTHER TRANSACTION", 12.34m)));
    }

    [Fact]
    public void ContainsMatcher_WithAmount_MatchesOnlyWhenAmountAndDescriptionMatch()
    {
        var matcher = new ContainsMatcher([new MatcherValue("BAR", -5.00m)]);

        Assert.True(matcher.TryMatch(TestHelpers.CreateTransaction("REFUND FROM BAR", -5.00m)));
        Assert.False(matcher.TryMatch(TestHelpers.CreateTransaction("REFUND FROM BAR", -5.01m)));
        Assert.False(matcher.TryMatch(TestHelpers.CreateTransaction("REFUND FROM BAR", 0m)));
        Assert.False(matcher.TryMatch(TestHelpers.CreateTransaction("SOMETHING ELSE", -5.00m)));
    }

    [Fact]
    public void MatcherFactory_CreatesExactMatcher_WithAmountParameter()
    {
        var matcherConfig = new CategoryMatcher
        {
            Type = "ExactMatch",
            Parameters = new Dictionary<string, object>
            {
                ["values"] = JsonSerializer.SerializeToElement(new[] { JsonSerializer.SerializeToElement(new { value = "FOO TRANSACTION", amount = 12.34m }) })
            }
        };

        var matcher = MatcherFactory.CreateMatcher(matcherConfig);

        Assert.NotNull(matcher);
        Assert.IsType<ExactMatcher>(matcher);
        Assert.True(matcher.TryMatch(TestHelpers.CreateTransaction("FOO TRANSACTION", 12.34m)));
        Assert.False(matcher.TryMatch(TestHelpers.CreateTransaction("FOO TRANSACTION", 12.35m)));
    }

    [Fact]
    public void MatcherFactory_CreatesContainsMatcher_WithAmountParameter()
    {
        var matcherConfig = new CategoryMatcher
        {
            Type = "Contains",
            Parameters = new Dictionary<string, object>
            {
                ["values"] = JsonSerializer.SerializeToElement(new[] { JsonSerializer.SerializeToElement(new { value = "BAR", amount = -5.00m }) })
            }
        };

        var matcher = MatcherFactory.CreateMatcher(matcherConfig);

        Assert.NotNull(matcher);
        Assert.IsType<ContainsMatcher>(matcher);
        Assert.True(matcher.TryMatch(TestHelpers.CreateTransaction("REFUND FROM BAR", -5.00m)));
        Assert.False(matcher.TryMatch(TestHelpers.CreateTransaction("REFUND FROM BAR", -6.00m)));
    }
}
