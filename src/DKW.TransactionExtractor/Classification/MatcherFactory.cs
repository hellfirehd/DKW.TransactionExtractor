using DKW.TransactionExtractor.Models;
using System.Text.Json;

namespace DKW.TransactionExtractor.Classification;

public class MatcherFactory
{
    public static ITransactionMatcher? CreateMatcher(CategoryMatcher? matcherConfig)
    {
        if (matcherConfig == null)
        {
            return null;
        }

        return matcherConfig.Type.ToLowerInvariant() switch
        {
            "exactmatch" => CreateExactMatcher(matcherConfig.Parameters),
            "contains" => CreateContainsMatcher(matcherConfig.Parameters),
            "regex" => CreateRegexMatcher(matcherConfig.Parameters),
            _ => null
        };
    }

    private static ExactMatcher? CreateExactMatcher(Dictionary<String, Object> parameters)
    {
        if (!parameters.TryGetValue("values", out var valuesObj) || valuesObj is not JsonElement valuesElement)
        {
            return null;
        }

        var values = valuesElement.EnumerateArray()
            .Select(e => e.GetString() ?? String.Empty)
            .ToArray();

        if (values.Length == 0)
        {
            return null;
        }

        var caseSensitive = parameters.ContainsKey("caseSensitive")
            && parameters["caseSensitive"] is JsonElement csElement
            && csElement.GetBoolean();

        return new ExactMatcher(values, caseSensitive);
    }

    private static ContainsMatcher? CreateContainsMatcher(Dictionary<String, Object> parameters)
    {
        // Support both old format (single "value") and new format (array "values")
        String[] parameterValues;

        if (parameters.TryGetValue("values", out var values) && values is JsonElement valuesElement)
        {
            parameterValues = valuesElement.EnumerateArray()
                .Select(e => e.GetString() ?? String.Empty)
                .ToArray();
        }
        else if (parameters.TryGetValue("value", out var value) && value is JsonElement valueElement)
        {
            // Legacy format support
            var singleValue = valueElement.GetString() ?? String.Empty;
            parameterValues = [singleValue];
        }
        else
        {
            return null;
        }

        if (parameterValues.Length == 0)
        {
            return null;
        }

        var caseSensitive = parameters.ContainsKey("caseSensitive")
            && parameters["caseSensitive"] is JsonElement csElement
            && csElement.GetBoolean();

        return new ContainsMatcher(parameterValues, caseSensitive);
    }

    private static RegexMatcher? CreateRegexMatcher(Dictionary<String, Object> parameters)
    {
        if (!parameters.TryGetValue("pattern", out var patternObj) || patternObj is not JsonElement patternElement)
        {
            return null;
        }

        var pattern = patternElement.GetString();
        if (String.IsNullOrWhiteSpace(pattern))
        {
            return null;
        }

        var ignoreCase = parameters.ContainsKey("ignoreCase")
            && parameters["ignoreCase"] is JsonElement icElement
            && icElement.GetBoolean();

        try
        {
            return new RegexMatcher(pattern, ignoreCase);
        }
        catch (ArgumentException)
        {
            // Invalid regex pattern
            return null;
        }
    }
}
