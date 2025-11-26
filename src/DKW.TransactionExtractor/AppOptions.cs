namespace DKW.TransactionExtractor;

public class AppOptions
{
    public String FolderPath { get; set; } = String.Empty;
    public Int32[] Years { get; set; } = [];

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
}
