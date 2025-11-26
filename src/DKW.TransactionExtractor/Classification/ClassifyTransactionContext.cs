using DKW.TransactionExtractor.Models;

namespace DKW.TransactionExtractor.Classification;

public record ClassifyTransactionContext(
    Transaction Transaction,
    Int32 CurrentIndex,
    Int32 TotalCount)
{
    /// <summary>
    /// Gets a formatted progress string (e.g., "14 of 48").
    /// </summary>
    public String ProgressText => $"{CurrentIndex} of {TotalCount}";

    /// <summary>
    /// Gets the number of transactions remaining to be classified.
    /// </summary>
    public Int32 RemainingCount => TotalCount - CurrentIndex;

    /// <summary>
    /// Gets the progress as a percentage (0-100).
    /// </summary>
    public Double ProgressPercentage => TotalCount > 0 ? (CurrentIndex / (Double)TotalCount) * 100 : 0;
}
