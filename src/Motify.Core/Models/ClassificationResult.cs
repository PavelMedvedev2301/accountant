namespace Motify.Core.Models;

public class ClassificationResult
{
    public string account_code { get; set; } = string.Empty;
    public string account_name { get; set; } = string.Empty;
    public string? parent_code { get; set; }
    public string status { get; set; } = string.Empty;
    public string? suggested_category { get; set; }
    public int confidence { get; set; }
    public bool needs_review { get; set; }
    public string? renumbered_from_code { get; set; }
    public string? renumbered_from_name { get; set; }
    public string evidence { get; set; } = "{}";
}

