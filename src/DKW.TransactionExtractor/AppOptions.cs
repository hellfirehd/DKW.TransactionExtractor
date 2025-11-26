namespace DKW.TransactionExtractor;

public class AppOptions
{
    public String FolderPath { get; set; } = String.Empty;
    
    /// <summary>
    /// Optional start date to filter statements (inclusive). 
    /// Only statements on or after this date will be processed.
    /// If null, no start date filtering is applied.
    /// </summary>
    public DateTime? StartDate { get; set; } = null;
    
    /// <summary>
    /// Optional end date to filter statements (exclusive). 
    /// Only statements before this date will be processed.
    /// If null, no end date filtering is applied.
    /// </summary>
    public DateTime? EndDate { get; set; } = null;

    /// <summary>
    /// When true, writes the extracted PDF text to a .txt file with the same name as the PDF.
    /// Useful for debugging parsing issues.
    /// </summary>
    public Boolean WriteExtractedText { get; set; } = false;

    /// <summary>
    /// Path to the category configuration JSON file.
    /// </summary>
    public String CategoryConfigPath { get; set; } = "categories.json";

    /// <summary>
    /// Output format for classified transactions: "Csv" or "Json".
    /// </summary>
    public String OutputFormat { get; set; } = "Csv";

    /// <summary>
    /// Directory path where classified transaction files will be written.
    /// </summary>
    public String OutputPath { get; set; } = "output";

    /// <summary>
    /// When true, includes uncategorized transactions in the output files (both transactions and summaries).
    /// When false, filters out uncategorized transactions from all output.
    /// Default is true.
    /// </summary>
    public Boolean OutputUncategorized { get; set; } = true;
}
