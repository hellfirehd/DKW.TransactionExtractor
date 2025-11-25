using System;
using DKW.TransactionExtractor.Models;
using DKW.TransactionExtractor.Providers.CTFS;
using Xunit;

namespace DKW.TransactionExtractor.Tests;

public class SupplementalDetailsTests
{
    [Fact]
    public void Parse_WithSupplementalDetails_ExcludesDetailTransactions()
    {
        var text = @"Statement date: October 21, 2025
Purchases 100.00
Oct 15 Oct 15 CANADIAN TIRE #123 KELOWNA BC 75.00
Oct 16 Oct 16 GROCERY STORE 25.00
Details of your Canadian Tire store purchases
Oct 15 Oct 15 2 @ MOTOR OIL 5W30 30.00
Oct 15 Oct 15 1 @ WINDSHIELD WIPERS 25.00
Oct 15 Oct 15 1 @ AIR FRESHENER 5.00";

        var parser = new CtfsMastercardTransactionParser();
        var context = new ParseContext { Text = text, FileName = "test-with-details.txt" };
        var result = parser.Parse(context);

        // Should only have the 2 main transactions, not the 3 detail lines
        Assert.Equal(2, result.Transactions.Count);
        Assert.Equal(100.00m, result.ComputedPurchasesTotal);
        Assert.Contains(result.Transactions, t => t.Description.Contains("CANADIAN TIRE"));
        Assert.Contains(result.Transactions, t => t.Description.Contains("GROCERY STORE"));
        
        // Verify no detail lines are included
        Assert.DoesNotContain(result.Transactions, t => t.Description.Contains("MOTOR OIL"));
        Assert.DoesNotContain(result.Transactions, t => t.Description.Contains("WINDSHIELD WIPERS"));
        Assert.DoesNotContain(result.Transactions, t => t.Description.Contains("AIR FRESHENER"));
    }

    [Fact]
    public void Parse_WithoutSupplementalDetails_IncludesAllTransactions()
    {
        var text = @"Statement date: October 21, 2025
Purchases 100.00
Oct 15 Oct 15 CANADIAN TIRE #123 KELOWNA BC 75.00
Oct 16 Oct 16 GROCERY STORE 25.00";

        var parser = new CtfsMastercardTransactionParser();
        var context = new ParseContext { Text = text, FileName = "test-without-details.txt" };
        var result = parser.Parse(context);

        Assert.Equal(2, result.Transactions.Count);
        Assert.Equal(100.00m, result.ComputedPurchasesTotal);
    }

    [Fact]
    public void Parse_DetailsSection_DoesNotCreateWarnings()
    {
        var text = @"Statement date: October 21, 2025
Purchases 50.00
Oct 15 Oct 15 CANADIAN TIRE #123 50.00
Details of your Canadian Tire store purchases
Oct 15 Oct 15 1 @ ITEM ONE 25.00
Oct 15 Oct 15 1 @ ITEM TWO 25.00";

        var parser = new CtfsMastercardTransactionParser();
        var context = new ParseContext { Text = text, FileName = "test-no-warnings.txt" };
        var result = parser.Parse(context);

        // Should have only 1 main transaction
        Assert.Single(result.Transactions);
        
        // Should not have warnings for the detail lines
        Assert.Empty(result.Warnings);
    }

    [Fact]
    public void Parse_DetailsSectionWithVariousFormats_AllSkipped()
    {
        var text = @"Statement date: October 21, 2025
Purchases 150.00
Oct 10 Oct 10 STORE A 50.00
Oct 15 Oct 15 CANADIAN TIRE #123 100.00
Details of your Canadian Tire store purchases
Oct 15 Oct 15 2 @ ITEM WITH QUANTITY 40.00
Oct 15 Oct 15 ITEM WITHOUT QUANTITY MARKER 30.00
Oct 15 Oct 15 3 X ANOTHER ITEM FORMAT 30.00";

        var parser = new CtfsMastercardTransactionParser();
        var context = new ParseContext { Text = text, FileName = "test-various-formats.txt" };
        var result = parser.Parse(context);

        // Should only have 2 main transactions (before the details section)
        Assert.Equal(2, result.Transactions.Count);
        Assert.Equal(150.00m, result.ComputedPurchasesTotal);
        Assert.True(result.IsMatch);
    }

    [Fact]
    public void Parse_DetailsSectionCaseInsensitive_Detected()
    {
        var text = @"Statement date: October 21, 2025
Purchases 50.00
Oct 15 Oct 15 STORE PURCHASE 50.00
DETAILS OF YOUR CANADIAN TIRE STORE PURCHASES
Oct 15 Oct 15 1 @ DETAIL ITEM 25.00";

        var parser = new CtfsMastercardTransactionParser();
        var context = new ParseContext { Text = text, FileName = "test-case-insensitive.txt" };
        var result = parser.Parse(context);

        Assert.Single(result.Transactions);
        Assert.Equal(50.00m, result.ComputedPurchasesTotal);
    }
}
