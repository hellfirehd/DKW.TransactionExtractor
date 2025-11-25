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
}
