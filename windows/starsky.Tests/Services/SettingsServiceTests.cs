using Microsoft.Extensions.Logging.Abstractions;
using Starsky.Desktop.Models;
using Starsky.Desktop.Services;

namespace starsky.Tests.Services;

[TestClass]
public sealed class SettingsServiceTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _settingsFile;

    public SettingsServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"starsky-test-{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempDir);
        _settingsFile = Path.Combine(_tempDir, "settings.json");
    }

    private SettingsService CreateService(string? settingsJson = null)
    {
        if (settingsJson != null)
        {
            File.WriteAllText(_settingsFile, settingsJson);
        }

        return new SettingsService(NullLogger<SettingsService>.Instance, _settingsFile);
    }

    [TestMethod]
    public void Load_MissingFile_ReturnsDefaults()
    {
        var svc = CreateService();
        var s = svc.Load();
        Assert.AreEqual(RuntimeMode.Local, s.Mode);
        Assert.IsTrue(s.UpdateCheckEnabled);
        Assert.IsEmpty(s.Windows);
    }

    [TestMethod]
    public void Load_ValidJson_RestoresSettings()
    {
        var json = """{"Mode":1,"RemoteBaseUrl":"https://example.com","UpdateCheckEnabled":false,"Windows":[]}""";
        var svc = CreateService(json);
        var s = svc.Load();
        Assert.AreEqual(RuntimeMode.Remote, s.Mode);
        Assert.AreEqual("https://example.com", s.RemoteBaseUrl);
        Assert.IsFalse(s.UpdateCheckEnabled);
    }

    [TestMethod]
    public void Load_CorruptJson_ReturnsDefaults()
    {
        var svc = CreateService("{not-valid-json}}}");
        var s = svc.Load();
        Assert.AreEqual(RuntimeMode.Local, s.Mode);
    }

    [TestMethod]
    public void Save_ThenLoad_RoundTrips()
    {
        var svc = CreateService();
        svc.Save(new DesktopSettings
        {
            Mode = RuntimeMode.Remote,
            RemoteBaseUrl = "https://example.com",
            UpdateCheckEnabled = false
        });

        var svc2 = new SettingsService(NullLogger<SettingsService>.Instance, _settingsFile);
        svc2.Load();
        Assert.AreEqual(RuntimeMode.Remote, svc2.Current.Mode);
        Assert.AreEqual("https://example.com", svc2.Current.RemoteBaseUrl);
    }

    [TestMethod]
    public void Save_NoArg_PersistsCurrentSettings()
    {
        var svc = CreateService();
        svc.Current.RemoteBaseUrl = "https://my-server.com";

        svc.Save();

        var svc2 = new SettingsService(NullLogger<SettingsService>.Instance, _settingsFile);
        svc2.Load();
        Assert.AreEqual("https://my-server.com", svc2.Current.RemoteBaseUrl);
    }

    [TestMethod]
    public void Save_ToInvalidPath_DoesNotThrow()
    {
        var badFile = Path.Combine(_tempDir, "sub", "deeper", "settings.json"); // parent doesn't exist
        var svc = new SettingsService(NullLogger<SettingsService>.Instance, badFile);

        Exception? ex = null;
        try { svc.Save(new DesktopSettings()); } catch (Exception e) { ex = e; }

        Assert.IsNull(ex);
    }

    [TestMethod]
    public void Load_ReturnsCurrentAfterLoad()
    {
        var svc = CreateService();

        var settings = svc.Load();

        Assert.AreSame(svc.Current, settings);
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, recursive: true); } catch { /* best-effort */ }
        GC.SuppressFinalize(this);
    }
}
