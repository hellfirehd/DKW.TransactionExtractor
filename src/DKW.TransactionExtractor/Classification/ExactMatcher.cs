namespace DKW.TransactionExtractor.Classification;

public class ExactMatcher : ITransactionMatcher
{
    private readonly HashSet<String> _values;
    private readonly Boolean _caseSensitive;

    public ExactMatcher(String[] values, Boolean caseSensitive = false)
    {
        _caseSensitive = caseSensitive;
        var comparer = caseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;
        _values = new HashSet<String>(values, comparer);
    }

    public Boolean TryMatch(String description)
    {
        return _values.Contains(description);
    }
}
