using DKW.TransactionExtractor.Classification;
using DKW.TransactionExtractor.Models;
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
            Parameters = [new MatcherValue("^WALMART #\\d+", 50.00m)]
        };

        var matcher = MatcherFactory.CreateMatcher(matcherConfig);

        Assert.NotNull(matcher);
        Assert.IsType<RegexMatcher>(matcher);
        Assert.True(matcher.TryMatch(new Transaction { Description = "WALMART #1234", Amount = 50.00m }));
        Assert.False(matcher.TryMatch(new Transaction { Description = "WALMART #1234", Amount = 49.99m }));
    }
}
