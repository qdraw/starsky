using Starsky.Desktop.Models;

namespace starsky.Tests.Models;

[TestClass]
public class DesktopSettingsTests
{
    [TestMethod]
    public void Defaults_AreCorrect()
    {
        var s = new DesktopSettings();
        Assert.AreEqual(RuntimeMode.Local, s.Mode);
        Assert.AreEqual(string.Empty, s.RemoteBaseUrl);
        Assert.IsTrue(s.UpdateCheckEnabled);
        Assert.IsNull(s.LastUpdateWarningShown);
        Assert.IsEmpty(s.Windows);
    }

    [TestMethod]
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

        Assert.AreEqual(original.Mode, restored.Mode);
        Assert.AreEqual(original.RemoteBaseUrl, restored.RemoteBaseUrl);
        Assert.AreEqual(original.UpdateCheckEnabled, restored.UpdateCheckEnabled);
        Assert.AreEqual(original.LastUpdateWarningShown, restored.LastUpdateWarningShown);
        Assert.HasCount(1, restored.Windows);
        Assert.AreEqual("?f=/test", restored.Windows[0].Route);
        Assert.AreEqual(50, restored.Windows[0].Left);
        Assert.IsTrue(restored.Windows[0].IsMaximized);
    }
}
