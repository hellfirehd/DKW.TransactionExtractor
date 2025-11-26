using DKW.TransactionExtractor.Models;
using DKW.TransactionExtractor.Providers.CTFS;
using Xunit;

namespace DKW.TransactionExtractor.Tests;

/// <summary>
/// Tests for PDF text extraction character sanitization and replacement.
/// Verifies that special characters, control characters, and extended Unicode
/// are properly converted to ASCII equivalents or removed.
/// </summary>
public class PdfCharacterSanitizationTests
{
    private readonly CtfsMastercardTransactionParser _parser;

    public PdfCharacterSanitizationTests()
    {
        _parser = new CtfsMastercardTransactionParser();
    }

    /// <summary>
    /// Helper method to sanitize text and then parse it, simulating the full extraction flow
    /// </summary>
    private ParseResult ParseWithSanitization(String rawText, String fileName = "test.txt")
    {
        // Simulate what happens in ExtractTextFromPdf: sanitize then parse
        var sanitizedText = CtfsMastercardPdfTextExtractor.SanitizeExtractedText(rawText);
        var context = new ParseContext { Text = sanitizedText, FileName = fileName };
        return _parser.Parse(context);
    }

    #region Smart Quotes and Apostrophes

    [Fact]
    public void Sanitize_SmartApostrophe_ConvertedToStraightApostrophe()
    {
        // Simulate PDF text with Unicode smart apostrophe (U+2019)
        var textWithSmartQuote = "Statement date: October 21, 2025\n" +
                                 "Oct 15 Oct 16 MCDONALD\u2019S #40610 KELOWNA BC 12.50\n";

        var result = ParseWithSanitization(textWithSmartQuote);
        var tx = result.Transactions.Single();

        Assert.Contains("MCDONALD'S", tx.Description);
        Assert.DoesNotContain("\u2019", tx.Description); // No smart quote
    }

    [Fact]
    public void Sanitize_LeftSingleQuotationMark_ConvertedToStraightApostrophe()
    {
        var textWithLeftQuote = "Statement date: October 21, 2025\n" +
                                "Oct 15 Oct 16 WENDY\u2018S #6517 KELOWNA BC 8.99\n";

        var result = ParseWithSanitization(textWithLeftQuote);
        var tx = result.Transactions.Single();

        Assert.Contains("WENDY'S", tx.Description);
        Assert.DoesNotContain("\u2018", tx.Description);
    }

    [Fact]
    public void Sanitize_DoubleQuotationMarks_ConvertedToStraightQuotes()
    {
        var textWithSmartDoubleQuotes = "Statement date: October 21, 2025\n" +
                                        "Oct 15 Oct 16 \u201CPIZZA PLACE\u201D KELOWNA BC 25.00\n";

        var result = ParseWithSanitization(textWithSmartDoubleQuotes);
        var tx = result.Transactions.Single();

        Assert.Contains("\"PIZZA PLACE\"", tx.Description);
        Assert.DoesNotContain("\u201C", tx.Description);
        Assert.DoesNotContain("\u201D", tx.Description);
    }

    #endregion

    #region Dashes

    [Fact]
    public void Sanitize_EnDash_ConvertedToHyphen()
    {
        var textWithEnDash = "Statement date: October 21, 2025\n" +
                             "Oct 15 Oct 16 STORE\u2013A KELOWNA BC 15.00\n";

        var result = ParseWithSanitization(textWithEnDash);
        var tx = result.Transactions.Single();

        Assert.Contains("STORE-A", tx.Description);
        Assert.DoesNotContain("\u2013", tx.Description);
    }

    [Fact]
    public void Sanitize_EmDash_ConvertedToHyphen()
    {
        var textWithEmDash = "Statement date: October 21, 2025\n" +
                             "Oct 15 Oct 16 COFFEE SHOP\u2014DOWNTOWN KELOWNA BC 5.50\n";

        var result = ParseWithSanitization(textWithEmDash);
        var tx = result.Transactions.Single();

        Assert.Contains("COFFEE SHOP-DOWNTOWN", tx.Description);
        Assert.DoesNotContain("\u2014", tx.Description);
    }

    #endregion

    #region Special Spaces

