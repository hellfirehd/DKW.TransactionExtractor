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
            _config = JsonSerializer.Deserialize<CategoryConfig>(json, JSO) ?? new CategoryConfig();
            _logger.LogLoadedCategories(_config.Categories.Count, _categoryFilePath);
        }
        catch (Exception ex)
        {
            _logger.LogFailedToLoadCategoryConfig(ex, _categoryFilePath);
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
            _logger.LogCategoryNotFoundForMatcher(categoryId);
            return;
        }

        // Regex matchers are always created as new instances (no merging)
        if (request.MatcherType == "Regex")
        {
            var newMatcher = CreateMatcherFromRequest(request);
            category.Matchers.Add(newMatcher);
            _logger.LogAddedRegexMatcher(categoryId);
            Save(_config);
            return;
        }

        // ExactMatch and Contains can be merged with existing matchers
        var merged = TryMergeWithExistingMatcher(category, request);

        if (merged)
        {
            _logger.LogMergedMatcherValues(request.MatcherType, categoryId);
        }
        else
        {
            var newMatcher = CreateMatcherFromRequest(request);
            category.Matchers.Add(newMatcher);
            _logger.LogAddedNewMatcher(request.MatcherType, categoryId);
        }

        Save(_config);
    }

    private static Boolean TryMergeWithExistingMatcher(Category category, MatcherCreationRequest request)
    {
        // Parse request values into MatcherValue[]
        if (!request.Parameters.TryGetValue("values", out var valuesObj) || valuesObj is not JsonElement valuesElement || valuesElement.ValueKind != JsonValueKind.Array)
            return false;

        var newValues = valuesElement.EnumerateArray()
            .Select(e =>
            {
                if (e.ValueKind == JsonValueKind.Object)
                {
                    var v = e.GetProperty("value").GetString() ?? String.Empty;
                    Decimal? amt = null;
                    if (e.TryGetProperty("amount", out var amtProp) && amtProp.ValueKind == JsonValueKind.Number && amtProp.TryGetDecimal(out var d))
                    {
                        amt = Decimal.Round(d, 2);
                    }

                    return new MatcherValue(v, amt);
                }

                return new MatcherValue(e.GetString() ?? String.Empty, null);
            })
            .ToArray();

        // Find existing matcher of the same type (case-insensitive matching forced)
        var existingMatcher = category.Matchers.FirstOrDefault(m =>
            m.Type.Equals(request.MatcherType, StringComparison.OrdinalIgnoreCase)
        );

        if (existingMatcher == null)
        {
            return false;
        }

        // Merge the values (existing matcher parameters expected to have "values" as array of objects)
        if (existingMatcher.Parameters.TryGetValue("values", out var existingValuesObj) && existingValuesObj is JsonElement existingValuesElement && existingValuesElement.ValueKind == JsonValueKind.Array)
        {
            var existingValues = existingValuesElement.EnumerateArray()
                .Select(e =>
                {
                    if (e.ValueKind == JsonValueKind.Object)
                    {
                        var v = e.GetProperty("value").GetString() ?? String.Empty;
                        Decimal? amt = null;
                        if (e.TryGetProperty("amount", out var amtProp) && amtProp.ValueKind == JsonValueKind.Number && amtProp.TryGetDecimal(out var d))
                        {
                            amt = Decimal.Round(d, 2);
                        }

                        return new MatcherValue(v, amt);
                    }

                    return new MatcherValue(e.GetString() ?? String.Empty, null);
                })
                .ToArray();

            var comparer = StringComparer.OrdinalIgnoreCase; // Force case-insensitive dedupe

            // Combine, dedupe by (value, amount) using string comparer for value and amount equality
            var mergedValues = existingValues
                .Concat(newValues)
                .Distinct(new MatcherValueEqualityComparer(comparer))
                .ToArray();

            // Convert merged values back to JsonElement array
            var mergedJsonElements = mergedValues.Select(mv =>
            {
                var dict = new Dictionary<String, Object>
                {
                    ["value"] = mv.Value
                };
                if (mv.Amount.HasValue)
                {
                    dict["amount"] = Decimal.Round(mv.Amount.Value, 2);
                }
                return JsonSerializer.SerializeToElement(dict);
            }).ToArray();

            existingMatcher.Parameters["values"] = JsonSerializer.SerializeToElement(mergedJsonElements);
            return true;
        }

        return false;
    }

    private static Decimal? GetAmountParameter(CategoryMatcher matcher)
    {
        if (matcher.Parameters.TryGetValue("amount", out var amtObj))
        {
            if (amtObj is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Number)
            {
                if (jsonElement.TryGetDecimal(out var d))
                {
                    return Decimal.Round(d, 2);
                }
            }

            if (amtObj is Decimal dec)
            {
                return Decimal.Round(dec, 2);
            }

            if (amtObj is Double dbl)
            {
                return Decimal.Round(Convert.ToDecimal(dbl), 2);
            }

            if (amtObj is String s && Decimal.TryParse(s, out var parsed))
            {
                return Decimal.Round(parsed, 2);
            }
        }

        return null;
    }

    private static String[] ExtractStringArray(Object valuesObj)
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

    private static CategoryMatcher CreateMatcherFromRequest(MatcherCreationRequest request)
    {
        return new CategoryMatcher
        {
            Type = request.MatcherType,
            Parameters = new Dictionary<String, Object>(request.Parameters)
        };
    }
}

internal class MatcherValueEqualityComparer : IEqualityComparer<MatcherValue>
{
    private readonly StringComparer _stringComparer;

    public MatcherValueEqualityComparer(StringComparer stringComparer)
    {
        _stringComparer = stringComparer;
    }

    public bool Equals(MatcherValue? x, MatcherValue? y)
    {
        if (x == null && y == null) return true;
        if (x == null || y == null) return false;

        var valueEqual = _stringComparer.Equals(x.Value, y.Value);
        var amountEqual = Nullable.Equals(x.Amount, y.Amount);
        return valueEqual && amountEqual;
    }

    public int GetHashCode(MatcherValue obj)
    {
        var hash = _stringComparer.GetHashCode(obj.Value ?? String.Empty);
        hash = (hash * 397) ^ (obj.Amount?.GetHashCode() ?? 0);
        return hash;
    }
}
