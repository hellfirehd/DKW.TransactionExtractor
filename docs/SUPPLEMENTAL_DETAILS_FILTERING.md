# Supplemental Details Section Filtering - Implementation Summary

## Overview
Implemented detection and filtering of supplemental transaction detail lines that appear after the "Details of your Canadian Tire store purchases" header. These lines are itemized purchase details that should not be included in the main transaction list.

## Problem
Credit card statements contain a supplemental details section that begins with the text "Details of your Canadian Tire store purchases". All transactions following this header are itemized details (often with quantity prefixes like "2 @" or "1 x") that break down a previous purchase. These should:
- **NOT** be added to `ParseResult.Transactions`
- **NOT** be counted in purchases totals
- **NOT** generate warnings

## Solution

### 1. **Added Details Section Detection Regex**
```csharp
[GeneratedRegex(@"Details\s+of\s+your\s+Canadian\s+Tire\s+store\s+purchases", 
    RegexOptions.IgnoreCase | RegexOptions.Compiled)]
private static partial Regex GetDetailsSectionStartRegex();
```
- Case-insensitive matching
- Detects the header line that starts the details section

### 2. **Updated ParseTransactions Method**
Added state tracking to detect when we enter the details section:

```csharp
private void ParseTransactions(string[] rawLines, ParseResult result)
{
    var inDetailsSection = false;
    
    for (var i = 0; i < rawLines.Length; i++)
    {
        // Check if entering details section
        if (DetailsSectionStartRegex.IsMatch(rawLine))
        {
            inDetailsSection = true;
            continue;
        }
        
        // Skip transactions when in details section
        if (inDetailsSection)
        {
            _logger.LogDebug("Skipping supplemental detail...");
            continue;
        }
        
        // Normal transaction processing
    }
}
```

### 3. **Key Behaviors**
- **Before "Details..." header**: All transactions are parsed normally
- **After "Details..." header**: All transactions are skipped
- **Debug logging**: Added to help diagnose issues
- **No warnings**: Detail lines don't generate warnings

## Example

### Input Statement:
```
Statement date: October 21, 2025
Purchases 100.00
Oct 15 Oct 15 CANADIAN TIRE #123 KELOWNA BC 75.00
Oct 16 Oct 16 GROCERY STORE 25.00
Details of your Canadian Tire store purchases
Oct 15 Oct 15 2 @ MOTOR OIL 5W30 30.00
Oct 15 Oct 15 1 @ WINDSHIELD WIPERS 25.00
Oct 15 Oct 15 1 @ AIR FRESHENER 5.00
```

### Result:
- **Transactions Parsed**: 2 (CANADIAN TIRE, GROCERY STORE)
- **Transactions Skipped**: 3 (detail lines)
- **ComputedPurchasesTotal**: $100.00
- **Warnings**: 0

## Test Coverage

Created `SupplementalDetailsTests.cs` with 5 comprehensive tests:

1. ? **Parse_WithSupplementalDetails_ExcludesDetailTransactions**
   - Verifies detail lines are excluded
   - Confirms main transactions are included

2. ? **Parse_WithoutSupplementalDetails_IncludesAllTransactions**
   - Ensures normal parsing works without details section

3. ? **Parse_DetailsSection_DoesNotCreateWarnings**
   - Verifies no warnings for detail lines

4. ? **Parse_DetailsSectionWithVariousFormats_AllSkipped**
   - Tests various detail line formats (with/without quantity markers)

5. ? **Parse_DetailsSectionCaseInsensitive_Detected**
   - Confirms case-insensitive header detection

## Benefits

### ? **Accurate Transaction Counts**
- No duplicate or inflated transaction counts
- Purchases total matches statement declaration

### ? **Clean Results**
- No warnings for expected detail lines
- Only actual transactions in results

### ? **Debug Support**
- Debug logging helps trace behavior
- Easy to diagnose parsing issues

### ? **Maintainable**
- Simple state machine (boolean flag)
- Clear separation of concerns

## Test Results
- **42 tests total**
- **38 passed**
- **4 skipped** (expected - future features)
- **0 failures**

## Implementation Notes

1. **Once-In Always-In**: Once the details section is detected, ALL subsequent transactions are skipped until end of file
2. **Case Insensitive**: Header detection ignores case
3. **Debug Logging**: All skipped transactions are logged at Debug level
4. **No Warnings**: Detail section lines don't pollute the warnings collection
5. **Performance**: Regex compiled once at class initialization

## Future Enhancements

Potential improvements if needed:
- Add configuration option to enable/disable details filtering
- Support multiple details sections in same statement (if ever needed)
- Extract detail section data for separate reporting
