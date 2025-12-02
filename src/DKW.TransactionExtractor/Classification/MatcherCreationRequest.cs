using System.Linq;
using System.Text.Json;

namespace DKW.TransactionExtractor.Classification;

/// <summary>
/// Represents a request to create a new matcher for a category.
/// Contains the matcher type and its configuration parameters.
/// </summary>
public record MatcherCreationRequest(
    String MatcherType,
    Dictionary<String, Object> Parameters
)
{
    /// <summary>
    /// Creates an ExactMatch request with the specified values.
    /// Each value may optionally include an amount.
    /// </summary>
    public static MatcherCreationRequest ExactMatch((String value, Decimal? amount)[] values)
    {
        var valueObjects = values
            .Select(v =>
            {
                var dict = new Dictionary<String, Object>
                {
                    ["value"] = v.value
                };
                if (v.amount.HasValue)
                {
                    dict["amount"] = Decimal.Round(v.amount.Value, 2);
                }
                return JsonSerializer.SerializeToElement(dict);
            })
            .ToArray();

        var dictParams = new Dictionary<String, Object>
        {
            { "values", JsonSerializer.SerializeToElement(valueObjects) }
        };

        return new MatcherCreationRequest(
            "ExactMatch",
            dictParams
        );
    }

    /// <summary>
    /// Creates a Contains request with the specified values.
    /// Each value may optionally include an amount.
    /// </summary>
    public static MatcherCreationRequest Contains((String value, Decimal? amount)[] values)
    {
        var valueObjects = values
            .Select(v =>
            {
                var dict = new Dictionary<String, Object>
                {
                    ["value"] = v.value
                };
                if (v.amount.HasValue)
                {
                    dict["amount"] = Decimal.Round(v.amount.Value, 2);
                }
                return JsonSerializer.SerializeToElement(dict);
            })
            .ToArray();

        var dictParams = new Dictionary<String, Object>
        {
            { "values", JsonSerializer.SerializeToElement(valueObjects) }
        };

        return new MatcherCreationRequest(
            "Contains",
            dictParams
        );
    }

    /// <summary>
    /// Creates a Regex request with the specified pattern.
    /// </summary>
    public static MatcherCreationRequest Regex(String pattern)
    {
        return new MatcherCreationRequest(
            "Regex",
            new Dictionary<String, Object>
            {
                { "pattern", pattern }
            }
        );
    }
}
