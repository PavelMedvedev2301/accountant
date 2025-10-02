using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using CsvHelper;
using System.Text.Json;
using Motify.Core.Models;
using Motify.Core.Services;

namespace Motify.Server.Api;

public static class ApiService
{
    private const string API_KEY = "tbx-dev-key-12345";  // TODO: Move to config/env

    public static void MapApiEndpoints(WebApplication app)
    {
        var csvService = new CsvService();
        var memoryService = new MemoryService();
        var configService = new ConfigService();

        // POST /classify
        app.MapPost("/classify", async (HttpContext context) =>
        {
            // API Key validation
            if (!ValidateApiKey(context))
            {
                return Results.Unauthorized();
            }

            try
            {
                var form = await context.Request.ReadFormAsync();
                
                var prevFile = form.Files["prev"];
                var currFile = form.Files["curr"];
                var clientId = form["client_id"].ToString();

                if (prevFile == null || currFile == null || string.IsNullOrEmpty(clientId))
                {
                    return Results.BadRequest(new { error = "Missing required fields: prev, curr, client_id" });
                }

                // Save temporary files
                var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                Directory.CreateDirectory(tempDir);

                var prevPath = Path.Combine(tempDir, "prev.csv");
                var currPath = Path.Combine(tempDir, "curr.csv");

                using (var stream = File.Create(prevPath))
                {
                    await prevFile.CopyToAsync(stream);
                }

                using (var stream = File.Create(currPath))
                {
                    await currFile.CopyToAsync(stream);
                }

                // Process
                var previousAccounts = csvService.ReadTrialBalance(prevPath);
                var currentAccounts = csvService.ReadTrialBalance(currPath);

                var enhancedClassifier = new EnhancedClassifierService(memoryService, configService);
                var results = enhancedClassifier.ClassifyAccounts(previousAccounts, currentAccounts, clientId);

                // Cleanup
                Directory.Delete(tempDir, true);

                // Return format
                var format = context.Request.Query["format"].ToString();
                if (format == "csv")
                {
                    var csvPath = Path.Combine(Path.GetTempPath(), $"results_{Guid.NewGuid()}.csv");
                    csvService.WriteClassificationResults(csvPath, results);
                    var csvBytes = await File.ReadAllBytesAsync(csvPath);
                    File.Delete(csvPath);
                    
                    return Results.File(csvBytes, "text/csv", "NewAccounts.csv");
                }

                return Results.Ok(new { 
                    client_id = clientId,
                    count = results.Count,
                    new_accounts = results.Count(r => r.status == "new"),
                    renumbered_accounts = results.Count(r => r.status == "likely_renumbered"),
                    results = results 
                });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        // POST /memory/update
        app.MapPost("/memory/update", ([FromBody] MemoryMapping mapping) =>
        {
            try
            {
                if (string.IsNullOrEmpty(mapping.client_id) || 
                    string.IsNullOrEmpty(mapping.name_norm) || 
                    string.IsNullOrEmpty(mapping.category))
                {
                    return Results.BadRequest(new { error = "Missing required fields: client_id, name_norm, category" });
                }

                if (string.IsNullOrEmpty(mapping.updated_at))
                {
                    mapping.updated_at = DateTime.UtcNow.ToString("o");
                }

                if (string.IsNullOrEmpty(mapping.source))
                {
                    mapping.source = "api";
                }

                memoryService.UpdateMapping(mapping);

                return Results.Ok(new { success = true, message = "Memory updated successfully" });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        // GET /memory/{client_id}
        app.MapGet("/memory/{clientId}", (string clientId) =>
        {
            try
            {
                var mappings = memoryService.LoadMemory(clientId);
                return Results.Ok(new { client_id = clientId, count = mappings.Count, mappings = mappings });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        // GET /config
        app.MapGet("/config", () =>
        {
            try
            {
                var config = configService.LoadConfig();
                return Results.Ok(config);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });
    }

    private static bool ValidateApiKey(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            return false;
        }

        var header = authHeader.ToString();
        if (!header.StartsWith("ApiKey "))
        {
            return false;
        }

        var key = header.Substring(7);
        return key == API_KEY;
    }
}

// Extension for CsvService to write ClassificationResult
public static class CsvServiceExtensions
{
    public static void WriteClassificationResults(this CsvService service, string filePath, List<ClassificationResult> results)
    {
        using var writer = new StreamWriter(filePath);
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        csv.WriteRecords(results);
    }
}

