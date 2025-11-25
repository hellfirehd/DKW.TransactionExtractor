# DKW.TransactionExtractor

A .NET 10 console application for extracting and validating credit card transactions from PDF statements. Currently supports Canadian Tire Financial Services (CTFS) Mastercard statements.

## Features

- **PDF Text Extraction**: Extracts text from PDF credit card statements using iText library
- **Transaction Parsing**: Parses individual transactions with dates, descriptions, and amounts
- **Automatic Validation**: Compares computed purchase totals against declared statement totals
- **Transaction Filtering**: Excludes fees, interest charges, and refunds from purchase calculations
- **Year-based Filtering**: Process only statements from specified years
- **Multi-line Transaction Support**: Handles transactions that span multiple lines in the PDF
- **Supplemental Details Detection**: Skips itemized purchase details to avoid double-counting
- **Configurable Logging**: Uses Serilog for structured logging to console and file
- **Debugging Support**: Optionally exports extracted PDF text for troubleshooting

## Requirements

- .NET 10 SDK or later
- Windows, macOS, or Linux

## Installation

1. Clone the repository:
    ```bash
    git clone https://github.com/hellfirehd/DKW.TransactionExtractor.git
    cd DKW.TransactionExtractor
    ```

2. Build the solution:
    ```bash
    dotnet build
    ```

## Configuration

Edit `appsettings.json` to configure the application:

```json
{
  "AppOptions": {
    "FolderPath": "C:\\path\\to\\statements",
    "Years": [ 2023, 2024, 2025 ],
    "WriteExtractedText": false
  },
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

### Configuration Options

#### AppOptions

- **FolderPath**: Directory containing PDF statement files
- **Years**: Array of years to process (filters by YYYY prefix in filename)
- **WriteExtractedText**: When `true`, saves extracted text to `.txt` files for debugging

#### ParserOptions

- **DifferenceTolerance**: Maximum acceptable difference between declared and computed totals (default: 0.01)
- **ExclusionPatterns**: Array of description patterns to exclude from purchase totals (fees, interest, etc.)

## Usage

1. Configure the folder path in `appsettings.json`
2. Place PDF statements in the configured folder (named with YYYY-MM-DD prefix)
3. Run the application:

    ```bash
    dotnet run --project DKW.TransactionExtractor
    ```

### Example Output

```
[2025-01-15 10:30:45.123] [INF] Starting Transaction Extractor application
[2025-01-15 10:30:45.234] [INF] Found 12 PDF files in D:\statements
[2025-01-15 10:30:45.345] [INF] Filename: 2024-10-21-statement.pdf
[2025-01-15 10:30:45.456] [INF] Declared Purchases: $2,348.84
[2025-01-15 10:30:45.567] [INF] Computed Purchases: $2,348.84
[2025-01-15 10:30:45.678] [INF] Transaction Count: 42
[2025-01-15 10:30:45.789] [INF] Excluded Transactions: 3
```

### Mismatch Detection

When totals don't match, the application logs detailed transaction information:

```
[2025-01-15 10:30:50.123] [WRN] Mismatch detected! Declared: $1,234.56, Computed: $1,230.21, Difference: $4.35
[2025-01-15 10:30:50.234] [WRN] Please investigate the following transactions:
[2025-01-15 10:30:50.345] [INF] Oct 15 | CANADIAN TIRE #123 | $75.00 | Include
[2025-01-15 10:30:50.456] [INF] Oct 16 | ANNUAL FEE | $50.00 | Exclude
```

## Project Structure

```
DKW.TransactionExtractor/
??? Models/
?   ??? ParseContext.cs          # Input context for parsing
?   ??? ParseResult.cs           # Output result with transactions and totals
?   ??? Transaction.cs           # Individual transaction model
?   ??? TransactionInclusionStatus.cs
??? Providers/
?   ??? CTFS/
?       ??? CtfsMastercardPdfTextExtractor.cs   # PDF text extraction
?       ??? CtfsMastercardTransactionParser.cs  # Transaction parsing logic
??? EncodingProviders/
?   ??? PdfAliasEncodingProvider.cs             # Custom encoding support
??? AppOptions.cs                # Application configuration model
??? ParserOptions.cs             # Parser configuration model
??? DefaultTransactionFilter.cs  # Transaction exclusion logic
??? ITransactionFilter.cs        # Transaction filter interface
??? ITransactionParser.cs        # Transaction parser interface
??? IPdfTextExtractor.cs         # PDF extractor interface
??? LogMessages.cs               # Structured logging messages
??? TransactionExtractor.cs      # Main processing logic
??? Program.cs                   # Application entry point

