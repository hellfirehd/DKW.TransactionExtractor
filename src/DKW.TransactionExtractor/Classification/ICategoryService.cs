using DKW.TransactionExtractor.Models;

namespace DKW.TransactionExtractor.Classification;

public interface ICategoryService
{
    /// <summary>
    /// Gets all available categories, sorted alphabetically by name.
    /// </summary>
    List<Category> GetAvailableCategories();

    /// <summary>
    /// Adds a new category to the repository.
    /// </summary>
    void AddCategory(Category category);

    /// <summary>
    /// Adds a matcher to an existing category.
    /// </summary>
    void AddMatcherToCategory(String categoryId, CategoryMatcher matcher);

    /// <summary>
    /// Adds a description value to an existing ExactMatch matcher in the category,
    /// or creates a new ExactMatch matcher if one doesn't exist.
    /// </summary>
    void AddDescriptionToCategory(String categoryId, String description);

    /// <summary>
    /// Checks if a category with the specified ID exists.
    /// </summary>
    Boolean CategoryExists(String categoryId);

    /// <summary>
    /// Gets all categories without sorting (for matching operations).
    /// </summary>
    List<Category> GetAllCategories();
}
