namespace DKW.TransactionExtractor.Models;

/// <summary>
/// Contains classified transactions and their category summaries for output.
/// </summary>
public class TransactionOutput
{
    public Int32 StatementCount { get; set; }

    /// <summary>
    /// Gets or sets the list of category summaries.
    /// </summary>
    public List<CategorySummary> CategorySummaries { get; set; } = [];

    /// <summary>
    /// Gets or sets the list of classified transactions.
    /// </summary>
    public List<ClassifiedTransaction> Transactions { get; set; } = [];

    /// <summary>
    /// Gets or sets the timestamp when this output was generated.
    /// </summary>
    public DateTime GeneratedAt { get; set; }
}
