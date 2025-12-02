using DKW.TransactionExtractor.Classification;
using DKW.TransactionExtractor.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Xunit;

namespace DKW.TransactionExtractor.Tests;

/// <summary>
/// Unit tests for the CategoryRepository class.
/// Verifies category loading, saving, and matcher management.
/// </summary>
public class CategoryRepositoryTests : IDisposable
{
    private readonly String _testConfigPath;
    private readonly CategoryRepository _repository;

    public CategoryRepositoryTests()
    {
        // Create a temporary test file path
        _testConfigPath = Path.Combine(Path.GetTempPath(), $"test-categories-{Guid.NewGuid()}.json");

        var options = Options.Create(new AppOptions
        {
            CategoryConfigPath = _testConfigPath
        });

        _repository = new CategoryRepository(NullLogger<CategoryRepository>.Instance, options);
    }

    public void Dispose()
    {
        // Cleanup test file
        if (File.Exists(_testConfigPath))
        {
            File.Delete(_testConfigPath);
        }

        GC.SuppressFinalize(this);
    }

    [Fact]
    public void Load_FileDoesNotExist_ReturnsEmptyConfig()
    {
        var config = _repository.Load();

        Assert.NotNull(config);
        Assert.Empty(config.Categories);
    }

    [Fact]
    public void Save_ValidConfig_WritesFileSuccessfully()
    {
        var valuesArray = new[]
        {
            JsonSerializer.SerializeToElement(new { value = "WALMART" }),
            JsonSerializer.SerializeToElement(new { value = "TARGET" })
        };

        var config = new CategoryConfig
        {
            Categories =
            [
                new Category
                {
                    Id = "groceries",
                    Name = "Groceries",
                    Matchers =
                    [
                        new CategoryMatcher
                        {
                            Type = "ExactMatch",
                            Parameters = new Dictionary<String, Object>
                            {
                                ["values"] = JsonSerializer.SerializeToElement(valuesArray)
                            }
                        }
                    ]
                }
            ]
        };

        _repository.Save(config);

        Assert.True(File.Exists(_testConfigPath));
        var savedJson = File.ReadAllText(_testConfigPath);
        Assert.Contains("groceries", savedJson);
        Assert.Contains("Groceries", savedJson);
        Assert.Contains("WALMART", savedJson);
    }

    [Fact]
    public void Load_ValidJsonFile_LoadsConfigCorrectly()
    {
        // Arrange: Create a test JSON file
        var valuesArray = new[]
        {
            JsonSerializer.SerializeToElement(new { value = "MCDONALD" }),
            JsonSerializer.SerializeToElement(new { value = "WENDY" })
        };

        var testConfig = new CategoryConfig
        {
            Categories =
            [
                new Category
                {
                    Id = "restaurants",
                    Name = "Restaurants",
                    Matchers =
                    [
                        new CategoryMatcher
                        {
                            Type = "Contains",
                            Parameters = new Dictionary<String, Object>
                            {
                                ["values"] = JsonSerializer.SerializeToElement(valuesArray)
                            }
                        }
                    ]
                }
            ]
        };

        _repository.Save(testConfig);

        // Act: Load the config
        var loadedConfig = _repository.Load();

        // Assert
        Assert.NotNull(loadedConfig);
        Assert.Single(loadedConfig.Categories);
        Assert.Equal("restaurants", loadedConfig.Categories[0].Id);
        Assert.Equal("Restaurants", loadedConfig.Categories[0].Name);
        Assert.Single(loadedConfig.Categories[0].Matchers);
    }

    [Fact]
    public void AddCategory_NewCategory_AddsSuccessfully()
    {
        var newCategory = new Category
        {
            Id = "entertainment",
            Name = "Entertainment",
            Matchers = []
        };

        _repository.AddCategory(newCategory);

        var config = _repository.Load();
        Assert.Contains(config.Categories, c => c.Id == "entertainment");
    }

