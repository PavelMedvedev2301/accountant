namespace Motify.Core.Models;

public class Evidence
{
    public string method { get; set; } = string.Empty;
    public double score { get; set; }
    public string? matched_name { get; set; }
    public string? matched_parent { get; set; }
    public string? matched_keyword { get; set; }
    public string? category { get; set; }
}

