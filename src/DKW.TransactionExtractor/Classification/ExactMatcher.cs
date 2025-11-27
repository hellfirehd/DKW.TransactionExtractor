namespace DKW.TransactionExtractor.Classification;

public class ExactMatcher : ITransactionMatcher
{
    private readonly HashSet<String> _values;
    private readonly Boolean _caseSensitive;

    public ExactMatcher(String[] values, Boolean caseSensitive = false)
    {
        ArgumentNullException.ThrowIfNull(values);
        
        if (values.Length == 0)
        {
            throw new ArgumentException("Values array cannot be empty", nameof(values));
        }

        _caseSensitive = caseSensitive;
        var comparer = caseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;
        _values = new HashSet<String>(values, comparer);
    }

    public Boolean TryMatch(String description)
    {
        if (String.IsNullOrWhiteSpace(description))
        {
            return false;
        }

        return _values.Contains(description);
    }
}
