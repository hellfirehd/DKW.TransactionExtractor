using DKW.TransactionExtractor.Models;
using DKW.TransactionExtractor.Providers.CTFS;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using Xunit;

namespace DKW.TransactionExtractor.Tests;

public class TransactionExclusionTests
{
    private static CtfsMastercardTransactionParser CreateParser(String[] exclusionPatterns)
    {
        var options = Options.Create(new ParserOptions
        {
            DifferenceTolerance = 0.01m,
            ExclusionPatterns = [.. exclusionPatterns]
        });

        var filter = new DefaultTransactionFilter(options, Microsoft.Extensions.Logging.Abstractions.NullLogger<DefaultTransactionFilter>.Instance);
        return new CtfsMastercardTransactionParser(
            options,
            Microsoft.Extensions.Logging.Abstractions.NullLogger<CtfsMastercardTransactionParser>.Instance,
            filter);
    }

    [Fact]
    public void Parse_InterestCharges_ExcludedFromPurchasesTotal()
    {
        // In real credit card statements, "Purchases" total excludes fees and interest
        // So if declared is $75, and we have $50 + $25 store purchases + $25 interest charges,
        // we should exclude the interest charges to match the declared total
        var text = "Statement date: October 21, 2025\n" +
                   "Purchases 75.00\n" +
                   "Oct 15 Oct 15 STORE PURCHASE 50.00\n" +
                   "Oct 16 Oct 16 INTEREST CHARGES 25.00\n" +
                   "Oct 17 Oct 17 ANOTHER STORE 25.00\n";

        var parser = CreateParser(["INTEREST CHARGES"]);
        var context = new ParseContext { Text = text, FileName = "test-statement.txt" };
        var result = parser.Parse(context);

        Assert.Equal(3, result.Transactions.Count);
        Assert.Equal(75.00m, result.DeclaredPurchasesTotal);
        Assert.Equal(75.00m, result.ComputedPurchasesTotal); // 50 + 25, excluding interest charges
        Assert.Equal(1, result.ExcludedCount);
        Assert.True(result.IsMatch);
        Assert.Equal("test-statement.txt", result.FileName);

        // Verify InclusionStatus is set correctly
        Assert.Equal(TransactionInclusionStatus.Include, result.Transactions[0].InclusionStatus);
        Assert.Equal(TransactionInclusionStatus.Exclude, result.Transactions[1].InclusionStatus);
        Assert.Equal(TransactionInclusionStatus.Include, result.Transactions[2].InclusionStatus);
    }

    [Fact]
    public void Parse_MultipleExclusionPatterns_AllExcluded()
    {
        var text = "Statement date: October 21, 2025\n" +
                   "Purchases 100.00\n" +
                   "Oct 15 Oct 15 STORE PURCHASE 50.00\n" +
                   "Oct 16 Oct 16 INTEREST CHARGES 10.00\n" +
                   "Oct 17 Oct 17 ANOTHER STORE 50.00\n" +
                   "Oct 18 Oct 18 ANNUAL FEE 120.00\n";

        var parser = CreateParser(["INTEREST CHARGES", "ANNUAL FEE"]);
        var context = new ParseContext { Text = text, FileName = "multi-exclusion.txt" };
        var result = parser.Parse(context);

        Assert.Equal(4, result.Transactions.Count);
        Assert.Equal(100.00m, result.DeclaredPurchasesTotal);
        Assert.Equal(100.00m, result.ComputedPurchasesTotal); // Only the two store purchases
        Assert.Equal(2, result.ExcludedCount);
        Assert.True(result.IsMatch);

        // Verify InclusionStatus is set correctly
        Assert.Equal(TransactionInclusionStatus.Include, result.Transactions[0].InclusionStatus);
        Assert.Equal(TransactionInclusionStatus.Exclude, result.Transactions[1].InclusionStatus);
        Assert.Equal(TransactionInclusionStatus.Include, result.Transactions[2].InclusionStatus);
        Assert.Equal(TransactionInclusionStatus.Exclude, result.Transactions[3].InclusionStatus);
    }

