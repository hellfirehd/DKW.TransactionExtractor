using System.Diagnostics;

namespace DKW.TransactionExtractor.Models;

[DebuggerDisplay("{Id}: {Name}")]
public class Category
{
    public String Id { get; set; } = String.Empty;
    public String Name { get; set; } = String.Empty;
    public List<CategoryMatcher> Matchers { get; set; } = [];
}
