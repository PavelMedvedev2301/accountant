namespace Motify.Core.Models;

public class NewAccountResult
{
    public string account_code { get; set; } = string.Empty;
    public string account_name { get; set; } = string.Empty;
    public string? parent_code { get; set; }
    public string status { get; set; } = string.Empty;
    public string? renumbered_from_code { get; set; }
    public string? renumbered_from_name { get; set; }
}



