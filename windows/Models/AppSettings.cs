namespace Starsky.Windows.Models;

public sealed class AppSettings
{
    public LocationMode Mode { get; set; } = LocationMode.Local;

    public string? RemoteUrl { get; set; }

    public bool UpdatePolicyEnabled { get; set; } = true;

    public DateTimeOffset? LastUpdateWarningUtc { get; set; }

    public List<WindowStateInfo> MainWindows { get; set; } = new();

    public WindowStateInfo? SettingsWindow { get; set; }
}