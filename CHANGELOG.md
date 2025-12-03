# Changelog

All notable changes to the DKW.TransactionExtractor project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

This project uses **Calendar-Semantic Versioning** in the format `YYYY.MINOR.PATCH`:

- **YYYY**: Year of release (e.g., 2025)
- **MINOR**: Incremented for new features or significant changes
- **PATCH**: Incremented for bug fixes and minor improvements

Versions are automatically managed by [GitVersion](https://gitversion.net/) based on Git tags.

## [Unreleased]

### Added

- **Matcher System Refactoring**: Comprehensive refactoring of the transaction matcher system
  - New `MatcherValue` record type combining pattern value and optional amount constraint
  - Shared `TransactionMatcherBase` abstract class for common matcher logic
  - Amount-based matching for all matcher types (ExactMatch, Contains, Regex)
  - Consolidated validation and amount comparison logic
  - Files: `MatcherValue.cs`, `TransactionMatcherBase.cs`
  - Updated Files: `ExactMatcher.cs`, `ContainsMatcher.cs`, `RegexMatcher.cs`, `MatcherFactory.cs`
  - Tests: `AmountMatcherTests.cs`, `MatcherFactoryTests.cs`, `CategoryConfigTests.cs`
  - Documentation: `docs/development/MATCHER_REFACTORING.md`

- **Transaction Comments**: Add optional notes to transactions during classification
  - New "Add comment" option (option 3) in classification menu
  - Comments stored in `ClassifyTransactionContext` and `ClassifiedTransaction`
  - Comments included in both CSV and JSON output formats
  - Useful for distinguishing purchases at the same merchant (gifts, tax-deductible, etc.)
  - Empty/whitespace comments treated as "no comment"
  - Menu default changed: pressing Enter now skips (option 4) instead of invalid choice
  - Files: `ClassifyTransactionContext.cs`, `ClassifiedTransaction.cs`, `CategorySelectionResult.cs`
  - Files: `ConsoleInteractionService.cs`, `TransactionClassifier.cs`, `CsvFormatter.cs`
  - Documentation: `docs/features/TRANSACTION_COMMENTS.md`

### Changed

- **Matcher Configuration Format**: Simplified and improved matcher parameter structure
  - Changed from `parameters: { values: [...] }` to `parameters: [{ value, amount? }, ...]`
  - Each parameter is now an object with required `value` and optional `amount` properties
  - `amount` constraint supported on all matcher types for precise matching
  - Amounts are compared at 2 decimal place precision
  - **Breaking Change**: Legacy configuration format no longer supported
  - Files: `CategoryMatcher.cs`, `MatcherFactory.cs`, `CategoryRepository.cs`

- **Case Sensitivity Enforcement**: All matchers are now case-insensitive by default
  - Removed `caseSensitive` parameter from all matcher configurations
  - ExactMatcher uses `StringComparison.OrdinalIgnoreCase`
  - ContainsMatcher uses `StringComparison.OrdinalIgnoreCase`
  - RegexMatcher uses `RegexOptions.IgnoreCase`
  - **Breaking Change**: No option to enable case-sensitive matching
  - Files: `ExactMatcher.cs`, `ContainsMatcher.cs`, `RegexMatcher.cs`

- **Smart Merging Enhancement**: Updated merging logic for new parameter structure
  - ExactMatch and Contains: Merge by (value, amount) pair with case-insensitive value comparison
  - Values with different amounts are treated as distinct entries
  - Regex: Still never merged (each pattern is unique)
  - File: `CategoryRepository.cs`

- **Classification Menu**: Reorganized classification menu options
  - Option 3: Changed from "Skip" to "Add comment"
  - Option 4: Now "Skip (leave uncategorized)" with [default] indicator
  - Pressing Enter without input defaults to option 4 (Skip)
  - File: `ConsoleInteractionService.cs`

- **CSV Output Format**: Added `Comment` column after `CategoryName`
  - Header: `...,CategoryName,Comment,InclusionStatus`
  - Empty comments appear as empty fields in CSV
  - File: `CsvFormatter.cs`

- **JSON Output Format**: Added `comment` property to classified transactions
  - Property appears in each transaction object
  - Null for transactions without comments
  - File: `JsonFormatter.cs` (automatic serialization)

### Documentation

- **Matcher Refactoring Guide**: Comprehensive documentation for matcher system changes
  - Overview of refactoring changes and benefits
  - Migration guide for legacy configuration format
  - Use cases for amount-based matching
  - Technical architecture details
  - Testing coverage summary
  - File: `docs/development/MATCHER_REFACTORING.md`

- **Transaction Comments Guide**: Comprehensive documentation for comment feature
  - Use cases and examples
  - Comment flow and UI walkthrough
  - Output format examples (CSV and JSON)
  - Best practices and comment patterns
  - Technical architecture details
  - File: `docs/features/TRANSACTION_COMMENTS.md`

- **Updated Classification Guide**: Consolidated and updated classification documentation
  - Removed duplicate content and consolidated into single guide
  - Updated matcher type documentation with amount parameters
  - Added amount-based matching use cases and examples
  - Migration instructions for legacy format
  - Updated JSON schema examples
  - File: `docs/CLASSIFICATION_GUIDE.md`

- **Updated README**: Enhanced feature descriptions
  - Added amount-based matching to features list
  - Updated category configuration examples
  - Added link to matcher refactoring documentation
  - File: `README.md`

### Testing

- **Comprehensive Test Updates**: All tests passing (194 total)
  - New `AmountMatcherTests`: Tests amount-based matching for all matcher types
  - New `MatcherFactoryTests`: Factory creation tests including amount parameters and real-world scenarios
  - New `CategoryConfigTests`: JSON serialization/deserialization roundtrip tests
  - Updated `TransactionClassifierTests` for new `CategorySelectionResult` signature
  - Tests verify amount comparison logic (2 decimal place rounding)
  - Tests verify case-insensitive enforcement
  - Tests verify invalid pattern handling
  - Files: `AmountMatcherTests.cs`, `MatcherFactoryTests.cs`, `CategoryConfigTests.cs`

### Breaking Changes

1. **Configuration Format Change**: Matcher `parameters` structure has changed
   - Old: `parameters: { values: ["FOO", "BAR"], caseSensitive: false }`
   - New: `parameters: [{ value: "FOO" }, { value: "BAR" }]`
   - Migration required for existing `categories.json` files
   - See [Matcher Refactoring Guide](docs/development/MATCHER_REFACTORING.md) for migration instructions

2. **Case Sensitivity Removed**: All matching is now case-insensitive
   - `caseSensitive` parameter has been removed
   - No option to enable case-sensitive matching
   - Existing case-sensitive matchers will need to be reviewed

3. **Backwards Compatibility**: Legacy configuration format is not supported
   - Old configuration files must be migrated manually
   - No automatic migration is provided

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
