using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using DKW.TransactionExtractor.Classification;
using DKW.TransactionExtractor.Formatting;

namespace DKW.TransactionExtractor;

internal class TransactionExtractor(
    ILogger<TransactionExtractor> logger,
    IOptions<AppOptions> options,
    ITransactionParser parser,
    IPdfTextExtractor pdfExtractor,
    ITransactionClassifier classifier,
    ITransactionFormatter formatter)
{
    private readonly ILogger<TransactionExtractor> _logger = logger;
    private readonly AppOptions _appConfig = options.Value;
    private readonly ITransactionParser _parser = parser;
    private readonly IPdfTextExtractor _pdfExtractor = pdfExtractor;
    private readonly ITransactionClassifier _classifier = classifier;
    private readonly ITransactionFormatter _formatter = formatter;

    public void Run()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.Clear();

        // Get the folder path from configuration
        var folderPath = _appConfig.FolderPath;

        if (String.IsNullOrEmpty(folderPath))
        {
            LogMessages.LogMissingFolderPath(_logger);
            return;
        }

        if (!Directory.Exists(folderPath))
        {
            LogMessages.LogFolderDoesNotExist(_logger, folderPath);
            return;
        }

        // Get the years to filter from configuration
        var years = _appConfig.Years;

        if (years == null || years.Length == 0)
        {
            LogMessages.LogNoYearsConfigured(_logger);
            return;
        }

        // Ensure output directory exists
        if (!Directory.Exists(_appConfig.OutputPath))
        {
            Directory.CreateDirectory(_appConfig.OutputPath);
        }

        // Get all PDF files in the folder
        var pdfFiles = Directory.GetFiles(folderPath, "*.pdf");

        // Filter PDF files by year prefix (YYYY-MM-DD format)
        var filteredFiles = pdfFiles.Where(file =>
        {
            var fileName = Path.GetFileName(file);
            if (fileName.Length >= 4 && Int32.TryParse(fileName.AsSpan(0, 4), out var fileYear))
            {
                return years.Contains(fileYear);
            }

            return false;
        }).ToArray();

        LogMessages.LogFoundFiles(_logger, filteredFiles.Length, folderPath);

        // Aggregate classified transactions for entire run
        var allClassified = new List<DKW.TransactionExtractor.Models.ClassifiedTransaction>();

        // Process each statement: extract -> parse -> classify -> format
        foreach (var pdfFile in filteredFiles)
        {
            var shouldExit = ProcessStatement(pdfFile, allClassified);
            if (shouldExit)
            {
                _logger.LogInformation("Processing stopped by user request.");
                break;
            }
        }

        // After processing all statements, write a single output file if any transactions
        if (allClassified.Count > 0)
        {
            // Filter out uncategorized transactions if configured
            var transactionsToOutput = _appConfig.OutputUncategorized 
                ? allClassified 
                : allClassified.Where(ct => !String.Equals(ct.CategoryId, "uncategorized", StringComparison.OrdinalIgnoreCase)).ToList();

            if (transactionsToOutput.Count > 0)
            {
                try
                {
                    // Generate category summaries from filtered transactions
                    var categorySummaries = GenerateCategorySummaries(transactionsToOutput);
                    
                    // Create output object
                    var output = new Models.TransactionOutput
                    {
                        Transactions = transactionsToOutput,
                        CategorySummaries = categorySummaries,
                        GeneratedAt = DateTime.Now
                    };
                    
                    // Write output using formatter (formatter decides single or multiple files)
                    var baseOutputPath = Path.Combine(_appConfig.OutputPath, "transactions");
                    _formatter.WriteOutput(output, baseOutputPath);
                    
                    _logger.LogInformation("Wrote {Count} classified transactions to output", transactionsToOutput.Count);
                    if (categorySummaries.Count > 0)
                    {
                        _logger.LogInformation("Generated summary for {CategoryCount} categories", categorySummaries.Count);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to write output");
                }
            }
            else
            {
                _logger.LogInformation("No transactions to output after filtering.");
            }
        }
    }

    private Boolean ProcessStatement(String pdfFile, List<DKW.TransactionExtractor.Models.ClassifiedTransaction> aggregate)
    {
        // Extract text from PDF
        var text = _pdfExtractor.ExtractTextFromPdf(pdfFile);
        var fileName = Path.GetFileName(pdfFile);

        // Write extracted text to file if enabled
        if (_appConfig.WriteExtractedText)
        {
            var textFilePath = Path.ChangeExtension(pdfFile, ".txt");
            try
            {
                File.WriteAllText(textFilePath, text);
                LogMessages.LogWroteExtractedText(_logger, textFilePath);
            }
            catch (Exception ex)
            {
                LogMessages.LogFailedToWriteExtractedText(_logger, ex, textFilePath);
            }
        }

        // Parse transactions
        var parseContext = new Models.ParseContext
        {
            Text = text,
            FileName = fileName
        };

        var parseResult = _parser.Parse(parseContext);
        parseResult.FilePath = pdfFile;

        // Output parsing summary
        LogMessages.LogFilename(_logger, parseResult.FileName);
        LogMessages.LogDeclaredPurchases(_logger, parseResult.DeclaredPurchasesTotal);
        LogMessages.LogComputedPurchases(_logger, parseResult.ComputedPurchasesTotal);
        LogMessages.LogTransactionsCount(_logger, parseResult.Transactions.Count);
        if (parseResult.ExcludedCount > 0)
        {
            LogMessages.LogExcludedTransactions(_logger, parseResult.ExcludedCount);
        }

        if (!parseResult.IsMatch)
        {
            LogMessages.LogMismatchDetails(_logger, parseResult.DeclaredPurchasesTotal, parseResult.ComputedPurchasesTotal, parseResult.Difference);
            LogMessages.LogInvestigate(_logger);
            foreach (var transaction in parseResult.Transactions)
            {
                _logger.LogTransactionDetail(transaction.TransactionDate, transaction.Description, transaction.Amount, transaction.InclusionStatus);
            }
        }

        // Classify transactions
        _logger.LogInformation("Classifying {Count} transactions...", parseResult.Transactions.Count);
        var classificationResult = _classifier.ClassifyTransactions(parseResult.Transactions);

        // Attach statement date to each transaction and add to aggregate
        foreach (var ct in classificationResult.ClassifiedTransactions)
        {
            ct.Transaction.StatementDate = parseResult.StatementDate;
            aggregate.Add(ct);
        }

        if (!classificationResult.RequestedEarlyExit)
        {
            _logger.LogInformation("Completed processing {FileName}", fileName);
            Console.WriteLine();
        }

        return classificationResult.RequestedEarlyExit;
    }

    private List<Models.CategorySummary> GenerateCategorySummaries(List<Models.ClassifiedTransaction> transactions)
    {
        var summaries = transactions
            .GroupBy(ct => new { ct.CategoryId, ct.CategoryName })
            .Select(g => new Models.CategorySummary
            {
                CategoryId = g.Key.CategoryId,
                CategoryName = g.Key.CategoryName,
                TransactionCount = g.Count(),
                TotalAmount = g.Sum(ct => ct.Transaction.Amount)
            })
            .ToList();

        // Sort alphabetically by category name
        return summaries.OrderBy(s => s.CategoryName).ToList();
    }
}
