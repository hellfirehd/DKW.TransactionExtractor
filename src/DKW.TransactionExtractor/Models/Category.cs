using System.Diagnostics;

namespace DKW.TransactionExtractor.Models;

[DebuggerDisplay("{Id}: {Name}")]
public class Category
{
    public String Id { get; set; } = String.Empty;
    public String Name { get; set; } = String.Empty;
    public List<CategoryMatcher> Matchers { get; set; } = [];
    public Int32 SortOrder { get; set; } = 0;
}

public class CategoryComparer : IComparer<Category>
{
    public Int32 Compare(Category? x, Category? y)
    {
        if (x == null && y == null)
        {
            return 0;
        }

        if (x == null)
        {
            return 1;
        }

        if (y == null)
        {
            return -1;
        }

        var sortOrderComparison = x.SortOrder.CompareTo(y.SortOrder);
        if (sortOrderComparison != 0)
        {
            return sortOrderComparison;
        }

        return String.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase);
    }
}