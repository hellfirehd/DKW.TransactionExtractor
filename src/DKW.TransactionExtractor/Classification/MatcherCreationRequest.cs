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
    /// Creates an ExactMatch request with the specified values and case sensitivity.
    /// </summary>
    public static MatcherCreationRequest ExactMatch(String[] values, Boolean caseSensitive = false)
    {
        return new MatcherCreationRequest(
            "ExactMatch",
            new Dictionary<String, Object>
            {
                { "values", values },
                { "caseSensitive", caseSensitive }
            }
        );
    }

    /// <summary>
    /// Creates a Contains request with the specified values and case sensitivity.
    /// </summary>
    public static MatcherCreationRequest Contains(String[] values, Boolean caseSensitive = false)
    {
        return new MatcherCreationRequest(
            "Contains",
            new Dictionary<String, Object>
            {
                { "values", values },
                { "caseSensitive", caseSensitive }
            }
        );
    }

    /// <summary>
    /// Creates a Regex request with the specified pattern and case sensitivity.
    /// </summary>
    public static MatcherCreationRequest Regex(String pattern, Boolean ignoreCase = true)
    {
        return new MatcherCreationRequest(
            "Regex",
            new Dictionary<String, Object>
            {
                { "pattern", pattern },
                { "ignoreCase", ignoreCase }
            }
        );
    }
}
