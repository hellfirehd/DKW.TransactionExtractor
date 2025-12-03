using DKW.TransactionExtractor.Models;
using System.Text.RegularExpressions;

namespace DKW.TransactionExtractor.Classification;

/// <summary>
/// RegexMatcher matches transactions whose description matches a regular expression.
/// Regex matching is performed using compiled expressions and is executed case-insensitively
/// by default in the current factory configuration. If an <c>amount</c> is supplied to the
/// matcher, a match will only succeed when both the regex matches the description and the
/// transaction amount equals the configured amount (rounded to 2 decimals).
/// </summary>
public class RegexMatcher : TransactionMatcherBase
{
    private readonly List<RegexMatcherEntry> _entries = [];

    /// <summary>
    /// Create a new RegexMatcher.
    /// </summary>
    /// <param name="pattern">Regular expression pattern to match against transaction descriptions.</param>
    /// <param name="amount">Optional amount that must match the transaction amount (rounded to 2 decimals) for a match to succeed.</param>
    /// <param name="ignoreCase">If true, the regex uses case-insensitive matching.</param>
    public RegexMatcher(IEnumerable<MatcherValue> parameters)
    {
        var options = RegexOptions.Compiled | RegexOptions.IgnoreCase;

        foreach (var param in parameters)
        {
            _entries.Add(new RegexMatcherEntry(CreateRegex(param.Value, options), param.Amount));
        }

        if (_entries.Count == 0)
        {
            throw new ArgumentException("At least one regex pattern must be provided.", nameof(parameters));
        }
    }

    private static Regex CreateRegex(String pattern, RegexOptions options)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pattern);

        try
        {
            return new Regex(pattern, options);
        }
        catch (RegexParseException ex)
        {
            throw new ArgumentException($"Invalid regex pattern: {pattern}", nameof(pattern), ex);
        }
    }

    private record RegexMatcherEntry(Regex Regex, Decimal? Amount)
    {
        public Boolean Matches(Transaction transaction, String description)
        {
            if (!Regex.IsMatch(description))
            {
                return false;
            }

            if (Amount.HasValue)
            {
                return AmountsEqual(Amount, transaction.Amount);
            }

            return true;
        }
    }

    /// <summary>
    /// Core matching logic executed after base validation. Description is guaranteed non-null/non-whitespace.
    /// </summary>
    protected override Boolean TryMatchCore(Transaction transaction, String description)
    {
        foreach (var entry in _entries)
        {
            if (entry.Matches(transaction, description))
            {
                return true;
            }
        }

        return false;
    }
}
