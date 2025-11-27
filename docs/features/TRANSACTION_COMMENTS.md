# Transaction Comments Feature

## Overview

The transaction comment feature allows users to add optional notes to transactions during the classification process. This is particularly useful for differentiating transactions at the same merchant that serve different purposes (e.g., Amazon purchases for gifts vs. personal items).

## Use Cases

- **Multiple Purchase Purposes**: Distinguish between purchases at the same store (Amazon for entertainment vs. household items)
- **Gift Tracking**: Note when a purchase was for someone else
- **Special Occasions**: Mark transactions related to specific events (birthdays, holidays, travel)
- **Budget Categories**: Add notes for transactions that might span multiple budget categories
- **Tax Documentation**: Mark business-related or deductible expenses
- **Future Reference**: Add context that will be helpful when reviewing expenses later

## How It Works

### Comment Flow

During transaction classification, when an unmatched transaction is encountered, users see an enhanced menu:

```
???????????????????????????????????????????????????????????
Transaction 14 of 48
Description: AMAZON.COM
Amount: $45.99
Date: 2025-01-15
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

### Adding a Comment

1. **Press `3`** to add a comment
2. **Enter comment text** or press Enter to cancel:
   ```
   Enter comment (or press Enter to cancel): Birthday gift for Sarah
   Comment added: Birthday gift for Sarah
   ```
3. **Menu redisplays** with the comment shown:
   ```
   ???????????????????????????????????????????????????????????
   Transaction 14 of 48
   Description: AMAZON.COM
   Amount: $45.99
   Date: 2025-01-15
   Comment: Birthday gift for Sarah
   No category match found.
   ???????????????????????????????????????????????????????????
   ```
4. **Continue classification** by selecting a category (options 1 or 2)

### Default Behavior

- **Pressing Enter** without typing anything defaults to option 4 (Skip)
- **Empty/whitespace comments** are treated as "changed mind" - no comment is added
- **Comments persist** if you navigate back to the main menu
- **Auto-matched transactions** do not have comments (they're already categorized automatically)

## Comment Storage

### ClassifiedTransaction Model

Comments are stored as an optional property on classified transactions:

```csharp
public class ClassifiedTransaction
{
    public Transaction Transaction { get; set; }
    public String CategoryId { get; set; }
    public String CategoryName { get; set; }
    public String? Comment { get; set; }  // Optional comment
}
```

### Context Integration

Comments are managed within the `ClassifyTransactionContext` during the classification flow:

```csharp
public record ClassifyTransactionContext(
    Transaction Transaction,
    Int32 CurrentIndex,
    Int32 TotalCount)
{
    public String? Comment { get; set; }
    // ...other properties
}
```

## Output Formats

Comments are included in both CSV and JSON output formats.

### CSV Output

A `Comment` column is added after `CategoryName`:

```csv
StatementDate,TransactionDate,PostedDate,Description,Amount,CategoryId,CategoryName,Comment,InclusionStatus
2025-01-15,2025-01-15,2025-01-16,"AMAZON.COM",45.99,gifts,Gifts,"Birthday gift for Sarah",Included
2025-01-16,2025-01-16,2025-01-17,"STARBUCKS #1234",5.75,dining,Dining,,Included
```

**Note:** Empty comments appear as empty fields in CSV (as shown in the Starbucks transaction above).

### JSON Output

Comments are included as a property in each classified transaction:

```json
[
  {
    "transaction": {
      "statementDate": "2025-01-15",
      "transactionDate": "2025-01-15",
      "postedDate": "2025-01-16",
      "description": "AMAZON.COM",
      "amount": 45.99,
      "inclusionStatus": "Included"
    },
    "categoryId": "gifts",
    "categoryName": "Gifts",
    "comment": "Birthday gift for Sarah"
  },
  {
    "transaction": {
      "statementDate": "2025-01-16",
      "transactionDate": "2025-01-16",
      "postedDate": "2025-01-17",
      "description": "STARBUCKS #1234",
      "amount": 5.75,
      "inclusionStatus": "Included"
    },
    "categoryId": "dining",
    "categoryName": "Dining",
    "comment": null
  }
]
```

## Examples

### Example 1: Gift Tracking

```
Transaction 5 of 20
Description: AMAZON.COM
Amount: $79.99

[User presses 3]
Enter comment: Christmas gift for Mom

