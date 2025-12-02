using DKW.TransactionExtractor.Models;

namespace DKW.TransactionExtractor.Classification;

public interface ITransactionMatcher
{
    Boolean TryMatch(Transaction transaction);
}
