using DKW.TransactionExtractor.Models;
using Microsoft.Extensions.Logging;

namespace DKW.TransactionExtractor.Classification;

public class TransactionClassifier : ITransactionClassifier
{
    private readonly ILogger<TransactionClassifier> _logger;
    private readonly ICategoryService _categoryService;
    private readonly IConsoleInteraction _consoleInteraction;
    private readonly MatcherFactory _matcherFactory;

    public TransactionClassifier(
        ILogger<TransactionClassifier> logger,
        ICategoryService categoryService,
        IConsoleInteraction consoleInteraction)
    {
        _logger = logger;
        _categoryService = categoryService;
        _consoleInteraction = consoleInteraction;
        _matcherFactory = new MatcherFactory();
    }

    public ClassificationResult ClassifyTransactions(List<Transaction> transactions)
    {
        var classifiedTransactions = new List<ClassifiedTransaction>();
        var totalCount = transactions.Count;

        for (var i = 0; i < transactions.Count; i++)
        {
            var context = new ClassifyTransactionContext(
                Transaction: transactions[i],
                CurrentIndex: i + 1,
                TotalCount: totalCount
            );

            var result = ClassifyTransaction(context);
            
            if (result.RequestedExit)
            {
                _logger.LogInformation("User requested early exit. Processed {Count} of {Total} transactions.", 
                    classifiedTransactions.Count, transactions.Count);
                return new ClassificationResult(classifiedTransactions, RequestedEarlyExit: true);
            }
            
            classifiedTransactions.Add(result.ClassifiedTransaction);
        }

        return new ClassificationResult(classifiedTransactions);
    }

    private (ClassifiedTransaction ClassifiedTransaction, Boolean RequestedExit) ClassifyTransaction(ClassifyTransactionContext context)
    {
        // Try to match against existing categories
        var categories = _categoryService.GetAllCategories();
        
        foreach (var category in categories)
        {
            foreach (var matcherConfig in category.Matchers)
            {
                var matcher = _matcherFactory.CreateMatcher(matcherConfig);
                if (matcher != null && matcher.TryMatch(context.Transaction.Description))
                {
                    _logger.LogDebug("Matched transaction '{Description}' to category '{Category}'", 
                        context.Transaction.Description, category.Name);
                    
                    // Show summary for automatically classified transaction
                    Console.WriteLine($"  [{context.CurrentIndex}/{context.TotalCount}] {context.Transaction.TransactionDate:MMM dd} | " +
                                    $"{context.Transaction.Description,-40} | {context.Transaction.Amount,10:C} ? {category.Name}");
                    
                    return (new ClassifiedTransaction
                    {
                        Transaction = context.Transaction,
                        CategoryId = category.Id,
                        CategoryName = category.Name
                    }, false);
                }
            }
        }

        // No match found, prompt user
        var selectionResult = _consoleInteraction.PromptForCategory(context);
        
        // Check for early exit
        if (selectionResult.RequestedExit)
        {
            return (new ClassifiedTransaction
            {
                Transaction = context.Transaction,
                CategoryId = "uncategorized",
                CategoryName = "Uncategorized"
            }, true);
        }

        // Normalize the category ID from user selection
        var normalizedCategoryId = CategoryIdNormalizer.Normalize(selectionResult.CategoryId);

        // If user chose to create a new category
        if (!_categoryService.CategoryExists(normalizedCategoryId))
        {
            var newCategory = new Category
            {
                Id = normalizedCategoryId,
                Name = selectionResult.CategoryName,
                Matchers = new List<CategoryMatcher>()
            };

            if (selectionResult.HasMatcherRequest)
            {
                var matcher = new CategoryMatcher
                {
                    Type = selectionResult.MatcherRequest!.MatcherType,
                    Parameters = new Dictionary<String, Object>(selectionResult.MatcherRequest.Parameters)
                };
                newCategory.Matchers.Add(matcher);
            }

            _categoryService.AddCategory(newCategory);
            _logger.LogInformation("Created new category '{Category}' with ID '{Id}'", selectionResult.CategoryName, normalizedCategoryId);
        }
        else if (selectionResult.HasMatcherRequest)
        {
            // Add matcher to existing category
            _categoryService.AddMatcherToCategory(normalizedCategoryId, selectionResult.MatcherRequest!);
            _logger.LogInformation("Added {MatcherType} rule to category '{Category}'", 
                selectionResult.MatcherRequest!.MatcherType, selectionResult.CategoryName);
        }

        return (new ClassifiedTransaction
        {
            Transaction = context.Transaction,
            CategoryId = normalizedCategoryId,
            CategoryName = selectionResult.CategoryName
        }, false);
    }
}