    [Fact]
    public void AddCategory_DuplicateCategory_DoesNotAddTwice()
    {
        var category = new Category
        {
            Id = "groceries",
            Name = "Groceries",
            Matchers = []
        };

        _repository.AddCategory(category);
        _repository.AddCategory(category); // Add same category again

        var config = _repository.Load();
        Assert.Single(config.Categories, c => c.Id == "groceries");
    }

    [Fact]
    public void AddMatcherToCategory_ExactMatchMatcher_AddsSuccessfully()
    {
        // Arrange: Create a category
        var category = new Category
        {
            Id = "groceries",
            Name = "Groceries",
            Matchers = []
        };
        _repository.AddCategory(category);

        // Act: Add a matcher
        var valuesArray = new[] { JsonSerializer.SerializeToElement(new { value = "WALMART" }) };

        var matcherRequest = new MatcherCreationRequest(
            MatcherType: "ExactMatch",
            Parameters: new Dictionary<String, Object>
            {
                ["values"] = JsonSerializer.SerializeToElement(valuesArray)
            }
        );

        _repository.AddMatcherToCategory("groceries", matcherRequest);

        // Assert
        var config = _repository.Load();
        var savedCategory = config.Categories.First(c => c.Id == "groceries");
        Assert.Single(savedCategory.Matchers);
        Assert.Equal("ExactMatch", savedCategory.Matchers[0].Type);
    }

    [Fact]
    public void AddMatcherToCategory_RegexMatcher_AddsAsNewInstance()
    {
        // Arrange: Create a category with existing regex matcher
        var category = new Category
        {
            Id = "stores",
            Name = "Stores",
            Matchers =
            [
                new CategoryMatcher
                {
                    Type = "Regex",
                    Parameters = new Dictionary<String, Object>
                    {
                        ["pattern"] = JsonSerializer.SerializeToElement("^WALMART #\\d+")
                    }
                }
            ]
        };
        _repository.AddCategory(category);

        // Act: Add another regex matcher
        var matcherRequest = new MatcherCreationRequest(
            MatcherType: "Regex",
            Parameters: new Dictionary<String, Object>
            {
                ["pattern"] = JsonSerializer.SerializeToElement("^TARGET #\\d+")
            }
        );

        _repository.AddMatcherToCategory("stores", matcherRequest);

        // Assert: Should have 2 separate regex matchers (no merging)
        var config = _repository.Load();
        var savedCategory = config.Categories.First(c => c.Id == "stores");
        Assert.Equal(2, savedCategory.Matchers.Count);
        Assert.All(savedCategory.Matchers, m => Assert.Equal("Regex", m.Type));
    }

    [Fact]
    public void AddMatcherToCategory_ExactMatchWithSameCaseSensitivity_MergesValues()
    {
        // Arrange: Create a category with existing ExactMatch matcher
        var existingValues = new[] { JsonSerializer.SerializeToElement(new { value = "WALMART" }) };
        var category = new Category
        {
            Id = "groceries",
            Name = "Groceries",
            Matchers =
            [
                new CategoryMatcher
                {
                    Type = "ExactMatch",
                    Parameters = new Dictionary<String, Object>
                    {
                        ["values"] = JsonSerializer.SerializeToElement(existingValues)
                    }
                }
            ]
        };
        _repository.AddCategory(category);

        // Act: Add another ExactMatch
        var newValues = new[] { JsonSerializer.SerializeToElement(new { value = "TARGET" }), JsonSerializer.SerializeToElement(new { value = "COSTCO" }) };
        var matcherRequest = new MatcherCreationRequest(
            MatcherType: "ExactMatch",
            Parameters: new Dictionary<String, Object>
            {
                ["values"] = JsonSerializer.SerializeToElement(newValues)
            }
        );

        _repository.AddMatcherToCategory("groceries", matcherRequest);

        // Assert: Should merge into single matcher with all values
        var config = _repository.Load();
        var savedCategory = config.Categories.First(c => c.Id == "groceries");
        Assert.Single(savedCategory.Matchers);

        var matcher = savedCategory.Matchers[0];
        Assert.Equal("ExactMatch", matcher.Type);

        var values = ExtractValuesFromParameters(matcher.Parameters);
        Assert.Equal(3, values.Length);
        Assert.Contains("WALMART", values);
        Assert.Contains("TARGET", values);
        Assert.Contains("COSTCO", values);
    }

