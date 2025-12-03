namespace DKW.TransactionExtractor.Models;

/// <summary>
/// Context information for parsing a credit card statement.
/// </summary>
public class StatementContext
{
    /// <summary>
    /// The raw text content of the statement to parse.
    /// </summary>
    public String RawText { get; init; } = String.Empty;

    /// <summary>
    /// The filename (without path) of the statement being parsed.
    /// </summary>
    public String FileName { get; init; } = String.Empty;
}
