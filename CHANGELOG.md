# Changelog

All notable changes to the DKW.TransactionExtractor project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

This project uses **Calendar-Semantic Versioning** in the format `YYYY.MINOR.PATCH`:
- **YYYY**: Year of release (e.g., 2025)
- **MINOR**: Incremented for new features or significant changes
- **PATCH**: Incremented for bug fixes and minor improvements

Versions are automatically managed by [GitVersion](https://gitversion.net/) based on Git tags.

## [Unreleased]

_No unreleased changes at this time._

## [2025.1.0] - 2025-11-27

### Added
- **Automatic Versioning**: Implemented GitVersion for automatic version management
  - Calendar-semantic versioning format: `YYYY.MINOR.PATCH`
  - Version automatically derived from Git tags
  - Application logs version at startup
  - Pre-release versions automatically tagged with `-alpha.X` between releases
  - Files: `GitVersion.yml`, `Program.cs`, `DKW.TransactionExtractor.csproj`

### Changed
- **Date Range Filtering**: Replaced year-based filtering with date range filtering
  - Added `StartDate` and `EndDate` properties to `AppOptions` (nullable DateTime)
  - Removed `Years` array property
  - Updated `GetFilteredPdfFiles()` to parse full dates (YYYY-MM-DD) from filenames
  - StartDate is inclusive (>=), EndDate is exclusive (<)
  - Added date range validation and logging in `ValidateConfiguration()`
  - Updated log messages: `LogNoDateRangeConfigured` and `LogDateRangeFilter`
  - **Breaking Change**: Configuration format changed from `"Years": [2024, 2025]` to `"StartDate": "2024-10-01"` and `"EndDate": null`
  - Files: `AppOptions.cs`, `TransactionExtractor.cs`, `LogMessages.cs`, `appsettings.json`

## [2025.0.2] - 2025-11-26 - Output and Data Model Enhancements

### Added
- **Transaction Summarization**: Added category summaries to output
  - Groups transactions by category with counts and totals
  - Sorted alphabetically by category name
  - Includes summary in both CSV and JSON output formats
  - Files: `TransactionExtractor.cs`, `CategorySummary.cs`, `TransactionOutput.cs`

- **Statement Date in Transaction Model**: Added `StatementDate` property to transactions
  - Tracks which statement each transaction came from
  - Attached during classification process
  - Useful for multi-statement analysis
  - File: `Transaction.cs`

- **Output Uncategorized Control**: Added `OutputUncategorized` configuration option
  - Controls whether uncategorized transactions appear in output
  - Defaults to `true` (include all transactions)
  - When `false`, filters uncategorized from both transactions and summaries
  - File: `AppOptions.cs`, `TransactionExtractor.cs`

### Changed
- **PDF Text Sanitization**: Added text sanitization during PDF extraction
  - Removes null characters and other problematic characters
  - Improves parsing reliability across different PDF sources
  - File: `CtfsMastercarrdPdfTextExtractor.cs`

- **Consolidated Output**: Changed from per-statement files to single consolidated output
  - Single `transactions.csv` or `transactions.json` file
  - Contains all processed transactions from all statements
  - Includes generation timestamp
  - Files: `CsvFormatter.cs`, `JsonFormatter.cs`

- **Summary File Naming**: Updated output file naming for summaries
  - More consistent and descriptive filenames
  - Better organization of output files

- **Category ID Normalization**: Normalizes category IDs for consistency
  - Converts to lowercase and removes special characters
  - Ensures consistent categorization across sessions
  - File: `CategoryRepository.cs`

### Fixed
- **Null Checks**: Added null checks for robustness
  - Prevents null reference exceptions in edge cases
  - Improved error handling throughout classification

## [2025.0.1] - 2025-11-25 - Initial Release

### Added
- **Transaction Classification System**: Complete classification framework
  - Automatic transaction categorization based on configurable rules
  - Interactive console prompts for manual classification
  - Smart merging of new matchers into existing categories
  - Category configuration via JSON file
  - Support for multiple matcher types: ExactMatch, Contains, Regex
  - Files: `TransactionClassifier.cs`, `CategoryRepository.cs`, `ConsoleInteractionService.cs`
  - Files: `ITransactionMatcher.cs`, `ExactMatcher.cs`, `ContainsMatcher.cs`, `RegexMatcher.cs`, `MatcherFactory.cs`
  - Models: `Category.cs`, `CategoryConfig.cs`, `CategoryMatcher.cs`, `ClassifiedTransaction.cs`

- **Interactive Matcher Creation**: Console UI for creating category matchers
  - Prompts for category assignment when no match found
  - Allows creating new categories or adding to existing
  - Multiple matcher type options (exact, contains, regex)
  - Case-insensitive matching by default
  - Smart pattern suggestion based on transaction description
  - File: `ConsoleInteractionService.cs`

- **Flexible Output Formats**: Export classified transactions as CSV or JSON
  - CSV formatter with proper escaping and formatting
  - JSON formatter with structured output including metadata
  - Configurable output format via `AppOptions.OutputFormat`
  - Files: `CsvFormatter.cs`, `JsonFormatter.cs`, `ITransactionFormatter.cs`

- **CTFS Mastercard Parser**: Full support for Canadian Tire Financial Services statements
  - Transaction format: `MMM DD MMM DD DESCRIPTION AMOUNT`
  - Statement date extraction from header
  - Purchases total extraction and validation
  - Multi-line transaction support
  - Negative amount handling (parentheses format)
  - Files: `CtfsMastercardTransactionParser.cs`, `CtfsMastercarrdPdfTextExtractor.cs`

- **PDF Text Extraction**: Extract text from PDF credit card statements
  - Uses iText library for reliable extraction
  - Custom encoding provider for PDF-specific character handling
  - Optional export of extracted text for debugging (`WriteExtractedText`)
  - Files: `IPdfTextExtractor.cs`, `CtfsMastercarrdPdfTextExtractor.cs`, `PdfAliasEncodingProvider.cs`
  - Documentation: `docs/features/EXTRACTED_TEXT_FEATURE.md`

- **Transaction Exclusion System**: Configurable patterns to exclude fees and interest
  - Regex-based pattern matching for transaction descriptions
  - Excludes charges from purchase total calculations
  - Configurable via `ParserOptions.ExclusionPatterns`
  - Detailed logging of excluded transactions
  - Files: `DefaultTransactionFilter.cs`, `ITransactionFilter.cs`
  - Documentation: `docs/features/TRANSACTION_EXCLUSION_FEATURE.md`

- **Supplemental Details Filtering**: Automatic detection and skipping of itemized details
  - Detects "Details of your Canadian Tire store purchases" section
  - Skips supplemental transaction lines to prevent double-counting
  - Maintains accurate purchase totals
  - File: `CtfsMastercardTransactionParser.cs`
  - Documentation: `docs/features/SUPPLEMENTAL_DETAILS_FILTERING.md`

- **Validation and Mismatch Detection**: Automatic validation of computed vs declared totals
  - Compares computed purchase totals against declared statement totals
  - Configurable difference tolerance (`ParserOptions.DifferenceTolerance`)
  - Detailed logging of mismatches with transaction breakdown
  - Transaction inclusion status tracking
  - Files: `ParseResult.cs`, `TransactionExtractor.cs`

- **Serilog File Logging**: Comprehensive logging to console and file
  - Daily rolling log files with 30-day retention
  - Color-coded console output
  - Structured logging with timestamps and log levels
  - Configurable log levels via `appsettings.json`
  - Output template: `[yyyy-MM-dd HH:mm:ss.fff] [Level] Message`
  - Files: `LogMessages.cs`, `Program.cs`, `appsettings.json`
  - Documentation: `docs/features/SERILOG_FILE_LOGGING.md`, `docs/features/LOGGING_QUICK_REFERENCE.md`

- **ParseContext Pattern**: Structured input for transaction parsing
  - Encapsulates parsing input parameters (Text, FileName)
  - Replaced raw string parameter with context object
  - Improved extensibility for future parsing options
  - Files: `ParseContext.cs`, `ITransactionParser.cs`
  - Documentation: `docs/development/PARSECONTEXT_REFACTORING.md`

- **Year-based Filtering**: Process only statements from specified years
  - Configurable via `AppOptions.Years` array
  - Filters by YYYY prefix in PDF filenames
  - **Note**: Replaced by date range filtering in later version
  - File: `TransactionExtractor.cs`

### Fixed
- **Line Ending Normalization**: Fixed mixed line ending issues in PDF extraction
  - PDF extraction now normalizes all line endings to `\n` (Unix LF)
  - Prevents parsing failures caused by mixed `\r\n`, `\n`, and `\r`
  - Fixes Visual Studio warnings about mixed line endings
  - Resolved missing Feb 29 transaction issue
  - File: `CtfsMastercarrdPdfTextExtractor.cs`
  - Documentation: `docs/development/LINE_ENDING_NORMALIZATION_FIX.md`

- **Leap Year Date Parsing**: Fixed Feb 29 transaction parsing in non-leap years
  - Added automatic fallback to previous year when date is invalid
  - Handles Feb 29 transactions from 2024 when application runs in 2025+
  - Prevents `ArgumentOutOfRangeException` for leap year dates
  - Resolved missing transaction and total mismatch issues
  - File: `CtfsMastercardTransactionParser.cs` (TryParseMonthDay method)
  - Documentation: `docs/development/LEAP_YEAR_FIX.md`

### Removed
- **Checksum-based Identification**: Replaced with filename-based identification
  - Removed `Checksum` property from `ParseResult` model
  - Removed `LogChecksum` from `LogMessages`
  - Filename is now primary identifier for tracking and logging
  - Files: `ParseResult.cs`, `LogMessages.cs`

- **Monthly CSV Output Files**: Removed per-month transaction file generation
  - Replaced with single consolidated output file
  - Simpler output structure and file management

### Documentation
- **Comprehensive Documentation**: Added extensive feature and development documentation
  - Feature documentation in `docs/features/`
  - Development documentation in `docs/development/`
  - Classification guide: `docs/CLASSIFICATION_GUIDE.md`
  - GitHub Copilot instructions for coding standards: `.github/copilot/instructions.md`
  - README with architecture, usage, and contributing guidelines

### Technical
- **.NET 10**: Project targeting .NET 10
- **C# 14.0**: Using latest C# language features
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection
- **Configuration**: Microsoft.Extensions.Configuration with appsettings.json
- **iText 9.4.0**: PDF text extraction library
- **Serilog**: Structured logging framework
- **MSTest**: Unit testing framework

### Testing
- **Comprehensive Test Suite**: Unit and integration tests
  - Transaction parsing tests
  - Edge case handling tests
  - Real statement validation tests
  - Transaction exclusion pattern tests
  - Supplemental details filtering tests
  - 28+ passing tests, 4 skipped (future features)
  - Files: `TransactionParserTests.cs`, `TransactionParserEdgeCasesTests.cs`
  - Files: `TransactionExclusionTests.cs`, `ParseResultTests.cs`, `RealStatementTests.cs`
  - Files: `SupplementalDetailsTests.cs`, `TriangleStatement2025Oct21Tests.cs`

---

## Project Information

**Repository**: https://github.com/hellfirehd/DKW.TransactionExtractor  
**License**: AGPL License  
**Framework**: .NET 10  
**Language**: C# 14.0  
**Created**: 2025-11-25

## Change Categories

This changelog uses the following change categories:
- **Added**: New features and functionality
- **Changed**: Changes in existing functionality
- **Deprecated**: Soon-to-be removed features
- **Removed**: Removed features and functionality
- **Fixed**: Bug fixes and corrections
- **Security**: Security vulnerability fixes
- **Documentation**: Documentation updates
- **Technical**: Technical infrastructure changes
- **Testing**: Test suite updates

## Breaking Changes

Breaking changes are marked with **Breaking Change** in the description and include:
- Configuration format changes
- API signature changes
- Removed functionality

## Migration Notes

### Migrating from Years to Date Range (Unreleased)
**Old configuration:**
```json
{
  "AppOptions": {
    "Years": [2024, 2025]
  }
}
```

**New configuration:**
```json
{
  "AppOptions": {
    "StartDate": "2024-01-01",
    "EndDate": null
  }
}
```

- `StartDate` is inclusive (>=)
- `EndDate` is exclusive (<)
- Both are nullable - omit for no filtering on that bound
- File naming must use YYYY-MM-DD format at start of filename
