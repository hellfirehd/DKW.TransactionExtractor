using DKW.TransactionExtractor.Models;

namespace DKW.TransactionExtractor.Classification;

public class CategoryService(ICategoryRepository repository) : ICategoryService
{
    private readonly ICategoryRepository _repository = repository;

    public List<Category> GetAvailableCategories()
    {
        return _repository.Load().Categories
            .OrderBy(c => c.Name)
            .ToList();
    }

    public List<Category> GetAllCategories()
    {
        return _repository.Load().Categories;
    }

    public void AddCategory(Category category)
    {
        // Normalize the category ID before adding
        category.Id = CategoryIdNormalizer.Normalize(category.Id);
        _repository.AddCategory(category);
    }

    public void AddMatcherToCategory(String categoryId, MatcherCreationRequest request)
    {
        // Normalize the category ID before looking it up
        var normalizedId = CategoryIdNormalizer.Normalize(categoryId);
        _repository.AddMatcherToCategory(normalizedId, request);
    }

    public Boolean CategoryExists(String categoryId)
    {
        // Normalize the category ID before checking
        var normalizedId = CategoryIdNormalizer.Normalize(categoryId);
        return _repository.Load().Categories.Any(c => c.Id == normalizedId);
    }
}
