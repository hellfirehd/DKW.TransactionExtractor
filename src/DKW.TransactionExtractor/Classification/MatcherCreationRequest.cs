namespace DKW.TransactionExtractor.Classification;

/// <summary>
/// Represents a request to create a new matcher for a category.
/// Contains the matcher type and its configuration parameters.
/// </summary>
public record MatcherCreationRequest(
    String MatcherType,
    IEnumerable<MatcherValue> Parameters
)
{
    /// <summary>
    /// Creates an ExactMatch request with the specified values.
    /// Each value may optionally include an amount.
    /// </summary>
    public static MatcherCreationRequest ExactMatch(TransactionContext context)
    {
        return CreateRequest(context);
    }

    /// <summary>
    /// Creates a Contains request with the specified values.
    /// Each value may optionally include an amount.
    /// </summary>
    public static MatcherCreationRequest Contains(TransactionContext context)
    {
        return CreateRequest(context);
    }

    /// <summary>
    /// Creates a Regex request with the specified pattern.
    /// </summary>
    public static MatcherCreationRequest Regex(TransactionContext context)
    {
        return CreateRequest(context);
    }

    private static MatcherCreationRequest CreateRequest(TransactionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.IncludeAmountInMatcher)
        {
            return new MatcherCreationRequest(context.MatcherType, [new(context.MatcherText, context.Transaction.Amount)]);
        }

        return new MatcherCreationRequest(context.MatcherType, [new(context.MatcherText, null)]);
    }
}
