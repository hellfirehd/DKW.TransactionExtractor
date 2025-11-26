using DKW.TransactionExtractor.Models;

namespace DKW.TransactionExtractor.Classification;

public class ConsoleInteractionService : IConsoleInteraction
{
    private readonly ICategoryService _categoryService;

    public ConsoleInteractionService(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    public CategorySelectionResult PromptForCategory(ClassifyTransactionContext context)
    {
        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("???????????????????????????????????????????????????????????");
            Console.WriteLine($"Transaction {context.ProgressText}");
            Console.WriteLine($"Description: {context.Transaction.Description}");
            Console.WriteLine($"Amount: {context.Transaction.Amount:C}");
            Console.WriteLine($"Date: {context.Transaction.TransactionDate:yyyy-MM-dd}");
            Console.WriteLine("No category match found.");
            Console.WriteLine("???????????????????????????????????????????????????????????");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  1. Select existing category");
            Console.WriteLine("  2. Create new category");
            Console.WriteLine("  3. Skip (leave uncategorized)");
            Console.WriteLine("  4. Exit (stop processing)");
            Console.WriteLine();
            Console.Write("Your choice [1-4]: ");

            var choice = Console.ReadLine()?.Trim();

            var result = choice switch
            {
                "1" => SelectExistingCategory(context),
                "2" => CreateNewCategory(context),
                "3" => new CategorySelectionResult("uncategorized", "Uncategorized", false),
                "4" => HandleExit(),
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

    private CategorySelectionResult SelectExistingCategory(ClassifyTransactionContext context)
    {
        var sortedCategories = _categoryService.GetAvailableCategories();

        Console.WriteLine();
        Console.WriteLine("Existing categories:");
        for (var i = 0; i < sortedCategories.Count; i++)
        {
            Console.WriteLine($"  {i + 1}. {sortedCategories[i].Name}");
        }
        Console.WriteLine("  0. Go back to main menu");
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
            
            Console.WriteLine();
            Console.Write($"Add '{context.Transaction.Description}' as a rule to '{category.Name}'? [Y/n]: ");
            var response = Console.ReadLine()?.Trim().ToLowerInvariant();
            var addRule = response != "n";

            return new CategorySelectionResult(category.Id, category.Name, addRule);
        }

        return CategorySelectionResult.Invalid;
    }

    private CategorySelectionResult CreateNewCategory(ClassifyTransactionContext context)
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

        Console.WriteLine();
        Console.Write($"Add '{context.Transaction.Description}' as a rule to '{categoryName}'? [Y/n]: ");
        var response = Console.ReadLine()?.Trim().ToLowerInvariant();
        var addRule = response != "n";

        return new CategorySelectionResult(categoryId, categoryName, addRule);
    }

    private CategorySelectionResult HandleExit()
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

        return new CategorySelectionResult("uncategorized", "Uncategorized", false, RequestedExit: true);
    }
}
