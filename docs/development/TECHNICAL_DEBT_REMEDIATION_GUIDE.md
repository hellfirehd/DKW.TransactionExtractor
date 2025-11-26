# Technical Debt - Recommended Code Changes

This document provides specific code examples for addressing the technical debt identified in `TECHNICAL_DEBT_ANALYSIS.md`.

---

## Issue #1: Fix Filename Typo

### Current State
File: `src/DKW.TransactionExtractor/Providers/CTFS/CtfsMastercardPdfTextExtractor.cs`
- Filename: `CtfsMastercardPdfTextExtractor.cs` ? (correctly fixed)
- Class name: `CtfsMastercardPdfTextExtractor` ?

### Status
? **COMPLETED** - File has been renamed to the correct spelling

**Note**: This was previously named `CtfsMastercarrdPdfTextExtractor.cs` (with double 'r') but has since been corrected.

**Impact**: None - only a filename fix with no code changes required

---

## Issue #2: Create Matcher Type Constants

### Current Problem
Magic strings scattered throughout codebase:
- `MatcherBuilderService.cs`: switch cases with "ExactMatch", "Contains", "Regex"
- `MatcherFactory.cs`: switch cases with same strings
- `MatcherCreationRequest.cs`: factory methods with hardcoded strings
- Test files: hardcoded matcher type strings

### Recommended Solution

**File**: `src/DKW.TransactionExtractor/Classification/MatcherTypeConstants.cs` (new file)

```csharp
namespace DKW.TransactionExtractor.Classification;

/// <summary>
/// Constants for all supported matcher types.
/// Provides a single source of truth for matcher type identifiers.
/// </summary>
public static class MatcherTypeConstants
{
    /// <summary>
    /// Exact string match (case sensitivity configurable).
    /// </summary>
    public const String ExactMatch = "ExactMatch";

    /// <summary>
    /// Contains substring match (case sensitivity configurable).
    /// </summary>
    public const String Contains = "Contains";

    /// <summary>
    /// Regular expression pattern match.
    /// </summary>
    public const String Regex = "Regex";

    /// <summary>
    /// Returns all supported matcher types.
    /// </summary>
    public static IReadOnlyList<String> All => new[] { ExactMatch, Contains, Regex };

    /// <summary>
    /// Validates if the given type is a supported matcher type.
    /// </summary>
    public static Boolean IsValid(String? matcherType)
    {
        return !String.IsNullOrWhiteSpace(matcherType) 
            && All.Contains(matcherType);
    }
}
```

### Update MatcherBuilderService.cs

```csharp
public MatcherCreationRequest? BuildMatcher(String transactionDescription)
{
    Console.WriteLine();
    Console.WriteLine("Select matcher type:");
    Console.WriteLine("  0. Cancel");
    Console.WriteLine($"  1. {MatcherTypeConstants.ExactMatch} - Match this exact description (fastest, recommended) [default]");
    Console.WriteLine($"  2. {MatcherTypeConstants.Contains} - Match descriptions containing a substring");
    Console.WriteLine($"  3. {MatcherTypeConstants.Regex} - Match using a regular expression pattern");
    Console.WriteLine();
    Console.Write("Your choice [1-3, default=1]: ");

    var choice = Console.ReadLine()?.Trim();

    if (String.IsNullOrEmpty(choice))
    {
        choice = "1";
    }

    return choice switch
    {
        "1" => BuildExactMatcher(transactionDescription),
        "2" => BuildContainsMatcher(transactionDescription),
        "3" => BuildRegexMatcher(transactionDescription),
        "0" => null,
        _ => null
    };
}
```

### Update MatcherCreationRequest.cs

```csharp
/// <summary>
/// Creates an ExactMatch request with the specified values and case sensitivity.
/// </summary>
public static MatcherCreationRequest ExactMatch(String[] values, Boolean caseSensitive = false)
{
    return new MatcherCreationRequest(
        MatcherTypeConstants.ExactMatch,  // Use constant
        new Dictionary<String, Object>
        {
            { "values", values },
            { "caseSensitive", caseSensitive }
        }
    );
}

/// <summary>
/// Creates a Contains request with the specified values and case sensitivity.
/// </summary>
public static MatcherCreationRequest Contains(String[] values, Boolean caseSensitive = false)
{
    return new MatcherCreationRequest(
        MatcherTypeConstants.Contains,  // Use constant
        new Dictionary<String, Object>
        {
            { "values", values },
            { "caseSensitive", caseSensitive }
        }
    );
}

/// <summary>
/// Creates a Regex request with the specified pattern and case sensitivity.
/// </summary>
public static MatcherCreationRequest Regex(String pattern, Boolean ignoreCase = true)
{
    return new MatcherCreationRequest(
        MatcherTypeConstants.Regex,  // Use constant
        new Dictionary<String, Object>
        {
            { "pattern", pattern },
            { "ignoreCase", ignoreCase }
        }
    );
}
```

