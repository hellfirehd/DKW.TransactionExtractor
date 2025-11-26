using DKW.TransactionExtractor.Models;

namespace DKW.TransactionExtractor.Formatting;

public interface ITransactionFormatter
{
    void WriteOutput(List<ClassifiedTransaction> transactions, String outputPath);
    
    void WriteSummary(List<CategorySummary> categorySummaries, String outputPath);
}
