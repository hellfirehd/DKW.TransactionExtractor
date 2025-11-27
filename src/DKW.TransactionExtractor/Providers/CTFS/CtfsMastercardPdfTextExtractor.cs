using DKW.TransactionExtractor.Models;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using System.Text;
using System.Text.RegularExpressions;

namespace DKW.TransactionExtractor.Providers.CTFS;

public partial class CtfsMastercardPdfTextExtractor : IPdfTextExtractor
{
    // Mapping of special/extended characters to their ASCII equivalents
    // These mappings handle common character encoding issues from PDF extraction
    private static readonly Dictionary<Char, String> CharacterReplacements = new()
    {
        // Smart quotes to straight quotes
        { '\u2018', "'" },  // Left single quotation mark
        { '\u2019', "'" },  // Right single quotation mark (apostrophe)
        { '\u201C', "\"" }, // Left double quotation mark
        { '\u201D', "\"" }, // Right double quotation mark
        
        // Dashes to hyphens
        { '\u2013', "-" },  // En dash
        { '\u2014', "-" },  // Em dash
        
        // Special spaces to regular space
        { '\u00A0', " " },  // Non-breaking space
        { '\u2002', " " },  // En space
        { '\u2003', " " },  // Em space
        { '\u2009', " " },  // Thin space
        
        // Common currency and symbols
        { '\u00A2', "c" },  // Cent sign
        { '\u00A3', "GBP" }, // Pound sign
        { '\u00A5', "JPY" }, // Yen sign
        { '\u20AC', "EUR" }, // Euro sign
        
        // Accented characters to base equivalents (common in Canadian French)
        { '\u00E0', "a" },  // à
        { '\u00E1', "a" },  // á
        { '\u00E2', "a" },  // â
        { '\u00E8', "e" },  // è
        { '\u00E9', "e" },  // é
        { '\u00EA', "e" },  // ê
        { '\u00EB', "e" },  // ë
        { '\u00EE', "i" },  // î
        { '\u00EF', "i" },  // ï
        { '\u00F4', "o" },  // ô
        { '\u00F9', "u" },  // ù
        { '\u00FB', "u" },  // û
        { '\u00FC', "u" },  // ü
        { '\u00E7', "c" },  // ç
        { '\u00C0', "A" },  // À
        { '\u00C9', "E" },  // É
        { '\u00C8', "E" },  // È
    };

    // Regex to match invalid control characters (0x00-0x1F except tab, LF, CR)
    // These characters are not valid in JSON strings and will be removed if not mapped
    [GeneratedRegex(@"[\x00-\x08\x0B\x0C\x0E-\x1F]")]
    private static partial Regex InvalidJsonCharacters();

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

        // Apply character replacements and sanitization
        return SanitizeExtractedText(normalizedText);
    }

    /// <summary>
    /// Sanitizes extracted text by replacing special characters with ASCII equivalents
    /// and removing invalid control characters. This is exposed for testing purposes.
    /// </summary>
    /// <param name="text">The text to sanitize</param>
    /// <returns>Sanitized text safe for JSON serialization</returns>
    internal static String SanitizeExtractedText(String text)
    {
        if (String.IsNullOrEmpty(text))
        {
            return text;
        }

        // Apply character replacements to convert special characters to ASCII equivalents
        var sanitizedText = ApplyCharacterReplacements(text);

        // Remove any remaining invalid control characters that would cause JSON serialization failures
        // This includes null bytes (0x00) and other control characters except tab, LF, CR
        sanitizedText = InvalidJsonCharacters().Replace(sanitizedText, String.Empty);

        return sanitizedText;
    }

    /// <summary>
    /// Applies character replacements to convert special/extended characters to ASCII equivalents.
    /// This preserves readability while ensuring JSON compatibility.
    /// Any extended character (above ASCII 127) that is not explicitly mapped will be removed.
    /// </summary>
    private static String ApplyCharacterReplacements(String text)
    {
        if (String.IsNullOrEmpty(text))
        {
            return text;
        }

        var result = new StringBuilder(text.Length);

        foreach (var ch in text)
        {
            if (CharacterReplacements.TryGetValue(ch, out var replacement))
            {
                // Use the explicit mapping
                result.Append(replacement);
            }
            else if (ch is '\n' or '\r' or '\t')
            {
                // Preserve line breaks and tabs (essential for parsing)
                result.Append(ch);
            }
            else if (ch is >= (Char)32 and <= (Char)126)
            {
                // Keep standard ASCII printable characters (space through tilde)
                result.Append(ch);
            }
            // else: Remove all other extended/special characters (above 126 or below 32)
            // This includes unmapped Unicode characters that cause display issues
        }

        return result.ToString();
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