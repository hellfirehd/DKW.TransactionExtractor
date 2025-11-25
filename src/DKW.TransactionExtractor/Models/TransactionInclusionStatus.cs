namespace DKW.TransactionExtractor.Models;

/// <summary>
/// Indicates whether a transaction should be included in the purchases total calculation.
/// </summary>
public enum TransactionInclusionStatus
{
    /// <summary>
    /// The transaction's inclusion status has not been determined.
    /// </summary>
    Undetermined = 0,
    
    /// <summary>
    /// The transaction should be included in the purchases total.
    /// </summary>
    Include = 1,
    
    /// <summary>
    /// The transaction should be excluded from the purchases total (e.g., fees, interest charges).
    /// </summary>
    Exclude = 2
}
