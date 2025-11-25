namespace DKW.TransactionExtractor.Models;

public class Transaction
{
    public DateTime TransactionDate { get; set; }
    public DateTime? PostedDate { get; set; }
    public String Description { get; set; } = String.Empty;
    public Decimal Amount { get; set; }
    public String RawText { get; set; } = String.Empty;
    public Int32 StartLineNumber { get; set; }
    
    /// <summary>
    /// Indicates whether this transaction should be included in purchases total calculations.
    /// </summary>
    public TransactionInclusionStatus InclusionStatus { get; set; } = TransactionInclusionStatus.Undetermined;
}