    [Fact]
    public void AddMatcherToCategory_ExactMatchWithDifferentCaseSensitivity_CreatesNewMatcher()
    {
        // This test updated for removed caseSensitivity: adding an ExactMatch will merge since all matchers are case-insensitive
        var existingValues = new[] { JsonSerializer.SerializeToElement(new { value = "WALMART" }) };
        var category = new Category
        {
            Id = "stores",
            Name = "Stores",
            Matchers =
            [
                new CategoryMatcher
                {
                    Type = "ExactMatch",
                    Parameters = new Dictionary<String, Object>
                    {
                        ["values"] = JsonSerializer.SerializeToElement(existingValues)
                    }
                }
            ]
        };
        _repository.AddCategory(category);

        // Act: Add another ExactMatch (previously case-sensitive difference created new matcher)
        var matcherRequest = new MatcherCreationRequest(
            MatcherType: "ExactMatch",
            Parameters: new Dictionary<String, Object>
            {
                ["values"] = JsonSerializer.SerializeToElement(new[] { JsonSerializer.SerializeToElement(new { value = "TARGET" }) })
            }
        );

        _repository.AddMatcherToCategory("stores", matcherRequest);

        // Assert: Should merge into single matcher under new rules
        var config = _repository.Load();
        var savedCategory = config.Categories.First(c => c.Id == "stores");
        Assert.Single(savedCategory.Matchers);
    }

    [Fact]
    public void AddMatcherToCategory_NonExistentCategory_DoesNotThrow()
    {
        var matcherRequest = new MatcherCreationRequest(
            MatcherType: "ExactMatch",
            Parameters: new Dictionary<String, Object>
            {
                ["values"] = JsonSerializer.SerializeToElement(new[] { JsonSerializer.SerializeToElement(new { value = "WALMART" }) })
            }
        );

        // Should not throw, just log warning
        _repository.AddMatcherToCategory("nonexistent", matcherRequest);

        var config = _repository.Load();
        Assert.Empty(config.Categories);
    }

    [Fact]
    public void Load_InvalidJson_ReturnsEmptyConfig()
    {
        // Write invalid JSON to file
        File.WriteAllText(_testConfigPath, "{ invalid json }");

        var config = _repository.Load();

        Assert.NotNull(config);
        Assert.Empty(config.Categories);
    }

    [Fact]
    public void Save_CreatesDirectoryIfNotExists()
    {
        // Use a path with non-existent directory
        var subDir = Path.Combine(Path.GetTempPath(), $"test-subdir-{Guid.NewGuid()}");
        var filePath = Path.Combine(subDir, "categories.json");

        try
        {
            var options = Options.Create(new AppOptions { CategoryConfigPath = filePath });
            var repo = new CategoryRepository(NullLogger<CategoryRepository>.Instance, options);

            var config = new CategoryConfig
            {
                Categories =
                [
                    new Category { Id = "test", Name = "Test", Matchers = [] }
                ]
            };

            repo.Save(config);

            Assert.True(Directory.Exists(subDir));
            Assert.True(File.Exists(filePath));
        }
        finally
        {
            if (Directory.Exists(subDir))
            {
                Directory.Delete(subDir, true);
            }
        }
    }

