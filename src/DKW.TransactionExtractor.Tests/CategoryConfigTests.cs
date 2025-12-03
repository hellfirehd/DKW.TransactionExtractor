using DKW.TransactionExtractor.Classification;
using DKW.TransactionExtractor.Models;
using System.Text.Json;
using Xunit;

namespace DKW.TransactionExtractor.Tests;

public class CategoryConfigTests
{
    [Fact]
    public void Can_Serialize_and_Deserialize()
    {
        // Arrange
        var cc = new CategoryConfig
        {
            Categories =
            [
                new Category
                {
                    Id = "sample-category",
                    Name = "Sample Category",
                    Matchers =
                    [
                        new CategoryMatcher
                        {
                            Type = "Contains",
                            Parameters = [
                                new MatcherValue("Alpha", 9.99m ),
                                new MatcherValue("Beta", null)
                            ]
                        }
                    ]
                }
            ]
        };

        // Act
        var serialized = JsonSerializer.Serialize(cc, SerializationHelper.JSO);
        var deserialized = JsonSerializer.Deserialize<CategoryConfig>(serialized, SerializationHelper.JSO);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Single(deserialized.Categories);
        var category = deserialized.Categories[0];
        Assert.Equal("sample-category", category.Id);
        Assert.Equal("Sample Category", category.Name);
        Assert.Single(category.Matchers);
        var matcher = category.Matchers[0];
        Assert.Equal("Contains", matcher.Type);
        Assert.Equal(2, matcher.Parameters.Count);
    }
}
