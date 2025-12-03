using DKW.TransactionExtractor.Models;

namespace DKW.TransactionExtractor.Classification;

/// <summary>
/// ContainsMatcher matches transactions whose description contains one of the configured substrings.
/// Matching is case-insensitive. Each configured value may optionally include an <c>Amount</c> which
/// restricts matches to transactions with that amount (rounded to 2 decimals).
/// </summary>
public class ContainsMatcher : TransactionMatcherBase
{
    private readonly MatcherValue[] _values;
    private readonly StringComparison _comparison = StringComparison.OrdinalIgnoreCase; // Force case-insensitive

    /// <summary>
    /// Constructs a new ContainsMatcher with the provided values.
    /// </summary>
    /// <param name="values">Array of value/amount pairs to match against. Cannot be null or empty.</param>
    public ContainsMatcher(IEnumerable<MatcherValue> values)
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
            if (description.Contains(mv.Value, _comparison))
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
