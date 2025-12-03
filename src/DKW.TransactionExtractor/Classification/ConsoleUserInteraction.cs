namespace DKW.TransactionExtractor.Classification;

public class ConsoleUserInteraction(ICategoryService categoryService) : IUserInteraction
{
    private readonly ICategoryService _categoryService = categoryService;

    public CategorySelectionResult PromptForCategory(TransactionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("═══════════════════════════════════════════════════════════");
            Console.WriteLine($"Transaction    {context.CurrentIndex} of {context.TotalCount}");
            Console.WriteLine($"Description:   {context.Transaction.Description}");
            Console.WriteLine($"Purchase Date: {context.Transaction.TransactionDate:yyyy-MM-dd}");
            Console.WriteLine($"Posted Date:   {context.Transaction.PostedDate:yyyy-MM-dd}");
            Console.WriteLine($"Amount:        {context.Transaction.Amount:C}");
            if (!String.IsNullOrWhiteSpace(context.Comment))
            {
                Console.WriteLine($"Comment:       {context.Comment}");
            }

            Console.WriteLine("No category match found.");
            Console.WriteLine("═══════════════════════════════════════════════════════════");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  0. Exit (stop processing)");
            Console.WriteLine("  1. Select existing category");
            Console.WriteLine("  2. Create new category");
            Console.WriteLine("  3. Add comment");
            Console.WriteLine("  4. Skip (leave uncategorized) [default]");
            Console.WriteLine();
            Console.Write("Your choice [0-4]: ");

            var choice = Console.ReadLine()?.Trim();

            // Default to skip if empty
            if (String.IsNullOrEmpty(choice))
            {
                choice = "4";
            }

            var result = choice switch
            {
                "1" => SelectExistingCategory(context),
                "2" => CreateNewCategory(context),
                "3" => AddComment(context),
                "4" => CreateSkipResult(context),
                "0" => HandleExit(),
                _ => null // Invalid choice, will loop
            };

            if (result == null)
            {
                Console.WriteLine("Invalid choice. Please try again.");
                continue;
            }

            if (result.IsGoBack || result.IsInvalid)
            {
                if (result.IsInvalid)
                {
                    Console.WriteLine("Invalid selection. Please try again.");
                }

                continue; // Loop back to appropriate menu
            }

            return result;
        }
    }

    public MatcherCreationRequest? PromptForMatcher(TransactionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        Console.WriteLine();
        Console.WriteLine("Select matcher type:");
        Console.WriteLine("  0. Cancel");
        Console.WriteLine("  1. ExactMatch - Match this exact description (fastest, recommended) [default]");
        Console.WriteLine("  2. Contains - Match descriptions containing a substring");
        Console.WriteLine("  3. Regex - Match using a regular expression pattern");
        Console.WriteLine();
        Console.Write("Your choice [1-3, default=1]: ");

        var choice = Console.ReadLine()?.Trim();

        // Default to ExactMatch if user just presses Enter
        if (String.IsNullOrEmpty(choice))
        {
            choice = "1";
        }

        return choice switch
        {
            "1" => BuildExactMatcher(context),
            "2" => BuildContainsMatcher(context),
            "3" => BuildRegexMatcher(context),
            "0" => null,
            _ => null
        };
    }

    public Boolean PromptYesNo(String prompt, Boolean defaultYes = true)
    {
        Console.Write($"{prompt} [{(defaultYes ? "Y/n" : "y/N")}]: ");
        var response = Console.ReadLine()?.Trim().ToLowerInvariant();

        if (String.IsNullOrEmpty(response))
        {
            return defaultYes;
        }

        return response is "y" or "yes";
    }

    public String? PromptForText(String prompt, Boolean allowEmpty = false)
    {
        Console.Write($"{prompt}: ");
        var input = Console.ReadLine()?.Trim();

        if (String.IsNullOrWhiteSpace(input) && !allowEmpty)
        {
            return null;
        }

        return input;
    }

    public Decimal? PromptForOptionalAmount(TransactionContext context, Boolean defaultToInclude)
    {
        var response = PromptYesNo($"Include the amount {context.Transaction.Amount:c} when matching?", defaultToInclude);

        if (response)
        {
            return context.Transaction.Amount;
        }

        return null;
    }

    public void DisplayAutoMatchSummary(TransactionContext context, String categoryName)
    {
        Console.WriteLine($"  [{context.CurrentIndex}/{context.TotalCount}] {context.Transaction.TransactionDate:MMM dd} | " +
                        $"{context.Transaction.Description,-40} | {context.Transaction.Amount,10:C} → {categoryName}");
    }

    private static CategorySelectionResult? AddComment(TransactionContext context)
    {
        Console.WriteLine();
        Console.Write("Enter comment (or press Enter to cancel): ");
        var comment = Console.ReadLine()?.Trim();

        if (!String.IsNullOrWhiteSpace(comment))
        {
            context.Comment = comment;
            Console.WriteLine($"Comment added: {comment}");
        }
        else
        {
            Console.WriteLine("No comment added.");
        }

        // Return GoBack to redisplay menu with comment
        return CategorySelectionResult.GoBack;
    }

    private CategorySelectionResult SelectExistingCategory(TransactionContext context)
    {
        var sortedCategories = _categoryService
            .GetCategories()
            .OrderBy(c => c.Name)
            .ToList();

        Console.WriteLine();
        Console.WriteLine("Existing categories:");
        Console.WriteLine("  0. Go back to main menu");
        for (var i = 0; i < sortedCategories.Count; i++)
        {
            Console.WriteLine($"  {i + 1}. {sortedCategories[i].Name}");
        }

        Console.WriteLine();
        Console.Write($"Select category [0-{sortedCategories.Count}]: ");

        var input = Console.ReadLine()?.Trim();

        if (input == "0")
        {
            return CategorySelectionResult.GoBack;
        }

        if (Int32.TryParse(input, out var index)
            && index >= 1 && index <= sortedCategories.Count)
        {
            var category = sortedCategories[index - 1];

            // Set category in context
            context.CategoryId = category.Id;
            context.CategoryName = category.Name;

            var addMatcher = PromptYesNo($"Add a matching rule for '{context.Transaction.Description}'?");

            MatcherCreationRequest? matcherRequest = null;
            if (addMatcher)
            {
                matcherRequest = PromptForMatcher(context);
            }

            return new CategorySelectionResult(category.Id, category.Name, matcherRequest, context.Comment);
        }

        return CategorySelectionResult.Invalid;
    }

    private CategorySelectionResult CreateNewCategory(TransactionContext context)
    {
        Console.WriteLine();
        Console.Write("Enter new category name: ");
        var categoryName = Console.ReadLine()?.Trim();

        if (String.IsNullOrWhiteSpace(categoryName))
        {
            Console.WriteLine("Category name cannot be empty.");
            return CategorySelectionResult.Invalid;
        }

        var categoryId = categoryName.ToLowerInvariant().Replace(" ", "-");

        // Set category in context
        context.CategoryId = categoryId;
        context.CategoryName = categoryName;

        var addMatcher = PromptYesNo($"Add a matching rule for '{context.Transaction.Description}'?");

        MatcherCreationRequest? matcherRequest = null;
        if (addMatcher)
        {
            matcherRequest = PromptForMatcher(context);
        }

        return new CategorySelectionResult(categoryId, categoryName, matcherRequest, context.Comment);
    }

    private static CategorySelectionResult HandleExit()
    {
        Console.WriteLine();
        Console.Write("Categories have been updated. Save changes before exiting? [Y/n]: ");
        var response = Console.ReadLine()?.Trim().ToLowerInvariant();

        if (response == "n")
        {
            Console.WriteLine("Warning: Unsaved category changes will be lost.");
            Console.Write("Are you sure you want to exit without saving? [y/N]: ");
            var confirm = Console.ReadLine()?.Trim().ToLowerInvariant();
            if (confirm != "y")
            {
                return CategorySelectionResult.GoBack;
            }
        }

        return new CategorySelectionResult("uncategorized", "Uncategorized", null, RequestedExit: true);
    }

    private MatcherCreationRequest BuildExactMatcher(TransactionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        Console.WriteLine();
        Console.WriteLine($"ExactMatch will match: '{context.Transaction.Description}'");

        context.MatcherText = context.Transaction.Description;

        var amount = PromptForOptionalAmount(context, defaultToInclude: true);

        context.IncludeAmountInMatcher = amount is not null;

        var request = MatcherCreationRequest.ExactMatch(context);

        Console.WriteLine();
        Console.WriteLine($"Added ExactMatch rule for '{context.Transaction.Description}'");

        return request;
    }

    private MatcherCreationRequest? BuildContainsMatcher(TransactionContext context)
    {
        Console.WriteLine();
        Console.WriteLine($"Current description: '{context.Transaction.Description}'");

        var substring = PromptForText("Enter the substring to match", allowEmpty: false);

        if (substring == null)
        {
            Console.WriteLine("Substring cannot be empty. Cancelling.");
            return null;
        }

        context.MatcherText = substring;

        var amount = PromptForOptionalAmount(context, defaultToInclude: false);

        context.IncludeAmountInMatcher = amount is not null;

        var request = MatcherCreationRequest.Contains(context);

        Console.WriteLine();
        Console.WriteLine($"Added Contains rule for '{substring}'");
        Console.WriteLine("  (This will be merged with any existing Contains rules)");

        return request;
    }

    private MatcherCreationRequest? BuildRegexMatcher(TransactionContext context)
    {
        Console.WriteLine();
        Console.WriteLine($"Current description: '{context.Transaction.Description}'");
        Console.WriteLine("Enter a regular expression pattern.");
        Console.WriteLine("Examples:");
        Console.WriteLine("  - Match Starbucks with store number: STARBUCKS\\s+#\\d+");
        Console.WriteLine("  - Match any cafe/coffee shop: (CAFE|COFFEE|STARBUCKS)");

        var pattern = PromptForText("Pattern", allowEmpty: false);

        if (pattern == null)
        {
            Console.WriteLine("Pattern cannot be empty. Cancelling.");
            return null;
        }

        context.MatcherText = pattern;

        var amount = PromptForOptionalAmount(context, defaultToInclude: false);

        context.IncludeAmountInMatcher = amount is not null;

        var request = MatcherCreationRequest.Regex(context);

        Console.WriteLine();
        Console.WriteLine($"Added Regex rule for pattern '{pattern}'");

        return request;
    }

    private static CategorySelectionResult CreateSkipResult(TransactionContext context)
    {
        context.CategoryId = "uncategorized";
        context.CategoryName = "Uncategorized";
        return new CategorySelectionResult("uncategorized", "Uncategorized", null, context.Comment);
    }
}
