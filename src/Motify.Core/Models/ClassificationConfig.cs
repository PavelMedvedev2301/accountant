namespace Motify.Core.Models;

public class ClassificationConfig
{
    public Thresholds Thresholds { get; set; } = new();
    public Weights Weights { get; set; } = new();
    public Dictionary<string, List<string>> Keywords { get; set; } = new();
    public Dictionary<string, string> Ontology { get; set; } = new();
}

public class Thresholds
{
    public double RenumberedSimilarity { get; set; } = 0.92;
    public double MemoryExactMatch { get; set; } = 1.0;
    public double KeywordMatch { get; set; } = 0.85;
    public double ParentMatch { get; set; } = 0.80;
    public double FuzzyMatch { get; set; } = 0.75;
    public int NeedsReviewBelow { get; set; } = 70;
}

public class Weights
{
    public double Memory { get; set; } = 0.50;
    public double Keyword { get; set; } = 0.25;
    public double Parent { get; set; } = 0.15;
    public double Fuzzy { get; set; } = 0.10;
}

