namespace DKW.TransactionExtractor.Classification;

/// <summary>
/// Service responsible for interactively building matcher configurations
/// based on user input.
/// </summary>
public interface IMatcherBuilder
{
    /// <summary>
    /// Prompts the user to configure a matcher for the given transaction description.
    /// Returns null if the user cancels the operation.
    /// </summary>
    MatcherCreationRequest? BuildMatcher(String transactionDescription);
}
