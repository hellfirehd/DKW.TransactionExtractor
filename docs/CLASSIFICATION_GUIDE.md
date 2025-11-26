# Transaction Classification System

## Overview
The transaction classification system categorizes transactions based on configurable matching rules. When a transaction cannot be automatically categorized, the system prompts the user to select an existing category or create a new one.

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
            "values": ["SAFEWAY", "WHOLE FOODS"],
            "caseSensitive": false
          }
        },
        {
          "type": "Contains",
          "parameters": {
            "value": "GROCERY",
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
Matches transaction descriptions exactly against a list of values.

**Parameters:**
- `values` (string[]): Array of exact values to match
- `caseSensitive` (bool, optional): Whether matching is case-sensitive (default: false)

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
Matches if the transaction description contains the specified substring.

**Parameters:**
- `value` (string): Substring to search for
- `caseSensitive` (bool, optional): Whether matching is case-sensitive (default: false)

**Example:**
```json
{
  "type": "Contains",
  "parameters": {
    "value": "RESTAURANT",
    "caseSensitive": false
  }
}
```

### Regex
Matches transaction descriptions using a regular expression pattern.

**Parameters:**
- `pattern` (string): Regular expression pattern
- `ignoreCase` (bool, optional): Whether to ignore case (default: false)

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
Transaction: UNKNOWN MERCHANT
Amount: $123.45
Date: 2024-01-15
No category match found.
???????????????????????????????????????????????????????????

Options:
  1. Select existing category
  2. Create new category
  3. Skip (leave uncategorized)

Your choice [1-3]:
```

### Option 1: Select Existing Category
- Displays numbered list of existing categories
- User selects by number
- System asks if the transaction description should be added as a matching rule
- If yes, an ExactMatch rule is added to the category

### Option 2: Create New Category
- User enters a new category name
- Category ID is auto-generated from the name (lowercase, spaces to hyphens)
- System asks if the transaction description should be added as a matching rule
- New category is saved to categories.json

### Option 3: Skip
- Transaction is categorized as "Uncategorized"
- No rules are added

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
4. Save updated categories to categories.json
5. Write classified transactions to output file

This ensures that new categories and rules added during processing are immediately available for subsequent statements.

## Future Extensibility

The system is designed for easy extension with new matcher types. To add a new matcher:

1. Create a new class implementing `ITransactionMatcher`
2. Add a case to `MatcherFactory.CreateMatcher()`
3. Document the new matcher type and its parameters

Example future matchers could include:
- `StartsWithMatcher`
- `EndsWithMatcher`
- `SoundexMatcher` (phonetic matching)
- `FuzzyMatcher` (Levenshtein distance)
