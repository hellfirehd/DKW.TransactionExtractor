# Transaction Exclusion Feature - Implementation Summary

## Overview
Implemented an extensible and configurable system to exclude certain transactions (like INTEREST CHARGES, fees, etc.) from the purchases total calculation in credit card statement parsing.

## Architecture

### 1. **ITransactionFilter Interface** (`ITransactionFilter.cs`)
```csharp
public interface ITransactionFilter
{
    bool ShouldExcludeFromPurchasesTotal(Transaction transaction);
}
```
- Provides extensibility point for custom filtering logic
- Any custom filter implementation can be registered via DI

### 2. **DefaultTransactionFilter** (`DefaultTransactionFilter.cs`)
- Default implementation using regex pattern matching
- Patterns are case-insensitive and compiled for performance
- Loads exclusion patterns from `ParserOptions` configuration
- Logs excluded transactions at Debug level for diagnostics
- Only excludes positive-amount transactions (negatives are already excluded from purchases)

### 3. **Configuration** (`ParserOptions.cs`)
```csharp
public class ParserOptions
{
    public decimal? DifferenceTolerance { get; set; }
    public List<string> ExclusionPatterns { get; set; } = [];
}
```

### 4. **appsettings.json Configuration**
```json
{
  "ParserOptions": {
    "DifferenceTolerance": 0.01,
    "ExclusionPatterns": [
      "INTEREST CHARGES",
      "ANNUAL FEE",
      "OVER LIMIT FEE",
      "FOREIGN EXCHANGE FEE"
    ]
  }
}
```

## How It Works

1. **Parser Initialization**: `CtfsMastercardTransactionParser` receives `ITransactionFilter` via dependency injection

2. **Transaction Parsing**: All transactions are parsed normally (no filtering during parsing)

3. **Purchases Calculation**: 
   ```csharp
   var includedTransactions = transactions
       .Where(t => t.Amount > 0 && !_transactionFilter.ShouldExcludeFromPurchasesTotal(t))
       .ToList();
   
   result.ComputedPurchasesTotal = includedTransactions.Sum(t => t.Amount);
   ```

4. **Logging**: Parser logs how many transactions were excluded for transparency

## Extensibility

### Custom Filter Implementation
Create your own filter by implementing `ITransactionFilter`:

```csharp
public class CustomTransactionFilter : ITransactionFilter
{
    public bool ShouldExcludeFromPurchasesTotal(Transaction transaction)
    {
        // Custom logic here
        return transaction.Amount > 1000m; // Example: exclude large transactions
    }
}

// Register in Program.cs:
services.AddSingleton<ITransactionFilter, CustomTransactionFilter>();
```

### Adding New Patterns
Simply add new regex patterns to `appsettings.json`:

```json
"ExclusionPatterns": [
  "INTEREST CHARGES",
  "ANNUAL FEE",
  "^SERVICE FEE.*",       // Regex: matches "SERVICE FEE" at start
  ".*PENALTY.*"            // Regex: matches any description containing "PENALTY"
]
```

## Testing

Comprehensive test suite in `TransactionExclusionTests.cs`:
- ? Single exclusion pattern (INTEREST CHARGES)
- ? Multiple exclusion patterns
- ? No exclusion patterns (all included)
- ? Regex partial matching
- ? Negative amounts not affected

## Why This Approach?

1. **Correct Business Logic**: Credit card statements' "Purchases" total already excludes fees and interest charges. Our parser now matches this behavior.

2. **Extensible**: New filter implementations can be created without modifying parser code

3. **Configurable**: Non-developers can add/remove exclusion patterns via configuration

4. **Testable**: Filter logic is isolated and easily unit tested

5. **Observable**: Logging provides visibility into which transactions are excluded

## Performance Considerations

- Regex patterns are compiled once at startup
- Patterns are checked only for positive-amount transactions
- No impact on transaction parsing itself (filtering happens after)

## Backward Compatibility

- Default behavior (no exclusion patterns configured) maintains existing functionality
- Existing tests continue to pass
- Parser constructor supports both test scenarios and DI scenarios
