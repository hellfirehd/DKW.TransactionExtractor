namespace DKW.TransactionExtractor;

public class ParserOptions
{
    public Decimal? DifferenceTolerance { get; set; }
    
    /// <summary>
    /// Regular expression patterns for transaction descriptions that should be excluded from purchases total.
    /// Patterns are case-insensitive.
    /// </summary>
    /// <example>
    /// ["INTEREST CHARGES", "ANNUAL FEE", "OVER LIMIT FEE"]
    /// </example>
    public List<String> ExclusionPatterns { get; set; } = [];
}
