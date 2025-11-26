using DKW.TransactionExtractor.Models;

namespace DKW.TransactionExtractor.Classification;

public record ClassificationResult(
    List<ClassifiedTransaction> ClassifiedTransactions,
    Boolean RequestedEarlyExit = false
);
