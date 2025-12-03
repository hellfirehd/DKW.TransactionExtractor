using DKW.TransactionExtractor.Models;
using DKW.TransactionExtractor.Providers.CTFS;
using System;
using Xunit;

namespace DKW.TransactionExtractor.Tests;

public class TransactionParsingIssuesTests
{
    [Fact]
    public void Parse_AppleComTransaction_RecognizedCorrectly()
    {
        // Use a non-leap year date to avoid DateTime validation issues
        var text = @"Statement date: March 21, 2024
Purchases 3.35
Mar 15 Mar 15 APPLE.COM/BILL 866-712-7753 ON 3.35";

        var parser = new CtfsMastercardTransactionParser();
        var context = new StatementContext { RawText = text, FileName = "apple-transaction-test.txt" };
        var result = parser.Parse(context);

        // Output warnings and transactions for debugging
        Assert.True(result.Transactions.Count > 0 || result.Warnings.Count > 0,
            $"No transactions or warnings found. Transactions: {result.Transactions.Count}, Warnings: {result.Warnings.Count}");

        if (result.Warnings.Count != 0)
        {
            var warning = result.Warnings[0];
            Assert.Fail($"Transaction was not parsed. Warning at line {warning.LineNumber}: {warning.Message} - RawText: '{warning.RawText}'");
        }

        // Should have 1 transaction
        Assert.Single(result.Transactions);

        var transaction = result.Transactions[0];
        Assert.Equal(3.35m, transaction.Amount);
        Assert.Contains("APPLE.COM/BILL", transaction.Description);
        Assert.Contains("866-712-7753", transaction.Description);
        Assert.Contains("ON", transaction.Description); // Ontario province code
        Assert.Equal(new DateTime(2024, 3, 15), transaction.TransactionDate);
        Assert.Equal(new DateTime(2024, 3, 15), transaction.PostedDate);
    }

    [Fact]
    public void Parse_TransactionWithSlashInDescription_Parsed()
    {
        var text = @"Statement date: October 21, 2025
Purchases 15.00
Oct 15 Oct 15 STORE/NAME WITH/SLASH 15.00";

        var parser = new CtfsMastercardTransactionParser();
        var context = new StatementContext { RawText = text, FileName = "slash-test.txt" };
        var result = parser.Parse(context);

        Assert.Single(result.Transactions);
        Assert.Equal(15.00m, result.Transactions[0].Amount);
        Assert.Contains("STORE/NAME", result.Transactions[0].Description);
    }

    [Fact]
    public void Parse_TransactionWithDotsInDescription_Parsed()
    {
        var text = @"Statement date: October 21, 2025
Purchases 10.00
Oct 15 Oct 15 DOMAIN.COM/BILL 10.00";

        var parser = new CtfsMastercardTransactionParser();
        var context = new StatementContext { RawText = text, FileName = "dots-test.txt" };
        var result = parser.Parse(context);

        Assert.Single(result.Transactions);
        Assert.Equal(10.00m, result.Transactions[0].Amount);
        Assert.Contains("DOMAIN.COM", result.Transactions[0].Description);
    }

    [Fact]
    public void Parse_AppleComTransaction_Debug()
    {
        var text = "Feb 29 Feb 29 APPLE.COM/BILL 866-712-7753 ON 3.35";

        // Test TransactionStartRegex
        var startPattern = @"^[A-Za-z]{3}\s+\d{1,2}\s+[A-Za-z]{3}\s+\d{1,2}\b";
        var startRegex = new System.Text.RegularExpressions.Regex(startPattern);
        var startMatch = startRegex.IsMatch(text);

        Assert.True(startMatch, $"TransactionStartRegex did not match. Text: '{text}'");

        // Test the full regex
        var pattern = @"^(?<date1>[A-Za-z]{3}\s+\d{1,2})\s+(?<date2>[A-Za-z]{3}\s+\d{1,2})\s+(?<desc>.+?)\s+(?<amount>\d+\.\d+)$";
        var regex = new System.Text.RegularExpressions.Regex(pattern);
        var match = regex.Match(text);

        Assert.True(match.Success, $"TransactionLineRegex did not match. Text: '{text}'");

        if (match.Success)
        {
            Assert.Equal("Feb 29", match.Groups["date1"].Value);
            Assert.Equal("Feb 29", match.Groups["date2"].Value);
            Assert.Equal("APPLE.COM/BILL 866-712-7753 ON", match.Groups["desc"].Value);
            Assert.Equal("3.35", match.Groups["amount"].Value);
        }
    }

    [Fact]
    public void Parse_TransactionWithOntarioProvinceCode_ParsesCorrectly()
    {
        // Test that "ON" (Ontario) in the description is recognized correctly, not as a separate word
        var text = @"Statement date: March 21, 2024
Purchases 10.00
Mar 15 Mar 15 STORE NAME ON 10.00";

        var parser = new CtfsMastercardTransactionParser();
        var context = new StatementContext { RawText = text, FileName = "ontario-test.txt" };
        var result = parser.Parse(context);

        if (result.Warnings.Count != 0)
        {
            Assert.Fail($"Transaction was not parsed. Warning: {result.Warnings[0].Message} - '{result.Warnings[0].RawText}'");
        }

        Assert.Single(result.Transactions);
        Assert.Contains("ON", result.Transactions[0].Description);
        Assert.Equal(new DateTime(2024, 3, 15), result.Transactions[0].TransactionDate);
    }

    [Fact]
    public void Parse_StatementDateExtraction_Works()
    {
        var text = @"Statement date: March 21, 2024
Purchases 10.00";

        var pattern = @"Statement date[:]?[\s]*(?<month>[A-Za-z]+)\s+(?<day>\d{1,2}),\s*(?<year>\d{4})";
        var regex = new System.Text.RegularExpressions.Regex(pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        var lines = text.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        var statementLine = lines[0];

        var match = regex.Match(statementLine);
        Assert.True(match.Success, $"Statement date regex did not match. Line: '{statementLine}'");

        if (match.Success)
        {
            Assert.Equal("March", match.Groups["month"].Value);
            Assert.Equal("21", match.Groups["day"].Value);
            Assert.Equal("2024", match.Groups["year"].Value);
        }
    }
}
