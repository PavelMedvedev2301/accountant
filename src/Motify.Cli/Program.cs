using System.Diagnostics;
using Motify.Core.Services;

namespace Motify.Cli;

class Program
{
    static int Main(string[] args)
    {
        try
        {
            if (args.Length == 0 || args[0] != "classify")
            {
                ShowHelp();
                return 1;
            }

            var options = ParseArguments(args);
            
            if (options == null)
            {
                ShowHelp();
                return 1;
            }

            Console.WriteLine("Trial Balance Classifier v2.0 (CLI Mode)");
            Console.WriteLine("==========================================\n");

            var stopwatch = Stopwatch.StartNew();

            // Read CSV files
            Console.WriteLine($"Reading previous trial balance: {options.PreviousFile}");
            var csvService = new CsvService();
            var previousAccounts = csvService.ReadTrialBalance(options.PreviousFile);
            Console.WriteLine($"  ✓ Loaded {previousAccounts.Count} accounts\n");

            Console.WriteLine($"Reading current trial balance: {options.CurrentFile}");
            var currentAccounts = csvService.ReadTrialBalance(options.CurrentFile);
            Console.WriteLine($"  ✓ Loaded {currentAccounts.Count} accounts\n");

            // Compare and detect changes
            Console.WriteLine("Analyzing accounts...");
            var memoryService = new MemoryService();
            var configService = new ConfigService();
            var classifier = new EnhancedClassifierService(memoryService, configService);
            
            var results = classifier.ClassifyAccounts(
                previousAccounts, 
                currentAccounts, 
                options.ClientId ?? "DEFAULT");

            var newCount = results.Count(r => r.status == "new");
            var renumberedCount = results.Count(r => r.status == "likely_renumbered");

            Console.WriteLine($"  ✓ Found {newCount} new account(s)");
            Console.WriteLine($"  ✓ Found {renumberedCount} likely renumbered account(s)\n");

            // Write output
            Console.WriteLine($"Writing results to: {options.OutputFile}");
            csvService.WriteClassificationResults(options.OutputFile, results);
            Console.WriteLine($"  ✓ Successfully written {results.Count} record(s)\n");

            stopwatch.Stop();
            Console.WriteLine($"Completed in {stopwatch.ElapsedMilliseconds}ms");

            return 0;
        }
        catch (FileNotFoundException ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error: {ex.Message}");
            Console.ResetColor();
            return 1;
        }
        catch (InvalidDataException ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Data validation error:\n{ex.Message}");
            Console.ResetColor();
            return 1;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Unexpected error: {ex.Message}");
            Console.WriteLine($"\nStack trace:\n{ex.StackTrace}");
            Console.ResetColor();
            return 1;
        }
    }

    private static CommandOptions? ParseArguments(string[] args)
    {
        var options = new CommandOptions();

        for (int i = 1; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--prev":
                    if (i + 1 < args.Length)
                    {
                        options.PreviousFile = args[++i];
                    }
                    break;
                case "--curr":
                    if (i + 1 < args.Length)
                    {
                        options.CurrentFile = args[++i];
                    }
                    break;
                case "--out":
                    if (i + 1 < args.Length)
                    {
                        options.OutputFile = args[++i];
                    }
                    break;
                case "--client":
                    if (i + 1 < args.Length)
                    {
                        options.ClientId = args[++i];
                    }
                    break;
                default:
                    Console.WriteLine($"Unknown option: {args[i]}");
                    return null;
            }
        }

        if (string.IsNullOrEmpty(options.PreviousFile) ||
            string.IsNullOrEmpty(options.CurrentFile) ||
            string.IsNullOrEmpty(options.OutputFile))
        {
            Console.WriteLine("Error: Missing required arguments.\n");
            return null;
        }

        return options;
    }

    private static void ShowHelp()
    {
        Console.WriteLine("Trial Balance Classifier CLI v2.0");
        Console.WriteLine("\nUsage:");
        Console.WriteLine("  tbx classify --prev <previous_file> --curr <current_file> --out <output_file> [--client <client_id>]\n");
        Console.WriteLine("Options:");
        Console.WriteLine("  --prev      Path to previous quarter trial balance CSV file");
        Console.WriteLine("  --curr      Path to current quarter trial balance CSV file");
        Console.WriteLine("  --out       Path to output CSV file");
        Console.WriteLine("  --client    Client ID for memory management (optional)\n");
        Console.WriteLine("Example:");
        Console.WriteLine("  tbx classify --prev TB_Previous.csv --curr TB_Current.csv --out NewAccounts.csv --client ACME");
    }

    private class CommandOptions
    {
        public string PreviousFile { get; set; } = string.Empty;
        public string CurrentFile { get; set; } = string.Empty;
        public string OutputFile { get; set; } = string.Empty;
        public string? ClientId { get; set; }
    }
}

