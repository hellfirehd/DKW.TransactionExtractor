using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using System.Text;
using DKW.TransactionExtractor.Models;

namespace DKW.TransactionExtractor.Providers.CTFS;

public class CtfsMastercardPdfTextExtractor : IPdfTextExtractor
{
    public String ExtractTextFromPdf(String filePath)
    {
        var text = new StringBuilder();

        using (var reader = new PdfReader(filePath))
        using (var document = new PdfDocument(reader))
        {
            for (var i = 1; i <= document.GetNumberOfPages(); i++)
            {
                var page = document.GetPage(i);
                var strategy = new SimpleTextExtractionStrategy();
                var pageText = iText.Kernel.Pdf.Canvas.Parser.PdfTextExtractor.GetTextFromPage(page, strategy);
                text.AppendLine(pageText);
            }
        }

        // Normalize line endings to handle mixed CRLF/LF/CR from PDF extraction
        // This ensures consistent line endings regardless of PDF source
        var extractedText = text.ToString();
        var normalizedText = extractedText.Replace("\r\n", "\n").Replace("\r", "\n");
        
        return normalizedText;
    }

    // Helper method left as instance member but not part of the interface
    public IEnumerable<Transaction> ExtractTransactions(String filePath, ITransactionParser? parser = null)
    {
        var text = ExtractTextFromPdf(filePath);
        var fileName = System.IO.Path.GetFileName(filePath);
        var usedParser = parser ?? new CtfsMastercardTransactionParser();
        var context = new ParseContext { Text = text, FileName = fileName };
        var result = usedParser.Parse(context);
        return result.Transactions;
    }
}