using System.Reflection;

namespace DKW.TransactionExtractor.Tests;

public static class TestResources
{
    private static readonly Assembly TestAssembly = typeof(TestResources).Assembly;

    private static String GetEmbeddedResource(String resourceName)
    {
        //                       DKW.TransactionExtractor.Tests.TestData.
        var fullResourceName = $"DKW.TransactionExtractor.Tests.TestData.{resourceName}";

        using var stream = TestAssembly.GetManifestResourceStream(fullResourceName)
            ?? throw new Exception($"Embedded resource '{fullResourceName}' was not found.");

        using var reader = new StreamReader(stream);
        var text = reader.ReadToEnd();
        if (String.IsNullOrWhiteSpace(text))
        {
            throw new Exception($"Embedded resource '{fullResourceName}' is empty.");
        }

        return text;
    }

    public static String GetTriangleStatement_2024_03_21()
        => GetEmbeddedResource("2024-03-21-Triangle-WorldEliteMastercard.txt");

    public static String GetTriangleStatement_2025_10_21()
        => GetEmbeddedResource("2025-10-21-Triangle-WorldEliteMastercard.txt");
}
