using DKW.TransactionExtractor.Models;
using System.Text.Json;

namespace DKW.TransactionExtractor.Formatting;

public class JsonFormatter : ITransactionFormatter
{
    public void WriteOutput(TransactionOutput output, String baseOutputPath)
    {
        var timestamp = output.GeneratedAt.ToString("yyyyMMdd-HHmmss");
        var directory = Path.GetDirectoryName(baseOutputPath) ?? String.Empty;
        var baseFileName = Path.GetFileNameWithoutExtension(baseOutputPath);
        var outputPath = Path.Combine(directory, $"{baseFileName}-{timestamp}.json");

        Object outputObject;

        if (output.CategorySummaries.Count > 0)
        {
            // Include summary in the output
            outputObject = new
            {
                Summary = new
                {
                    Categories = output.CategorySummaries
                },
                output.Transactions
            };
        }
        else
        {
            // No summary available, output transactions only
            outputObject = output.Transactions;
        }

        var json = JsonSerializer.Serialize(outputObject, SerializationHelper.JSO);
        File.WriteAllText(outputPath, json);
    }
}
