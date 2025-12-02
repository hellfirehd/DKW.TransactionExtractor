using DKW.TransactionExtractor.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.RegularExpressions;

namespace DKW.TransactionExtractor.Providers.CTFS;

public partial class CtfsMastercardTransactionParser : ITransactionParser
{
    private readonly Decimal _differenceTolerance;
    private readonly ILogger<CtfsMastercardTransactionParser> _logger;
    private readonly ITransactionFilter _transactionFilter;

    private const String CommaChar = ",";
    private const String DollarChar = "$";
    private const String OpenParenChar = "(";
    private const String CloseParenChar = ")";

    // Keep parameterless constructor for tests; create default options with default settings and use a logger factory to create a null logger
    internal CtfsMastercardTransactionParser() : this(
        Options.Create(new ParserOptions { DifferenceTolerance = 0.01m }),
        Microsoft.Extensions.Logging.Abstractions.NullLogger<CtfsMastercardTransactionParser>.Instance,
        new DefaultTransactionFilter(Options.Create(new ParserOptions { DifferenceTolerance = 0.01m }),
            Microsoft.Extensions.Logging.Abstractions.NullLogger<DefaultTransactionFilter>.Instance))
    {
    }

    // New constructor required by DI: IOptions<ParserSettings> and ILogger<TransactionParser> (required)
    public CtfsMastercardTransactionParser(IOptions<ParserOptions> options, ILogger<CtfsMastercardTransactionParser> logger, ITransactionFilter transactionFilter)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(transactionFilter);

