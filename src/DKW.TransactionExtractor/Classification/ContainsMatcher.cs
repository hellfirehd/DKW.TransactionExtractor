namespace DKW.TransactionExtractor.Classification;

public class ContainsMatcher(String[] values, Boolean caseSensitive = false) : ITransactionMatcher
{
    private readonly String[] _values = values;
    private readonly StringComparison _comparison = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

    public Boolean TryMatch(String description)
    {
        return _values.Any(v => description.Contains(v, _comparison));
    }
}