### Update MatcherFactory.cs

```csharp
public class MatcherFactory
{
    public ITransactionMatcher? CreateMatcher(CategoryMatcher matcherConfig)
    {
        if (!MatcherTypeConstants.IsValid(matcherConfig.Type))
        {
            return null;
        }

        return matcherConfig.Type switch
        {
            MatcherTypeConstants.ExactMatch => CreateExactMatcher(matcherConfig.Parameters),
            MatcherTypeConstants.Contains => CreateContainsMatcher(matcherConfig.Parameters),
            MatcherTypeConstants.Regex => CreateRegexMatcher(matcherConfig.Parameters),
            _ => null
        };
    }
    
    // ... rest of class remains the same
}
```

**Benefits**:
- ? Single source of truth for matcher types
- ? Type-safe access to constants
- ? Validation method prevents typos
- ? Easy to add new matcher types
- ? Better refactoring support

---

## Issue #3: Type-Safe Matcher Configuration

### Current Problem
Using `Dictionary<string, object>` with unsafe casting:

```csharp
// Current approach - unsafe
var valuesElement = (JsonElement)parameters["values"];  // Can throw KeyNotFoundException
var caseSensitive = ((JsonElement)parameters["caseSensitive"]).GetBoolean();  // Can throw InvalidOperationException
```

### Recommended Solution

**File**: `src/DKW.TransactionExtractor/Classification/MatcherConfigurationModels.cs` (new file)

```csharp
using System.Text.Json.Serialization;

namespace DKW.TransactionExtractor.Classification;

/// <summary>
/// Type-safe configuration models for matcher parameters.
/// These models enable compile-time validation and JSON source generation.
/// </summary>

/// <summary>
/// Configuration for ExactMatch matcher.
/// </summary>
public record ExactMatchConfig(
    [property: JsonPropertyName("values")] String[] Values,
    [property: JsonPropertyName("caseSensitive")] Boolean CaseSensitive = false
);

/// <summary>
/// Configuration for Contains matcher.
/// </summary>
public record ContainsMatchConfig(
    [property: JsonPropertyName("values")] String[] Values,
    [property: JsonPropertyName("caseSensitive")] Boolean CaseSensitive = false
);

/// <summary>
/// Configuration for Regex matcher.
/// </summary>
public record RegexMatchConfig(
    [property: JsonPropertyName("pattern")] String Pattern,
    [property: JsonPropertyName("ignoreCase")] Boolean IgnoreCase = true
);

/// <summary>
/// Union type for all matcher configurations.
/// </summary>
public record MatcherConfiguration(
    [property: JsonPropertyName("type")] String Type,
    [property: JsonPropertyName("parameters")] Dictionary<String, Object> Parameters
);
```

### Update MatcherFactory.cs to Use Type-Safe Models

