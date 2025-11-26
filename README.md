# DKW.TransactionExtractor

A .NET 10 console application for extracting and validating credit card transactions from PDF statements. Currently supports Canadian Tire Financial Services (CTFS) Mastercard statements.

## Features

- **PDF Text Extraction**: Extracts text from PDF credit card statements using iText library
- **Transaction Parsing**: Parses individual transactions with dates, descriptions, and amounts
- **Transaction Classification**: Categorizes transactions using configurable matching rules
- **Interactive Console UI**: Prompts for manual classification when needed
- **Flexible Output Formats**: Export classified transactions as CSV or JSON
- **Automatic Validation**: Compares computed purchase totals against declared statement totals
- **Transaction Filtering**: Excludes fees, interest charges, and refunds from purchase calculations
- **Date Range Filtering**: Process statements within a configurable date range (start/end dates)
- **Automatic Versioning**: Uses GitVersion for automatic version management
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
    "StartDate": "2024-10-01",
    "EndDate": null,
    "WriteExtractedText": false,
    "CategoryConfigPath": "categories.json",
    "OutputFormat": "Csv",
    "OutputPath": "output",
    "OutputUncategorized": true
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
- **StartDate**: Optional start date to filter statements (inclusive). Format: `"YYYY-MM-DD"`. If `null`, no start date filtering is applied.
- **EndDate**: Optional end date to filter statements (exclusive). Format: `"YYYY-MM-DD"`. If `null`, no end date filtering is applied.
- **WriteExtractedText**: When `true`, saves extracted text to `.txt` files for debugging
- **CategoryConfigPath**: Path to the category configuration JSON file
- **OutputFormat**: Output format for classified transactions (`"Csv"` or `"Json"`)
- **OutputPath**: Directory where classified transaction files will be written
- **OutputUncategorized**: When `true`, includes uncategorized transactions in output. When `false`, filters them out.

#### ParserOptions

- **DifferenceTolerance**: Maximum acceptable difference between declared and computed totals (default: 0.01)
- **ExclusionPatterns**: Array of description patterns to exclude from purchase totals (fees, interest, etc.)

## Usage

