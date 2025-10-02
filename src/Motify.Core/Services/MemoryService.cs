using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Motify.Core.Models;

namespace Motify.Core.Services;

public class MemoryService
{
    private readonly string _memoryDirectory;
    private readonly CsvConfiguration _config;

    public MemoryService(string? memoryDirectory = null)
    {
        _memoryDirectory = memoryDirectory ?? Path.Combine(Directory.GetCurrentDirectory(), "Memory");
        Directory.CreateDirectory(_memoryDirectory);

        _config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            MissingFieldFound = null,
            HeaderValidated = null,
            IgnoreBlankLines = true,
            TrimOptions = TrimOptions.Trim
        };
    }

    public List<MemoryMapping> LoadMemory(string clientId)
    {
        var filePath = GetMemoryFilePath(clientId);
        
        if (!File.Exists(filePath))
        {
            return new List<MemoryMapping>();
        }

        try
        {
            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, _config);
            return csv.GetRecords<MemoryMapping>().ToList();
        }
        catch
        {
            return new List<MemoryMapping>();
        }
    }

    public void SaveMemory(string clientId, List<MemoryMapping> mappings)
    {
        var filePath = GetMemoryFilePath(clientId);
        
        using var writer = new StreamWriter(filePath);
        using var csv = new CsvWriter(writer, _config);
        csv.WriteRecords(mappings);
    }

    public void UpdateMapping(MemoryMapping mapping)
    {
        var mappings = LoadMemory(mapping.client_id);
        
        // Remove existing mapping with same name_norm
        mappings.RemoveAll(m => m.name_norm == mapping.name_norm && 
                                m.parent_norm == mapping.parent_norm);
        
        // Add new mapping
        mappings.Add(mapping);
        
        SaveMemory(mapping.client_id, mappings);
    }

    public MemoryMapping? FindMatch(string clientId, string nameNorm, string? parentNorm = null)
    {
        var mappings = LoadMemory(clientId);
        
        // Try exact match with parent
        if (!string.IsNullOrEmpty(parentNorm))
        {
            var exactMatch = mappings.FirstOrDefault(m => 
                m.name_norm == nameNorm && m.parent_norm == parentNorm);
            if (exactMatch != null) return exactMatch;
        }
        
        // Try exact match without parent
        return mappings.FirstOrDefault(m => m.name_norm == nameNorm);
    }

    private string GetMemoryFilePath(string clientId)
    {
        var safeClientId = string.Join("_", clientId.Split(Path.GetInvalidFileNameChars()));
        return Path.Combine(_memoryDirectory, $"{safeClientId}_memory.csv");
    }
}