        _differenceTolerance = options.Value?.DifferenceTolerance ?? 0.01m;
        _logger = logger;
        _transactionFilter = transactionFilter;
    }

    public ParseResult Parse(ParseContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var result = new ParseResult
        {
            FileName = context.FileName,
            StatementDate = DateTime.MinValue
        };

        if (String.IsNullOrWhiteSpace(context.Text))
        {
            return result;
        }

        // Split on newline (text should already be normalized by the PDF extractor)
        var rawLines = context.Text.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in rawLines)
        {
            var match = PurchasesTotalRegex().Match(line);
            if (match.Success)
            {
                var raw = match.Groups["amount"].Value.Replace(CommaChar, "").Replace(DollarChar, "");
                if (Decimal.TryParse(raw, NumberStyles.Number | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var declared))
                {
                    result.DeclaredPurchasesTotal = declared;
                }
            }

            match = StatementDateRegex().Match(line);
            if (match.Success)
            {
                var statementDate = DateTime.MinValue;
                // statement date detection moved to helper
                if (TryExtractStatementDate(line, ref statementDate))
                {
                    result.StatementDate = statementDate;
                }
            }

            if (result.StatementDate > DateTime.MinValue && result.DeclaredPurchasesTotal > Decimal.Zero)
            {
                break;
            }
        }

        ParseTransactions(rawLines, result);

        // Determine inclusion status for each transaction
        foreach (var transaction in result.Transactions)
        {
            if (transaction.Amount <= 0)
            {
                // Negative amounts (payments, refunds) are not purchases
                transaction.InclusionStatus = TransactionInclusionStatus.Exclude;
            }
            else if (_transactionFilter.ShouldExcludeFromPurchasesTotal(transaction))
            {
                // Positive amounts that match exclusion patterns (fees, interest)
                transaction.InclusionStatus = TransactionInclusionStatus.Exclude;
            }
            else
            {
                // Positive amounts that don't match exclusion patterns
                transaction.InclusionStatus = TransactionInclusionStatus.Include;
            }
        }

        var includedTransactions = result.Transactions
            .Where(t => t.InclusionStatus == TransactionInclusionStatus.Include)
            .ToList();

        result.ComputedPurchasesTotal = includedTransactions.Sum(t => t.Amount);
        result.Difference = result.DeclaredPurchasesTotal - result.ComputedPurchasesTotal;
        result.IsMatch = Math.Abs(result.Difference) < _differenceTolerance;

        result.ExcludedCount = result.Transactions.Count(t => t.InclusionStatus == TransactionInclusionStatus.Exclude && t.Amount > 0);

        return result;
    }

    private void ParseTransactions(String[] rawLines, ParseResult result)
    {
        for (var i = 0; i < rawLines.Length; i++)
        {
            var rawLine = rawLines[i].Trim();

            // Check if we're entering the supplemental details section
            if (DetailsSectionStartRegex().IsMatch(rawLine))
            {
                // No need to process the remaining lines. Anything after this point
                // that looks like a transaction is actually Purchase Detail.
                break;
            }

            if (!IsStartOfTransaction(rawLine))
            {
                continue;
            }

            var (combined, lastIndex) = CombineLines(rawLines, i);

            // Record the start line number (1-based)
            var startLineNumber = i + 1;

            i = lastIndex;

            if (TryParseTransactionFromCombined(combined, result.StatementDate, out var transaction))
            {
                // attach the start line number
                transaction.StartLineNumber = startLineNumber;
                result.Transactions.Add(transaction);
            }
            else
            {
                var warning = new ParseWarning
                {
                    LineNumber = startLineNumber,
                    Message = "Unmatched transaction text",
                    RawText = combined
                };

                result.Warnings.Add(warning);

                LogMessages.LogUnmatchedTransactionText(_logger, startLineNumber, combined);
            }
        }
    }

    // Try extract statement year from a line like: "Statement date: October 21, 2025"
    private Boolean TryExtractStatementDate(String line, ref DateTime statementDate)
    {
        if (String.IsNullOrWhiteSpace(line))
        {
            return false;
        }

        var m = StatementDateFullRegex().Match(line);
        if (!m.Success)
        {
            return false;
        }

        var extracted = line.Replace("statement date", "", StringComparison.OrdinalIgnoreCase).Replace(":", "", StringComparison.OrdinalIgnoreCase).Trim();

        if (DateTime.TryParse(extracted, out statementDate))
        {
            return true;
        }

        LogMessages.LogFailedToParseStatementDate(_logger, line);

        return false;
    }

    // Combine lines starting at index 'start' until a full transaction can be matched or a new transaction starts.
    // Returns the combined string and the index of the last line consumed.
    private static (String combined, Int32 lastIndex) CombineLines(String[] lines, Int32 start)
    {
        var combined = lines[start].Trim();
        var j = start;

        while (true)
        {
            var norm = WhitespaceNormalizationRegex().Replace(combined, " ").Trim();

            // If the normalized text looks like a full transaction line, verify the amount portion is likely the real amount.
            var txMatch = TransactionLineRegex().Match(norm);
            if (txMatch.Success)
            {
                // If the matched amount is a short integer-like token (no decimal/comma/currency) it may actually be part of a multiline description.
                var amountGroup = txMatch.Groups["amount"].Value ?? String.Empty;
                var looksLikeBareInteger = !amountGroup.Contains('.') && !amountGroup.Contains(',') && !amountGroup.Contains('$');

                if (looksLikeBareInteger && j + 1 < lines.Length)
                {
                    var next = lines[j + 1].Trim();

                    // If the next line is not the start of a new transaction and is not an amount-only line,
                    // then the bare integer at the end of the current combined text is likely part of the description.
                    if (!IsStartOfTransaction(next) && !AmountOnlyRegex().IsMatch(next))
                    {
                        combined += " " + next;
                        j++;
                        continue;
                    }
                }

                // Otherwise accept the match as a full transaction line.
                break;
            }

            if (j + 1 >= lines.Length)
            {
                break;
            }

            var nextLine = lines[j + 1].Trim();

            if (IsStartOfTransaction(nextLine))
            {
                break;
            }

            if (AmountOnlyRegex().IsMatch(nextLine))
            {
                combined += " " + nextLine;
                j++;
                break;
            }

            combined += " " + nextLine;
            j++;
        }

        return (combined, j);
    }

    private static Boolean TryParseTransactionFromCombined(String combined, DateTime statementDate, [NotNullWhen(true)] out Transaction? transaction)
    {
        transaction = null;

        var norm = WhitespaceNormalizationRegex().Replace(combined, " ").Trim();
        var match = TransactionLineRegex().Match(norm);
        if (!match.Success)
        {
            return false;
        }

        var date1 = match.Groups["date1"].Value;
        var date2 = match.Groups["date2"].Value;
        var desc = match.Groups["desc"].Value;
        var amountStr = match.Groups["amount"].Value.Replace(CommaChar, "").Replace(DollarChar, "");

        var negative = false;
        if (amountStr.StartsWith(OpenParenChar) && amountStr.EndsWith(CloseParenChar))
        {
            negative = true;
            amountStr = amountStr[1..^1];
        }

        if (!TryParseMonthDay(date1, statementDate.Year, out var transDate))
        {
            return false;
        }

        var postedDate = DateTime.MinValue;
        TryParseMonthDay(date2, statementDate.Year, out postedDate);

        if (transDate != DateTime.MinValue && transDate.Month > statementDate.Month && transDate.Year > DateTime.MinValue.Year)
        {
            transDate = transDate.AddYears(-1);
        }

        if (postedDate != DateTime.MinValue && postedDate.Month > statementDate.Month && postedDate.Year > DateTime.MinValue.Year)
        {
            postedDate = postedDate.AddYears(-1);
        }

        if (!Decimal.TryParse(amountStr, NumberStyles.Number | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var amount))
        {
            return false;
        }

        if (negative)
        {
            amount = -amount;
        }

        transaction = new Transaction
        {
            StatementDate = statementDate,
            TransactionDate = transDate,
            PostedDate = postedDate,
            Description = WhitespaceNormalizationRegex().Replace(desc, " ").Trim(),
            Amount = amount,
            RawText = norm
        };

        return true;
    }

    private static Boolean IsStartOfTransaction(String line)
    {
        if (String.IsNullOrWhiteSpace(line))
        {
            return false;
        }

        return TransactionStartRegex().IsMatch(line);
    }

    private static Boolean TryParseMonthDay(String monthDay, Int32 year, out DateTime result)
    {
        result = DateTime.MinValue;
        if (String.IsNullOrWhiteSpace(monthDay))
        {
            return false;
        }

        var tokens = monthDay.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length != 2)
        {
            return false;
        }

        var monthPart = LookupMonth(tokens[0]);
        if (monthPart == 0)
        {
            return false;
        }

        if (!Int32.TryParse(tokens[1], out var dayPart))
        {
            return false;
        }

        // Try current year first
        if (TryCreateDate(year, monthPart, dayPart, out result))
        {
            return true;
        }

        // Fallback to previous year (for transactions at year boundary)
        return TryCreateDate(year - 1, monthPart, dayPart, out result);
    }

    private static Boolean TryCreateDate(Int32 year, Int32 month, Int32 day, out DateTime result)
    {
        try
        {
            result = new DateTime(year, month, day);
            return true;
        }
        catch (ArgumentOutOfRangeException)
        {
            result = DateTime.MinValue;
            return false;
        }
    }

    private static Int32 LookupMonth(String monthName)
    {
        if (String.IsNullOrWhiteSpace(monthName))
        {
            return 0;
        }

        var token = monthName.Trim();
        if (token.Length >= 3)
        {
            token = token[..3];
        }

        return token.ToLowerInvariant() switch
        {
            "jan" => 1,
            "feb" => 2,
            "mar" => 3,
            "apr" => 4,
            "may" => 5,
            "jun" => 6,
            "jul" => 7,
            "aug" => 8,
            "sep" => 9,
            "oct" => 10,
            "nov" => 11,
            "dec" => 12,
            _ => 0,
        };
    }

    /// <summary>
    /// Matches a complete transaction line with two dates, description, and amount.
    /// </summary>
    /// <example>
    /// Matches: "Oct 15 Oct 15 CANADIAN TIRE #123 KELOWNA BC 75.00"
    /// Matches: "Mar 29 Mar 29 APPLE.COM/BILL 866-712-7753 ON 3.35"
    /// Matches: "Nov 01 Nov 02 CIBC BANK PMT/PAIEMENT BCIC (3,463.00)" (parentheses indicate negative)
    /// </example>
    [GeneratedRegex(@"^(?<date1>[A-Za-z]{3}\s+\d{1,2})\s+(?<date2>[A-Za-z]{3}\s+\d{1,2})\s+(?<desc>.+?)\s+(?<amount>\(?[-+]?\$?\d{1,3}(?:,\d{3})*(?:\.\d{2})?\)?)$", RegexOptions.Compiled)]
    private static partial Regex TransactionLineRegex();

    /// <summary>
    /// Matches the statement date header line (partial match to detect presence).
    /// </summary>
    /// <example>
    /// Matches: "Statement date: October 21, 2025"
    /// Matches: "Statement date October 21, 2025"
    /// </example>
    [GeneratedRegex(@"Statement date[:]?", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-US")]
    private static partial Regex StatementDateRegex();

    /// <summary>
    /// Matches and extracts the full statement date with month, day, and year components.
    /// </summary>
    /// <example>
    /// Matches: "Statement date: October 21, 2025" → month="October", day="21", year="2025"
    /// Matches: "Statement date March 15, 2024" → month="March", day="15", year="2024"
    /// </example>
    [GeneratedRegex(@"Statement date[:]?:?[\s]*(?<month>[A-Za-z]+)\s+(?<day>\d{1,2}),\s*(?<year>\d{4})", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-US")]
    private static partial Regex StatementDateFullRegex();

    /// <summary>
    /// Matches the purchases total amount line and extracts the numeric value.
    /// </summary>
    /// <example>
    /// Matches: "Purchases 2,348.84" → amount="2,348.84"
    /// Matches: "Purchases $1,500.00" → amount="$1,500.00"
    /// Matches: "Purchases 100.00" → amount="100.00"
    /// </example>
    [GeneratedRegex(@"Purchases\s+(?<amount>[-+]?\$?[0-9]{1,3}(?:,[0-9]{3})*(?:\.[0-9]+)?)", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-US")]
    private static partial Regex PurchasesTotalRegex();

    /// <summary>
    /// Detects if a line starts with the transaction date pattern (two month-day pairs).
    /// Used to identify the beginning of a transaction line that may span multiple lines.
    /// </summary>
    /// <example>
    /// Matches: "Oct 15 Oct 15 STORE NAME..."
    /// Matches: "Feb 29 Feb 29 APPLE.COM/BILL..."
    /// Does not match: "STORE NAME 123.45" (no date prefix)
    /// </example>
    [GeneratedRegex(@"^[A-Za-z]{3}\s+\d{1,2}\s+[A-Za-z]{3}\s+\d{1,2}\b", RegexOptions.Compiled)]
    private static partial Regex TransactionStartRegex();

    /// <summary>
    /// Matches a line that contains only a monetary amount (with optional currency symbols and separators).
    /// Used to identify amount-only lines when transactions span multiple lines.
    /// </summary>
    /// <example>
    /// Matches: "59.36"
    /// Matches: "$122.08"
    /// Matches: "1,234.56"
    /// Does not match: "36" (requires decimal point)
    /// </example>
    [GeneratedRegex(@"[-+]?\$?[0-9]+(?:[,.]{1})(?:[0-9]+)?$", RegexOptions.Compiled)]
    private static partial Regex AmountOnlyRegex();

    /// <summary>
    /// Matches one or more whitespace characters for normalization purposes.
    /// Used to collapse multiple spaces/tabs/newlines into a single space.
    /// </summary>
    /// <example>
    /// Replaces: "STORE    NAME" → "STORE NAME"
    /// Replaces: "TEXT\t\tTAB" → "TEXT TAB"
    /// </example>
    [GeneratedRegex(@"\s+", RegexOptions.Compiled)]
    private static partial Regex WhitespaceNormalizationRegex();

    /// <summary>
    /// Matches the header line that indicates the start of the supplemental purchase details section.
    /// After this line, all transactions are itemized details that should not be included in the transaction list.
    /// </summary>
    /// <example>
    /// Matches: "Details of your Canadian Tire store purchases"
    /// Matches: "DETAILS OF YOUR CANADIAN TIRE STORE PURCHASES" (case-insensitive)
    /// </example>
    [GeneratedRegex(@"Details\s+of\s+your\s+Canadian\s+Tire\s+store\s+purchases", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex DetailsSectionStartRegex();
}