```csharp
public class MatcherFactory
{
    public ITransactionMatcher? CreateMatcher(CategoryMatcher matcherConfig)
    {
        if (!MatcherTypeConstants.IsValid(matcherConfig.Type))
        {
            throw new InvalidOperationException($"Unknown matcher type: {matcherConfig.Type}");
        }

        return matcherConfig.Type switch
        {
            MatcherTypeConstants.ExactMatch => CreateExactMatcher(ParseExactMatchConfig(matcherConfig.Parameters)),
            MatcherTypeConstants.Contains => CreateContainsMatcher(ParseContainsMatchConfig(matcherConfig.Parameters)),
            MatcherTypeConstants.Regex => CreateRegexMatcher(ParseRegexMatchConfig(matcherConfig.Parameters)),
            _ => null
        };
    }

    private ITransactionMatcher CreateExactMatcher(ExactMatchConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);
        ArgumentNullException.ThrowIfNull(config.Values);
        
        return new ExactMatcher(config.Values, config.CaseSensitive);
    }

    private ITransactionMatcher CreateContainsMatcher(ContainsMatchConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);
        ArgumentNullException.ThrowIfNull(config.Values);
        
        return new ContainsMatcher(config.Values, config.CaseSensitive);
    }

    private ITransactionMatcher CreateRegexMatcher(RegexMatchConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);
        ArgumentNullException.ThrowIfNull(config.Pattern);
        
        return new RegexMatcher(config.Pattern, config.IgnoreCase);
    }

    private static ExactMatchConfig ParseExactMatchConfig(Dictionary<String, Object> parameters)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        if (!parameters.TryGetValue("values", out var valuesObj) || valuesObj is not JsonElement valuesElement)
        {
            throw new InvalidOperationException("ExactMatch matcher requires 'values' parameter");
        }

        var values = ParseStringArray(valuesElement);
        var caseSensitive = parameters.TryGetValue("caseSensitive", out var caseObj) 
            && caseObj is JsonElement caseElement 
            && caseElement.GetBoolean();

        return new ExactMatchConfig(values, caseSensitive);
    }

    private static ContainsMatchConfig ParseContainsMatchConfig(Dictionary<String, Object> parameters)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        String[] values;

        if (parameters.TryGetValue("values", out var valuesObj) && valuesObj is JsonElement valuesElement)
        {
            values = ParseStringArray(valuesElement);
        }
        else if (parameters.TryGetValue("value", out var valueObj) && valueObj is JsonElement valueElement)
        {
            // Legacy format support
            values = new[] { valueElement.GetString() ?? String.Empty };
        }
        else
        {
            throw new InvalidOperationException("Contains matcher requires either 'value' or 'values' parameter");
        }

        var caseSensitive = parameters.TryGetValue("caseSensitive", out var caseObj)
            && caseObj is JsonElement caseElement
            && caseElement.GetBoolean();

        return new ContainsMatchConfig(values, caseSensitive);
    }

    private static RegexMatchConfig ParseRegexMatchConfig(Dictionary<String, Object> parameters)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        if (!parameters.TryGetValue("pattern", out var patternObj) || patternObj is not JsonElement patternElement)
        {
            throw new InvalidOperationException("Regex matcher requires 'pattern' parameter");
        }

        var pattern = patternElement.GetString();
        if (String.IsNullOrWhiteSpace(pattern))
        {
            throw new InvalidOperationException("Regex pattern cannot be empty");
        }

        var ignoreCase = parameters.TryGetValue("ignoreCase", out var ignoreObj)
            && ignoreObj is JsonElement ignoreElement
            && ignoreElement.GetBoolean();

        return new RegexMatchConfig(pattern, ignoreCase);
    }

    private static String[] ParseStringArray(JsonElement arrayElement)
    {
        try
        {
            return arrayElement.EnumerateArray()
                .Select(e => e.GetString() ?? String.Empty)
                .ToArray();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Invalid string array in matcher parameters", ex);
        }
    }
}
```

**Benefits**:
- ? Type-safe configuration objects
- ? Clear error messages with validation
- ? Null argument checks prevent crashes
- ? Can be extended with JSON source generation
- ? Backward compatible with existing JSON format
- ? Easier to test with known types

---

## Issue #4: Add Magic String Validation Test

**File**: `src/DKW.TransactionExtractor.Tests/MatcherTypeConstantsTests.cs` (new file)

```csharp
using DKW.TransactionExtractor.Classification;
using Xunit;

namespace DKW.TransactionExtractor.Tests;

public class MatcherTypeConstantsTests
{
    [Fact]
    public void MatcherTypeConstants_HasExpectedTypes()
    {
        Assert.NotNull(MatcherTypeConstants.ExactMatch);
        Assert.NotNull(MatcherTypeConstants.Contains);
        Assert.NotNull(MatcherTypeConstants.Regex);
    }

    [Fact]
    public void MatcherTypeConstants_AllContainsAllTypes()
    {
        Assert.Contains(MatcherTypeConstants.ExactMatch, MatcherTypeConstants.All);
        Assert.Contains(MatcherTypeConstants.Contains, MatcherTypeConstants.All);
        Assert.Contains(MatcherTypeConstants.Regex, MatcherTypeConstants.All);
    }

    [Theory]
    [InlineData("ExactMatch", true)]
    [InlineData("Contains", true)]
    [InlineData("Regex", true)]
    [InlineData("Unknown", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsValid_WithVariousInputs_ReturnsExpected(String? matcherType, Boolean expected)
    {
        Assert.Equal(expected, MatcherTypeConstants.IsValid(matcherType));
    }
}
```

---

## Issue #5: Extract Date Parsing Logic

### Current Problem
Date parsing logic is embedded in `CtfsMastercardTransactionParser` (lines 235-260)

### Recommended Solution

**File**: `src/DKW.TransactionExtractor/Providers/CTFS/TransactionDateParser.cs` (new file)

