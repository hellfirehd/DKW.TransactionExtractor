using DKW.TransactionExtractor.Models;
using System.Text.Json;

namespace DKW.TransactionExtractor.Classification;

public class MatcherFactory
{
    public static ITransactionMatcher? CreateMatcher(CategoryMatcher matcherConfig)
    {
        return matcherConfig.Type switch
        {
            "ExactMatch" => CreateExactMatcher(matcherConfig.Parameters),
            "Contains" => CreateContainsMatcher(matcherConfig.Parameters),
            "Regex" => CreateRegexMatcher(matcherConfig.Parameters),
            _ => null
        };
    }

    private static ITransactionMatcher CreateExactMatcher(Dictionary<String, Object> parameters)
    {
        var valuesElement = (JsonElement)parameters["values"];
        var values = valuesElement.EnumerateArray()
            .Select(e => e.GetString() ?? String.Empty)
            .ToArray();

        var caseSensitive = parameters.ContainsKey("caseSensitive")
            && ((JsonElement)parameters["caseSensitive"]).GetBoolean();

        return new ExactMatcher(values, caseSensitive);
    }

    private static ITransactionMatcher CreateContainsMatcher(Dictionary<String, Object> parameters)
    {
        // Support both old format (single "value") and new format (array "values")
        String[] parameterValues;

        if (parameters.TryGetValue("values", out var values))
        {
            var valuesElement = (JsonElement)values;
            parameterValues = valuesElement.EnumerateArray()
                .Select(e => e.GetString() ?? String.Empty)
                .ToArray();
        }
        else if (parameters.TryGetValue("value", out var value))
        {
            // Legacy format support
            var singleValue = ((JsonElement)value).GetString() ?? String.Empty;
            parameterValues = [singleValue];
        }
        else
        {
            throw new InvalidOperationException("Contains matcher requires either 'value' or 'values' parameter");
        }

        var caseSensitive = parameters.ContainsKey("caseSensitive")
            && ((JsonElement)parameters["caseSensitive"]).GetBoolean();

        return new ContainsMatcher(parameterValues, caseSensitive);
    }

    private static ITransactionMatcher CreateRegexMatcher(Dictionary<String, Object> parameters)
    {
        var pattern = ((JsonElement)parameters["pattern"]).GetString() ?? String.Empty;
        var ignoreCase = parameters.ContainsKey("ignoreCase")
            && ((JsonElement)parameters["ignoreCase"]).GetBoolean();

        return new RegexMatcher(pattern, ignoreCase);
    }
}