    [Fact]
    public void Parse_NoExclusionPatterns_AllIncluded()
    {
        var text = "Statement date: October 21, 2025\n" +
                   "Purchases 100.00\n" +
                   "Oct 15 Oct 15 STORE PURCHASE 50.00\n" +
                   "Oct 16 Oct 16 INTEREST CHARGES 25.00\n" +
                   "Oct 17 Oct 17 ANOTHER STORE 25.00\n";

        var parser = CreateParser([]);
        var context = new ParseContext { Text = text, FileName = "no-exclusion.txt" };
        var result = parser.Parse(context);

        Assert.Equal(3, result.Transactions.Count);
        Assert.Equal(100.00m, result.DeclaredPurchasesTotal);
        Assert.Equal(100.00m, result.ComputedPurchasesTotal);
        Assert.Equal(0, result.ExcludedCount);
        Assert.True(result.IsMatch);

        // Verify InclusionStatus is set correctly
        Assert.Equal(TransactionInclusionStatus.Include, result.Transactions[0].InclusionStatus);
        Assert.Equal(TransactionInclusionStatus.Include, result.Transactions[1].InclusionStatus);
        Assert.Equal(TransactionInclusionStatus.Include, result.Transactions[2].InclusionStatus);
    }

    [Fact]
    public void Parse_RegexPattern_MatchesPartialDescription()
    {
        var text = "Statement date: October 21, 2025\n" +
                   "Purchases 75.00\n" +
                   "Oct 15 Oct 15 STORE PURCHASE 50.00\n" +
                   "Oct 16 Oct 16 MONTHLY INTEREST CHARGES FEE 25.00\n" +
                   "Oct 17 Oct 17 ANOTHER STORE 25.00\n";

        var parser = CreateParser(["INTEREST"]);
        var context = new ParseContext { Text = text, FileName = "regex-pattern.txt" };
        var result = parser.Parse(context);

        Assert.Equal(3, result.Transactions.Count);
        Assert.Equal(75.00m, result.ComputedPurchasesTotal); // Excludes the interest charges transaction
        Assert.Equal(1, result.ExcludedCount);
        Assert.True(result.IsMatch);

        // Verify InclusionStatus is set correctly
        Assert.Equal(TransactionInclusionStatus.Include, result.Transactions[0].InclusionStatus);
        Assert.Equal(TransactionInclusionStatus.Exclude, result.Transactions[1].InclusionStatus);
        Assert.Equal(TransactionInclusionStatus.Include, result.Transactions[2].InclusionStatus);
    }

    [Fact]
    public void Parse_NegativeAmounts_NotExcluded()
    {
        var text = "Statement date: October 21, 2025\n" +
                   "Purchases 50.00\n" +
                   "Oct 15 Oct 15 STORE PURCHASE 50.00\n" +
                   "Oct 16 Oct 16 INTEREST CHARGES REFUND -25.00\n";

        var parser = CreateParser(["INTEREST CHARGES"]);
        var context = new ParseContext { Text = text, FileName = "negative-amounts.txt" };
        var result = parser.Parse(context);

        Assert.Equal(2, result.Transactions.Count);
        Assert.Equal(50.00m, result.ComputedPurchasesTotal); // Only positive amounts count as purchases
        Assert.Equal(0, result.ExcludedCount); // Negative amounts are not excluded by filter (already not in purchases)
        Assert.True(result.IsMatch);

        // Verify InclusionStatus: positive purchase included, negative amount excluded (not a purchase)
        Assert.Equal(TransactionInclusionStatus.Include, result.Transactions[0].InclusionStatus);
        Assert.Equal(TransactionInclusionStatus.Exclude, result.Transactions[1].InclusionStatus);
    }

