using System.Text.RegularExpressions;

namespace DKW.TransactionExtractor.Classification;

public class RegexMatcher : ITransactionMatcher
{
    private readonly Regex _regex;

    public RegexMatcher(String pattern, Boolean ignoreCase = false)
    {
        ArgumentException.ThrowIfNullOrEmpty(pattern);

        var options = RegexOptions.Compiled;
        if (ignoreCase)
        {
            options |= RegexOptions.IgnoreCase;
        }

        try
        {
            _regex = new Regex(pattern, options);
        }
        catch (RegexParseException ex)
        {
            throw new ArgumentException($"Invalid regex pattern: {pattern}", nameof(pattern), ex);
        }
    }

    public Boolean TryMatch(String description)
    {
        if (String.IsNullOrWhiteSpace(description))
        {
            return false;
        }

        return _regex.IsMatch(description);
    }
}