1. Configure the folder path in `appsettings.json`
2. Create a `categories.json` file with your category definitions (see [Transaction Classification](#transaction-classification))
3. Place PDF statements in the configured folder (named with YYYY-MM-DD prefix)
4. Run the application:

    ```bash
    dotnet run --project src/DKW.TransactionExtractor
    ```

### Example Output

```
[2025-11-27 10:30:45.123] [INF] Starting Transaction Extractor v2025.1.0+abc1234
[2025-11-27 10:30:45.234] [INF] Filtering statements: StartDate=2024-10-01, EndDate=<null>
[2025-11-27 10:30:45.345] [INF] Found 12 PDF file(s) matching configured date range in D:\statements
[2025-11-27 10:30:45.456] [INF] Processed File: 2024-10-21-statement.pdf
[2025-11-27 10:30:45.567] [INF] Declared Purchases: $2,348.84
[2025-11-27 10:30:45.678] [INF] Computed Purchases: $2,348.84
[2025-11-27 10:30:45.789] [INF] Transaction Count: 42
[2025-11-27 10:30:45.890] [INF] Excluded Transactions: 3
[2025-11-27 10:30:45.991] [INF] Classifying 42 transactions...
[2025-11-27 10:30:46.100] [INF] Wrote 42 classified transactions to output
[2025-11-27 10:30:46.200] [INF] Generated summary for 15 categories
```

### Transaction Classification

The application automatically categorizes transactions based on configurable rules. When a transaction cannot be automatically categorized, the console prompts for manual classification.

See the **[Transaction Classification Guide](docs/CLASSIFICATION_GUIDE.md)** for detailed documentation on:
- Category configuration
- Matcher types (ExactMatch, Contains, Regex)
- Interactive matcher creation
- Smart merging behavior
- Output formats

For complete documentation, visit the **[Documentation Index](docs/README.md)**.

### Mismatch Detection

When totals don't match, the application logs detailed transaction information:

```
[2025-11-27 10:30:50.123] [WRN] Declared: $1,234.56 | Computed: $1,230.21 | Difference: $4.35
[2025-11-27 10:30:50.234] [WRN] Investigate this file and update parser logic if necessary.
[2025-11-27 10:30:50.345] [INF] 2024-10-15 | CANADIAN TIRE #123 | 75.00 | Excluded: Include
[2025-11-27 10:30:50.456] [INF] 2024-10-16 | ANNUAL FEE | 50.00 | Excluded: Exclude
```

## Development

### Versioning

This project uses [GitVersion](https://gitversion.net/) for automatic version management. See **[Versioning Workflow](docs/development/VERSIONING_WORKFLOW.md)** for details on:
- Version format (YYYY.MINOR.PATCH)
- Creating releases with Git tags
- Pre-release versioning
- Version increment guidelines

### Coding Standards

Before contributing, please review our coding standards documented in **[GitHub Copilot Instructions](.github/copilot-instructions.md)** which include:
- Naming conventions (use `String`, `Boolean`, `Int32`)
- Architecture patterns (Service Layer, DI, Immutability)
- Code organization guidelines
- Documentation placement (always in `./docs/`)

### Project Documentation

Visit the **[Documentation Index](docs/README.md)** for:
- [Transaction Classification Guide](docs/CLASSIFICATION_GUIDE.md)
- [Versioning Workflow](docs/development/VERSIONING_WORKFLOW.md)
- Feature documentation in `docs/features/`
- Development guides in `docs/development/`

### Testing

Run the test suite:

```bash
dotnet test
```

The test project includes:
- Unit tests for transaction parsing logic
- Edge case handling tests
- Real statement validation tests
- Transaction exclusion pattern tests

### Contributing

Contributions are welcome! Please:
1. Read the **[Coding Standards](.github/copilot-instructions.md)**
2. Review the **[Documentation](docs/README.md)**
3. Fork the repository
4. Create a feature branch
5. Add tests for new functionality
6. Ensure code follows our standards
7. Update documentation in `./docs/` as needed
8. Submit a pull request

## Project Structure

```
DKW.TransactionExtractor/
??? Classification/
?   ??? ITransactionClassifier.cs
?   ??? TransactionClassifier.cs
?   ??? CategoryRepository.cs
?   ??? ConsoleInteractionService.cs
?   ??? ITransactionMatcher.cs
?   ??? ExactMatcher.cs
?   ??? ContainsMatcher.cs
?   ??? RegexMatcher.cs
?   ??? MatcherFactory.cs
??? Formatting/
?   ??? ITransactionFormatter.cs
?   ??? CsvFormatter.cs
?   ??? JsonFormatter.cs
??? Models/
?   ??? ParseContext.cs
?   ??? ParseResult.cs
?   ??? Transaction.cs
?   ??? ClassifiedTransaction.cs
?   ??? Category.cs
?   ??? CategoryMatcher.cs
?   ??? TransactionInclusionStatus.cs
??? Providers/
?   ??? CTFS/
?       ??? CtfsMastercardPdfTextExtractor.cs
?       ??? CtfsMastercardTransactionParser.cs
??? EncodingProviders/
?   ??? PdfAliasEncodingProvider.cs
??? AppOptions.cs
??? ParserOptions.cs
??? DefaultTransactionFilter.cs
??? TransactionExtractor.cs
??? Program.cs

DKW.TransactionExtractor.Tests/
??? TransactionParserTests.cs
??? TransactionParserEdgeCasesTests.cs
??? TransactionExclusionTests.cs
??? ParseResultTests.cs
??? ...
```

## Architecture

The application follows a clean, modular architecture with dependency injection:

- **TransactionExtractor**: Main orchestrator that processes PDF files
- **IPdfTextExtractor**: Extracts raw text from PDF documents
- **ITransactionParser**: Parses transactions from extracted text
- **ITransactionClassifier**: Classifies transactions into categories
- **ITransactionFormatter**: Formats classified transactions for output
- **ITransactionFilter**: Determines which transactions to include in totals
- **Models**: Data transfer objects for transactions and parse results

## Dependencies

- **iText 9.4.0**: PDF text extraction
- **GitVersion.MsBuild 6.5.0**: Automatic version management
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

See **[Serilog File Logging](docs/features/SERILOG_FILE_LOGGING.md)** for detailed configuration options.

## Troubleshooting

### No transactions found

- Enable `WriteExtractedText: true` in `appsettings.json`
- Review the generated `.txt` file to verify PDF text extraction
- Check that statement format matches expected CTFS Mastercard format

### Mismatch between declared and computed totals

- Review logged transaction details for exclusion status
- Add missing exclusion patterns to `ParserOptions.ExclusionPatterns`
- Verify multi-line transactions are being combined correctly
- See **[Transaction Exclusion Feature](docs/features/TRANSACTION_EXCLUSION_FEATURE.md)**

### Classification issues

- Check that `categories.json` exists and is valid JSON
- Review category matcher patterns for accuracy
- Enable debug logging to see matching attempts
- See **[Transaction Classification Guide](docs/CLASSIFICATION_GUIDE.md)**

### Encoding issues

The application includes custom PDF encoding providers. If you encounter unusual characters, check `PdfAliasEncodingProvider.cs`.

### Date filtering issues

- Ensure PDF filenames start with `YYYY-MM-DD` format (e.g., `2024-10-21-statement.pdf`)
- Verify `StartDate` and `EndDate` are in `YYYY-MM-DD` format
- StartDate is inclusive (>=), EndDate is exclusive (<)