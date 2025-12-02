using DKW.TransactionExtractor.Models;

namespace DKW.TransactionExtractor.Classification;

/// <summary>
/// Console-specific user interaction interface.
/// Provides all user interaction methods including category selection and matcher building.
/// </summary>
public interface IUserInteraction
{
    /// <summary>
    /// Prompts the user to select or create a category for an unmatched transaction.
    /// </summary>
    /// <param name="context">Context containing the transaction and progress information.</param>
    /// <returns>Result containing the selected category, optional matcher request, and user action.</returns>
    CategorySelectionResult PromptForCategory(TransactionContext context);

    /// <summary>
    /// Prompts the user to build a matcher configuration for the given transaction context.
    /// </summary>
    /// <param name="context">The transaction context to build a matcher for.</param>
    /// <returns>A matcher creation request, or null if the user cancels.</returns>
    MatcherCreationRequest? PromptForMatcher(TransactionContext context);

    /// <summary>
    /// Prompts the user for a yes/no decision.
    /// </summary>
    /// <param name="prompt">The question to ask the user.</param>
    /// <param name="defaultYes">Whether the default choice is 'yes' (true) or 'no' (false).</param>
    /// <returns>True if user chose yes, false otherwise.</returns>
    Boolean PromptYesNo(String prompt, Boolean defaultYes = true);

    /// <summary>
    /// Prompts the user to enter text input.
    /// </summary>
    /// <param name="prompt">The prompt to display.</param>
    /// <param name="allowEmpty">Whether empty input is valid.</param>
    /// <returns>The user's input, or null if cancelled or empty when not allowed.</returns>
    String? PromptForText(String prompt, Boolean allowEmpty = false);

    /// <summary>
    /// Prompts the user to enter an optional decimal amount.
    /// </summary>
    /// <param name="prompt">The prompt to display.</param>
    /// <returns>The parsed amount, or null if user skips or enters invalid input.</returns>
    Decimal? PromptForOptionalAmount(String prompt);

    /// <summary>
    /// Displays a summary line for an automatically classified transaction.
    /// </summary>
    /// <param name="context">Context containing transaction and progress information.</param>
    /// <param name="categoryName">The name of the category the transaction was matched to.</param>
    void DisplayAutoMatchSummary(TransactionContext context, String categoryName);
}