[User presses 1, selects "Gifts" category]
```

**Result:** Transaction is categorized as "Gifts" with comment "Christmas gift for Mom"

### Example 2: Multiple Categories

```
Transaction 12 of 30
Description: COSTCO #1234
Amount: $156.78

[User presses 3]
Enter comment: Food $80, Electronics $77

[User presses 1, selects "Groceries" category]
```

**Result:** Transaction is categorized as "Groceries" with note about split between categories

### Example 3: Tax Documentation

```
Transaction 8 of 25
Description: HOME DEPOT #5678
Amount: $234.50

[User presses 3]
Enter comment: Office renovation - tax deductible

[User presses 1, selects "Home Improvement" category]
```

**Result:** Transaction has both category and tax-related note for future reference

### Example 4: Changed Mind

```
Transaction 15 of 40
Description: SHELL GAS STATION
Amount: $52.00

[User presses 3]
Enter comment: [presses Enter without typing]
No comment added.

[User presses 4 to skip]
```

**Result:** Transaction categorized as "Uncategorized" with no comment

## Technical Details

### Architecture

The comment feature integrates cleanly with the existing classification system:

1. **ClassifyTransactionContext**: Holds the mutable `Comment` property during classification
2. **ConsoleInteractionService**: Manages comment input and display in the UI
3. **CategorySelectionResult**: Passes the comment from UI to classifier
4. **TransactionClassifier**: Assigns comment from selection result to classified transaction
5. **Formatters**: Include comment in output (CSV and JSON)

### Comment Lifecycle

```
1. Transaction needs classification
   ?
2. Context created with Comment = null
   ?
3. User prompted for category
   ?
4. [Optional] User adds comment to context
   ?
5. User selects category
   ?
6. CategorySelectionResult includes context.Comment
   ?
7. Classifier assigns comment to ClassifiedTransaction
   ?
8. Comment written to output file
```

### Code Locations

- **Model**: `src/DKW.TransactionExtractor/Models/ClassifiedTransaction.cs`
- **Context**: `src/DKW.TransactionExtractor/Classification/ClassifyTransactionContext.cs`
- **UI**: `src/DKW.TransactionExtractor/Classification/ConsoleInteractionService.cs`
- **Classifier**: `src/DKW.TransactionExtractor/Classification/TransactionClassifier.cs`
- **CSV Output**: `src/DKW.TransactionExtractor/Formatting/CsvFormatter.cs`
- **JSON Output**: `src/DKW.TransactionExtractor/Formatting/JsonFormatter.cs`

## Best Practices

### When to Add Comments

&#9989; **Do add comments when:**
- The same merchant serves different purposes
- Tracking gifts or purchases for others
- Marking tax-deductible or business expenses
- Noting transactions that span multiple budget categories
- Adding context for future expense reviews

&#10060; **Don't add comments when:**
- The category is self-explanatory
- The transaction is routine and obvious
- Auto-matched transactions (they're already categorized)

### Comment Guidelines

- **Be concise**: Keep comments brief and to the point
- **Be consistent**: Use a consistent format (e.g., "Gift for [Name]")
- **Be specific**: Include relevant details (who, what, why)
- **Use keywords**: Make comments searchable (e.g., "tax deductible", "business expense")

### Example Comment Patterns

```
? Good Comments:
  - "Birthday gift for Sarah"
  - "Office supplies - tax deductible"
  - "Food $45, Clothes $30"
  - "Travel - Chicago conference"
  - "Split: Personal (60%) / Business (40%)"

? Less Useful Comments:
  - "Stuff"
  - "Things I bought"
  - "See receipt"
  - "???"
```

## Future Enhancements

Potential future enhancements to the comment feature:

- **Comment Templates**: Pre-defined comment templates for common scenarios
- **Comment History**: Show previous comments for the same merchant
- **Comment Search**: Filter/search transactions by comment text
- **Comment Statistics**: Report on commented vs. uncommented transactions
- **Comment Validation**: Optional validation rules for required comment patterns
- **Bulk Comments**: Add the same comment to multiple transactions
- **Comment Import/Export**: Import comments from previous periods

## Related Documentation

- [Transaction Classification Guide](../CLASSIFICATION_GUIDE.md) - Main classification documentation
- [Output Formats](#output-formats) - Details on CSV and JSON output
- [User Interaction](#user-interaction) - Complete classification menu guide

---

**Feature Added**: 2025-01-27  
**Version**: 2025.1.0+  
**Status**: &#9989; Production Ready