DKW.TransactionExtractor.Tests/
??? TransactionParserTests.cs
??? TransactionParserEdgeCasesTests.cs
??? TransactionExclusionTests.cs
??? TransactionParsingIssuesTests.cs
??? ParseResultTests.cs
??? SupplementalDetailsTests.cs
??? RealStatementTests.cs
```

## Architecture

The application follows a clean, modular architecture with dependency injection:

- **TransactionExtractor**: Main orchestrator that processes PDF files
- **IPdfTextExtractor**: Extracts raw text from PDF documents
- **ITransactionParser**: Parses transactions from extracted text
- **ITransactionFilter**: Determines which transactions to include in totals
- **Models**: Data transfer objects for transactions and parse results

## Testing

Run the test suite:

```bash
dotnet test
```

The test project includes:
- Unit tests for transaction parsing logic
- Edge case handling tests
- Real statement validation tests
- Transaction exclusion pattern tests

## Dependencies

- **iText 9.4.0**: PDF text extraction
- **Microsoft.Extensions.Configuration**: Configuration management
- **Microsoft.Extensions.DependencyInjection**: Dependency injection
- **Serilog**: Structured logging
- **MSTest**: Unit testing framework

## Supported Statement Formats

### CTFS Mastercard

- **Transaction Format**: `MMM DD MMM DD DESCRIPTION AMOUNT`
  - Example: `Oct 15 Oct 15 CANADIAN TIRE #123 KELOWNA BC 75.00`
- **Statement Date**: Extracted from header (e.g., "Statement date: October 21, 2025")
- **Purchases Total**: Extracted from "Purchases" line
- **Negative Amounts**: Represented with parentheses, e.g., `(3,463.00)`
- **Multi-line Support**: Handles descriptions that span multiple lines
- **Supplemental Details**: Automatically skips "Details of your Canadian Tire store purchases" section

## Extensibility

To add support for additional credit card providers:

1. Implement `IPdfTextExtractor` for provider-specific text extraction
2. Implement `ITransactionParser` for provider-specific parsing logic
3. Register implementations in `Program.cs` dependency injection configuration
4. Add provider-specific configuration to `appsettings.json`

## Logging

Logs are written to:
- **Console**: Real-time output with color coding
- **File**: Daily rolling logs in `logs/transaction-extractor-YYYYMMDD.log`
- **Retention**: 30 days of log files

Log levels can be adjusted in `appsettings.json` under the `Serilog` section.

## Troubleshooting

### No transactions found

- Enable `WriteExtractedText: true` in `appsettings.json`
- Review the generated `.txt` file to verify PDF text extraction
- Check that statement format matches expected CTFS Mastercard format

### Mismatch between declared and computed totals

- Review logged transaction details for exclusion status
- Add missing exclusion patterns to `ParserOptions.ExclusionPatterns`
- Verify multi-line transactions are being combined correctly

### Encoding issues

The application includes custom PDF encoding providers. If you encounter unusual characters, check `PdfAliasEncodingProvider.cs`.

## License

This project is available under the [AGPL License](LICENSE).

## Contributing

Contributions are welcome! Please:
1. Fork the repository
2. Create a feature branch
3. Add tests for new functionality
4. Submit a pull request

## Acknowledgments

- iText library for PDF processing
- Serilog for structured logging
- Microsoft Extensions for configuration and dependency injection