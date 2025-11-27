using Microsoft.Extensions.Logging;
using DKW.TransactionExtractor.Models;

namespace DKW.TransactionExtractor;

internal static partial class LogMessages
{
    // ========================================
    // Configuration and Validation (1000-1099)
    // ========================================
    
    [LoggerMessage(EventId = 1000, Level = LogLevel.Error, Message = "Folder path not configured in appsettings.json")]
    public static partial void LogMissingFolderPath(this ILogger logger);

    [LoggerMessage(EventId = 1010, Level = LogLevel.Error, Message = "Folder does not exist: {FolderPath}")]
    public static partial void LogFolderDoesNotExist(this ILogger logger, String folderPath);

    [LoggerMessage(EventId = 1020, Level = LogLevel.Warning, Message = "No date range configured. All PDF files will be processed.")]
    public static partial void LogNoDateRangeConfigured(this ILogger logger);

    [LoggerMessage(EventId = 1025, Level = LogLevel.Information, Message = "Filtering statements: StartDate={StartDate}, EndDate={EndDate}")]
    public static partial void LogDateRangeFilter(this ILogger logger, DateTime? startDate, DateTime? endDate);

    [LoggerMessage(EventId = 1030, Level = LogLevel.Information, Message = "Found {Count} PDF file(s) matching configured date range in {Folder}")]
    public static partial void LogFoundFiles(this ILogger logger, Int32 count, String folder);

    // ========================================
    // Transaction Processing (1100-1199)
    // ========================================
    
    [LoggerMessage(EventId = 1100, Level = LogLevel.Information, Message = "Processing stopped by user request.")]
    public static partial void LogProcessingStoppedByUser(this ILogger logger);

    [LoggerMessage(EventId = 1110, Level = LogLevel.Information, Message = "No transactions to output after filtering.")]
    public static partial void LogNoTransactionsAfterFiltering(this ILogger logger);

    [LoggerMessage(EventId = 1120, Level = LogLevel.Error, Message = "Failed to write output")]
    public static partial void LogFailedToWriteOutput(this ILogger logger, Exception ex);

    [LoggerMessage(EventId = 1130, Level = LogLevel.Information, Message = "Wrote {Count} classified transactions to output")]
    public static partial void LogWroteClassifiedTransactions(this ILogger logger, Int32 count);

    [LoggerMessage(EventId = 1140, Level = LogLevel.Information, Message = "Generated summary for {CategoryCount} categories")]
    public static partial void LogGeneratedCategorySummary(this ILogger logger, Int32 categoryCount);

    [LoggerMessage(EventId = 1150, Level = LogLevel.Debug, Message = "Wrote extracted text to {TextFile}")]
    public static partial void LogWroteExtractedText(this ILogger logger, String textFile);

    [LoggerMessage(EventId = 1160, Level = LogLevel.Warning, Message = "Failed to write extracted text to {TextFile}")]
    public static partial void LogFailedToWriteExtractedText(this ILogger logger, Exception ex, String textFile);

    [LoggerMessage(EventId = 1170, Level = LogLevel.Information, Message = "Processed File: {FileName}")]
    public static partial void LogFilename(this ILogger logger, String fileName);

    [LoggerMessage(EventId = 1180, Level = LogLevel.Information, Message = "Classifying {Count} transactions...")]
    public static partial void LogClassifyingTransactions(this ILogger logger, Int32 count);

    [LoggerMessage(EventId = 1190, Level = LogLevel.Information, Message = "Completed processing {FileName}")]
    public static partial void LogCompletedProcessingFile(this ILogger logger, String fileName);

    // ========================================
    // Parsing and Validation (1200-1299)
    // ========================================
    
    [LoggerMessage(EventId = 1200, Level = LogLevel.Information, Message = "Declared Purchases: {Declared}")]
    public static partial void LogDeclaredPurchases(this ILogger logger, decimal? declared);

    [LoggerMessage(EventId = 1210, Level = LogLevel.Information, Message = "Computed Purchases: {Computed}")]
    public static partial void LogComputedPurchases(this ILogger logger, decimal computed);

    [LoggerMessage(EventId = 1220, Level = LogLevel.Information, Message = "Transaction Count: {Count}")]
    public static partial void LogTransactionsCount(this ILogger logger, Int32 count);

    [LoggerMessage(EventId = 1230, Level = LogLevel.Debug, Message = "Excluded {ExcludedCount} transaction(s) from purchases total calculation.")]
    public static partial void LogExcludedTransactions(this ILogger logger, Int32 excludedCount);

    [LoggerMessage(EventId = 1240, Level = LogLevel.Warning, Message = "Declared: {Declared} | Computed: {Computed} | Difference: {Difference}")]
    public static partial void LogMismatchDetails(this ILogger logger, decimal? declared, decimal computed, decimal difference);

    [LoggerMessage(EventId = 1250, Level = LogLevel.Warning, Message = "Investigate this file and update parser logic if necessary.")]
    public static partial void LogInvestigate(this ILogger logger);

    [LoggerMessage(EventId = 1260, Level = LogLevel.Information, Message = "{Date:yyyy-MM-dd} | {Description} | {Amount} | Excluded: {Status}")]
    public static partial void LogTransactionDetail(this ILogger logger, DateTime date, String description, decimal amount, TransactionInclusionStatus status);

    [LoggerMessage(EventId = 1270, Level = LogLevel.Debug, Message = "Entering supplemental details section at line {Line}")]
    public static partial void LogEnteringSupplementalDetails(this ILogger logger, Int32 line);

