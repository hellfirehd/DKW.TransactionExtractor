namespace DKW.TransactionExtractor.Models;

public class Category
{
    public String Id { get; set; } = String.Empty;
    public String Name { get; set; } = String.Empty;
    public List<CategoryMatcher> Matchers { get; set; } = [];
}
