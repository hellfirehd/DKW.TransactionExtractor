# Extracted Text File Writing Feature

## Overview
Added a debugging feature to write extracted PDF text to `.txt` files with the same name as the source PDF. This helps with debugging parsing issues by allowing inspection of the raw extracted text.

## Changes Made

### 1. **Updated `AppOptions`** (`AppOptions.cs`)
Added new property:
```csharp
/// <summary>
/// When true, writes the extracted PDF text to a .txt file with the same name as the PDF.
/// Useful for debugging parsing issues.
/// </summary>
public bool WriteExtractedText { get; set; } = false;
```

### 2. **Updated `TransactionExtractor`** (`TransactionExtractor.cs`)
Added code to write extracted text when enabled:
```csharp
// Write extracted text to file if enabled
if (_appConfig.WriteExtractedText)
{
    var textFilePath = Path.ChangeExtension(pdfFile, ".txt");
    try
    {
        File.WriteAllText(textFilePath, text);
        _logger.LogDebug("Wrote extracted text to {TextFile}", textFilePath);
    }
    catch (Exception ex)
    {
        _logger.LogWarning(ex, "Failed to write extracted text to {TextFile}", textFilePath);
    }
}
```

### 3. **Updated `appsettings.json`**
Added configuration setting:
```json
{
  "AppOptions": {
    "FolderPath": "D:\\source\\DKW.TransactionExtractor\\DKW.TransactionExtractor\\statements",
    "Years": [ 2023, 2024, 2025 ],
    "WriteExtractedText": false
  }
}
```

## Usage

### To Enable Text File Writing:
1. Open `appsettings.json`
2. Set `WriteExtractedText` to `true`:
   ```json
   "WriteExtractedText": true
   ```
3. Run the application

### Output:
- For each PDF file processed, a corresponding `.txt` file will be created in the same directory
- Example:
  - PDF: `2024-03-21-Triangle-WorldEliteMasterca.pdf`
  - Output: `2024-03-21-Triangle-WorldEliteMasterca.txt`

## Features

### ? **Error Handling**
- Wrapped in try-catch to prevent failures from stopping processing
- Logs warnings if file writing fails
- Logs debug messages when files are successfully written

### ? **Non-Intrusive**
- Disabled by default (`false`)
- No performance impact when disabled
- Can be toggled without code changes

### ? **Debugging Benefits**
- Inspect raw extracted text to understand parsing issues
- Verify PDF extraction is working correctly
- Analyze text structure and formatting
- Share text files for remote debugging

## Example Scenario

When debugging a parsing issue:
1. Enable `WriteExtractedText: true`
2. Run the application
3. Check the generated `.txt` file to see the raw extracted text
4. Analyze why the parser isn't recognizing certain transactions
5. Update parser regex or logic as needed
6. Disable `WriteExtractedText: false` when done debugging

## Notes
- Text files are created in the same directory as the PDF files
- Existing `.txt` files will be overwritten
- File encoding is UTF-8 (default for `File.WriteAllText`)
- Debug log level may need to be enabled in logging configuration to see the "Wrote extracted text" messages
