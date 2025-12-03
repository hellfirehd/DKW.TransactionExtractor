using DKW.TransactionExtractor.Classification;
using System.Diagnostics;

namespace DKW.TransactionExtractor.Models;

[DebuggerDisplay("{Type}")]
public class CategoryMatcher
{
    public String Type { get; set; } = String.Empty;
    public HashSet<MatcherValue> Parameters { get; set; } = [];
}
