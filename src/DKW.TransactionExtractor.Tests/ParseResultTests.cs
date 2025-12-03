using DKW.TransactionExtractor.Models;
using DKW.TransactionExtractor.Providers.CTFS;
using Xunit;

namespace DKW.TransactionExtractor.Tests;

public class ParseResultTests
{
    [Fact]
    public void Parse_SampleFile_PurchasesMatch()
    {
        var text = TestResources.GetTriangleStatement_2025_10_21();
        var parser = new CtfsMastercardTransactionParser();
        var context = new StatementContext { RawText = text, FileName = "Parse_SampleFile_PurchasesMatch.txt" };
        var result = parser.Parse(context);

        Assert.Equal(2348.84m, result.DeclaredPurchasesTotal);
        Assert.True(result.IsMatch, "Declared purchases should match computed purchases");
        Assert.Equal(result.DeclaredPurchasesTotal, result.ComputedPurchasesTotal);
    }

    [Fact]
    public void Parse_MismatchDetected_WhenDeclaredChanged()
    {
        var text = TestResources.GetTriangleStatement_2025_10_21();
        // tamper declared purchases line to create mismatch
        var tampered = text.Replace("Purchases 2,348.84", "Purchases 2,300.00");

        var parser = new CtfsMastercardTransactionParser();
        var context = new StatementContext { RawText = tampered, FileName = "Parse_MismatchDetected_WhenDeclaredChanged.txt" };
        var result = parser.Parse(context);

        Assert.Equal(2300.00m, result.DeclaredPurchasesTotal);
        Assert.False(result.IsMatch, "Mismatch should be detected after tampering declared total");
        Assert.NotEqual(result.DeclaredPurchasesTotal, result.ComputedPurchasesTotal);
    }
}
