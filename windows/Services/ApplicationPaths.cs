using System.Reflection;

namespace Starsky.Desktop.Services;

public static class ApplicationPaths
{
    public static string AppData { get; } =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "starsky");

    public static string LocalAppData { get; } =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "starsky");

    public static string LogsDir => Path.Combine(AppData, "logs");
    public static string WebView2UserData => Path.Combine(AppData, "webview2");
    public static string SettingsFile => Path.Combine(AppData, "settings.json");
    public static string DatabaseFile => Path.Combine(AppData, "starsky.db");
    public static string TempFolder => Path.Combine(LocalAppData, "tempFolder");
    public static string ThumbnailTempFolder => Path.Combine(AppData, "thumbnailTempFolder");

    public static string RuntimeDir
    {
        get
        {
            var exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? AppContext.BaseDirectory;
            return Path.Combine(exeDir, "runtime-starsky-win-x64");
        }
    }

    public static void EnsureDirectories()
    {
        Directory.CreateDirectory(AppData);
        Directory.CreateDirectory(LogsDir);
        Directory.CreateDirectory(WebView2UserData);
        Directory.CreateDirectory(TempFolder);
        Directory.CreateDirectory(ThumbnailTempFolder);
    }
}
