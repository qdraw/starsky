namespace Starsky.Windows.Models;

public sealed class UrlValidationResult
{
    public bool IsValid { get; set; }

    public bool IsLocal { get; set; }

    public string Location { get; set; } = string.Empty;

    public string? Reason { get; set; }
}