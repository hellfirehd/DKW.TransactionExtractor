namespace DKW.TransactionExtractor;

/// <summary>
/// Interface for filtering transactions that should be excluded from purchases total calculations.
/// </summary>
public interface ITransactionFilter
{
    /// <summary>
    /// Determines whether a transaction should be excluded from the purchases total.
    /// </summary>
    /// <param name="transaction">The transaction to evaluate.</param>
    /// <returns>True if the transaction should be excluded; otherwise, false.</returns>
    Boolean ShouldExcludeFromPurchasesTotal(Models.Transaction transaction);
}
