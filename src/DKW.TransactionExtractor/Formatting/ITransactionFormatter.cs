using DKW.TransactionExtractor.Models;

namespace DKW.TransactionExtractor.Formatting;

public interface ITransactionFormatter
{
    void WriteOutput(TransactionOutput output, String baseOutputPath);
}
