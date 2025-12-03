using DKW.TransactionExtractor.Models;
using DKW.TransactionExtractor.Providers.CTFS;
using System;
using System.Linq;
using Xunit;

namespace DKW.TransactionExtractor.Tests;

public class RealStatementTests
{
    [Fact]
    public void Parse_2024March21Statement_ParsesAllTransactions()
    {
        var text = TestResources.GetTriangleStatement_2024_03_21();

        if (String.IsNullOrEmpty(text))
        {
            // Skip test if resource doesn't exist
            return;
        }

        var parser = new CtfsMastercardTransactionParser();
        var context = new StatementContext { RawText = text, FileName = "2024-03-21-Triangle-WorldEliteMastercard.txt" };
        var result = parser.Parse(context);

        // The statement shows "Total purchases $1,609.11"
        Assert.Equal(1609.11m, result.DeclaredPurchasesTotal);

        // Check that we parsed transactions
        Assert.True(result.Transactions.Count > 0, $"No transactions parsed. Warnings: {result.Warnings.Count}");

        // Log all warnings
        if (result.Warnings.Count != 0)
        {
            var warningsText = String.Join("\n", result.Warnings.Select(w => $"Line {w.LineNumber}: {w.Message} - {w.RawText}"));
            Assert.Fail($"Found {result.Warnings.Count} warning(s):\n{warningsText}");
        }

        // Check for the Feb 29 transaction
        var feb29Transaction = result.Transactions.FirstOrDefault(t =>
            t.TransactionDate.Month == 2 &&
            t.TransactionDate.Day == 29 &&
            t.Description.Contains("APPLE.COM/BILL"));

        Assert.NotNull(feb29Transaction);
        Assert.Equal(3.35m, feb29Transaction.Amount);
        Assert.Equal(new DateTime(2024, 2, 29), feb29Transaction.TransactionDate);

        // Verify we have 47 transactions (46 shown + 1 Feb 29)
        Assert.Equal(47, result.Transactions.Count);
    }

    [Fact]
    public void Parse_2024March21Statement_MatchesTotalPurchases()
    {
        var text = TestResources.GetTriangleStatement_2024_03_21();

        if (String.IsNullOrEmpty(text))
        {
            return;
        }

        var parser = new CtfsMastercardTransactionParser();
        var context = new StatementContext { RawText = text, FileName = "2024-03-21-Triangle-WorldEliteMastercard.txt" };
        var result = parser.Parse(context);

        // Statement shows:
        // Declared: $1,609.11
        // Interest charges: $60.01 (should be excluded)

        Assert.Equal(1609.11m, result.DeclaredPurchasesTotal);
        Assert.Equal(1609.11m, result.ComputedPurchasesTotal);
        Assert.True(result.IsMatch,
            $"Declared: {result.DeclaredPurchasesTotal}, Computed: {result.ComputedPurchasesTotal}, Diff: {result.Difference}. " +
            $"Warnings: {result.Warnings.Count}, Transactions: {result.Transactions.Count}");

        // Verify interest charges were excluded
        var interestTransaction = result.Transactions.FirstOrDefault(t => t.Description.Contains("INTEREST CHARGES"));
        if (interestTransaction != null)
        {
            Assert.Equal(TransactionInclusionStatus.Exclude, interestTransaction.InclusionStatus);
        }
    }
}
