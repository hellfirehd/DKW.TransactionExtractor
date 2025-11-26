namespace DKW.TransactionExtractor.Classification;

public class ContainsMatcher : ITransactionMatcher
{
    private readonly String[] _values;
    private readonly StringComparison _comparison;

    public ContainsMatcher(String[] values, Boolean caseSensitive = false)
    {
        _values = values;
        _comparison = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
    }

    public Boolean TryMatch(String description)
    {
        return _values.Any(v => description.Contains(v, _comparison));
    }
}