    [Fact]
    public void Parse_RealWorldScenario_WithFeesAndInterest()
    {
        // Real-world credit card statement scenario:
        // - Regular purchases: $1,500
        // - Interest charges: $45.50
        // - Annual fee: $120
        // - Statement says "Purchases: $1,500.00" (excluding fees/interest)
        var text = "Statement date: November 15, 2025\n" +
                   "Purchases 1,500.00\n" +
                   "Nov 01 Nov 02 GROCERY STORE 125.50\n" +
                   "Nov 03 Nov 03 GAS STATION 75.00\n" +
                   "Nov 05 Nov 06 RESTAURANT 89.99\n" +
                   "Nov 08 Nov 08 ONLINE STORE 310.51\n" +
                   "Nov 10 Nov 10 INTEREST CHARGES 45.50\n" +
                   "Nov 12 Nov 13 DEPARTMENT STORE 450.00\n" +
                   "Nov 13 Nov 13 ANNUAL FEE 120.00\n" +
                   "Nov 14 Nov 15 ELECTRONICS STORE 449.00\n";

        var parser = CreateParser(["INTEREST CHARGES", "ANNUAL FEE"]);
        var context = new ParseContext { Text = text, FileName = "real-world-scenario.txt" };
        var result = parser.Parse(context);

        Assert.Equal(8, result.Transactions.Count);
        Assert.Equal(1500.00m, result.DeclaredPurchasesTotal);
        Assert.Equal(1500.00m, result.ComputedPurchasesTotal); // Excludes $45.50 interest + $120 fee
        Assert.Equal(2, result.ExcludedCount);
        Assert.True(result.IsMatch);
        Assert.Equal(0m, result.Difference);

        // Verify InclusionStatus for all transactions
        Assert.Equal(TransactionInclusionStatus.Include, result.Transactions[0].InclusionStatus); // GROCERY
        Assert.Equal(TransactionInclusionStatus.Include, result.Transactions[1].InclusionStatus); // GAS
        Assert.Equal(TransactionInclusionStatus.Include, result.Transactions[2].InclusionStatus); // RESTAURANT
        Assert.Equal(TransactionInclusionStatus.Include, result.Transactions[3].InclusionStatus); // ONLINE
        Assert.Equal(TransactionInclusionStatus.Exclude, result.Transactions[4].InclusionStatus); // INTEREST
        Assert.Equal(TransactionInclusionStatus.Include, result.Transactions[5].InclusionStatus); // DEPARTMENT
        Assert.Equal(TransactionInclusionStatus.Exclude, result.Transactions[6].InclusionStatus); // ANNUAL FEE
        Assert.Equal(TransactionInclusionStatus.Include, result.Transactions[7].InclusionStatus); // ELECTRONICS
    }

    [Fact]
    public void Parse_InclusionStatus_ReflectsFilteringDecisions()
    {
        var text = "Statement date: October 21, 2025\n" +
                   "Purchases 50.00\n" +
                   "Oct 15 Oct 15 PURCHASE 50.00\n" +
                   "Oct 16 Oct 16 INTEREST CHARGES 10.00\n" +
                   "Oct 17 Oct 17 PAYMENT -100.00\n";

        var parser = CreateParser(["INTEREST CHARGES"]);
        var context = new ParseContext { Text = text, FileName = "inclusion-status.txt" };
        var result = parser.Parse(context);

        // Verify we have all three types
        Assert.Equal(TransactionInclusionStatus.Include, result.Transactions[0].InclusionStatus);  // Regular purchase
        Assert.Equal(TransactionInclusionStatus.Exclude, result.Transactions[1].InclusionStatus);  // Excluded by filter
        Assert.Equal(TransactionInclusionStatus.Exclude, result.Transactions[2].InclusionStatus);  // Negative amount

        // Verify only included transactions are counted
        Assert.Single(result.Transactions, t => t.InclusionStatus == TransactionInclusionStatus.Include);
        Assert.Equal(2, result.Transactions.Count(t => t.InclusionStatus == TransactionInclusionStatus.Exclude));
        Assert.Equal(0, result.Transactions.Count(t => t.InclusionStatus == TransactionInclusionStatus.Undetermined));
    }
}
