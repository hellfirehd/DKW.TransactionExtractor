# Transaction Classification System

## Overview
The transaction classification system categorizes transactions based on configurable matching rules. When a transaction cannot be automatically categorized, the system prompts the user to select an existing category or create a new one, with the option to add various types of matching rules interactively.

## Configuration

### appsettings.json
Add the following properties to the `AppOptions` section:

```json
"AppOptions": {
  "CategoryConfigPath": "categories.json",  // Path to category configuration file
  "OutputFormat": "Csv",                     // Output format: "Csv" or "Json"
  "OutputPath": "output"                     // Directory for classified transaction files
}
```

### categories.json
Define categories and their matching rules:

```json
{
  "categories": [
    {
      "id": "groceries",
      "name": "Groceries",
      "matchers": [
        {
          "type": "ExactMatch",
          "parameters": {
            "values": [
              { "value": "SAFEWAY" },
              { "value": "WHOLE FOODS" },
              { "value": "TRADER JOES" }
            ]
          }
        },
        {
          "type": "Contains",
          "parameters": {
            "values": [
              { "value": "GROCERY" },
              { "value": "SUPERMARKET" }
            ]
          }
        }
      ]
    }
  ]
}
```

## Matcher Types

### ExactMatch
Matches transaction descriptions exactly against a list of values. Multiple values are automatically merged into a single matcher.

**Parameters:**
- `values` (object[]): Array of objects, each representing an exact value to match
  - `value` (string): The value to match
  - `amount` (number, optional): Restricts the matcher to only match transactions with this amount (rounded to 2 decimal places)

**Merging Behavior:** When adding new values, they are merged into an existing ExactMatch matcher.

**Example:**
```json
{
  "type": "ExactMatch",
  "parameters": {
    "values": [
      { "value": "SAFEWAY" },
      { "value": "WHOLE FOODS" },
      { "value": "TRADER JOES" }
    ]
  }
}
```

### Contains
Matches if the transaction description contains any of the specified substrings. Multiple values are automatically merged into a single matcher.

**Parameters:**
- `values` (object[]): Array of objects, each representing a substring to search for
  - `value` (string): The substring to search for
  - `amount` (number, optional): Restricts the matcher to only match transactions with this amount (rounded to 2 decimal places)

**Merging Behavior:** When adding new substrings, they are merged into an existing Contains matcher.

**Example:**
```json
{
  "type": "Contains",
  "parameters": {
    "values": [
      { "value": "RESTAURANT", "amount": -50.00 },
      { "value": "BISTRO" },
      { "value": "CAFE", "amount": 10.99 }
    ]
  }
}
```

### Regex
Matches transaction descriptions using a regular expression pattern. Each regex pattern is always created as a separate matcher (no merging).

**Parameters:**
- `pattern` (string): Regular expression pattern
- `amount` (number, optional): Restricts the matcher to only match transactions with this amount (rounded to 2 decimal places)

**Merging Behavior:** Regex matchers are never merged; each pattern creates a new matcher instance.

**Example:**
```json
{
  "type": "Regex",
  "parameters": {
    "pattern": "^WALMART #\\d+",
    "amount": 50.00
  }
}
```

## Matching Process

1. The system tries each category in order
2. Within each category, matchers are evaluated in order
3. **First match wins** - classification stops at the first successful match
4. If no match is found, the user is prompted

## User Interaction

When a transaction cannot be automatically categorized, the console displays:

```
???????????????????????????????????????????????????????????
Transaction 1 of 50
Description: UNKNOWN MERCHANT
Amount: $123.45
Date: 2024-01-15
No category match found.
???????????????????????????????????????????????????????????

Options:
  0. Exit (stop processing)
  1. Select existing category
  2. Create new category
  3. Add comment
  4. Skip (leave uncategorized) [default]

Your choice [0-4]:
```

### Option 1: Select Existing Category
1. Displays numbered list of existing categories
2. User selects by number
3. System asks: `Add a matching rule for 'UNKNOWN MERCHANT'? [Y/n]:`
4. If yes, user selects matcher type and provides parameters

### Option 2: Create New Category
1. User enters a new category name
2. Category ID is auto-generated (lowercase, spaces to hyphens)
3. System asks: `Add a matching rule for 'UNKNOWN MERCHANT'? [Y/n]:`
4. If yes, user selects matcher type and provides parameters
5. New category is saved to categories.json

### Option 3: Add Comment
1. User enters an optional comment for the transaction
2. Menu redisplays with comment shown
3. User can then select a category (options 1 or 2) or skip (option 4)
4. Comment is saved with the classified transaction

**Use Cases for Comments:**
- Distinguish between purchases at the same merchant (e.g., "Birthday gift" vs. "Personal use")
- Track tax-deductible or business expenses
- Note transactions that span multiple categories
- Add context for future expense reviews

