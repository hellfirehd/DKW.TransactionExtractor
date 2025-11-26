using DKW.TransactionExtractor.Models;
using System.Text;

namespace DKW.TransactionExtractor.Formatting;

public class CsvFormatter : ITransactionFormatter
{
    public void WriteOutput(List<ClassifiedTransaction> transactions, String outputPath)
    {
        var sb = new StringBuilder();
        
        // Header
        sb.AppendLine("TransactionDate,PostedDate,Description,Amount,CategoryId,CategoryName,InclusionStatus");

        // Data rows
        foreach (var ct in transactions)
        {
            var t = ct.Transaction;
            sb.AppendLine($"{t.TransactionDate:yyyy-MM-dd}," +
                         $"{(t.PostedDate.HasValue ? t.PostedDate.Value.ToString("yyyy-MM-dd") : "")}," +
                         $"\"{EscapeCsv(t.Description)}\"," +
                         $"{t.Amount}," +
                         $"\"{EscapeCsv(ct.CategoryId)}\"," +
                         $"\"{EscapeCsv(ct.CategoryName)}\"," +
                         $"{t.InclusionStatus}");
        }

        File.WriteAllText(outputPath, sb.ToString());
    }

    private String EscapeCsv(String value)
    {
        if (value.Contains('"'))
        {
            return value.Replace("\"", "\"\"");
        }
        return value;
    }
}
