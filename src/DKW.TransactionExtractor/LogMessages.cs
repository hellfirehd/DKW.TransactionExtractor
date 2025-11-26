using Microsoft.Extensions.Logging;
using DKW.TransactionExtractor.Models;

namespace DKW.TransactionExtractor;

internal static partial class LogMessages
{
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

    [LoggerMessage(EventId = 1050, Level = LogLevel.Information, Message = "Declared Purchases: {Declared}")]
    public static partial void LogDeclaredPurchases(this ILogger logger, decimal? declared);

    [LoggerMessage(EventId = 1060, Level = LogLevel.Information, Message = "Computed Purchases: {Computed}")]
    public static partial void LogComputedPurchases(this ILogger logger, decimal computed);

    [LoggerMessage(EventId = 1070, Level = LogLevel.Information, Message = "Is Match: {IsMatch}")]
    public static partial void LogIsMatch(this ILogger logger, bool isMatch);

    [LoggerMessage(EventId = 1080, Level = LogLevel.Information, Message = "Transaction Count: {Count}")]
    public static partial void LogTransactionsCount(this ILogger logger, Int32 count);

    [LoggerMessage(EventId = 1100, Level = LogLevel.Warning, Message = "File: {File}")]
    public static partial void LogFile(this ILogger logger, String file);

    [LoggerMessage(EventId = 1110, Level = LogLevel.Warning, Message = "Declared: {Declared} | Computed: {Computed} | Difference: {Difference}")]
    public static partial void LogMismatchDetails(this ILogger logger, decimal? declared, decimal computed, decimal difference);

    [LoggerMessage(EventId = 1120, Level = LogLevel.Warning, Message = "Investigate this file and update parser logic if necessary.")]
    public static partial void LogInvestigate(this ILogger logger);

    [LoggerMessage(EventId = 1130, Level = LogLevel.Debug, Message = "Excluded {ExcludedCount} transaction(s) from purchases total calculation.")]
    public static partial void LogExcludedTransactions(this ILogger logger, Int32 excludedCount);

    [LoggerMessage(EventId = 1140, Level = LogLevel.Information, Message = "Processed File: {FileName}")]
    public static partial void LogFilename(this ILogger logger, String fileName);

    // Parser and filter specific messages (kept from prior edits)
    [LoggerMessage(EventId = 1150, Level = LogLevel.Debug, Message = "Entering supplemental details section at line {Line}")]
    public static partial void LogEnteringSupplementalDetails(this ILogger logger, Int32 line);

    [LoggerMessage(EventId = 1160, Level = LogLevel.Debug, Message = "Skipping supplemental detail transaction at line {Line}: {Description}")]
    public static partial void LogSkippingSupplementalDetailTransaction(this ILogger logger, Int32 line, String description);

    [LoggerMessage(EventId = 1170, Level = LogLevel.Debug, Message = "Skipping unmatched line in details section at line {Line}: {Text}")]
    public static partial void LogSkippingUnmatchedLineInDetails(this ILogger logger, Int32 line, String text);

    [LoggerMessage(EventId = 1180, Level = LogLevel.Warning, Message = "Unmatched transaction text at line {Line}: {Text}")]
    public static partial void LogUnmatchedTransactionText(this ILogger logger, Int32 line, String text);

    [LoggerMessage(EventId = 1190, Level = LogLevel.Warning, Message = "Failed to parse statement date from line: {Line}")]
    public static partial void LogFailedToParseStatementDate(this ILogger logger, String line);

    [LoggerMessage(EventId = 1200, Level = LogLevel.Error, Message = "Invalid exclusion pattern: {Pattern}")]
    public static partial void LogInvalidExclusionPattern(this ILogger logger, String pattern, Exception ex);

    [LoggerMessage(EventId = 1210, Level = LogLevel.Information, Message = "No exclusion patterns configured. All positive-amount transactions will be included in purchases total.")]
    public static partial void LogNoExclusionPatternsConfigured(this ILogger logger);

    [LoggerMessage(EventId = 1220, Level = LogLevel.Information, Message = "Loaded {Count} exclusion pattern(s) for transaction filtering")]
    public static partial void LogLoadedExclusionPatterns(this ILogger logger, Int32 count);

    [LoggerMessage(EventId = 1230, Level = LogLevel.Debug, Message = "Transaction excluded from purchases total: {Description} (Amount: {Amount})")]
    public static partial void LogTransactionExcludedFromPurchasesTotal(this ILogger logger, String description, decimal amount);

    // TransactionExtractor specific messages
    [LoggerMessage(EventId = 1240, Level = LogLevel.Debug, Message = "Wrote extracted text to {TextFile}")]
    public static partial void LogWroteExtractedText(this ILogger logger, String textFile);

    [LoggerMessage(EventId = 1250, Level = LogLevel.Warning, Message = "Failed to write extracted text to {TextFile}")]
    public static partial void LogFailedToWriteExtractedText(this ILogger logger, Exception ex, String textFile);

    [LoggerMessage(EventId = 1260, Level = LogLevel.Information, Message = "{Date:yyyy-MM-dd} | {Description} | {Amount} | Excluded: {Status}")]
    public static partial void LogTransactionDetail(this ILogger logger, DateTime date, String description, decimal amount, TransactionInclusionStatus status);
}
