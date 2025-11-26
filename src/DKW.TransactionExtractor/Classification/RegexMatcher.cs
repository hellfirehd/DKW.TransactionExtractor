using System.Text.RegularExpressions;

namespace DKW.TransactionExtractor.Classification;

public class RegexMatcher : ITransactionMatcher
{
    private readonly Regex _regex;

    public RegexMatcher(String pattern, Boolean ignoreCase = false)
    {
        var options = RegexOptions.Compiled;
        if (ignoreCase)
        {
            options |= RegexOptions.IgnoreCase;
        }

        _regex = new Regex(pattern, options);
    }

    public Boolean TryMatch(String description)
    {
        return _regex.IsMatch(description);
    }
}