    [LoggerMessage(EventId = 1280, Level = LogLevel.Debug, Message = "Skipping supplemental detail transaction at line {Line}: {Description}")]
    public static partial void LogSkippingSupplementalDetailTransaction(this ILogger logger, Int32 line, String description);

    [LoggerMessage(EventId = 1290, Level = LogLevel.Debug, Message = "Skipping unmatched line in details section at line {Line}: {Text}")]
    public static partial void LogSkippingUnmatchedLineInDetails(this ILogger logger, Int32 line, String text);

    [LoggerMessage(EventId = 1300, Level = LogLevel.Warning, Message = "Unmatched transaction text at line {Line}: {Text}")]
    public static partial void LogUnmatchedTransactionText(this ILogger logger, Int32 line, String text);

    [LoggerMessage(EventId = 1310, Level = LogLevel.Warning, Message = "Failed to parse statement date from line: {Line}")]
    public static partial void LogFailedToParseStatementDate(this ILogger logger, String line);

    // ========================================
    // Transaction Filtering (1400-1499)
    // ========================================
    
    [LoggerMessage(EventId = 1400, Level = LogLevel.Error, Message = "Invalid exclusion pattern: {Pattern}")]
    public static partial void LogInvalidExclusionPattern(this ILogger logger, String pattern, Exception ex);

    [LoggerMessage(EventId = 1410, Level = LogLevel.Information, Message = "No exclusion patterns configured. All positive-amount transactions will be included in purchases total.")]
    public static partial void LogNoExclusionPatternsConfigured(this ILogger logger);

    [LoggerMessage(EventId = 1420, Level = LogLevel.Information, Message = "Loaded {Count} exclusion pattern(s) for transaction filtering")]
    public static partial void LogLoadedExclusionPatterns(this ILogger logger, Int32 count);

    [LoggerMessage(EventId = 1430, Level = LogLevel.Debug, Message = "Transaction excluded from purchases total: {Description} (Amount: {Amount})")]
    public static partial void LogTransactionExcludedFromPurchasesTotal(this ILogger logger, String description, decimal amount);

    // ========================================
    // Category Repository (1500-1599)
    // ========================================
    
    [LoggerMessage(EventId = 1500, Level = LogLevel.Warning, Message = "Category configuration file not found at {Path}. Starting with empty configuration.")]
    public static partial void LogCategoryConfigNotFound(this ILogger logger, String path);

    [LoggerMessage(EventId = 1510, Level = LogLevel.Debug, Message = "Loaded {Count} categories from {Path}")]
    public static partial void LogLoadedCategories(this ILogger logger, Int32 count, String path);

    [LoggerMessage(EventId = 1520, Level = LogLevel.Error, Message = "Failed to load category configuration from {Path}")]
    public static partial void LogFailedToLoadCategoryConfig(this ILogger logger, Exception ex, String path);

    [LoggerMessage(EventId = 1530, Level = LogLevel.Information, Message = "Normalizing category ID from '{OldId}' to '{NewId}'")]
    public static partial void LogNormalizingCategoryId(this ILogger logger, String oldId, String newId);

    [LoggerMessage(EventId = 1540, Level = LogLevel.Information, Message = "Category IDs have been normalized and saved")]
    public static partial void LogCategoryIdsNormalized(this ILogger logger);

    [LoggerMessage(EventId = 1550, Level = LogLevel.Information, Message = "Saved {Count} categories to {Path}")]
    public static partial void LogSavedCategories(this ILogger logger, Int32 count, String path);

    [LoggerMessage(EventId = 1560, Level = LogLevel.Error, Message = "Failed to save category configuration to {Path}")]
    public static partial void LogFailedToSaveCategoryConfig(this ILogger logger, Exception ex, String path);

    [LoggerMessage(EventId = 1570, Level = LogLevel.Warning, Message = "Cannot add matcher to category '{CategoryId}': category not found")]
    public static partial void LogCategoryNotFoundForMatcher(this ILogger logger, String categoryId);

    [LoggerMessage(EventId = 1580, Level = LogLevel.Information, Message = "Added new Regex matcher to category '{CategoryId}'")]
    public static partial void LogAddedRegexMatcher(this ILogger logger, String categoryId);

    [LoggerMessage(EventId = 1590, Level = LogLevel.Information, Message = "Merged {MatcherType} values into existing matcher for category '{CategoryId}'")]
    public static partial void LogMergedMatcherValues(this ILogger logger, String matcherType, String categoryId);

    [LoggerMessage(EventId = 1600, Level = LogLevel.Information, Message = "Added new {MatcherType} matcher to category '{CategoryId}'")]
    public static partial void LogAddedNewMatcher(this ILogger logger, String matcherType, String categoryId);

    // ========================================
    // Transaction Classification (1700-1799)
    // ========================================
    
    [LoggerMessage(EventId = 1700, Level = LogLevel.Information, Message = "User requested early exit. Processed {Count} of {Total} transactions.")]
    public static partial void LogUserRequestedEarlyExit(this ILogger logger, Int32 count, Int32 total);

    [LoggerMessage(EventId = 1710, Level = LogLevel.Debug, Message = "Matched transaction '{Description}' to category '{Category}'")]
    public static partial void LogMatchedTransaction(this ILogger logger, String description, String category);

    [LoggerMessage(EventId = 1720, Level = LogLevel.Information, Message = "Created new category '{Category}' with ID '{Id}'")]
    public static partial void LogCreatedNewCategory(this ILogger logger, String category, String id);

    [LoggerMessage(EventId = 1730, Level = LogLevel.Information, Message = "Added {MatcherType} rule to category '{Category}'")]
    public static partial void LogAddedRuleToCategory(this ILogger logger, String matcherType, String category);
}
