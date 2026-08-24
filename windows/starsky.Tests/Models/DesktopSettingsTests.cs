using System.Text.Json;
using Starsky.Desktop.Models;

namespace starsky.Tests.Models;

public class DesktopSettingsTests
{
    [Fact]
    public void Defaults_AreCorrect()
    {
        var s = new DesktopSettings();
        Assert.Equal(RuntimeMode.Local, s.Mode);
        Assert.Equal(string.Empty, s.RemoteBaseUrl);
        Assert.True(s.UpdateCheckEnabled);
        Assert.Null(s.LastUpdateWarningShown);
        Assert.Empty(s.Windows);
    }

    [Fact]
    public void RoundTrip_Json_PreservesAllFields()
    {
        var original = new DesktopSettings
        {
            Mode = RuntimeMode.Remote,
            RemoteBaseUrl = "https://example.com",
            UpdateCheckEnabled = false,
            LastUpdateWarningShown = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc),
            Windows =
            [
                new SavedWindowState { Route = "?f=/test", Left = 50, Top = 60, Width = 800, Height = 600, IsMaximized = true }
            ]
        };

        var json = JsonSerializer.Serialize(original);
        var restored = JsonSerializer.Deserialize<DesktopSettings>(json)!;

        Assert.Equal(original.Mode, restored.Mode);
        Assert.Equal(original.RemoteBaseUrl, restored.RemoteBaseUrl);
        Assert.Equal(original.UpdateCheckEnabled, restored.UpdateCheckEnabled);
        Assert.Equal(original.LastUpdateWarningShown, restored.LastUpdateWarningShown);
        Assert.Single(restored.Windows);
        Assert.Equal("?f=/test", restored.Windows[0].Route);
        Assert.Equal(50, restored.Windows[0].Left);
        Assert.True(restored.Windows[0].IsMaximized);
    }
}
