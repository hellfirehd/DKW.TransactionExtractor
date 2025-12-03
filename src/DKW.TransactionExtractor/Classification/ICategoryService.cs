using DKW.TransactionExtractor.Models;

namespace DKW.TransactionExtractor.Classification;

public interface ICategoryService
{
    /// <summary>
    /// Gets all available categories. Caller is responsible for sorting if needed.
    /// </summary>
    List<Category> GetCategories();

    /// <summary>
    /// Adds a new category to the repository.
    /// </summary>
    void AddCategory(Category category);

    /// <summary>
    /// Adds a matcher to an existing category based on the provided request.
    /// For ExactMatch and Contains types, this will merge with existing matchers.
    /// For Regex types, this will always create a new matcher.
    /// </summary>
    void AddMatcherToCategory(String categoryId, MatcherCreationRequest request);

    /// <summary>
    /// Checks if a category with the specified ID exists.
    /// </summary>
    Boolean CategoryExists(String categoryId);
}
