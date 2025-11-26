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
            "values": ["SAFEWAY", "WHOLE FOODS", "TRADER JOES"],
            "caseSensitive": false
          }
        },
        {
          "type": "Contains",
          "parameters": {
            "values": ["GROCERY", "SUPERMARKET"],
            "caseSensitive": false
          }
        }
      ]
    }
  ]
}
```

## Matcher Types

### ExactMatch
Matches transaction descriptions exactly against a list of values. Multiple values with the same case sensitivity are automatically merged into a single matcher.

**Parameters:**
- `values` (string[]): Array of exact values to match
- `caseSensitive` (bool, optional): Whether matching is case-sensitive (default: false)

**Merging Behavior:** When adding new values, they are merged into an existing ExactMatch matcher with the same `caseSensitive` setting.

**Example:**
```json
{
  "type": "ExactMatch",
  "parameters": {
    "values": ["SAFEWAY", "WHOLE FOODS", "TRADER JOES"],
    "caseSensitive": false
  }
}
```

### Contains
Matches if the transaction description contains any of the specified substrings. Multiple values with the same case sensitivity are automatically merged into a single matcher.

**Parameters:**
- `values` (string[]): Array of substrings to search for
- `caseSensitive` (bool, optional): Whether matching is case-sensitive (default: false)

**Merging Behavior:** When adding new substrings, they are merged into an existing Contains matcher with the same `caseSensitive` setting.

**Example:**
```json
{
  "type": "Contains",
  "parameters": {
    "values": ["RESTAURANT", "BISTRO", "CAFE"],
    "caseSensitive": false
  }
}
```

**Note:** Legacy format with single `value` parameter is still supported for backward compatibility.

### Regex
Matches transaction descriptions using a regular expression pattern. Each regex pattern is always created as a separate matcher (no merging).

**Parameters:**
- `pattern` (string): Regular expression pattern
- `ignoreCase` (bool, optional): Whether to ignore case (default: true)

**Merging Behavior:** Regex matchers are never merged; each pattern creates a new matcher instance.

**Example:**
```json
{
  "type": "Regex",
  "parameters": {
    "pattern": "\\b(CAFE|COFFEE|BISTRO)\\b",
    "ignoreCase": true
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
  1. Select existing category
  2. Create new category
  3. Skip (leave uncategorized)
  4. Exit (stop processing)

Your choice [1-4]:
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

### Option 3: Skip
- Transaction is categorized as "Uncategorized"
- No rules are added

### Option 4: Exit
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
    "values": ["SAFEWAY", "WHOLE FOODS"],
    "caseSensitive": false
  }
}

// After (merged)
{
  "type": "ExactMatch",
  "parameters": {
    "values": ["SAFEWAY", "WHOLE FOODS", "TRADER JOES"],
    "caseSensitive": false
  }
}
```

### Regex Never Merges
Each regex pattern is unique and creates a new matcher instance.

## Output Files

Classified transactions are written to the `OutputPath` directory with the same name as the input PDF file.

### CSV Format (default)
```csv
TransactionDate,PostedDate,Description,Amount,CategoryId,CategoryName,InclusionStatus
2024-01-15,2024-01-16,"SAFEWAY STORE #123",45.67,groceries,Groceries,Included
```

### JSON Format
```json
[
  {
    "transaction": {
      "transactionDate": "2024-01-15",
      "postedDate": "2024-01-16",
      "description": "SAFEWAY STORE #123",
      "amount": 45.67,
      "rawText": "...",
      "startLineNumber": 10,
      "inclusionStatus": "Included"
    },
    "categoryId": "groceries",
    "categoryName": "Groceries"
  }
]
```

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
