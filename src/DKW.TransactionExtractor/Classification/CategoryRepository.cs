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
            _logger.LogInformation("Loaded {Count} categories from {Path}", _config.Categories.Count, _categoryFilePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load category configuration from {Path}", _categoryFilePath);
            _config = new CategoryConfig();
        }

        return _config;
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

    public void AddDescriptionToCategory(String categoryId, String description)
    {
        var category = _config.Categories.FirstOrDefault(c => c.Id == categoryId);
        if (category == null)
        {
            return;
        }

        // Try to find an existing ExactMatch matcher
        var exactMatcher = category.Matchers.FirstOrDefault(m => 
            m.Type.Equals("ExactMatch", StringComparison.OrdinalIgnoreCase));

        if (exactMatcher != null)
        {
            // Append to existing ExactMatch values
            if (exactMatcher.Parameters.TryGetValue("values", out var valuesObj))
            {
                var existingValues = new List<String>();
                
                // Handle different serialization formats
                if (valuesObj is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Array)
                {
                    existingValues = jsonElement.EnumerateArray()
                        .Select(e => e.GetString() ?? String.Empty)
                        .ToList();
                }
                else if (valuesObj is String[] stringArray)
                {
                    existingValues = stringArray.ToList();
                }
                else if (valuesObj is List<String> stringList)
                {
                    existingValues = stringList;
                }

                // Add new value if not already present (case-insensitive check)
                if (!existingValues.Any(v => v.Equals(description, StringComparison.OrdinalIgnoreCase)))
                {
                    existingValues.Add(description);
                    exactMatcher.Parameters["values"] = existingValues.ToArray();
                    _logger.LogInformation("Added description '{Description}' to existing ExactMatch in category '{CategoryId}'", 
                        description, categoryId);
                }
                else
                {
                    _logger.LogDebug("Description '{Description}' already exists in category '{CategoryId}'", 
                        description, categoryId);
                }
            }
        }
        else
        {
            // Create new ExactMatch matcher
            var newMatcher = new CategoryMatcher
            {
                Type = "ExactMatch",
                Parameters = new Dictionary<String, Object>
                {
                    { "values", new[] { description } },
                    { "caseSensitive", false }
                }
            };
            category.Matchers.Add(newMatcher);
            _logger.LogInformation("Created new ExactMatch with description '{Description}' in category '{CategoryId}'", 
                description, categoryId);
        }

        Save(_config);
    }
}
