using Microsoft.Extensions.Configuration;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Microsoft.Extensions.Hosting;
using DKW.TransactionExtractor.Providers.CTFS;
using DKW.TransactionExtractor.EncodingProviders;
using DKW.TransactionExtractor.Classification;
using DKW.TransactionExtractor.Formatting;

namespace DKW.TransactionExtractor;

internal class Program
{
    static void Main(String[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();

        try
        {
            Log.Information("Starting Transaction Extractor application");

            var host = Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.SetBasePath(Directory.GetCurrentDirectory())
                          .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                })
                .ConfigureServices((context, services) =>
                {
                    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                    Encoding.RegisterProvider(new PdfAliasEncodingProvider());

                    services.Configure<AppOptions>(context.Configuration.GetSection(nameof(AppOptions)));
                    services.Configure<ParserOptions>(context.Configuration.GetSection(nameof(ParserOptions)));

                    services.AddSingleton(TimeProvider.System);
                    services.AddSingleton<ITransactionFilter, DefaultTransactionFilter>();
                    services.AddTransient<IPdfTextExtractor, CtfsMastercardPdfTextExtractor>();
                    services.AddTransient<ITransactionParser, CtfsMastercardTransactionParser>();
                    
                    // Classification services
                    services.AddSingleton<ICategoryRepository, CategoryRepository>();
                    services.AddSingleton<ICategoryService, CategoryService>();
                    services.AddTransient<IConsoleInteraction, ConsoleInteractionService>();
                    services.AddTransient<ITransactionClassifier, TransactionClassifier>();
                    
                    // Formatting services - register based on configuration
                    services.AddTransient<ITransactionFormatter>(sp =>
                    {
                        var appOptions = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<AppOptions>>().Value;
                        return appOptions.OutputFormat.ToLowerInvariant() switch
                        {
                            "json" => new JsonFormatter(),
                            _ => new CsvFormatter()
                        };
                    });
                    
                    services.AddTransient<TransactionExtractor>();
                })
                .Build();

            var runner = host.Services.GetRequiredService<TransactionExtractor>();
            runner.Run();

            Log.Information("Transaction Extractor completed successfully");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Unhandled exception occurred");
            throw;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
