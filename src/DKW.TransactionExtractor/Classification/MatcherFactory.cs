using DKW.TransactionExtractor.Models;

namespace DKW.TransactionExtractor.Classification;

public class MatcherFactory
{
    public static ITransactionMatcher? CreateMatcher(CategoryMatcher? matcherConfig)
    {
        if (matcherConfig == null)
        {
            return null;
        }

        try
        {
            return matcherConfig.Type.ToLowerInvariant() switch
            {
                "exactmatch" => new ExactMatcher(matcherConfig.Parameters),
                "contains" => new ContainsMatcher(matcherConfig.Parameters),
                "regex" => new RegexMatcher(matcherConfig.Parameters),
                _ => null
            };

        }
        catch (Exception)
        {
            return null;
        }
    }
}