    [Fact]
    public void Sanitize_NonBreakingSpace_ConvertedToRegularSpace()
    {
        var textWithNbsp = "Statement date: October 21, 2025\n" +
                           "Oct 15 Oct 16 STORE\u00A0NAME KELOWNA BC 10.00\n";

        var result = ParseWithSanitization(textWithNbsp);
        var tx = result.Transactions.Single();

        Assert.Contains("STORE NAME", tx.Description);
        Assert.DoesNotContain("\u00A0", tx.Description);
    }

    [Fact]
    public void Sanitize_EnSpace_ConvertedToRegularSpace()
    {
        var textWithEnSpace = "Statement date: October 21, 2025\n" +
                              "Oct 15 Oct 16 COFFEE\u2002SHOP KELOWNA BC 7.25\n";

        var result = ParseWithSanitization(textWithEnSpace);
        var tx = result.Transactions.Single();

        Assert.Contains("COFFEE SHOP", tx.Description);
        Assert.DoesNotContain("\u2002", tx.Description);
    }

    [Fact]
    public void Sanitize_ThinSpace_ConvertedToRegularSpace()
    {
        var textWithThinSpace = "Statement date: October 21, 2025\n" +
                                "Oct 15 Oct 16 RESTAURANT\u2009A KELOWNA BC 45.00\n";

        var result = ParseWithSanitization(textWithThinSpace);
        var tx = result.Transactions.Single();

        Assert.Contains("RESTAURANT A", tx.Description);
        Assert.DoesNotContain("\u2009", tx.Description);
    }

    #endregion

    #region French Accented Characters

    [Fact]
    public void Sanitize_FrenchAccentedCharacters_ConvertedToBaseLetters()
    {
        var textWithAccents = "Statement date: October 21, 2025\n" +
                              "Oct 15 Oct 16 CAF\u00C9 MONTR\u00C9AL QUEBEC QC 18.50\n";

        var result = ParseWithSanitization(textWithAccents);
        var tx = result.Transactions.Single();

        Assert.Contains("CAFE MONTREAL", tx.Description);
        Assert.DoesNotContain("\u00C9", tx.Description); // No É
    }

    [Fact]
    public void Sanitize_VariousFrenchAccents_AllConvertedProperly()
    {
        var textWithVariousAccents = "Statement date: October 21, 2025\n" +
                                     "Oct 15 Oct 16 \u00C0 LA MODE \u00E7A VA \u00C8TRE KELOWNA BC 30.00\n";

        var result = ParseWithSanitization(textWithVariousAccents);
        var tx = result.Transactions.Single();

        // Should convert: À->A, ç->c, È->E
        Assert.Contains("A LA MODE cA VA ETRE", tx.Description);
    }

    #endregion

    #region Control Characters and Null Bytes

    [Fact]
    public void Sanitize_NullBytes_RemovedFromText()
    {
        // Simulate the actual issue: null byte between characters
        var textWithNullByte = "Statement date: October 21, 2025\n" +
                               "Oct 15 Oct 16 MCDONALD\x00'S #40610 KELOWNA BC 12.50\n";

        var result = ParseWithSanitization(textWithNullByte);
        var tx = result.Transactions.Single();

        // Null byte should be removed, leaving the apostrophe
        Assert.Contains("MCDONALD'S", tx.Description);
        // Verify no null bytes exist by checking the byte array
        Assert.DoesNotContain<Char>('\0', tx.Description.ToCharArray());
    }

    [Fact]
    public void Sanitize_VariousControlCharacters_RemovedFromText()
    {
        // Test various control characters (0x01-0x08, 0x0B, 0x0C, 0x0E-0x1F)
        var textWithControlChars = "Statement date: October 21, 2025\n" +
                                   "Oct 15 Oct 16 STORE\x01NAME\x02TEST\x03 KELOWNA BC 15.00\n";

        var result = ParseWithSanitization(textWithControlChars);
        var tx = result.Transactions.Single();

        // All control characters should be removed
        Assert.Contains("STORENAMETEST", tx.Description);
        // Verify no control characters exist
        Assert.DoesNotContain<Char>('\x01', tx.Description.ToCharArray());
        Assert.DoesNotContain<Char>('\x02', tx.Description.ToCharArray());
        Assert.DoesNotContain<Char>('\x03', tx.Description.ToCharArray());
    }

