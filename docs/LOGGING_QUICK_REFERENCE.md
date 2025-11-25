# Serilog File Logging - Quick Reference

## ?? Log File Location
```
DKW.TransactionExtractor/logs/transaction-extractor-YYYYMMDD.log
```

## ?? Quick Start

### Run Application
```bash
cd DKW.TransactionExtractor
dotnet run
```

### Find Today's Log
```bash
# Windows
dir logs\transaction-extractor-20250121.log
type logs\transaction-extractor-20250121.log

# PowerShell
Get-Content logs\transaction-extractor-20250121.log

# View last 50 lines
Get-Content logs\transaction-extractor-20250121.log -Tail 50
```

## ?? Configuration

### Change Log Level (appsettings.json)
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug"  // Options: Verbose, Debug, Information, Warning, Error, Fatal
    }
  }
}
```

### Change Retention (Program.cs, line ~17)
```csharp
retainedFileCountLimit: 30  // Days to keep (default: 30)
```

## ?? Log Format
```
[2025-01-21 21:39:51.123] [INF] Message here
 ?? Timestamp with ms     ?? Level  ?? Log message
```

**Levels**: `[VRB]` `[DBG]` `[INF]` `[WRN]` `[ERR]` `[FTL]`

## ?? Features
- ? **Daily rotation**: New file each day at midnight
- ? **30-day retention**: Automatic cleanup
- ? **Dual output**: Console (real-time) + File (persistent)
- ? **Millisecond timestamps**: Precise timing
- ? **Auto-creation**: `logs/` directory created automatically

## ?? Sharing Logs
1. Navigate to `logs/` directory
2. Find the file: `transaction-extractor-YYYYMMDD.log`
3. Copy entire file for analysis

## ?? Common Log Patterns

### Success
```
[INF] Processed File: 2024-03-21-Triangle-WorldEliteMastercard.pdf
[INF] Declared Purchases: 1609.11
[INF] Computed Purchases: 1609.11
[INF] Transaction Count: 47
```

### Warning (Mismatch)
```
[WRN] Unmatched transaction text at line 112: Feb 29 Feb 29 APPLE.COM/BILL...
[WRN] Declared purchases 1609.11 total does NOT match computed total 1605.76
```

### Error
```
[ERR] Failed to parse PDF file: sample.pdf
System.Exception: PDF format not recognized
   at DKW.TransactionExtractor...
```

## ??? Troubleshooting

| Issue | Solution |
|-------|----------|
| No `logs/` folder | Run app once - created automatically |
| No log files | Check permissions, verify app ran successfully |
| Logs too verbose | Change `MinimumLevel` to `Warning` or `Error` |
| Disk full | Reduce `retainedFileCountLimit` |
| Can't find today's log | Check date format: `YYYYMMDD` (20250121) |

## ?? Example Commands

```powershell
# View all logs
dir logs\*.log

# View today's log
Get-Content logs\transaction-extractor-20250121.log

# Search for errors
Select-String -Path logs\*.log -Pattern "\[ERR\]"

# Search for warnings
Select-String -Path logs\*.log -Pattern "\[WRN\]"

# Count warnings in today's log
(Select-String -Path logs\transaction-extractor-20250121.log -Pattern "\[WRN\]").Count

# View last 20 lines
Get-Content logs\transaction-extractor-20250121.log -Tail 20

# Follow log in real-time
Get-Content logs\transaction-extractor-20250121.log -Wait
```

## ?? File Naming Convention

| Date | Filename |
|------|----------|
| Jan 21, 2025 | `transaction-extractor-20250121.log` |
| Jan 22, 2025 | `transaction-extractor-20250122.log` |
| Feb 01, 2025 | `transaction-extractor-20250201.log` |

Old files are automatically deleted after 30 days.
