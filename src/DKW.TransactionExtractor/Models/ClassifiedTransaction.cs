using System.Diagnostics;

namespace DKW.TransactionExtractor.Models;

[DebuggerDisplay("{Transaction.Description}: {Transaction.Amount} - {CategoryId}")]

public class ClassifiedTransaction
{
    public required Transaction Transaction { get; init; }
    public String CategoryId { get; set; } = String.Empty;
    public String CategoryName { get; set; } = String.Empty;
    public String Comment { get; set; } = String.Empty;

    /// <summary>
    /// The type of matcher that matched this transaction (e.g. "ExactMatch", "Contains", "Regex").
    /// Empty or null when the transaction was classified manually without adding a matcher.
    /// </summary>
    public String? MatcherType { get; set; }
}