    #endregion

    #region Combined Scenarios

    [Fact]
    public void Sanitize_MultipleSpecialCharacters_AllHandledCorrectly()
    {
        // Real-world scenario: smart quotes, accents, special spaces, and null bytes
        var complexText = "Statement date: October 21, 2025\n" +
                          "Oct 15 Oct 16 MCDONALD\x00\u2019S\u00A0\u2013\u00A0CAF\u00C9 KELOWNA BC 25.00\n";

        var result = ParseWithSanitization(complexText);
        var tx = result.Transactions.Single();

        // Should be: MCDONALD'S - CAFE
        Assert.Contains("MCDONALD'S", tx.Description);
        Assert.Contains("CAFE", tx.Description);
        Assert.Contains("-", tx.Description);

        // Should not contain any special characters - check character arrays
        Assert.DoesNotContain<Char>('\0', tx.Description.ToCharArray());
        Assert.DoesNotContain<Char>('\u2019', tx.Description.ToCharArray());
        Assert.DoesNotContain<Char>('\u00A0', tx.Description.ToCharArray());
        Assert.DoesNotContain<Char>('\u2013', tx.Description.ToCharArray());
        Assert.DoesNotContain<Char>('\u00C9', tx.Description.ToCharArray());
    }

    [Fact]
    public void Sanitize_LineEndingsMixedWithSpecialCharacters_BothHandledCorrectly()
    {
        // Test that line ending normalization happens before character replacement
        var mixedText = "Statement date: October 21, 2025\r\n" +
                        "Oct 15 Oct 16 WENDY\u2019S KELOWNA BC\r" +
                        "8.99\n";

        // Manually normalize line endings like ExtractTextFromPdf does
        var normalizedText = mixedText.Replace("\r\n", "\n").Replace("\r", "\n");
        var result = ParseWithSanitization(normalizedText);
        var tx = result.Transactions.Single();

        Assert.Equal(8.99m, tx.Amount);
        Assert.Contains("WENDY'S", tx.Description);
    }

    #endregion

    #region JSON Serialization Safety

    [Fact]
    public void Sanitize_DescriptionWithSpecialCharacters_SafeForJsonSerialization()
    {
        var textWithSpecialChars = "Statement date: October 21, 2025\n" +
                                   "Oct 15 Oct 16 STORE\x00NAME\u2019S\u00A0\u2013\u00A0CAF\u00C9 KELOWNA BC 20.00\n";

        var result = ParseWithSanitization(textWithSpecialChars);
        var tx = result.Transactions.Single();

        // Verify the description can be serialized to JSON without exceptions
        var json = System.Text.Json.JsonSerializer.Serialize(tx);
        Assert.NotNull(json);
        Assert.DoesNotContain("\\u0000", json); // No null bytes in JSON

        // Verify it can be deserialized back
        var deserialized = System.Text.Json.JsonSerializer.Deserialize<Transaction>(json);
        Assert.NotNull(deserialized);
        Assert.Equal(tx.Description, deserialized.Description);
    }

    #endregion

    #region Currency Symbols

    [Fact]
    public void Sanitize_CurrencySymbols_ConvertedToTextEquivalents()
    {
        var textWithCurrency = "Statement date: October 21, 2025\n" +
                               "Oct 15 Oct 16 EUR\u20AC5.00 GBP\u00A35.00 STORE KELOWNA BC 10.00\n";

        var result = ParseWithSanitization(textWithCurrency);
        var tx = result.Transactions.Single();

        // Currency symbols should be replaced with text codes
        Assert.Contains("EUREUR", tx.Description); // EUR + EUR replacement
        Assert.Contains("GBPGBP", tx.Description); // GBP + GBP replacement
        Assert.DoesNotContain("\u20AC", tx.Description); // No Euro symbol
        Assert.DoesNotContain("\u00A3", tx.Description); // No Pound symbol
    }

    #endregion

    #region Real-World PDF Extraction Scenarios

