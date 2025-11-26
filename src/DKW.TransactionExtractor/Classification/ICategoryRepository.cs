using DKW.TransactionExtractor.Models;

namespace DKW.TransactionExtractor.Classification;

public interface ICategoryRepository
{
    CategoryConfig Load();
    void Save(CategoryConfig config);
    void AddCategory(Category category);
    void AddMatcherToCategory(String categoryId, CategoryMatcher matcher);
    void AddDescriptionToCategory(String categoryId, String description);
}
