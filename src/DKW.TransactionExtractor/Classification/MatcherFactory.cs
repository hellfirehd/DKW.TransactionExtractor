using DKW.TransactionExtractor.Models;
using System.Text.Json;
using System.Globalization;

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
        if (!parameters.TryGetValue("values", out var valuesObj) || valuesObj is not JsonElement valuesElement || valuesElement.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        var values = valuesElement.EnumerateArray()
            .Select(e =>
            {
                if (e.ValueKind == JsonValueKind.Object)
                {
                    var v = e.GetProperty("value").GetString() ?? String.Empty;
                    Decimal? amt = null;
                    if (e.TryGetProperty("amount", out var amtProp) && amtProp.ValueKind == JsonValueKind.Number && amtProp.TryGetDecimal(out var d))
                    {
                        amt = Decimal.Round(d, 2);
                    }

                    return new MatcherValue(v, amt);
                }

                // Fallback - treat as string (but per instructions we are using object form only)
                return new MatcherValue(e.GetString() ?? String.Empty, null);
            })
            .ToArray();

        if (values.Length == 0)
        {
            return null;
        }

        return new ExactMatcher(values);
    }

    private static ContainsMatcher? CreateContainsMatcher(Dictionary<String, Object> parameters)
    {
        if (!parameters.TryGetValue("values", out var valuesObj) || valuesObj is not JsonElement valuesElement || valuesElement.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        var values = valuesElement.EnumerateArray()
            .Select(e =>
            {
                if (e.ValueKind == JsonValueKind.Object)
                {
                    var v = e.GetProperty("value").GetString() ?? String.Empty;
                    Decimal? amt = null;
                    if (e.TryGetProperty("amount", out var amtProp) && amtProp.ValueKind == JsonValueKind.Number && amtProp.TryGetDecimal(out var d))
                    {
                        amt = Decimal.Round(d, 2);
                    }

                    return new MatcherValue(v, amt);
                }

                // Fallback - treat as string (but per instructions we are using object form only)
                return new MatcherValue(e.GetString() ?? String.Empty, null);
            })
            .ToArray();

        if (values.Length == 0)
        {
            return null;
        }

        return new ContainsMatcher(values);
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

        // Always ignore case for regex per new requirement
        var ignoreCase = true;

        // Parse optional amount parameter
        Decimal? amount = null;
        if (parameters.TryGetValue("amount", out var amtObj))
        {
            if (amtObj is JsonElement amtElement && amtElement.ValueKind == JsonValueKind.Number && amtElement.TryGetDecimal(out var d))
            {
                amount = Decimal.Round(d, 2);
            }
            else if (amtObj is Decimal dec)
            {
                amount = Decimal.Round(dec, 2);
            }
            else if (amtObj is Double dbl)
            {
                amount = Decimal.Round(Convert.ToDecimal(dbl), 2);
            }
            else if (amtObj is String s && Decimal.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed))
            {
                amount = Decimal.Round(parsed, 2);
            }
        }

        try
        {
            return new RegexMatcher(pattern, ignoreCase, amount);
        }
        catch (ArgumentException)
        {
            // Invalid regex pattern
            return null;
        }
    }
}
