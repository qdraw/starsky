using Starsky.Desktop;
using Starsky.Desktop.Services;

namespace starsky.Tests.Services;

[TestClass]
public class ApplicationPathsTests
{
    [TestMethod]
    public void AppData_IsUnderApplicationData()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        Assert.IsTrue(ApplicationPaths.AppData.StartsWith(appData, StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public void TempFolder_IsUnderLocalApplicationData()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        Assert.IsTrue(ApplicationPaths.TempFolder.StartsWith(localAppData, StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public void AllPaths_ContainStarskySubdirectory()
    {
        Assert.IsTrue(ApplicationPaths.AppData.Contains("starsky", StringComparison.OrdinalIgnoreCase));
        Assert.IsTrue(ApplicationPaths.TempFolder.Contains("starsky", StringComparison.OrdinalIgnoreCase));
        Assert.IsTrue(ApplicationPaths.LogsDir.Contains("starsky", StringComparison.OrdinalIgnoreCase));
        Assert.IsTrue(ApplicationPaths.SettingsFile.Contains("starsky", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public void RuntimeDir_IsRelativeToExecutable()
    {
        Assert.IsTrue(ApplicationPaths.RuntimeDir.EndsWith("runtime-starsky-win-x64", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public void SettingsFile_HasJsonExtension()
    {
        Assert.IsTrue(ApplicationPaths.SettingsFile.EndsWith(".json", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public void AllSettingsPaths_HaveJsonExtension()
    {
        Assert.IsTrue(ApplicationPaths.AppSettingsFile.EndsWith(".json", StringComparison.OrdinalIgnoreCase));
        Assert.IsTrue(ApplicationPaths.AppSettingsLocalFile.EndsWith(".json", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public void DatabaseFile_HasDbExtension()
    {
        Assert.IsTrue(ApplicationPaths.DatabaseFile.EndsWith(".db", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public void WebView2UserData_IsUnderAppData()
    {
        Assert.IsTrue(ApplicationPaths.WebView2UserData.StartsWith(ApplicationPaths.AppData, StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public void ThumbnailTempFolder_ContainsStarsky()
    {
        Assert.IsTrue(ApplicationPaths.ThumbnailTempFolder.Contains("starsky", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public void EnsureDirectories_CreatesRequiredFolders()
    {
        ApplicationPaths.EnsureDirectories();

        Assert.IsTrue(Directory.Exists(ApplicationPaths.AppData));
        Assert.IsTrue(Directory.Exists(ApplicationPaths.LogsDir));
        Assert.IsTrue(Directory.Exists(ApplicationPaths.TempFolder));
    }

    [TestMethod]
    public void ApplicationInfo_Version_MatchesSemver()
    {
        StringAssert.Matches(ApplicationInfo.Version, new Regex(@"^\d+\.\d+\.\d+"));
    }

    [TestMethod]
    public void ApplicationInfo_Version_DoesNotContainBuildMetadata()
    {
        Assert.IsFalse(ApplicationInfo.Version.Contains("+"));
    }
}
