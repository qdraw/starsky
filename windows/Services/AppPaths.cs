namespace Starsky.Windows.Services;

public sealed class AppPaths
{
    public string BaseAppDataPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "starsky");

    public string LogsPath => Path.Combine(BaseAppDataPath, "logs");

    public string SettingsFilePath => Path.Combine(BaseAppDataPath, "starsky-settings.json");

    public string TempFolderPath => Path.Combine(Path.GetTempPath(), "starsky");

    public string TempWorkspacePath => Path.Combine(BaseAppDataPath, "edit-workspace");

    public string ThumbnailTempFolderPath => Path.Combine(BaseAppDataPath, "thumbnailTempFolder");

    public string BackendTempFolderPath => Path.Combine(BaseAppDataPath, "tempFolder");

    public string BackendDatabasePath => Path.Combine(BaseAppDataPath, "starsky.db");

    public string BackendAppSettingsPath => Path.Combine(BaseAppDataPath, "appsettings.json");

    public string BackendAppSettingsLocalPath => Path.Combine(BaseAppDataPath, "appsettings.local.json");

    public string WebViewUserDataPath => Path.Combine(BaseAppDataPath, "webview2");

    public void EnsureDirectories()
    {
        Directory.CreateDirectory(BaseAppDataPath);
        Directory.CreateDirectory(LogsPath);
        Directory.CreateDirectory(TempFolderPath);
        Directory.CreateDirectory(TempWorkspacePath);
        Directory.CreateDirectory(ThumbnailTempFolderPath);
        Directory.CreateDirectory(BackendTempFolderPath);
        Directory.CreateDirectory(WebViewUserDataPath);
    }

    public string ResolveBackendExecutablePath()
    {
        var packagedPath = Path.Combine(AppContext.BaseDirectory, "runtime-starsky-win-x64", "starsky.exe");
        if (File.Exists(packagedPath))
        {
            return packagedPath;
        }

        string? projectRoot = FindAncestorContaining("windows.slnx");
        if (!string.IsNullOrWhiteSpace(projectRoot))
        {
            var developmentPreferredPath = Path.Combine(projectRoot, "starsky", "starsky-win-x64", "starsky.exe");
            if (File.Exists(developmentPreferredPath))
            {
                return developmentPreferredPath;
            }

            var developmentPath = Path.Combine(projectRoot, "starsky", "win-x64", "starsky.exe");
            if (File.Exists(developmentPath))
            {
                return developmentPath;
            }
        }

        throw new FileNotFoundException("Unable to locate starsky.exe for the Windows desktop shell.");
    }

    private static string? FindAncestorContaining(string fileName)
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, fileName)))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        return null;
    }
}