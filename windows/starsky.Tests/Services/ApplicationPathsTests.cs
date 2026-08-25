using Starsky.Desktop.Services;

namespace starsky.Tests.Services;

public class ApplicationPathsTests
{
    [Fact]
    public void AppData_IsUnderApplicationData()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        Assert.StartsWith(appData, ApplicationPaths.AppData, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TempFolder_IsUnderLocalApplicationData()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        Assert.StartsWith(localAppData, ApplicationPaths.TempFolder, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AllPaths_ContainStarskySubdirectory()
    {
        Assert.Contains("starsky", ApplicationPaths.AppData, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("starsky", ApplicationPaths.TempFolder, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("starsky", ApplicationPaths.LogsDir, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("starsky", ApplicationPaths.SettingsFile, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void RuntimeDir_IsRelativeToExecutable()
    {
        Assert.EndsWith("runtime-starsky-win-x64", ApplicationPaths.RuntimeDir, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void SettingsFile_HasJsonExtension()
    {
        Assert.EndsWith(".json", ApplicationPaths.SettingsFile, StringComparison.OrdinalIgnoreCase);
    }
}
