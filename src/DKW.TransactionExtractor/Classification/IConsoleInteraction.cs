using DKW.TransactionExtractor.Models;

namespace DKW.TransactionExtractor.Classification;

public interface IConsoleInteraction
{
    CategorySelectionResult PromptForCategory(ClassifyTransactionContext context);
}
