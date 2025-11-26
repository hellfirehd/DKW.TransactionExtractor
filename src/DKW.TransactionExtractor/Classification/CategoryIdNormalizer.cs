using System.Text;
using System.Text.RegularExpressions;

namespace DKW.TransactionExtractor.Classification;

/// <summary>
/// Provides normalization for category IDs to ensure consistency across the system.
/// </summary>
public static partial class CategoryIdNormalizer
{
    /// <summary>
    /// Normalizes a category ID by:
    /// 1. Converting to lowercase
    /// 2. Replacing non-alphanumeric characters with hyphens
    /// 3. Collapsing multiple consecutive hyphens to a single hyphen
    /// 4. Trimming leading and trailing hyphens
    /// </summary>
    /// <param name="categoryId">The category ID to normalize</param>
    /// <returns>Normalized category ID</returns>
    public static String Normalize(String categoryId)
    {
        if (String.IsNullOrWhiteSpace(categoryId))
        {
            return String.Empty;
        }

        // Convert to lowercase
        var normalized = categoryId.ToLowerInvariant();

        // Replace non-alphanumeric characters with hyphens
        normalized = NonAlphanumericPattern().Replace(normalized, "-");

        // Collapse multiple consecutive hyphens to a single hyphen
        normalized = MultipleHyphensPattern().Replace(normalized, "-");

        // Trim leading and trailing hyphens
        normalized = normalized.Trim('-');

        return normalized;
    }

    [GeneratedRegex(@"[^a-z0-9]+")]
    private static partial Regex NonAlphanumericPattern();

    [GeneratedRegex(@"-{2,}")]
    private static partial Regex MultipleHyphensPattern();
}
