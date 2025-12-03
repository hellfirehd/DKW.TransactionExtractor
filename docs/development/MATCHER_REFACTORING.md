# Matcher System Refactoring

**Date**: 2025-11-27  
**Status**: Complete  
**Impact**: Breaking changes to matcher configuration format

## Overview

The matcher system has been significantly refactored to support amount-specific matching and improve code maintainability. This refactoring introduces the `MatcherValue` record type and consolidates matcher logic into a shared base class.

## Key Changes

### 1. Introduction of `MatcherValue` Record

**New Model** (`Classification/MatcherValue.cs`):
```csharp
public record MatcherValue(String Value, Decimal? Amount);
```

This record type encapsulates both a pattern value (string) and an optional amount constraint. When an amount is specified, the matcher will only match transactions where:
- The pattern matches the description (exact, contains, or regex)
- **AND** the transaction amount equals the specified amount (rounded to 2 decimal places)

### 2. Shared Base Class: `TransactionMatcherBase`

**New Abstract Class** (`Classification/TransactionMatcherBase.cs`):
- Provides common validation for transactions and descriptions
- Implements `ITransactionMatcher.TryMatch()` with null/whitespace checks
- Defines abstract `TryMatchCore()` method for concrete implementations
- Includes `AmountsEqual()` helper for amount comparison logic

**Benefits**:
- &#x2705; Eliminates code duplication across matcher types
- &#x2705; Centralizes description validation
- &#x2705; Standardizes amount comparison logic (2 decimal place rounding)
- &#x2705; Simplifies concrete matcher implementations

### 3. Updated Matcher Implementations

All three matcher types now inherit from `TransactionMatcherBase`:

#### ExactMatcher
- Accepts `IEnumerable&lt;MatcherValue&gt;` in constructor
- Matches exact description equality (case-insensitive)
- Optionally filters by amount when `MatcherValue.Amount` is not null

#### ContainsMatcher
- Accepts `IEnumerable&lt;MatcherValue&gt;` in constructor
- Matches substring containment (case-insensitive)
- Optionally filters by amount when `MatcherValue.Amount` is not null

#### RegexMatcher
- Accepts `IEnumerable&lt;MatcherValue&gt;` in constructor
- Creates compiled regex patterns with `IgnoreCase` flag
- Optionally filters by amount when `MatcherValue.Amount` is not null
- Includes pattern validation with `RegexParseException` handling

### 4. Configuration Format Changes

#### Old Format (Removed)
```json
{
  "type": "ExactMatch",
  "parameters": {
    "values": ["WALMART", "TARGET"],
    "caseSensitive": false
  }
}
```

#### New Format
```json
{
  "type": "ExactMatch",
  "parameters": [
    { "value": "WALMART" },
    { "value": "TARGET" }
  ]
}
```

#### With Amount Constraints
```json
{
  "type": "ExactMatch",
  "parameters": [
    { "value": "APPLE.COM/BILL 866-712-7753 ON", "amount": 32.42 },
    { "value": "STARBUCKS" }
  ]
}
```

### 5. Case Sensitivity Enforcement

**Breaking Change**: All matchers are now **forced to be case-insensitive**.

- `caseSensitive` parameter has been **removed** from all matcher configurations
- `ExactMatcher` uses `StringComparison.OrdinalIgnoreCase`
- `ContainsMatcher` uses `StringComparison.OrdinalIgnoreCase`
- `RegexMatcher` uses `RegexOptions.IgnoreCase`

**Rationale**: Transaction descriptions are inconsistent in casing across statements. Case-insensitive matching is more practical and user-friendly.

### 6. CategoryMatcher Model Update

**Updated Model** (`Models/CategoryMatcher.cs`):
```csharp
public class CategoryMatcher
{
    public String Type { get; set; } = String.Empty;
    public HashSet&lt;MatcherValue&gt; Parameters { get; set; } = [];
}
```

- Changed `Parameters` from `Dictionary&lt;string, JsonElement&gt;` to `HashSet&lt;MatcherValue&gt;`
- Provides type-safe access to matcher parameters
- Supports automatic deduplication via `HashSet`

### 7. MatcherFactory Updates

**Updated Factory** (`Classification/MatcherFactory.cs`):
```csharp
public static ITransactionMatcher? CreateMatcher(CategoryMatcher? matcherConfig)
{
    if (matcherConfig == null)
    {
        return null;
    }

    try
    {
        return matcherConfig.Type.ToLowerInvariant() switch
        {
            "exactmatch" => new ExactMatcher(matcherConfig.Parameters),
            "contains" => new ContainsMatcher(matcherConfig.Parameters),
            "regex" => new RegexMatcher(matcherConfig.Parameters),
            _ => null
        };
    }
    catch (Exception)
    {
        return null;
    }
}
```

- Simplified parameter extraction (direct access to `Parameters`)
- Returns `null` on any exception (invalid patterns, empty parameters, etc.)
- Case-insensitive matcher type names

