using DKW.TransactionExtractor.Models;

namespace DKW.TransactionExtractor.Classification;

/// <summary>
/// ExactMatcher matches transactions whose description exactly equals one of the configured values.
/// Matching is case-insensitive. Each configured value may optionally include an <c>Amount</c> which
/// restricts matches to transactions with that amount (rounded to 2 decimals).
/// </summary>
public class ExactMatcher : TransactionMatcherBase
{
    private readonly MatcherValue[] _values;

    /// <summary>
    /// Creates a new ExactMatcher.
    /// </summary>
    /// <param name="values">Array of value/amount pairs to match against. Cannot be null or empty.</param>
    public ExactMatcher(IEnumerable<MatcherValue> values)
    {
        ArgumentNullException.ThrowIfNull(values);

        _values = values.ToArray();

        if (_values.Length == 0)
        {
            throw new ArgumentException("Values array cannot be empty", nameof(values));
        }
    }

    /// <summary>
    /// Core matching logic executed after base validation. Description is guaranteed non-null/non-whitespace.
    /// </summary>
    protected override Boolean TryMatchCore(Transaction transaction, String description)
    {
        var amount = transaction.Amount;

        foreach (var mv in _values)
        {
            if (String.Equals(mv.Value, description, StringComparison.OrdinalIgnoreCase))
            {
                if (mv.Amount.HasValue)
                {
                    if (AmountsEqual(mv.Amount, amount))
                    {
                        return true;
                    }
                }
                else
                {
                    return true;
                }
            }
        }

        return false;
    }
}
