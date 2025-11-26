using DKW.TransactionExtractor.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace DKW.TransactionExtractor.Classification;

public class CategoryRepository : ICategoryRepository
{
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
            _logger.LogWarning("Category configuration file not found at {Path}. Starting with empty configuration.", _categoryFilePath);
            _config = new CategoryConfig();
            return _config;
        }

        try
        {
            var json = File.ReadAllText(_categoryFilePath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            _config = JsonSerializer.Deserialize<CategoryConfig>(json, options) ?? new CategoryConfig();
            _logger.LogDebug("Loaded {Count} categories from {Path}", _config.Categories.Count, _categoryFilePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load category configuration from {Path}", _categoryFilePath);
            _config = new CategoryConfig();
        }

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
                _logger.LogInformation("Normalizing category ID from '{OldId}' to '{NewId}'", category.Id, normalizedId);
                category.Id = normalizedId;
                hasChanges = true;
            }
        }

        if (hasChanges)
        {
            Save(_config);
            _logger.LogInformation("Category IDs have been normalized and saved");
        }
    }

    public void Save(CategoryConfig config)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            var json = JsonSerializer.Serialize(config, options);

            var directory = Path.GetDirectoryName(_categoryFilePath);
            if (!String.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(_categoryFilePath, json);
            _config = config;
            _logger.LogInformation("Saved {Count} categories to {Path}", config.Categories.Count, _categoryFilePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save category configuration to {Path}", _categoryFilePath);
        }
    }

    public void AddCategory(Category category)
    {
        var existing = _config.Categories.FirstOrDefault(c => c.Id == category.Id);
        if (existing == null)
        {
            _config.Categories.Add(category);
            Save(_config);
        }
    }

    public void AddMatcherToCategory(String categoryId, CategoryMatcher matcher)
    {
        var category = _config.Categories.FirstOrDefault(c => c.Id == categoryId);
        if (category != null)
        {
            category.Matchers.Add(matcher);
            Save(_config);
        }
    }

    public void AddMatcherToCategory(String categoryId, MatcherCreationRequest request)
    {
        var category = _config.Categories.FirstOrDefault(c => c.Id == categoryId);
        if (category == null)
        {
            _logger.LogWarning("Cannot add matcher to category '{CategoryId}': category not found", categoryId);
            return;
        }

        // Regex matchers are always created as new instances (no merging)
        if (request.MatcherType == "Regex")
        {
            var newMatcher = CreateMatcherFromRequest(request);
            category.Matchers.Add(newMatcher);
            _logger.LogInformation("Added new Regex matcher to category '{CategoryId}'", categoryId);
            Save(_config);
            return;
        }

        // ExactMatch and Contains can be merged with existing matchers
        var merged = TryMergeWithExistingMatcher(category, request);

        if (merged)
        {
            _logger.LogInformation("Merged {MatcherType} values into existing matcher for category '{CategoryId}'",
                request.MatcherType, categoryId);
        }
        else
        {
            var newMatcher = CreateMatcherFromRequest(request);
            category.Matchers.Add(newMatcher);
            _logger.LogInformation("Added new {MatcherType} matcher to category '{CategoryId}'",
                request.MatcherType, categoryId);
        }

        Save(_config);
    }

    private Boolean TryMergeWithExistingMatcher(Category category, MatcherCreationRequest request)
    {
        var caseSensitive = request.Parameters.TryGetValue("caseSensitive", out var csValue)
            && Convert.ToBoolean(csValue);

        // Find existing matcher of the same type with matching case sensitivity
        var existingMatcher = category.Matchers.FirstOrDefault(m =>
            m.Type.Equals(request.MatcherType, StringComparison.OrdinalIgnoreCase) &&
            GetCaseSensitiveParameter(m) == caseSensitive);

        if (existingMatcher == null)
        {
            return false;
        }

        // Merge the values
        if (existingMatcher.Parameters.TryGetValue("values", out var existingValuesObj))
        {
            var existingValues = ExtractStringArray(existingValuesObj);
            var newValues = ExtractStringArray(request.Parameters["values"]);

            // Combine and deduplicate values (case-insensitive comparison for deduplication)
            var comparer = caseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;
            var mergedValues = existingValues
                .Concat(newValues)
                .Distinct(comparer)
                .ToArray();

            existingMatcher.Parameters["values"] = mergedValues;
            return true;
        }

        return false;
    }

    private Boolean GetCaseSensitiveParameter(CategoryMatcher matcher)
    {
        if (matcher.Parameters.TryGetValue("caseSensitive", out var csObj))
        {
            if (csObj is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.True)
            {
                return true;
            }

            if (csObj is Boolean boolValue)
            {
                return boolValue;
            }
        }

        return false;
    }

    private String[] ExtractStringArray(Object valuesObj)
    {
        if (valuesObj is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Array)
        {
            return jsonElement.EnumerateArray()
                .Select(e => e.GetString() ?? String.Empty)
                .ToArray();
        }
        else if (valuesObj is String[] stringArray)
        {
            return stringArray;
        }
        else if (valuesObj is List<String> stringList)
        {
            return stringList.ToArray();
        }

        return Array.Empty<String>();
    }

    private CategoryMatcher CreateMatcherFromRequest(MatcherCreationRequest request)
    {
        return new CategoryMatcher
        {
            Type = request.MatcherType,
            Parameters = new Dictionary<String, Object>(request.Parameters)
        };
    }
}
