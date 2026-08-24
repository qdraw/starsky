using System.Windows;
using Microsoft.Extensions.Logging;
using Starsky.Desktop.Models;
using Starsky.Desktop.Windows;

namespace Starsky.Desktop.Services;

public class WindowManager
{
    private readonly SettingsService _settings;
    private readonly RoutePersistenceService _routes;
    private readonly NavigationService _navigation;
    private readonly WebViewEnvironmentService _webViewEnv;
    private readonly FileDownloadService _fileDownload;
    private readonly ILogger<WindowManager> _logger;
    private readonly List<MainWindow> _mainWindows = [];
    private int? _localPort;

    public WindowManager(
        SettingsService settings,
        RoutePersistenceService routes,
        NavigationService navigation,
        WebViewEnvironmentService webViewEnv,
        FileDownloadService fileDownload,
        ILogger<WindowManager> logger)
    {
        _settings = settings;
        _routes = routes;
        _navigation = navigation;
        _webViewEnv = webViewEnv;
        _fileDownload = fileDownload;
        _logger = logger;
    }

    public void SetLocalPort(int port) => _localPort = port;

    public void OpenMainWindow(string? route, SavedWindowState? geometry)
    {
        var baseUrl = _navigation.GetEffectiveBaseUrl(_localPort);
        var offset = _mainWindows.Count * 24;

        var state = geometry ?? new SavedWindowState
        {
            Left = 100 + offset,
            Top = 100 + offset,
            Width = 1200,
            Height = 800
        };

        var window = new MainWindow(
            _settings, _routes, _navigation, _webViewEnv, _fileDownload,
            baseUrl, route ?? "?f=/", state, _mainWindows.Count, this, _logger);

        _mainWindows.Add(window);
        window.Closed += (_, _) =>
        {
            _mainWindows.Remove(window);
            if (_mainWindows.Count == 0 && Application.Current != null)
            {
	            Application.Current.Shutdown();
            }
        };

        window.Show();
    }

    public void RestoreWindows()
    {
        var saved = _routes.GetRoutes();
        if (saved.Count == 0)
        {
            OpenMainWindow(null, null);
            return;
        }

        foreach (var state in saved)
        {
	        OpenMainWindow(state.Route, state);
        }
    }

    public void CloseAll()
    {
        foreach (var w in _mainWindows.ToList())
        {
            try { w.Close(); } catch { /* best-effort */ }
        }
        _mainWindows.Clear();
    }

    public void ReopenAll()
    {
        _routes.ClearAll();
        CloseAll();
        OpenMainWindow(null, null);
    }

    public void ReloadAll()
    {
        foreach (var w in _mainWindows)
        {
	        w.Reload();
        }
    }
}
