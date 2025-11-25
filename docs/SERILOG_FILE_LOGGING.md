# Serilog File Logging Configuration

## Overview
Configured Serilog to write logs to both Console and File sinks, making it easy to capture and share production log output for debugging and analysis.

## Changes Made

### 1. **Added Serilog.Sinks.File Package** (`DKW.TransactionExtractor.csproj`)
```xml
<PackageReference Include="Serilog.Sinks.File" Version="6.0.0" />
```

### 2. **Updated Program.cs with File Sink**
```csharp
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        path: "logs/transaction-extractor-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();
```

### 3. **Added Serilog Configuration** (`appsettings.json`)
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    }
  }
}
```

## Log File Configuration

### **File Location**
- **Directory**: `logs/` (relative to executable)
- **Filename Pattern**: `transaction-extractor-YYYYMMDD.log`
- **Example**: `transaction-extractor-20250121.log`

### **Rolling Strategy**
- **Rolling Interval**: Daily
- **File Creation**: New log file created each day at midnight
- **Retention**: Automatically deletes logs older than 30 days

### **Log Format**
```
[2025-01-21 21:39:51.123] [INF] Starting Transaction Extractor application
[2025-01-21 21:39:51.456] [INF] Loaded 4 exclusion pattern(s) for transaction filtering
[2025-01-21 21:39:51.789] [INF] Found 34 PDF file(s) matching configured years
[2025-01-21 21:39:52.012] [WRN] Unmatched transaction text at line 112: Feb 29 Feb 29 APPLE.COM/BILL 866-712-7753 ON 3.35
[2025-01-21 21:39:52.234] [ERR] Failed to parse PDF file: sample.pdf
```

**Format Fields**:
- **Timestamp**: `[yyyy-MM-dd HH:mm:ss.fff]` with milliseconds
- **Level**: `[INF]`, `[WRN]`, `[ERR]`, `[FTL]`, `[DBG]`
- **Message**: Structured log message with properties
- **Exception**: Stack trace (if present)

## Log Levels

### **Default Levels**
- **Information**: Application events, transaction processing
- **Warning**: Mismatches, unmatched transactions
- **Error**: PDF extraction failures, configuration errors
- **Fatal**: Unhandled exceptions, application crashes

### **Override Levels**
- **Microsoft.\***: Warning (reduces verbosity from framework)
- **System.\***: Warning (reduces verbosity from system libraries)

## Usage

### **Running the Application**
```bash
cd DKW.TransactionExtractor
dotnet run
```

### **Output**
1. **Console**: Real-time log output (color-coded)
2. **File**: `logs/transaction-extractor-20250121.log` (persistent)

### **Finding Log Files**
```bash
# Windows
dir logs\*.log

# List today's log
dir logs\transaction-extractor-20250121.log

# View log content
type logs\transaction-extractor-20250121.log
```

### **Sharing Logs for Debugging**
1. Navigate to the `logs/` directory
2. Find the log file for the run date
3. Copy or share the entire file for analysis

## Configuration Options

### **Change Log Level** (appsettings.json)
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug"  // Change to: Verbose, Debug, Information, Warning, Error, Fatal
    }
  }
}
```

### **Change Retention Period** (Program.cs)
```csharp
.WriteTo.File(
    path: "logs/transaction-extractor-.log",
    rollingInterval: RollingInterval.Day,
    retainedFileCountLimit: 90  // Keep 90 days of logs
)
```

### **Change Log Location** (Program.cs)
```csharp
.WriteTo.File(
    path: "C:\\Logs\\TransactionExtractor\\app-.log",  // Absolute path
    // OR
    path: "../logs/app-.log",  // Relative path
    rollingInterval: RollingInterval.Day
)
```

### **Change Rolling Interval** (Program.cs)
```csharp
rollingInterval: RollingInterval.Hour     // Hourly: app-2025012121.log
rollingInterval: RollingInterval.Day      // Daily: app-20250121.log
rollingInterval: RollingInterval.Month    // Monthly: app-202501.log
rollingInterval: RollingInterval.Year     // Yearly: app-2025.log
rollingInterval: RollingInterval.Infinite // Single file: app.log
```

## Benefits

### ? **Persistent Logs**
- Logs saved to disk for later analysis
- Easy to share with others for debugging

### ? **Automatic Management**
- Daily rotation prevents large file sizes
- Automatic cleanup after 30 days

### ? **Detailed Timestamps**
- Millisecond precision for timing analysis
- Easy to correlate events

### ? **Structured Logging**
- Properties preserved in log output
- Easy to search and filter

### ? **Dual Output**
- Console: Real-time monitoring
- File: Permanent record

## Troubleshooting

### **Logs Directory Not Created**
The `logs/` directory is created automatically on first run. If it doesn't exist:
- Check file system permissions
- Verify the application has write access to the directory

### **Log Files Not Rotating**
- Verify the application is restarted across midnight
- Check `rollingInterval` setting in Program.cs

### **Disk Space Issues**
- Reduce `retainedFileCountLimit` (default: 30 days)
- Use `RollingInterval.Hour` for high-volume logging with shorter retention

### **Missing Logs**
- Check `MinimumLevel` in appsettings.json
- Verify `Log.CloseAndFlush()` is called in finally block
- Ensure application isn't crashing before logs are written

## Example Production Run

After running the application, you'll find a log file like this:

**File**: `logs/transaction-extractor-20250121.log`

```
[2025-01-21 21:39:51.000] [INF] Starting Transaction Extractor application
[2025-01-21 21:39:51.100] [INF] Loaded 4 exclusion pattern(s) for transaction filtering
[2025-01-21 21:39:51.200] [INF] Found 34 PDF file(s) matching configured years in D:\source\...
[2025-01-21 21:39:51.300] [INF] Processed File: 2024-03-21-Triangle-WorldEliteMastercard.pdf
[2025-01-21 21:39:51.400] [INF] Declared Purchases: 1609.11
[2025-01-21 21:39:51.500] [INF] Computed Purchases: 1609.11
[2025-01-21 21:39:51.600] [INF] Transaction Count: 47
[2025-01-21 21:39:51.700] [INF] Transaction Extractor completed successfully
```

You can then share this file for debugging, analysis, or record-keeping purposes.

## Advanced Configuration

### **JSON Structured Logging**
For machine-readable logs:
```csharp
.WriteTo.File(
    new Serilog.Formatting.Json.JsonFormatter(),
    path: "logs/transaction-extractor-.json",
    rollingInterval: RollingInterval.Day
)
```

### **Separate Files by Log Level**
```csharp
.WriteTo.Logger(lc => lc
    .Filter.ByIncludingOnly(evt => evt.Level == LogEventLevel.Error)
    .WriteTo.File("logs/errors-.log", rollingInterval: RollingInterval.Day))
.WriteTo.Logger(lc => lc
    .Filter.ByIncludingOnly(evt => evt.Level == LogEventLevel.Warning)
    .WriteTo.File("logs/warnings-.log", rollingInterval: RollingInterval.Day))
```

### **Conditional File Logging**
Only log to file in production:
```csharp
var loggerConfig = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .WriteTo.Console();

#if RELEASE
loggerConfig.WriteTo.File(
    path: "logs/transaction-extractor-.log",
    rollingInterval: RollingInterval.Day);
#endif

Log.Logger = loggerConfig.CreateLogger();
```
