using DKW.TransactionExtractor.Classification;
using DKW.TransactionExtractor.Models;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using System.Text.Json;
using Xunit;

namespace DKW.TransactionExtractor.Tests;

/// <summary>
/// Unit tests for the TransactionClassifier class.
/// Verifies transaction classification logic and category matching.
/// </summary>
public class TransactionClassifierTests
{
    private readonly ICategoryService _mockCategoryService;
    private readonly IUserInteraction _mockConsoleInteraction;
    private readonly TransactionClassifier _classifier;

    public TransactionClassifierTests()
    {
        _mockCategoryService = Substitute.For<ICategoryService>();
        _mockConsoleInteraction = Substitute.For<IUserInteraction>();

        _classifier = new TransactionClassifier(
            NullLogger<TransactionClassifier>.Instance,
            _mockCategoryService,
            _mockConsoleInteraction
        );
    }

    [Fact]
    public void ClassifyTransactions_EmptyList_ReturnsEmptyResult()
    {
        var transactions = new List<Transaction>();

        var result = _classifier.ClassifyTransactions(transactions);

        Assert.NotNull(result);
        Assert.Empty(result.ClassifiedTransactions);
        Assert.False(result.RequestedEarlyExit);
    }

    [Fact]
    public void ClassifyTransactions_AutomaticMatch_ClassifiesCorrectly()
    {
        // Arrange: Setup category with matcher
        var category = new Category
        {
            Id = "groceries",
            Name = "Groceries",
            Matchers =
            [
                new CategoryMatcher
                {
                    Type = "Contains",
                    Parameters = new Dictionary<String, Object>
                    {
                        ["values"] = JsonSerializer.SerializeToElement(new[] { "WALMART" }),
                        ["caseSensitive"] = JsonSerializer.SerializeToElement(false)
                    }
                }
            ]
        };

        _mockCategoryService.GetAllCategories().Returns([category]);

        var transactions = new List<Transaction>
        {
            new() {
                TransactionDate = new DateTime(2025, 10, 15),
                Description = "WALMART SUPERCENTER #1234",
                Amount = 50.00m
            }
        };

        // Act
        var result = _classifier.ClassifyTransactions(transactions);

        // Assert
        Assert.Single(result.ClassifiedTransactions);
        var classified = result.ClassifiedTransactions[0];
        Assert.Equal("groceries", classified.CategoryId);
        Assert.Equal("Groceries", classified.CategoryName);
        Assert.Equal(transactions[0], classified.Transaction);
        Assert.False(result.RequestedEarlyExit);
    }

    [Fact]
    public void ClassifyTransactions_NoMatch_PromptsUser()
    {
        // Arrange: No matching categories
        _mockCategoryService.GetAllCategories().Returns([]);

        var selectionResult = new CategorySelectionResult(
            CategoryId: "entertainment",
            CategoryName: "Entertainment",
            MatcherRequest: null,
            RequestedExit: false
        );

        _mockConsoleInteraction.PromptForCategory(Arg.Any<TransactionContext>())
            .Returns(selectionResult);

        _mockCategoryService.CategoryExists("entertainment").Returns(true);

        var transactions = new List<Transaction>
        {
            new() {
                TransactionDate = new DateTime(2025, 10, 15),
                Description = "MOVIE THEATER",
                Amount = 25.00m
            }
        };

        // Act
        var result = _classifier.ClassifyTransactions(transactions);

        // Assert
        Assert.Single(result.ClassifiedTransactions);
        Assert.Equal("entertainment", result.ClassifiedTransactions[0].CategoryId);
        _mockConsoleInteraction.Received(1).PromptForCategory(Arg.Any<TransactionContext>());
    }

    [Fact]
    public void ClassifyTransactions_UserRequestsExit_StopsProcessing()
    {
        // Arrange
        _mockCategoryService.GetAllCategories().Returns([]);

        var exitResult = new CategorySelectionResult(
            CategoryId: "",
            CategoryName: "",
            MatcherRequest: null,
            RequestedExit: true
        );

        _mockConsoleInteraction.PromptForCategory(Arg.Any<TransactionContext>())
            .Returns(exitResult);

        var transactions = new List<Transaction>
        {
            new() { TransactionDate = DateTime.Now, Description = "STORE 1", Amount = 10m },
            new() { TransactionDate = DateTime.Now, Description = "STORE 2", Amount = 20m },
            new() { TransactionDate = DateTime.Now, Description = "STORE 3", Amount = 30m }
        };

        // Act
        var result = _classifier.ClassifyTransactions(transactions);

        // Assert: When user exits, classified transactions is empty because exit happens before adding
        Assert.Empty(result.ClassifiedTransactions);
        Assert.True(result.RequestedEarlyExit);
        _mockConsoleInteraction.Received(1).PromptForCategory(Arg.Any<TransactionContext>());
    }

