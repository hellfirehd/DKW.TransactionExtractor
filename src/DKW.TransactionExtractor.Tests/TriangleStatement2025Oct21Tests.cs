using DKW.TransactionExtractor.Models;
using DKW.TransactionExtractor.Providers.CTFS;
using System;
using System.Linq;
using Xunit;

namespace DKW.TransactionExtractor.Tests;

public class TriangleStatement2025Oct21Tests
{
    [Fact]
    public void Parse_2025Oct21Statement_ParsesAllTransactions()
    {
        var text = TestResources.GetTriangleStatement_2025_10_21();

        if (String.IsNullOrEmpty(text))
        {
            // Skip test if file doesn't exist
            return;
        }

        var parser = new CtfsMastercardTransactionParser();
        var context = new ParseContext { Text = text, FileName = "2025-10-21-Triangle-WorldEliteMasterca.txt" };
        var result = parser.Parse(context);

        // The statement shows "Total purchases $2,348.84"
        Assert.Equal(2348.84m, result.DeclaredPurchasesTotal);

        // Check that we parsed transactions
        Assert.True(result.Transactions.Count > 0, $"No transactions parsed. Warnings: {result.Warnings.Count}");

        // Log all warnings if any
        if (result.Warnings.Count != 0)
        {
            var warningsText = String.Join("\n", result.Warnings.Select(w => $"Line {w.LineNumber}: {w.Message} - {w.RawText}"));
            Assert.Fail($"Found {result.Warnings.Count} warning(s):\n{warningsText}");
        }
    }

    [Fact]
    public void Parse_2025Oct21Statement_MatchesTotalPurchases()
    {
        var text = TestResources.GetTriangleStatement_2025_10_21();

        if (String.IsNullOrEmpty(text))
        {
            return;
        }

        var parser = new CtfsMastercardTransactionParser();
        var context = new ParseContext { Text = text, FileName = "2025-10-21-Triangle-WorldEliteMasterca.txt" };
        var result = parser.Parse(context);

        // Statement shows: Declared: $2,348.84
        Assert.Equal(2348.84m, result.DeclaredPurchasesTotal);
        Assert.Equal(2348.84m, result.ComputedPurchasesTotal);
        Assert.True(result.IsMatch,
            $"Declared: {result.DeclaredPurchasesTotal}, Computed: {result.ComputedPurchasesTotal}, Diff: {result.Difference}. " +
            $"Warnings: {result.Warnings.Count}, Transactions: {result.Transactions.Count}");
    }
}
