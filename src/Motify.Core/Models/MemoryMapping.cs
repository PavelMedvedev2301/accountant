namespace Motify.Core.Models;

public class MemoryMapping
{
    public string client_id { get; set; } = string.Empty;
    public string name_norm { get; set; } = string.Empty;
    public string? parent_norm { get; set; }
    public string category { get; set; } = string.Empty;
    public string source { get; set; } = string.Empty;
    public string updated_at { get; set; } = string.Empty;
}

