using System.Diagnostics.CodeAnalysis;

namespace DKW.TransactionExtractor.Classification;

public class ContainsMatcher : ITransactionMatcher
{
    private readonly String[] _values;
    private readonly StringComparison _comparison;

    public ContainsMatcher([NotNull] String[] values, Boolean caseSensitive = false)
    {
        ArgumentNullException.ThrowIfNull(values);

        if (values.Length == 0)
        {
            throw new ArgumentException("Values array cannot be empty", nameof(values));
        }

        _values = values;
        _comparison = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
    }

    public Boolean TryMatch(String description)
    {
        if (String.IsNullOrWhiteSpace(description))
        {
            return false;
        }

        return _values.Any(v => description.Contains(v, _comparison));
    }
}
