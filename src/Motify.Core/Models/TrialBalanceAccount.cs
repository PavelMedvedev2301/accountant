namespace Motify.Core.Models;

public class TrialBalanceAccount
{
    public string account_code { get; set; } = string.Empty;
    public string account_name { get; set; } = string.Empty;
    public string? parent_code { get; set; }
    public string? level { get; set; }
    public string? opening_balance { get; set; }
    public string? debit { get; set; }
    public string? credit { get; set; }
    public string? closing_balance { get; set; }
    public string? currency { get; set; }
}



