using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Motify.Core.Models;

namespace Motify.Core.Services;

public class CsvService
{
    private readonly CsvConfiguration _config;

    public CsvService()
    {
        _config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            MissingFieldFound = null,
            HeaderValidated = null,
            IgnoreBlankLines = true,
            TrimOptions = TrimOptions.Trim
        };
    }

    public List<TrialBalanceAccount> ReadTrialBalance(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"File not found: {filePath}");
        }

        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, _config);

        // Read header
        csv.Read();
        csv.ReadHeader();
        
        if (csv.HeaderRecord == null)
        {
            throw new InvalidDataException($"No header found in file: {filePath}");
        }

        // Validate required columns
        ValidateRequiredColumns(csv.HeaderRecord, filePath);

        var records = new List<TrialBalanceAccount>();
        var accountCodes = new HashSet<string>();

        while (csv.Read())
        {
            var record = new TrialBalanceAccount
            {
                account_code = csv.GetField<string>("account_code")?.Trim() ?? string.Empty,
                account_name = csv.GetField<string>("account_name")?.Trim() ?? string.Empty
            };

            // Validate required fields
            if (string.IsNullOrWhiteSpace(record.account_code))
            {
                throw new InvalidDataException($"Empty account_code found at row {csv.Parser.Row} in file: {filePath}");
            }

            if (string.IsNullOrWhiteSpace(record.account_name))
            {
                throw new InvalidDataException($"Empty account_name found at row {csv.Parser.Row} in file: {filePath}");
            }

            // Check for duplicate account_code
            if (!accountCodes.Add(record.account_code))
            {
                throw new InvalidDataException($"Duplicate account_code '{record.account_code}' found at row {csv.Parser.Row} in file: {filePath}");
            }

            // Read optional fields
            record.parent_code = csv.GetField<string>("parent_code");
            record.level = csv.GetField<string>("level");
            record.opening_balance = csv.GetField<string>("opening_balance");
            record.debit = csv.GetField<string>("debit");
            record.credit = csv.GetField<string>("credit");
            record.closing_balance = csv.GetField<string>("closing_balance");
            record.currency = csv.GetField<string>("currency");

            records.Add(record);
        }

        return records;
    }

    private void ValidateRequiredColumns(string[] headers, string filePath)
    {
        var requiredColumns = new[] { "account_code", "account_name" };
        var headersLower = headers.Select(h => h.ToLower()).ToList();

        foreach (var required in requiredColumns)
        {
            if (!headersLower.Contains(required))
            {
                throw new InvalidDataException(
                    $"Missing required column '{required}' in file: {filePath}\n\n" +
                    "Required format:\n" +
                    "  account_code,account_name[,parent_code,level,opening_balance,debit,credit,closing_balance,currency]\n\n" +
                    "Example:\n" +
                    "  account_code,account_name,parent_code,opening_balance\n" +
                    "  1000,Cash,0,5000.00\n" +
                    "  1100,Bank Account,1000,15000.00"
                );
            }
        }
    }

    public void WriteNewAccounts(string filePath, List<NewAccountResult> results)
    {
        using var writer = new StreamWriter(filePath);
        using var csv = new CsvWriter(writer, _config);

        csv.WriteRecords(results);
    }

    public void WriteClassificationResults(string filePath, List<ClassificationResult> results)
    {
        using var writer = new StreamWriter(filePath);
        using var csv = new CsvWriter(writer, _config);

        csv.WriteRecords(results);
    }
}



