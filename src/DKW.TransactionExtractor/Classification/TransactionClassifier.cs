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

        // If user chose to create a new category
        if (!_categoryService.CategoryExists(selectionResult.CategoryId))
        {
            var newCategory = new Category
            {
                Id = selectionResult.CategoryId,
                Name = selectionResult.CategoryName,
                Matchers = new List<CategoryMatcher>()
            };

            if (selectionResult.AddRule)
            {
                newCategory.Matchers.Add(new CategoryMatcher
                {
                    Type = "ExactMatch",
                    Parameters = new Dictionary<String, Object>
                    {
                        { "values", new[] { context.Transaction.Description } },
                        { "caseSensitive", false }
                    }
                });
            }

            _categoryService.AddCategory(newCategory);
            _logger.LogInformation("Created new category '{Category}' with ID '{Id}'", selectionResult.CategoryName, selectionResult.CategoryId);
        }
        else if (selectionResult.AddRule)
        {
            // Add description to existing category's ExactMatch
            _categoryService.AddDescriptionToCategory(selectionResult.CategoryId, context.Transaction.Description);
            _logger.LogInformation("Added rule for '{Description}' to category '{Category}'", 
                context.Transaction.Description, selectionResult.CategoryName);
        }

        return (new ClassifiedTransaction
        {
            Transaction = context.Transaction,
            CategoryId = selectionResult.CategoryId,
            CategoryName = selectionResult.CategoryName
        }, false);
    }
}
