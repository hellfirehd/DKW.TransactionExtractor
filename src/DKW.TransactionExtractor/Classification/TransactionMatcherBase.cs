using DKW.TransactionExtractor.Models;

namespace DKW.TransactionExtractor.Classification;

/// <summary>
/// Base class for transaction matchers. Provides common validation for transactions and a helper
/// for amount comparison. Concrete matchers implement <see cref="TryMatchCore"/> to perform
/// pattern-specific matching logic. This centralizes behavior such as null/whitespace checks for
/// transaction descriptions and amount equality semantics.
/// </summary>
public abstract class TransactionMatcherBase : ITransactionMatcher
{
    /// <summary>
    /// Entry point for matching. Validates input and delegates to <see cref="TryMatchCore"/>.
    /// </summary>
    public Boolean TryMatch(Transaction transaction)
    {
        if (transaction == null)
            return false;

        var description = transaction.Description;
        if (String.IsNullOrWhiteSpace(description))
            return false;

        return TryMatchCore(transaction, description);
    }

    /// <summary>
    /// Concrete matchers implement this core method. The description parameter is guaranteed
    /// to be non-null and non-whitespace.
    /// </summary>
    protected abstract Boolean TryMatchCore(Transaction transaction, String description);

    /// <summary>
    /// Helper to compare amounts when a matcher or value specifies an amount requirement.
    /// If <paramref name="expected"/> is null the helper returns true (no amount restriction).
    /// Otherwise both values are rounded to 2 decimal places and compared for equality.
    /// </summary>
    protected static Boolean AmountsEqual(Decimal? expected, Decimal actual)
    {
        if (!expected.HasValue)
            return true; // no amount requirement

        return Decimal.Round(actual, 2) == Decimal.Round(expected.Value, 2);
    }
}
