using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;

namespace DKW.TransactionExtractor;

/// <summary>
/// Default implementation of ITransactionFilter that excludes transactions based on configurable description patterns.
/// </summary>
public partial class DefaultTransactionFilter : ITransactionFilter
{
    private readonly ILogger<DefaultTransactionFilter> _logger;
    private readonly List<Regex> _exclusionPatterns;

    public DefaultTransactionFilter(IOptions<ParserOptions> options, ILogger<DefaultTransactionFilter> logger)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);

        _logger = logger;
        _exclusionPatterns = [];

        var patterns = options.Value?.ExclusionPatterns ?? [];

        foreach (var pattern in patterns)
        {
            try
            {
                _exclusionPatterns.Add(new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled));
            }
            catch (ArgumentException ex)
            {
                LogMessages.LogInvalidExclusionPattern(_logger, pattern, ex);
            }
        }

        if (_exclusionPatterns.Count == 0)
        {
            LogMessages.LogNoExclusionPatternsConfigured(_logger);
        }
        else
        {
            LogMessages.LogLoadedExclusionPatterns(_logger, _exclusionPatterns.Count);
        }
    }

    public Boolean ShouldExcludeFromPurchasesTotal(Models.Transaction transaction)
    {
        ArgumentNullException.ThrowIfNull(transaction);

        if (transaction.Amount <= 0)
        {
            return false;
        }

        foreach (var pattern in _exclusionPatterns)
        {
            if (pattern.IsMatch(transaction.Description))
            {
                LogMessages.LogTransactionExcludedFromPurchasesTotal(_logger, transaction.Description, transaction.Amount);
                return true;
            }
        }

        return false;
    }
}