    [Theory]
    [InlineData("MCDONALD\x00'S", "MCDONALD'S")] // Null byte before apostrophe
    [InlineData("WENDY\x00'S", "WENDY'S")]       // Null byte before apostrophe
    [InlineData("DOMINO\x00'S", "DOMINO'S")]     // Null byte before apostrophe
    [InlineData("HECTOR\u2019S", "HECTOR'S")]    // Smart quote
    [InlineData("DENNY\u2019S", "DENNY'S")]      // Smart quote
    public void Sanitize_CommonRestaurantNames_ConvertedCorrectly(String input, String expected)
    {
        var text = $"Statement date: October 21, 2025\n" +
                   $"Oct 15 Oct 16 {input} KELOWNA BC 15.00\n";

        var result = ParseWithSanitization(text);
        var tx = result.Transactions.Single();

        Assert.Contains(expected, tx.Description);
    }

    #endregion

    #region Direct Sanitization Tests

    [Fact]
    public void SanitizeExtractedText_EmptyString_ReturnsEmpty()
    {
        var result = CtfsMastercardPdfTextExtractor.SanitizeExtractedText(String.Empty);
        Assert.Equal(String.Empty, result);
    }

    [Fact]
    public void SanitizeExtractedText_NullString_ReturnsNull()
    {
        var result = CtfsMastercardPdfTextExtractor.SanitizeExtractedText(null);
        Assert.Null(result);
    }

    [Fact]
    public void SanitizeExtractedText_PlainAsciiText_RemainsUnchanged()
    {
        var input = "MCDONALD'S #40610 KELOWNA BC";
        var result = CtfsMastercardPdfTextExtractor.SanitizeExtractedText(input);
        Assert.Equal(input, result);
    }

    [Fact]
    public void SanitizeExtractedText_SmartQuoteToApostrophe_Replaced()
    {
        var input = "MCDONALD\u2019S";
        var result = CtfsMastercardPdfTextExtractor.SanitizeExtractedText(input);
        Assert.Equal("MCDONALD'S", result);
    }

    [Fact]
    public void SanitizeExtractedText_NullByteWithApostrophe_NullByteRemoved()
    {
        var input = "MCDONALD\x00'S";
        var result = CtfsMastercardPdfTextExtractor.SanitizeExtractedText(input);
        Assert.Equal("MCDONALD'S", result);
    }

    [Fact]
    public void SanitizeExtractedText_MultipleReplacements_AllApplied()
    {
        var input = "CAF\u00C9\u00A0\u2013\u00A0\u201CSPECIAL\u201D";
        var result = CtfsMastercardPdfTextExtractor.SanitizeExtractedText(input);
        Assert.Equal("CAFE - \"SPECIAL\"", result);
    }

    #endregion

    #region Unmapped Extended Characters

    [Fact]
    public void SanitizeExtractedText_UnmappedExtendedCharacter_RemovedFromText()
    {
        // Test with an extended character that is not in the mapping dictionary
        var input = "WENDY\u0192S RESTAURANT"; // ƒ (Latin small letter f with hook)
        var result = CtfsMastercardPdfTextExtractor.SanitizeExtractedText(input);
        Assert.Equal("WENDYS RESTAURANT", result);
        Assert.DoesNotContain("\u0192", result);
    }

    [Fact]
    public void SanitizeExtractedText_MixedMappedAndUnmappedCharacters_OnlyMappedKept()
    {
        // Mix of mapped (smart quote) and unmapped extended characters
        var input = "STORE\u2019S\u0192TEST"; // smart quote + Latin f with hook
        var result = CtfsMastercardPdfTextExtractor.SanitizeExtractedText(input);
        Assert.Equal("STORE'STEST", result); // Smart quote mapped to ', unmapped char removed
    }

    [Theory]
    [InlineData("TEST\u0100ABC", "TESTABC")]  // Latin A with macron
    [InlineData("TEST\u0152ABC", "TESTABC")]  // OE ligature
    [InlineData("TEST\u0160ABC", "TESTABC")]  // S with caron
    public void SanitizeExtractedText_VariousUnmappedExtendedCharacters_AllRemoved(String input, String expected)
    {
        var result = CtfsMastercardPdfTextExtractor.SanitizeExtractedText(input);
        Assert.Equal(expected, result);
    }

    #endregion
}
