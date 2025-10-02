using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Motify.Core.Models;

namespace Motify.Core.Services;

public class ConfigService
{
    private ClassificationConfig? _config;
    private readonly string _configPath;

    public ConfigService(string? configPath = null)
    {
        _configPath = configPath ?? Path.Combine(Directory.GetCurrentDirectory(), "config.yaml");
    }

    public ClassificationConfig LoadConfig()
    {
        if (_config != null) return _config;

        if (!File.Exists(_configPath))
        {
            _config = GetDefaultConfig();
            return _config;
        }

        try
        {
            var yaml = File.ReadAllText(_configPath);
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            _config = deserializer.Deserialize<ClassificationConfig>(yaml);
            return _config ?? GetDefaultConfig();
        }
        catch
        {
            _config = GetDefaultConfig();
            return _config;
        }
    }

    private ClassificationConfig GetDefaultConfig()
    {
        return new ClassificationConfig
        {
            Thresholds = new Thresholds
            {
                RenumberedSimilarity = 0.92,
                MemoryExactMatch = 1.0,
                KeywordMatch = 0.85,
                ParentMatch = 0.80,
                FuzzyMatch = 0.75,
                NeedsReviewBelow = 70
            },
            Weights = new Weights
            {
                Memory = 0.50,
                Keyword = 0.25,
                Parent = 0.15,
                Fuzzy = 0.10
            },
            Keywords = new Dictionary<string, List<string>>(),
            Ontology = new Dictionary<string, string>()
        };
    }
}

