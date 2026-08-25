using Microsoft.Extensions.Logging.Abstractions;
using Starsky.Desktop.Models;
using Starsky.Desktop.Services;

namespace starsky.Tests.Services;

public class SettingsServiceTests : IDisposable
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

    [Fact]
    public void Load_MissingFile_ReturnsDefaults()
    {
        var svc = CreateService();
        var s = svc.Load();
        Assert.Equal(RuntimeMode.Local, s.Mode);
        Assert.True(s.UpdateCheckEnabled);
        Assert.Empty(s.Windows);
    }

    [Fact]
    public void Load_ValidJson_RestoresSettings()
    {
        var json = """{"Mode":1,"RemoteBaseUrl":"https://example.com","UpdateCheckEnabled":false,"Windows":[]}""";
        var svc = CreateService(json);
        var s = svc.Load();
        Assert.Equal(RuntimeMode.Remote, s.Mode);
        Assert.Equal("https://example.com", s.RemoteBaseUrl);
        Assert.False(s.UpdateCheckEnabled);
    }

    [Fact]
    public void Load_CorruptJson_ReturnsDefaults()
    {
        var svc = CreateService("{not-valid-json}}}");
        var s = svc.Load();
        Assert.Equal(RuntimeMode.Local, s.Mode);
    }

    [Fact]
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
        Assert.Equal(RuntimeMode.Remote, svc2.Current.Mode);
        Assert.Equal("https://example.com", svc2.Current.RemoteBaseUrl);
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, recursive: true); } catch { /* best-effort */ }
    }
}
