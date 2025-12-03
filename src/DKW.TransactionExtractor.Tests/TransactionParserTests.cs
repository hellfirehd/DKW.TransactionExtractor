using DKW.TransactionExtractor.Models;
using DKW.TransactionExtractor.Providers.CTFS;
using Xunit;

namespace DKW.TransactionExtractor.Tests;

public class TransactionParserTests
{
    [Fact]
    public void Parse_SampleExtractedText_Returns48Transactions()
    {
        var text = TestResources.GetTriangleStatement_2025_10_21();
        var parser = new CtfsMastercardTransactionParser();
        var context = new StatementContext { RawText = text, FileName = "Parse_SampleExtractedText_Returns48Transactions" };
        var result = parser.Parse(context);

        Assert.Equal(48, result.Transactions.Count);
    }
}
