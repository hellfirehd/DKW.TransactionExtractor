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
          "parameters": [
            { "value": "SAFEWAY" },
            { "value": "WHOLE FOODS" },
            { "value": "TRADER JOES" }
          ]
        },
        {
          "type": "Contains",
          "parameters": [
            { "value": "GROCERY" },
            { "value": "SUPERMARKET" }
          ]
        }
      ]
    }
  ]
}
```

## Matcher Types

All matchers support an optional `amount` parameter that restricts matches to transactions with a specific amount (rounded to 2 decimal places). All matching is **case-insensitive**.

### ExactMatch
Matches transaction descriptions exactly against a list of values. Multiple values are automatically merged into a single matcher.

**Parameters:**
- Array of objects, each with:
  - `value` (string, required): The exact value to match
  - `amount` (number, optional): Restricts matching to transactions with this amount (rounded to 2 decimal places)

**Matching Behavior:**
- Case-insensitive exact comparison
- If `amount` is specified, both description AND amount must match

**Merging Behavior:** When adding new values, they are merged into an existing ExactMatch matcher. Values are deduplicated by (value, amount) pair.

**Example:**
```json
{
  "type": "ExactMatch",
  "parameters": [
    { "value": "SAFEWAY" },
    { "value": "WHOLE FOODS" },
    { "value": "APPLE.COM/BILL 866-712-7753 ON", "amount": 32.42 }
  ]
}
```

### Contains
Matches if the transaction description contains any of the specified substrings. Multiple values are automatically merged into a single matcher.

**Parameters:**
- Array of objects, each with:
  - `value` (string, required): The substring to search for
  - `amount` (number, optional): Restricts matching to transactions with this amount (rounded to 2 decimal places)

**Matching Behavior:**
- Case-insensitive substring search
- If `amount` is specified, both description AND amount must match

**Merging Behavior:** When adding new substrings, they are merged into an existing Contains matcher. Values are deduplicated by (value, amount) pair.

**Example:**
```json
{
  "type": "Contains",
  "parameters": [
    { "value": "RESTAURANT" },
    { "value": "BISTRO" },
    { "value": "STARBUCKS", "amount": 10.99 }
  ]
}
```

### Regex
Matches transaction descriptions using a regular expression pattern. Each regex pattern is always created as a separate matcher (no merging).

**Parameters:**
- Array of objects, each with:
  - `value` (string, required): Regular expression pattern
  - `amount` (number, optional): Restricts matching to transactions with this amount (rounded to 2 decimal places)

**Matching Behavior:**
- Case-insensitive regex matching (forced with `RegexOptions.IgnoreCase`)
- Patterns are compiled for performance
- If `amount` is specified, both pattern AND amount must match

**Merging Behavior:** Regex matchers are never merged; each pattern creates a new matcher instance.

**Example:**
```json
{
  "type": "Regex",
  "parameters": [
    { "value": "^WALMART #\\d+" },
    { "value": "STARBUCKS #\\d{4}", "amount": 5.50 }
  ]
}
```

## Amount-Based Matching

### Use Cases

#### 1. Subscription Services
Distinguish between different subscription tiers from the same merchant:

```json
{
  "type": "ExactMatch",
  "parameters": [
    { "value": "APPLE.COM/BILL", "amount": 14.55 },  // Apple Music
    { "value": "APPLE.COM/BILL", "amount": 32.42 }   // iCloud Storage
  ]
}
```

#### 2. Recurring vs. One-Time Purchases
Separate recurring charges from occasional purchases:

```json
{
  "type": "Contains",
  "parameters": [
    { "value": "AMAZON", "amount": 16.99 },  // Amazon Prime (monthly)
    { "value": "AMAZON" }                     // All other Amazon purchases
  ]
}
```

#### 3. Specific Refunds
Track specific refund amounts:

```json
{
  "type": "Contains",
  "parameters": [
    { "value": "REFUND", "amount": -50.00 }
  ]
}
```

### Amount Comparison Logic

- Amounts are rounded to **2 decimal places** before comparison
- When `amount` is `null` or omitted, no amount filtering is applied
- Both the configured amount and transaction amount are rounded using `Decimal.Round(value, 2)`

**Example Comparisons:**
- `32.42` matches `32.4199` (both round to `32.42`)
- `32.42` does NOT match `32.43`
- `null` amount matches any transaction amount

## Matching Process

1. The system tries each category in order
2. Within each category, matchers are evaluated in order
3. **First match wins** - classification stops at the first successful match
4. If no match is found, the user is prompted

## User Interaction

When a transaction cannot be automatically categorized, the console displays:

```
??????????????????????????????????????????????????????????????????????????????
Transaction 1 of 50
Description: UNKNOWN MERCHANT
Amount: $123.45
Date: 2024-01-15
No category match found.
??????????????????????????????????????????????????????????????????????????????

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

When adding a rule, the system prompts for matcher type:

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

&#x2705; Will add ExactMatch rule for 'STARBUCKS #12345'
  (This will be merged with any existing ExactMatch rules)
```

### Creating a Contains Matcher
```
Your choice [0-3]: 2

Current description: 'STARBUCKS #12345'
Enter the substring to match: STARBUCKS

&#x2705; Will add Contains rule for 'STARBUCKS'
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

&#x2705; Will add Regex rule for pattern 'STARBUCKS\s+#\d+' (case-insensitive)
```

## Smart Matcher Merging

The system intelligently merges matchers to keep your configuration clean:

### ExactMatch and Contains Merging
- Multiple values are merged into a single matcher
- Values are deduplicated by (value, amount) pair using case-insensitive comparison
- A matcher without an amount is distinct from one with an amount

**Example:** Adding "TRADER JOES" to existing ExactMatch ["SAFEWAY", "WHOLE FOODS"]:
```json
// Before
{
  "type": "ExactMatch",
  "parameters": [
    { "value": "SAFEWAY" },
    { "value": "WHOLE FOODS" }
  ]
}

// After (merged)
{
  "type": "ExactMatch",
  "parameters": [
    { "value": "SAFEWAY" },
    { "value": "WHOLE FOODS" },
    { "value": "TRADER JOES" }
  ]
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

1. Create a class inheriting from `TransactionMatcherBase` and implementing `ITransactionMatcher`
2. Implement the `TryMatchCore(Transaction, String)` method
3. Add a case to `MatcherFactory.CreateMatcher()`
4. Add a method to `MatcherBuilderService` for interactive creation
5. Update the smart merging logic in `CategoryRepository` if applicable
6. Document the new matcher type and its parameters

Example future matchers:
- `StartsWithMatcher` - Match descriptions starting with specific text
- `EndsWithMatcher` - Match descriptions ending with specific text
- `FuzzyMatcher` - Match using Levenshtein distance for typo tolerance
- `AmountRangeMatcher` - Match based on transaction amount ranges

## Migration from Legacy Format

If you have an old configuration using the legacy format, you'll need to convert it:

**Old Format (No Longer Supported):**
```json
{
  "type": "ExactMatch",
  "parameters": {
    "values": ["WALMART", "TARGET"],
    "caseSensitive": false
  }
}
```

**New Format:**
```json
{
  "type": "ExactMatch",
  "parameters": [
    { "value": "WALMART" },
    { "value": "TARGET" }
  ]
}
```

**Notes:**
- `caseSensitive` parameter has been removed (all matching is case-insensitive)
- `values` is now an array of objects instead of strings
- Each value object requires a `value` property
- Optionally include an `amount` property for amount-specific matching

For detailed migration instructions, see **[Matcher Refactoring Guide](development/MATCHER_REFACTORING.md)**.
