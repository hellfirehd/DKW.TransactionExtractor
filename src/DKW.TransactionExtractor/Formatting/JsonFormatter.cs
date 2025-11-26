using DKW.TransactionExtractor.Models;
using System.Text.Json;

namespace DKW.TransactionExtractor.Formatting;

public class JsonFormatter : ITransactionFormatter
{
    public void WriteOutput(List<ClassifiedTransaction> transactions, String outputPath)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var json = JsonSerializer.Serialize(transactions, options);
        File.WriteAllText(outputPath, json);
    }
}