### 8. Smart Merging Updates

**CategoryRepository** merging logic updated:

- `ExactMatch` and `Contains`: Values are merged into existing matchers using `HashSet&lt;MatcherValue&gt;`
- Deduplication considers both `value` and `amount` (case-insensitive value comparison)
- `Regex`: Always creates new matcher instances (no merging)

## Migration Guide

### Step 1: Update Configuration Files

Convert existing `categories.json` to the new format:

**Before:**
```json
{
  "type": "ExactMatch",
  "parameters": {
    "values": ["WALMART", "TARGET"],
    "caseSensitive": false
  }
}
```

**After:**
```json
{
  "type": "ExactMatch",
  "parameters": [
    { "value": "WALMART" },
    { "value": "TARGET" }
  ]
}
```

### Step 2: Add Amount Constraints (Optional)

For transactions that need amount-specific matching:

```json
{
  "type": "ExactMatch",
  "parameters": [
    { "value": "APPLE.COM/BILL", "amount": 14.55 },
    { "value": "APPLE.COM/BILL", "amount": 32.42 }
  ]
}
```

### Step 3: Remove Case Sensitivity Parameters

Delete any `caseSensitive` parameters from matcher configurations. All matching is now case-insensitive by default.

## Use Cases for Amount Constraints

### 1. Recurring Subscriptions
Match specific subscription tiers by amount:
```json
{
  "type": "ExactMatch",
  "parameters": [
    { "value": "APPLE.COM/BILL", "amount": 14.55 },  // Apple Music
    { "value": "APPLE.COM/BILL", "amount": 32.42 }   // iCloud Storage
  ]
}
```

### 2. Distinguishing Similar Merchants
Categorize purchases from the same merchant differently based on amount:
```json
{
  "type": "Contains",
  "parameters": [
    { "value": "AMAZON", "amount": 16.99 },  // Amazon Prime (subscription)
    { "value": "AMAZON" }                     // All other Amazon purchases
  ]
}
```

### 3. Refund Tracking
Match specific refund amounts:
```json
{
  "type": "Contains",
  "parameters": [
    { "value": "REFUND", "amount": -50.00 }  // Track specific refund amount
  ]
}
```

## Testing

### New Test Files
- `AmountMatcherTests.cs`: Tests amount-based matching for all matcher types
- `MatcherFactoryTests.cs`: Comprehensive factory tests including amount parameters
- `CategoryConfigTests.cs`: Serialization/deserialization tests for new format

### Test Coverage
- &#x2705; ExactMatcher with/without amount constraints
- &#x2705; ContainsMatcher with/without amount constraints
- &#x2705; RegexMatcher with/without amount constraints
- &#x2705; MatcherFactory creation from JSON
- &#x2705; CategoryConfig serialization roundtrip
- &#x2705; Amount comparison logic (2 decimal place rounding)
- &#x2705; Case-insensitive enforcement
- &#x2705; Invalid pattern handling

## Breaking Changes Summary

1. **Configuration Format**: `parameters` is now an array of objects instead of a dictionary
2. **Case Sensitivity**: All matchers are now case-insensitive (no override option)
3. **Amount Support**: All matcher types now support optional amount constraints
4. **Backwards Compatibility**: Old configuration format is **not** supported

## Benefits

### For Users
- &#x2705; More precise categorization with amount constraints
- &#x2705; Simpler configuration (no case sensitivity decisions)
- &#x2705; Consistent behavior across matcher types

### For Developers
- &#x2705; Reduced code duplication
- &#x2705; Type-safe matcher parameters
- &#x2705; Easier to add new matcher types
- &#x2705; Centralized validation logic
- &#x2705; Better testability

## Related Files

### Modified
- `Classification/ExactMatcher.cs`
- `Classification/ContainsMatcher.cs`
- `Classification/RegexMatcher.cs`
- `Classification/MatcherFactory.cs`
- `Classification/CategoryRepository.cs`
- `Models/CategoryMatcher.cs`

### Added
- `Classification/MatcherValue.cs`
- `Classification/TransactionMatcherBase.cs`
- `Tests/AmountMatcherTests.cs`
- `Tests/MatcherFactoryTests.cs`
- `Tests/CategoryConfigTests.cs`

## Future Enhancements

### Potential Extensions
- Date-based matching (e.g., recurring charges on specific days)
- Amount range matching (e.g., `amountMin`/`amountMax`)
- Multiple field matching (e.g., description + date + amount)
- Transaction type filtering (debit vs. credit)

### Architecture Improvements
- Consider adding a `MatcherOptions` class for future extensibility
- Explore fluent builder API for matcher creation
- Add matcher validation during configuration load

## References

- [Transaction Classification Guide](../CLASSIFICATION_GUIDE.md)
- [GitHub Copilot Instructions](../../.github/copilot-instructions.md)
- [CHANGELOG.md](../../CHANGELOG.md)

---

**Last Updated**: 2025-11-27  
**Authors**: GitHub Copilot, Project Team
