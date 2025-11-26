namespace DKW.TransactionExtractor.Classification;

public class ContainsMatcher : ITransactionMatcher
{
    private readonly String _value;
    private readonly StringComparison _comparison;

    public ContainsMatcher(String value, Boolean caseSensitive = false)
    {
        _value = value;
        _comparison = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
    }

    public Boolean TryMatch(String description)
    {
        return description.Contains(_value, _comparison);
    }
}
