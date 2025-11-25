
namespace DKW.TransactionExtractor.Models;

public class ParseResult
{
    public List<Transaction> Transactions { get; set; } = [];
    public Decimal DeclaredPurchasesTotal { get; set; }
    public Decimal ComputedPurchasesTotal { get; set; }
    public Decimal Difference { get; set; }
    public Boolean IsMatch { get; set; }

    public String FilePath { get; set; } = String.Empty;
    public String FileName { get; set; } = String.Empty;
    
    public List<ParseWarning> Warnings { get; set; } = [];
    
    /// <summary>
    /// Number of transactions excluded from the purchases total calculation (e.g., fees, interest charges).
    /// </summary>
    public Int32 ExcludedCount { get; set; }
    public DateTime StatementDate { get; set; }
}

public class ParseWarning
{
    public Int32 LineNumber { get; set; }
    public String Message { get; set; } = String.Empty;
    public String RawText { get; set; } = String.Empty;
}
