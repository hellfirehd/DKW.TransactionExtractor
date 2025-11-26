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
        // Support both old format (single "value") and new format (array "values")
        String[] values;
        
        if (parameters.ContainsKey("values"))
        {
            var valuesElement = (JsonElement)parameters["values"];
            values = valuesElement.EnumerateArray()
                .Select(e => e.GetString() ?? String.Empty)
                .ToArray();
        }
        else if (parameters.ContainsKey("value"))
        {
            // Legacy format support
            var value = ((JsonElement)parameters["value"]).GetString() ?? String.Empty;
            values = new[] { value };
        }
        else
        {
            throw new InvalidOperationException("Contains matcher requires either 'value' or 'values' parameter");
        }

        var caseSensitive = parameters.ContainsKey("caseSensitive") 
            && ((JsonElement)parameters["caseSensitive"]).GetBoolean();

        return new ContainsMatcher(values, caseSensitive);
    }

    private ITransactionMatcher CreateRegexMatcher(Dictionary<String, Object> parameters)
    {
        var pattern = ((JsonElement)parameters["pattern"]).GetString() ?? String.Empty;
        var ignoreCase = parameters.ContainsKey("ignoreCase") 
            && ((JsonElement)parameters["ignoreCase"]).GetBoolean();

        return new RegexMatcher(pattern, ignoreCase);
    }
}
