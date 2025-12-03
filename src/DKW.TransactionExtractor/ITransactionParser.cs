using DKW.TransactionExtractor.Models;

namespace DKW.TransactionExtractor;

public interface ITransactionParser
{
    ParseResult Parse(StatementContext context);
}
