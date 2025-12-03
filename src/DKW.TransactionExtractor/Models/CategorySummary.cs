using System.Diagnostics;

namespace DKW.TransactionExtractor.Models;

/// <summary>
/// Summary statistics for a category of transactions.
/// </summary>
[DebuggerDisplay("{CategoryId}: {CategoryName}")]
public class CategorySummary
{
    /// <summary>
    /// Gets or sets the category identifier.
    /// </summary>
    public String CategoryId { get; set; } = String.Empty;

    /// <summary>
    /// Gets or sets the category display name.
    /// </summary>
    public String CategoryName { get; set; } = String.Empty;

    /// <summary>
    /// Gets or sets the number of transactions in this category.
    /// </summary>
    public Int32 TransactionCount { get; set; }

    /// <summary>
    /// Gets or sets the total amount of all transactions in this category.
    /// </summary>
    public Decimal TotalAmount { get; set; }
}