See **[Transaction Comments Feature](features/TRANSACTION_COMMENTS.md)** for complete documentation.

### Option 4: Skip (Default)
- Transaction is categorized as "Uncategorized"
- No rules are added
- **Pressing Enter** without typing defaults to this option

### Option 0: Exit
- Stops processing remaining transactions
- Prompts to save category changes

## Interactive Matcher Creation

When adding a rule, the system prompts for matcher type. The matcher builder receives the full `Transaction` object (not just the description) so implementations can access other transaction fields such as `Amount` when constructing matcher parameters.

```
Select matcher type:
  1. ExactMatch - Match this exact description (fastest, recommended)
  2. Contains - Match descriptions containing a substring
  3. Regex - Match using a regular expression pattern
  0. Cancel

Your choice [0-3]:
```

### Creating an ExactMatch
```
Your choice [0-3]: 1

ExactMatch will match: 'STARBUCKS #12345'
Case-sensitive matching? [y/N]: n

? Will add ExactMatch rule for 'STARBUCKS #12345'
  (This will be merged with any existing ExactMatch rules)
```

### Creating a Contains Matcher
```
Your choice [0-3]: 2

Current description: 'STARBUCKS #12345'
Enter the substring to match: STARBUCKS
Case-sensitive matching? [y/N]: n

? Will add Contains rule for 'STARBUCKS' (case-insensitive)
  (This will be merged with any existing Contains rules)
```

### Creating a Regex Matcher
```
Your choice [0-3]: 3

Current description: 'STARBUCKS #12345'
Enter a regular expression pattern.
Examples:
  - Match Starbucks with store number: STARBUCKS\s+#\d+
  - Match any cafe/coffee shop: (CAFE|COFFEE|STARBUCKS)
Pattern: STARBUCKS\s+#\d+
Case-insensitive matching? [Y/n]: y

? Will add Regex rule for pattern 'STARBUCKS\s+#\d+' (case-insensitive)
```

## Smart Matcher Merging

The system intelligently merges matchers to keep your configuration clean:

### ExactMatch and Contains Merging
- Multiple values with **the same case sensitivity** are merged into a single matcher
- Values are deduplicated (case-insensitive comparison)
- If case sensitivity differs, separate matchers are created

**Example:** Adding "TRADER JOES" to existing ExactMatch ["SAFEWAY", "WHOLE FOODS"]:
```json
// Before
{
  "type": "ExactMatch",
  "parameters": {
    "values": [
      { "value": "SAFEWAY" },
      { "value": "WHOLE FOODS" }
    ]
  }
}

// After (merged)
{
  "type": "ExactMatch",
  "parameters": {
    "values": [
      { "value": "SAFEWAY" },
      { "value": "WHOLE FOODS" },
      { "value": "TRADER JOES" }
    ]
  }
}
```

### Regex Never Merges
Each regex pattern is unique and creates a new matcher instance.

## Output Files

Classified transactions are written to the `OutputPath` directory with timestamps in the filename.

### CSV Format (default)
```csv
StatementDate,TransactionDate,PostedDate,Description,Amount,Comment,CategoryId,CategoryName,MatcherType,InclusionStatus
2024-01-15,2024-01-15,2024-01-16,"SAFEWAY STORE #123",45.67,,groceries,Groceries,ExactMatch,Included
2024-01-16,2024-01-16,2024-01-17,"AMAZON.COM",79.99,"Birthday gift for Sarah",gifts,Gifts,Contains,Included
```

**Note:** The CSV now includes a `MatcherType` column (before `InclusionStatus`) that indicates which matcher type produced the automated classification. The `Comment` column appears before the category columns; empty comments appear as empty fields.

### JSON Format
```json
[
  {
    "transaction": {
      "statementDate": "2024-01-15",
      "transactionDate": "2024-01-15",
      "postedDate": "2024-01-16",
      "description": "SAFEWAY STORE #123",
      "amount": 45.67,
      "inclusionStatus": "Included"
    },
    "categoryId": "groceries",
    "categoryName": "Groceries",
    "comment": null,
    "matcherType": "ExactMatch"
  },
  {
    "transaction": {
      "statementDate": "2024-01-16",
      "transactionDate": "2024-01-16",
      "postedDate": "2024-01-17",
      "description": "AMAZON.COM",
      "amount": 79.99,
      "inclusionStatus": "Included"
    },
    "categoryId": "gifts",
    "categoryName": "Gifts",
    "comment": "Birthday gift for Sarah",
    "matcherType": "Contains"
  }
]
```

**Note:** The JSON output now includes a top-level `matcherType` property in each classified transaction object when applicable; it is `null` for manually classified transactions without a matcher addition.

## Processing Flow

For each PDF statement:
1. Extract text from PDF
2. Parse transactions
3. Classify each transaction (with user interaction if needed)
4. Save updated categories to categories.json after each change
5. Write classified transactions to output file

