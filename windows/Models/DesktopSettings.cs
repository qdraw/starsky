namespace Starsky.Desktop.Models;

public class DesktopSettings
{
    public RuntimeMode Mode { get; set; } = RuntimeMode.Local;
    public string RemoteBaseUrl { get; set; } = string.Empty;
    public bool UpdateCheckEnabled { get; set; } = true;
    public DateTime? LastUpdateWarningShown { get; set; }
    public List<SavedWindowState> Windows { get; set; } = [];
}
