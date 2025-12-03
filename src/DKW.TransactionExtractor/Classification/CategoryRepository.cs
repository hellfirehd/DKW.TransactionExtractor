using DKW.TransactionExtractor.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace DKW.TransactionExtractor.Classification;

public class CategoryRepository : ICategoryRepository
{
    private static readonly JsonSerializerOptions JSO = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly ILogger<CategoryRepository> _logger;
    private readonly String _categoryFilePath;
    private CategoryConfig _config;

    public CategoryRepository(ILogger<CategoryRepository> logger, IOptions<AppOptions> options)
    {
        _logger = logger;
        _categoryFilePath = options.Value.CategoryConfigPath;
        _config = new CategoryConfig();

        // Load and normalize existing categories
        Load();
        NormalizeExistingCategoryIds();
    }

    public CategoryConfig Load()
    {
        if (!File.Exists(_categoryFilePath))
        {
            _logger.LogCategoryConfigNotFound(_categoryFilePath);
            _config = new CategoryConfig();
            return _config;
        }

        try
        {
            var json = File.ReadAllText(_categoryFilePath);
            _config = LoadFromJson(json);
        }
        catch (Exception ex)
        {
            _logger.LogFailedToLoadCategoryConfig(ex, _categoryFilePath);
            _config = new CategoryConfig();
        }

        return _config;
    }

    public CategoryConfig LoadFromJson(String json)
    {
        _config = JsonSerializer.Deserialize<CategoryConfig>(json, JSO)
            ?? throw new InvalidOperationException("Unable to deserialize the JSON as CategoryConfig.");
        _logger.LogLoadedCategories(_config.Categories.Count, _categoryFilePath);
        return _config;
    }

    /// <summary>
    /// Normalizes category IDs in the existing configuration.
    /// This is called once during initialization to migrate any legacy category IDs.
    /// </summary>
    private void NormalizeExistingCategoryIds()
    {
        var hasChanges = false;

        foreach (var category in _config.Categories)
        {
            var normalizedId = CategoryIdNormalizer.Normalize(category.Id);

            if (category.Id != normalizedId)
            {
                _logger.LogNormalizingCategoryId(category.Id, normalizedId);
                category.Id = normalizedId;
                hasChanges = true;
            }
        }

        if (hasChanges)
        {
            Save(_config);
            _logger.LogCategoryIdsNormalized();
        }
    }

    public void Save(CategoryConfig config)
    {
        try
        {
            var json = JsonSerializer.Serialize(config, JSO);

            var directory = Path.GetDirectoryName(_categoryFilePath);
            if (!String.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(_categoryFilePath, json);
            _config = config;
            _logger.LogSavedCategories(config.Categories.Count, _categoryFilePath);
        }
        catch (Exception ex)
        {
            _logger.LogFailedToSaveCategoryConfig(ex, _categoryFilePath);
        }
    }

    public CategoryConfig GetConfig()
    {
        return _config;
    }

    public void AddCategory(Category category)
    {
        var existing = _config.Categories.SingleOrDefault(c => c.Id == category.Id);
        if (existing == null)
        {
            _config.Categories.Add(category);
            Save(_config);
        }
    }

    public void AddMatcherToCategory(String categoryId, CategoryMatcher matcher)
    {
        var category = _config.Categories.SingleOrDefault(c => c.Id == categoryId);
        if (category != null)
        {
            var existingMatcher = category.Matchers.FirstOrDefault(m => m.Type == matcher.Type);

            if (existingMatcher is null)
            {
                category.Matchers.Add(matcher);
            }
            else
            {
                foreach (var p in matcher.Parameters)
                {
                    existingMatcher.Parameters.Add(p);
                }
            }

            Save(_config);
        }
    }

    public void AddMatcherToCategory(String categoryId, MatcherCreationRequest request)
    {
        var newMatcher = CreateMatcherFromRequest(request);

        AddMatcherToCategory(categoryId, newMatcher);
    }

    private static CategoryMatcher CreateMatcherFromRequest(MatcherCreationRequest request)
    {
        var matcher = new CategoryMatcher() { Type = request.MatcherType };

        foreach (var parameter in request.Parameters)
        {
            matcher.Parameters.Add(parameter);
        }

        return matcher;
    }
}

internal class MatcherValueEqualityComparer(StringComparer stringComparer) : IEqualityComparer<MatcherValue>
{
    private readonly StringComparer _stringComparer = stringComparer;

    public Boolean Equals(MatcherValue? x, MatcherValue? y)
    {
        if (x == null && y == null)
        {
            return true;
        }

        if (x == null || y == null)
        {
            return false;
        }

        var valueEqual = _stringComparer.Equals(x.Value, y.Value);
        var amountEqual = Nullable.Equals(x.Amount, y.Amount);
        return valueEqual && amountEqual;
    }

    public Int32 GetHashCode(MatcherValue obj)
    {
        var hash = _stringComparer.GetHashCode(obj.Value ?? String.Empty);
        hash = (hash * 397) ^ (obj.Amount?.GetHashCode() ?? 0);
        return hash;
    }
}
