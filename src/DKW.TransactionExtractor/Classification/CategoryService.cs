using DKW.TransactionExtractor.Models;

namespace DKW.TransactionExtractor.Classification;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _repository;

    public CategoryService(ICategoryRepository repository)
    {
        _repository = repository;
    }

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
        _repository.AddCategory(category);
    }

    public void AddMatcherToCategory(String categoryId, MatcherCreationRequest request)
    {
        _repository.AddMatcherToCategory(categoryId, request);
    }

    public Boolean CategoryExists(String categoryId)
    {
        return _repository.Load().Categories.Any(c => c.Id == categoryId);
    }
}
