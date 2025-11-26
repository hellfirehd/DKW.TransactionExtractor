namespace DKW.TransactionExtractor.Classification;

public interface ITransactionMatcher
{
    Boolean TryMatch(String description);
}
