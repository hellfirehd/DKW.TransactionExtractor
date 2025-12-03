using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace DKW.TransactionExtractor.Models;

[DebuggerDisplay("{RawText}")]
public class Transaction
{
    /// <summary>
    /// Gets or sets the date of the statement this transaction came from
    /// </summary>
    public required DateTime StatementDate { get; init; }
    public required DateTime TransactionDate { get; init; }
    public required DateTime PostedDate { get; init; }
    public required String Description { get; init; } = String.Empty;
    public required Decimal Amount { get; set; }
    public String RawText { get; set; } = String.Empty;
    public Int32 StartLineNumber { get; set; }
    public TransactionInclusionStatus InclusionStatus { get; set; } = TransactionInclusionStatus.Undetermined;

    // Parameterless constructor to ensure tests that use object initializers without
    // setting the required date properties still produce a valid Transaction instance.
    [SetsRequiredMembers]
    public Transaction()
    {
        var dt = DateTime.Today;
        StatementDate = dt;
        TransactionDate = dt;
        PostedDate = dt;
    }
}