```csharp
using System.Globalization;

namespace DKW.TransactionExtractor.Providers.CTFS;

/// <summary>
/// Handles parsing of transaction dates from credit card statement format.
/// Responsible for converting month-day strings to DateTime objects with proper year handling.
/// </summary>
public static class TransactionDateParser
{
    /// <summary>
    /// Attempts to parse a month-day string to a DateTime in the specified year.
    /// If the date is invalid for the given year (e.g., Feb 29 in non-leap year),
    /// automatically tries the previous year.
    /// </summary>
    /// <param name="monthDay">Month-day string (e.g., "Oct 15", "Feb 29")</param>
    /// <param name="year">Target year</param>
    /// <param name="result">Parsed DateTime if successful</param>
    /// <returns>True if parse succeeded, false otherwise</returns>
    /// <remarks>
    /// This method handles leap year edge cases by attempting fallback to the previous year
    /// when a date is invalid in the specified year. This is useful for transactions dated
    /// Feb 29 when parsed in a non-leap year.
    /// </remarks>
    public static Boolean TryParseMonthDay(String monthDay, Int32 year, out DateTime result)
    {
        result = DateTime.MinValue;
        if (String.IsNullOrWhiteSpace(monthDay))
            return false;

        var tokens = monthDay.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length != 2)
            return false;

        var monthPart = LookupMonth(tokens[0]);
        if (monthPart == 0)
            return false;

        if (!Int32.TryParse(tokens[1], out var dayPart))
            return false;

        // Try current year first
        if (TryCreateDate(year, monthPart, dayPart, out result))
            return true;

        // Fallback to previous year (for leap years and year boundary transactions)
        return TryCreateDate(year - 1, monthPart, dayPart, out result);
    }

    /// <summary>
    /// Attempts to create a DateTime from year, month, and day components.
    /// </summary>
    /// <param name="year">Year</param>
    /// <param name="month">Month (1-12)</param>
    /// <param name="day">Day of month</param>
    /// <param name="result">Created DateTime if successful</param>
    /// <returns>True if DateTime creation succeeded, false if date is invalid</returns>
    private static Boolean TryCreateDate(Int32 year, Int32 month, Int32 day, out DateTime result)
    {
        try
        {
            result = new DateTime(year, month, day);
            return true;
        }
        catch (ArgumentOutOfRangeException)
        {
            result = DateTime.MinValue;
            return false;
        }
    }

    /// <summary>
    /// Converts a three-letter month abbreviation to its numeric month value.
    /// </summary>
    /// <param name="monthName">Month name (e.g., "Jan", "Feb", "October")</param>
    /// <returns>Month number (1-12) or 0 if invalid</returns>
    private static Int32 LookupMonth(String monthName)
    {
        if (String.IsNullOrWhiteSpace(monthName))
            return 0;

        var token = monthName.Trim();
        if (token.Length >= 3)
            token = token[..3];

        return token.ToLowerInvariant() switch
        {
            "jan" => 1,
            "feb" => 2,
            "mar" => 3,
            "apr" => 4,
            "may" => 5,
            "jun" => 6,
            "jul" => 7,
            "aug" => 8,
            "sep" => 9,
            "oct" => 10,
            "nov" => 11,
            "dec" => 12,
            _ => 0,
        };
    }
}
```

### Update CtfsMastercardTransactionParser.cs

```csharp
// Remove TryParseMonthDay, TryCreateDate, LookupMonth methods
// Update TryParseTransactionFromCombined to use extracted class:

if (!TransactionDateParser.TryParseMonthDay(date1, statementDate.Year, out var transDate))
    return false;

var postedDate = DateTime.MinValue;
TransactionDateParser.TryParseMonthDay(date2, statementDate.Year, out postedDate);
```

**Benefits**:
- ? Single responsibility for date parsing
- ? Easier to test date logic in isolation
- ? Can be reused in other parsers
- ? Reduces parser complexity
- ? Clearer separation of concerns

---

## Summary of Changes

| Issue | Solution | Effort | Impact |
|-------|----------|--------|--------|
| Filename typo | Rename file | 1 hour | Low impact, high visibility |
| Magic strings | Create constants | 2 hours | Medium impact, high ROI |
| Type safety | Create config models | 4 hours | High impact, improves robustness |
| Date parsing | Extract to separate class | 2 hours | Medium impact, improves testability |

**Total Estimated Effort**: 8-10 hours

These changes maintain backward compatibility while improving:
- Code clarity and discoverability
- Type safety at compile time
- Test coverage and isolatability
- Future extensibility

---

**Document Version**: 1.0  
**Last Updated**: 2025-01-26