    [Fact]
    public void ClassifyTransactions_CreateNewCategory_AddsCategory()
    {
        // Arrange: No existing categories
        _mockCategoryService.GetAllCategories().Returns([]);

        var matcherRequest = new MatcherCreationRequest(
            MatcherType: "Contains",
            Parameters: new Dictionary<String, Object>
            {
                ["values"] = new[] { "COFFEE" },
                ["caseSensitive"] = false
            }
        );

        var selectionResult = new CategorySelectionResult(
            CategoryId: "coffee-shops",
            CategoryName: "Coffee Shops",
            MatcherRequest: matcherRequest,
            RequestedExit: false
        );

        _mockConsoleInteraction.PromptForCategory(Arg.Any<TransactionContext>())
            .Returns(selectionResult);

        _mockCategoryService.CategoryExists("coffee-shops").Returns(false);

        var transactions = new List<Transaction>
        {
            new() {
                TransactionDate = new DateTime(2025, 10, 15),
                Description = "STARBUCKS COFFEE",
                Amount = 5.50m
            }
        };

        // Act
        var result = _classifier.ClassifyTransactions(transactions);

        // Assert
        _mockCategoryService.Received(1).AddCategory(Arg.Is<Category>(c =>
            c.Id == "coffee-shops" &&
            c.Name == "Coffee Shops" &&
            c.Matchers.Count == 1
        ));
    }

    [Fact]
    public void ClassifyTransactions_AddMatcherToExistingCategory_AddsMatcherOnly()
    {
        // Arrange: Existing category without matching rule for this transaction
        _mockCategoryService.GetAllCategories().Returns([]);

        var matcherRequest = new MatcherCreationRequest(
            MatcherType: "Contains",
            Parameters: new Dictionary<String, Object>
            {
                ["values"] = new[] { "TARGET" },
                ["caseSensitive"] = false
            }
        );

        var selectionResult = new CategorySelectionResult(
            CategoryId: "groceries",
            CategoryName: "Groceries",
            MatcherRequest: matcherRequest,
            RequestedExit: false
        );

        _mockConsoleInteraction.PromptForCategory(Arg.Any<TransactionContext>())
            .Returns(selectionResult);

        _mockCategoryService.CategoryExists("groceries").Returns(true);

        var transactions = new List<Transaction>
        {
            new() {
                TransactionDate = new DateTime(2025, 10, 15),
                Description = "TARGET STORE #1234",
                Amount = 45.00m
            }
        };

        // Act
        var result = _classifier.ClassifyTransactions(transactions);

        // Assert
        _mockCategoryService.Received(1).AddMatcherToCategory("groceries", matcherRequest);
        _mockCategoryService.DidNotReceive().AddCategory(Arg.Any<Category>());
    }

    [Fact]
    public void ClassifyTransactions_MultipleTransactions_ClassifiesAll()
    {
        // Arrange: Multiple categories
        var categories = new List<Category>
        {
            new() {
                Id = "groceries",
                Name = "Groceries",
                Matchers =
                [
                    new CategoryMatcher
                    {
                        Type = "Contains",
                        Parameters = new Dictionary<String, Object>
                        {
                            ["values"] = JsonSerializer.SerializeToElement(new[] { "WALMART" }),
                            ["caseSensitive"] = JsonSerializer.SerializeToElement(false)
                        }
                    }
                ]
            },
            new() {
                Id = "restaurants",
                Name = "Restaurants",
                Matchers =
                [
                    new CategoryMatcher
                    {
                        Type = "Contains",
                        Parameters = new Dictionary<String, Object>
                        {
                            ["values"] = JsonSerializer.SerializeToElement(new[] { "MCDONALD" }),
                            ["caseSensitive"] = JsonSerializer.SerializeToElement(false)
                        }
                    }
                ]
            }
        };

        _mockCategoryService.GetAllCategories().Returns(categories);

        var transactions = new List<Transaction>
        {
            new() { TransactionDate = DateTime.Now, Description = "WALMART #1234", Amount = 50m },
            new() { TransactionDate = DateTime.Now, Description = "MCDONALD'S #456", Amount = 12m },
            new() { TransactionDate = DateTime.Now, Description = "WALMART SUPERCENTER", Amount = 75m }
        };

        // Act
        var result = _classifier.ClassifyTransactions(transactions);

        // Assert
        Assert.Equal(3, result.ClassifiedTransactions.Count);
        Assert.Equal(2, result.ClassifiedTransactions.Count(ct => ct.CategoryId == "groceries"));
        Assert.Single(result.ClassifiedTransactions, ct => ct.CategoryId == "restaurants");
    }

