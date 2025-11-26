namespace DKW.TransactionExtractor.Classification;

/// <summary>
/// Interactive service that prompts users to build matcher configurations.
/// </summary>
public class MatcherBuilderService : IMatcherBuilder
{
    public MatcherCreationRequest? BuildMatcher(String transactionDescription)
    {
        Console.WriteLine();
        Console.WriteLine("Select matcher type:");
        Console.WriteLine("  1. ExactMatch - Match this exact description (fastest, recommended)");
        Console.WriteLine("  2. Contains - Match descriptions containing a substring");
        Console.WriteLine("  3. Regex - Match using a regular expression pattern");
        Console.WriteLine("  0. Cancel");
        Console.WriteLine();
        Console.Write("Your choice [0-3]: ");

        var choice = Console.ReadLine()?.Trim();

        return choice switch
        {
            "1" => BuildExactMatcher(transactionDescription),
            "2" => BuildContainsMatcher(transactionDescription),
            "3" => BuildRegexMatcher(transactionDescription),
            "0" => null,
            _ => null
        };
    }

    private MatcherCreationRequest BuildExactMatcher(String transactionDescription)
    {
        Console.WriteLine();
        Console.WriteLine($"ExactMatch will match: '{transactionDescription}'");
        Console.Write("Case-sensitive matching? [y/N]: ");
        var caseSensitive = Console.ReadLine()?.Trim().ToLowerInvariant() == "y";

        var request = MatcherCreationRequest.ExactMatch(
            new[] { transactionDescription },
            caseSensitive
        );

        Console.WriteLine();
        Console.WriteLine($"? Will add ExactMatch rule for '{transactionDescription}'");
        Console.WriteLine("  (This will be merged with any existing ExactMatch rules)");

        return request;
    }

    private MatcherCreationRequest BuildContainsMatcher(String transactionDescription)
    {
        Console.WriteLine();
        Console.WriteLine($"Current description: '{transactionDescription}'");
        Console.Write("Enter the substring to match: ");
        var substring = Console.ReadLine()?.Trim();

        if (String.IsNullOrWhiteSpace(substring))
        {
            Console.WriteLine("Substring cannot be empty. Cancelling.");
            return null!;
        }

        Console.Write("Case-sensitive matching? [y/N]: ");
        var caseSensitive = Console.ReadLine()?.Trim().ToLowerInvariant() == "y";

        var request = MatcherCreationRequest.Contains(
            new[] { substring },
            caseSensitive
        );

        Console.WriteLine();
        Console.WriteLine($"? Will add Contains rule for '{substring}' (case-{(caseSensitive ? "sensitive" : "insensitive")})");
        Console.WriteLine("  (This will be merged with any existing Contains rules)");

        return request;
    }

    private MatcherCreationRequest BuildRegexMatcher(String transactionDescription)
    {
        Console.WriteLine();
        Console.WriteLine($"Current description: '{transactionDescription}'");
        Console.WriteLine("Enter a regular expression pattern.");
        Console.WriteLine("Examples:");
        Console.WriteLine("  - Match Starbucks with store number: STARBUCKS\\s+#\\d+");
        Console.WriteLine("  - Match any cafe/coffee shop: (CAFE|COFFEE|STARBUCKS)");
        Console.Write("Pattern: ");
        var pattern = Console.ReadLine()?.Trim();

        if (String.IsNullOrWhiteSpace(pattern))
        {
            Console.WriteLine("Pattern cannot be empty. Cancelling.");
            return null!;
        }

        Console.Write("Case-insensitive matching? [Y/n]: ");
        var response = Console.ReadLine()?.Trim().ToLowerInvariant();
        var ignoreCase = response != "n";

        var request = MatcherCreationRequest.Regex(pattern, ignoreCase);

        Console.WriteLine();
        Console.WriteLine($"? Will add Regex rule for pattern '{pattern}' (case-{(ignoreCase ? "insensitive" : "sensitive")})");

        return request;
    }
}
