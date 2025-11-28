using DKW.TransactionExtractor.Models;

namespace DKW.TransactionExtractor.Classification;

/// <summary>
/// Console-specific user interaction interface.
/// This interface is maintained for backward compatibility but inherits from IUserInteraction.
/// </summary>
public interface IConsoleInteraction : IUserInteraction
{
    // No additional methods - IUserInteraction defines the contract
}