This ensures that new categories and rules added during processing are immediately available for subsequent transactions.

## Extensibility

The system is designed for easy extension with new matcher types. To add a new matcher:

1. Create a class implementing `ITransactionMatcher`
2. Add a case to `MatcherFactory.CreateMatcher()`
3. Add a method to `MatcherBuilderService` for interactive creation
4. Update the smart merging logic in `CategoryRepository` if applicable
5. Document the new matcher type and its parameters

Example future matchers:
- `StartsWithMatcher` - Match descriptions starting with specific text
- `EndsWithMatcher` - Match descriptions ending with specific text
- `FuzzyMatcher` - Match using Levenshtein distance for typo tolerance
- `AmountRangeMatcher` - Match based on transaction amount ranges

# Classification Guide

This document describes the matcher configuration schema and runtime behavior for the classification subsystem (matchers) after the recent refactor.

Key changes in this version

- `values` now uses an object form for each entry: `{ "value": "...", "amount": 12.34 }`.
  - `value` (string) is required for each entry.
  - `amount` (number, optional) is optional and, when present, restricts the matcher to only match transactions with that amount (rounded to 2 decimal places).
- All matchers are now forced to be case-insensitive. The previous `caseSensitive` parameter and the behaviour it implied have been removed.
- Regex matchers may optionally include an `amount` and will only match when both the regex matches the description and the amount (when provided) equals the transaction amount to 2 decimal places.
- Matchers now share a common base class `TransactionMatcherBase` which centralizes description validation and amount comparison logic.
- Backwards compatibility for the legacy string-array `values` format has been intentionally removed. The repository and factory expect `values` to be an array of objects.

JSON schema (example)

- Example `ExactMatch` rule (no amount):

```json
{
  "type": "ExactMatch",
  "parameters": {
    "values": [
      { "value": "WALMART" },
      { "value": "TARGET" }
    ]
  }
}
```

- Example `Contains` rule where one value has an amount and another does not:

```json
{
  "type": "Contains",
  "parameters": {
    "values": [
      { "value": "REFUND", "amount": -5.00 },
      { "value": "STARBUCKS" }
    ]
  }
}
```

- Example `Regex` rule with amount:

```json
{
  "type": "Regex",
  "parameters": {
    "pattern": "^WALMART #\\d+",
    "amount": 50.00
  }
}
```

Runtime behavior summary

- ExactMatch
  - Compares the full transaction description to each `value` entry (case-insensitive).
  - If a `value` entry includes an `amount`, the transaction must also have that amount (rounded to 2 decimals) for the match to succeed.

- Contains
  - Tests whether the transaction description contains the `value` substring (case-insensitive).
  - If a `value` entry includes an `amount`, the transaction must also have that amount (rounded to 2 decimals) for the match to succeed.

- Regex
  - Tests the configured regular expression against the transaction description. Regexes are always executed case-insensitively in the current codebase.
  - If an `amount` parameter is provided for the regex matcher, the transaction amount must equal it (rounded to 2 decimals) for the match to succeed.

Merging rules

- ExactMatch and Contains matchers are merged when a new matcher of the same `Type` is added to a category. Values are combined and deduplicated by `(value, amount)`.
- Regex matchers are always added as separate matchers (no merging).
- Deduplication is case-insensitive and considers `null` and absent amounts as distinct from explicit numeric amounts.

Migration notes

- Existing configuration files using the legacy string-array format for `values` must be migrated to the new object form. The migration is intentionally not automatic in this release.
- Remove `caseSensitive` parameters from matcher definitions; matching is always case-insensitive.

Implementation notes for developers

- The shared base class `TransactionMatcherBase` provides:
  - A `TryMatch` implementation that verifies `transaction` and `transaction.Description` are non-null and non-whitespace.
  - An abstract `TryMatchCore(Transaction transaction, string description)` method for concrete matchers to implement.
  - A protected helper `AmountsEqual(Decimal? expected, Decimal actual)` that rounds both sides to 2 decimal places and returns `true` when `expected` is `null` (no amount requirement).

- When adding new matcher types:
  1. Implement `ITransactionMatcher` by inheriting `TransactionMatcherBase` and implementing `TryMatchCore`.
  2. Add creation logic in `MatcherFactory.CreateMatcher` to parse the `parameters` and construct the matcher.
  3. If the new matcher supports per-value amounts, follow the `{ value, amount? }` shape for `values`.

Testing

- Unit tests were updated to construct matcher parameters using `System.Text.Json.JsonSerializer.SerializeToElement(new { value = "...", amount = ... })` for the `values` array entries.
- Add tests for regex matchers that include an amount to ensure both pattern and amount checks are performed.

Contact

- If you need migration tooling or a compatibility layer for older config files, open an issue or create a PR with a migration plan.
