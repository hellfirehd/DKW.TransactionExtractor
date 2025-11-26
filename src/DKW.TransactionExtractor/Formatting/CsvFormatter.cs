using DKW.TransactionExtractor.Models;
using System.Text;

namespace DKW.TransactionExtractor.Formatting;

public class CsvFormatter : ITransactionFormatter
{
    public void WriteOutput(TransactionOutput output, String baseOutputPath)
    {
        var timestamp = output.GeneratedAt.ToString("yyyyMMdd-HHmmss");
        var directory = Path.GetDirectoryName(baseOutputPath) ?? String.Empty;
        var baseFileName = Path.GetFileNameWithoutExtension(baseOutputPath);

        // Write transactions file
        var transactionsPath = Path.Combine(directory, $"{baseFileName}-{timestamp}.csv");
        WriteTransactions(output.Transactions, transactionsPath);

        // Write summary file if summaries exist
        if (output.CategorySummaries.Count > 0)
        {
            var summaryPath = Path.Combine(directory, $"{baseFileName}-{timestamp}-summary.csv");
            WriteSummary(output.CategorySummaries, summaryPath);
        }
    }

    private void WriteTransactions(List<ClassifiedTransaction> transactions, String outputPath)
    {
        var sb = new StringBuilder();

        // Header
        sb.AppendLine("StatementDate,TransactionDate,PostedDate,Description,Amount,CategoryId,CategoryName,InclusionStatus");

        // Data rows
        foreach (var ct in transactions)
        {
            var t = ct.Transaction;
            sb.AppendLine($"{t.StatementDate:yyyy-MM-dd}," +
                          $"{t.TransactionDate:yyyy-MM-dd}," +
                          $"{(t.PostedDate.HasValue ? t.PostedDate.Value.ToString("yyyy-MM-dd") : "")}," +
                          $"\"{EscapeCsv(t.Description)}\"," +
                          $"{t.Amount}," +
                          $"\"{EscapeCsv(ct.CategoryId)}\"," +
                          $"\"{EscapeCsv(ct.CategoryName)}\"," +
                          $"{t.InclusionStatus}"
                         );
        }

        File.WriteAllText(outputPath, sb.ToString());
    }

    private void WriteSummary(List<CategorySummary> categorySummaries, String outputPath)
    {
        var sb = new StringBuilder();

        // Header
        sb.AppendLine("CategoryId,CategoryName,TransactionCount,TotalAmount");

        // Data rows - summaries are already sorted alphabetically by caller
        foreach (var summary in categorySummaries)
        {
            sb.AppendLine($"\"{EscapeCsv(summary.CategoryId)}\"," +
                          $"\"{EscapeCsv(summary.CategoryName)}\"," +
                          $"{summary.TransactionCount}," +
                          $"{summary.TotalAmount}");
        }

        File.WriteAllText(outputPath, sb.ToString());
    }

    private String EscapeCsv(String value)
    {
        if (String.IsNullOrEmpty(value))
        {
            return String.Empty;
        }

        if (value.Contains('"'))
        {
            return value.Replace("\"", "\"\"");
        }

        return value;
    }
}
