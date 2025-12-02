using DKW.TransactionExtractor.Models;
using System.Diagnostics.CodeAnalysis;

namespace DKW.TransactionExtractor.Classification;

public record TransactionContext(
    [NotNull] Transaction Transaction,
    Int32 CurrentIndex,
    Int32 TotalCount)
{
    /// <summary>
    /// Gets or sets an optional comment for this transaction.
    /// </summary>
    public String? Comment { get; set; }

    /// <summary>
    /// Gets or sets the selected category ID for this transaction.
    /// </summary>
    public String? CategoryId { get; set; }

    /// <summary>
    /// Gets or sets the selected category name for this transaction.
    /// </summary>
    public String? CategoryName { get; set; }
}
