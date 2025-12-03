using DKW.TransactionExtractor.Models;
using DKW.TransactionExtractor.Providers.CTFS;
using System;
using System.Linq;
using Xunit;

namespace DKW.TransactionExtractor.Tests;

public class TransactionParserEdgeCasesTests
{
    [Fact]
    public void Parse_MultilineDescriptionAndAmountOnNextLine_ParsedCorrectly()
    {
        var text = "Statement date: October 21, 2025\n" +
                   "Oct 15 Oct 15 DOMINOcS PIZZA #39022 250-861-5731\n" +
                   "BC\n" +
                   "59.36\n";

        var parser = new CtfsMastercardTransactionParser();
        var context = new StatementContext { RawText = text, FileName = "multiline-test.txt" };
        var result = parser.Parse(context);
        var tx = result.Transactions.Single();

        Assert.Equal(new DateTime(2025, 10, 15), tx.TransactionDate);
        Assert.Equal(59.36m, tx.Amount);
        Assert.Equal("DOMINOcS PIZZA #39022 250-861-5731 BC", tx.Description);
    }

    [Fact]
    public void Parse_AmountOnSameLine_NegativeValue_ParsedCorrectly()
    {
        var text = "Statement date: October 21, 2025\n" +
                   "Oct 01 Oct 02 CIBC BANK PMT/PAIEMENT BCIC -3,463.00\n";

        var parser = new CtfsMastercardTransactionParser();
        var context = new StatementContext { RawText = text, FileName = "negative-amount-test.txt" };
        var result = parser.Parse(context);
        var tx = result.Transactions.Single();

        Assert.Equal(new DateTime(2025, 10, 1), tx.TransactionDate);
        Assert.Equal(-3463.00m, tx.Amount);
        Assert.Contains("CIBC BANK", tx.Description);
    }

    [Fact]
    public void Parse_LeadingCurrencySymbol_IsHandled()
    {
        var text = "Statement date: October 21, 2025\n" +
                   "Oct 01 Oct 02 APPLE.COM/BILL 866-712-7753 ON $14.16\n";

        var parser = new CtfsMastercardTransactionParser();
        var context = new StatementContext { RawText = text, FileName = "currency-symbol-test.txt" };
        var result = parser.Parse(context);
        var tx = result.Transactions.Single();

        Assert.Equal(14.16m, tx.Amount);
    }

    [Fact]
    public void Parse_DescriptionWithNumbers_IsHandled()
    {
        var text = """
            Statement date: October 21, 2025
            Nov 01 Nov 04 MICROSOFT*MICROSOFT 36 
            MISSISSAUGA ON
            122.08            
            """;

        var parser = new CtfsMastercardTransactionParser();
        var context = new StatementContext { RawText = text, FileName = "description-with-numbers.txt" };
        var result = parser.Parse(context);
        var tx = result.Transactions.Single();

        Assert.Equal(122.08m, tx.Amount);
    }

    [Fact]
    public void Parse_YearBoundary_DecTransactionAssignedPreviousYear()
    {
        var text = "Statement date: January 10, 2026\n" +
                   "Dec 31 Jan 02 STORE XYZ 10.00\n";

        var parser = new CtfsMastercardTransactionParser();
        var context = new StatementContext { RawText = text, FileName = "year-boundary-test.txt" };
        var result = parser.Parse(context);
        var tx = result.Transactions.Single();

        Assert.Equal(new DateTime(2025, 12, 31), tx.TransactionDate);
        Assert.Equal(new DateTime(2026, 1, 2), tx.PostedDate);
    }
}