    [Fact]
    public void ClassifyTransactions_FirstMatcherWins_StopsCheckingOtherMatchers()
    {
        // Arrange: Category with multiple matchers
        var category = new Category
        {
            Id = "stores",
            Name = "Stores",
            Matchers =
            [
                new CategoryMatcher
                {
                    Type = "Contains",
                    Parameters = new Dictionary<String, Object>
                    {
                        ["values"] = JsonSerializer.SerializeToElement(new[] { "WALMART" }),
                        ["caseSensitive"] = JsonSerializer.SerializeToElement(false)
                    }
                },
                new CategoryMatcher
                {
                    Type = "Contains",
                    Parameters = new Dictionary<String, Object>
                    {
                        ["values"] = JsonSerializer.SerializeToElement(new[] { "SUPER" }), // Would also match
                        ["caseSensitive"] = JsonSerializer.SerializeToElement(false)
                    }
                }
            ]
        };

        _mockCategoryService.GetAllCategories().Returns([category]);

        var transactions = new List<Transaction>
        {
            new() {
                TransactionDate = DateTime.Now,
                Description = "WALMART SUPERCENTER", // Matches both matchers
                Amount = 50m
            }
        };

        // Act
        var result = _classifier.ClassifyTransactions(transactions);

        // Assert: Should be classified to "stores" category (first match wins)
        Assert.Single(result.ClassifiedTransactions);
        Assert.Equal("stores", result.ClassifiedTransactions[0].CategoryId);
    }

    [Fact]
    public void ClassifyTransactions_CategoryIdNormalized_UsesNormalizedId()
    {
        // Arrange
        _mockCategoryService.GetAllCategories().Returns([]);

        var selectionResult = new CategorySelectionResult(
            CategoryId: "Coffee Shops & Cafes!", // Non-normalized ID
            CategoryName: "Coffee Shops & Cafes",
            MatcherRequest: null,
            RequestedExit: false
        );

        _mockConsoleInteraction.PromptForCategory(Arg.Any<TransactionContext>())
            .Returns(selectionResult);

        _mockCategoryService.CategoryExists("coffee-shops-cafes").Returns(true); // Normalized version

        var transactions = new List<Transaction>
        {
            new() { TransactionDate = DateTime.Now, Description = "STARBUCKS", Amount = 5m }
        };

        // Act
        var result = _classifier.ClassifyTransactions(transactions);

        // Assert: Should use normalized ID
        Assert.Equal("coffee-shops-cafes", result.ClassifiedTransactions[0].CategoryId);
    }

    [Fact]
    public void ClassifyTransactions_ContextPassedCorrectly_ContainsProgressInfo()
    {
        // Arrange
        _mockCategoryService.GetAllCategories().Returns([]);

        var capturedContexts = new List<TransactionContext>();
        _mockConsoleInteraction.PromptForCategory(Arg.Do<TransactionContext>(capturedContexts.Add))
            .Returns(new CategorySelectionResult("test", "Test", null, null, false));

        _mockCategoryService.CategoryExists(Arg.Any<String>()).Returns(true);

        var transactions = new List<Transaction>
        {
            new() { TransactionDate = DateTime.Now, Description = "TEST", Amount = 10m },
            new() { TransactionDate = DateTime.Now, Description = "TEST2", Amount = 20m }
        };

        // Act
        _classifier.ClassifyTransactions(transactions);

        // Assert: Check contexts for both transactions
        Assert.Equal(2, capturedContexts.Count);

        // First transaction
        Assert.Equal(1, capturedContexts[0].CurrentIndex);
        Assert.Equal(2, capturedContexts[0].TotalCount);
        Assert.Equal(transactions[0], capturedContexts[0].Transaction);

        // Second transaction
        Assert.Equal(2, capturedContexts[1].CurrentIndex);
        Assert.Equal(2, capturedContexts[1].TotalCount);
        Assert.Equal(transactions[1], capturedContexts[1].Transaction);
    }
}
