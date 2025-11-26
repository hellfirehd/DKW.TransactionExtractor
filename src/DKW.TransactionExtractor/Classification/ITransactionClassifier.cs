using DKW.TransactionExtractor.Models;

namespace DKW.TransactionExtractor.Classification;

public interface ITransactionClassifier
{
    ClassificationResult ClassifyTransactions(List<Transaction> transactions);
}