    [Fact]
    public void AddMatcherToCategory_ContainsMatcherMerging_WorksCorrectly()
    {
        // Arrange: Create category with Contains matcher
        var existingValues = new[] { JsonSerializer.SerializeToElement(new { value = "MCDONALD" }) };
        var category = new Category
        {
            Id = "restaurants",
            Name = "Restaurants",
            Matchers =
            [
                new CategoryMatcher
                {
                    Type = "Contains",
                    Parameters = new Dictionary<String, Object>
                    {
                        ["values"] = JsonSerializer.SerializeToElement(existingValues)
                    }
                }
            ]
        };
        _repository.AddCategory(category);

        // Act: Add another Contains matcher
        var newValues = new[] { JsonSerializer.SerializeToElement(new { value = "WENDY" }), JsonSerializer.SerializeToElement(new { value = "BURGER" }) };
        var matcherRequest = new MatcherCreationRequest(
            MatcherType: "Contains",
            Parameters: new Dictionary<String, Object>
            {
                ["values"] = JsonSerializer.SerializeToElement(newValues)
            }
        );

        _repository.AddMatcherToCategory("restaurants", matcherRequest);

        // Assert: Should merge values
        var config = _repository.Load();
        var savedCategory = config.Categories.First(c => c.Id == "restaurants");
        Assert.Single(savedCategory.Matchers);

        var values = ExtractValuesFromParameters(savedCategory.Matchers[0].Parameters);
        Assert.Equal(3, values.Length);
        Assert.Contains("MCDONALD", values);
        Assert.Contains("WENDY", values);
        Assert.Contains("BURGER", values);
    }

    [Fact]
    public void AddMatcherToCategory_DuplicateValues_DeduplicatesCorrectly()
    {
        // Arrange: Create category with matcher
        var existingValues = new[] { JsonSerializer.SerializeToElement(new { value = "WALMART" }), JsonSerializer.SerializeToElement(new { value = "TARGET" }) };
        var category = new Category
        {
            Id = "stores",
            Name = "Stores",
            Matchers =
            [
                new CategoryMatcher
                {
                    Type = "ExactMatch",
                    Parameters = new Dictionary<String, Object>
                    {
                        ["values"] = JsonSerializer.SerializeToElement(existingValues)
                    }
                }
            ]
        };
        _repository.AddCategory(category);

        // Act: Add matcher with duplicate value
        var matcherRequest = new MatcherCreationRequest(
            MatcherType: "ExactMatch",
            Parameters: new Dictionary<String, Object>
            {
                ["values"] = JsonSerializer.SerializeToElement(new[] { JsonSerializer.SerializeToElement(new { value = "WALMART" }), JsonSerializer.SerializeToElement(new { value = "COSTCO" }) }) // WALMART is duplicate
            }
        );

        _repository.AddMatcherToCategory("stores", matcherRequest);

        // Assert: WALMART should only appear once
        var config = _repository.Load();
        var savedCategory = config.Categories.First(c => c.Id == "stores");
        var values = ExtractValuesFromParameters(savedCategory.Matchers[0].Parameters);

        Assert.Equal(3, values.Length); // WALMART, TARGET, COSTCO (no duplicate)
        Assert.Single(values, v => v == "WALMART");
    }

    private static String[] ExtractValuesFromParameters(Dictionary<String, Object> parameters)
    {
        if (parameters.TryGetValue("values", out var valuesObj))
        {
            if (valuesObj is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Array)
            {
                return jsonElement.EnumerateArray()
                    .Select(e =>
                    {
                        if (e.ValueKind == JsonValueKind.Object && e.TryGetProperty("value", out var v))
                            return v.GetString() ?? String.Empty;
                        return e.GetString() ?? String.Empty;
                    })
                    .ToArray();
            }
            else if (valuesObj is String[] stringArray)
            {
                return stringArray;
            }
            else if (valuesObj is IEnumerable<String> enumerable)
            {
                return enumerable.ToArray();
            }
        }

        return Array.Empty<String>();
    }
}
