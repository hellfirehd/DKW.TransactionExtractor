using DKW.TransactionExtractor.Models;
using System.Text.Json;

namespace DKW.TransactionExtractor.Classification;

public class MatcherFactory
{
    public ITransactionMatcher? CreateMatcher(CategoryMatcher matcherConfig)
    {
        return matcherConfig.Type switch
        {
            "ExactMatch" => CreateExactMatcher(matcherConfig.Parameters),
            "Contains" => CreateContainsMatcher(matcherConfig.Parameters),
            "Regex" => CreateRegexMatcher(matcherConfig.Parameters),
            _ => null
        };
    }

    private ITransactionMatcher CreateExactMatcher(Dictionary<String, Object> parameters)
    {
        var valuesElement = (JsonElement)parameters["values"];
        var values = valuesElement.EnumerateArray()
            .Select(e => e.GetString() ?? String.Empty)
            .ToArray();

        var caseSensitive = parameters.ContainsKey("caseSensitive") 
            && ((JsonElement)parameters["caseSensitive"]).GetBoolean();

        return new ExactMatcher(values, caseSensitive);
    }

    private ITransactionMatcher CreateContainsMatcher(Dictionary<String, Object> parameters)
    {
        var value = ((JsonElement)parameters["value"]).GetString() ?? String.Empty;
        var caseSensitive = parameters.ContainsKey("caseSensitive") 
            && ((JsonElement)parameters["caseSensitive"]).GetBoolean();

        return new ContainsMatcher(value, caseSensitive);
    }

    private ITransactionMatcher CreateRegexMatcher(Dictionary<String, Object> parameters)
    {
        var pattern = ((JsonElement)parameters["pattern"]).GetString() ?? String.Empty;
        var ignoreCase = parameters.ContainsKey("ignoreCase") 
            && ((JsonElement)parameters["ignoreCase"]).GetBoolean();

        return new RegexMatcher(pattern, ignoreCase);
    }
}
