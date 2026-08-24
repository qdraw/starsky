using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Web.WebView2.Core;
using Starsky.Desktop.Models;
using Starsky.Desktop.Services;

namespace Starsky.Desktop.Windows;

public partial class MainWindow : Window
{
    private const string AppVersion = "0.8.1";
    private const string DocsUrl = "https://qdraw.nl/special/starsky/docs/";
    private const string ReleasesUrl = "https://github.com/qdraw/starsky/releases";

    private readonly SettingsService _settings;
    private readonly RoutePersistenceService _routes;
    private readonly NavigationService _navigation;
    private readonly WebViewEnvironmentService _webViewEnv;
    private readonly FileDownloadService _fileDownload;
    private readonly WindowManager _windowManager;
    private readonly ILogger _logger;

    private readonly string _baseUrl;
    private readonly int _windowIndex;
    private string _currentRoute;

    public MainWindow(
        SettingsService settings,
        RoutePersistenceService routes,
        NavigationService navigation,
        WebViewEnvironmentService webViewEnv,
        FileDownloadService fileDownload,
        string baseUrl,
        string initialRoute,
        SavedWindowState geometry,
        int windowIndex,
        WindowManager windowManager,
        ILogger logger)
    {
        InitializeComponent();

        _settings = settings;
        _routes = routes;
        _navigation = navigation;
        _webViewEnv = webViewEnv;
        _fileDownload = fileDownload;
        _windowManager = windowManager;
        _logger = logger;
        _baseUrl = baseUrl;
        _windowIndex = windowIndex;
        _currentRoute = initialRoute;

        Left = geometry.Left;
        Top = geometry.Top;
        Width = geometry.Width;
        Height = geometry.Height;
        if (geometry.IsMaximized)
        {
	        WindowState = WindowState.Maximized;
        }

        Loaded += MainWindow_Loaded;
        Closing += MainWindow_Closing;
        KeyDown += MainWindow_KeyDown;
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            var env = await _webViewEnv.GetEnvironmentAsync();
            await WebView.EnsureCoreWebView2Async(env);

            // Set user agent
            var uaBase = WebView.CoreWebView2.Settings.UserAgent;
            WebView.CoreWebView2.Settings.UserAgent = $"{uaBase} starsky/{AppVersion}";

            WebView.CoreWebView2.NavigationStarting += CoreWebView2_NavigationStarting;
            WebView.CoreWebView2.SourceChanged += CoreWebView2_SourceChanged;
            WebView.CoreWebView2.NewWindowRequested += CoreWebView2_NewWindowRequested;

            var startUrl = _navigation.BuildStartUrl(_baseUrl, _currentRoute);
            _logger.LogInformation("Navigating main window to {Url}", startUrl);
            WebView.CoreWebView2.Navigate(startUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "WebView2 initialization failed");
            new ErrorWindow($"Failed to initialize WebView2:\n{ex.Message}").ShowDialog();
        }
    }

    private void CoreWebView2_NavigationStarting(object? sender, CoreWebView2NavigationStartingEventArgs e)
    {
        if (!Uri.TryCreate(e.Uri, UriKind.Absolute, out var uri))
        {
	        return;
        }

        if (_navigation.IsAllowedOrigin(uri, _baseUrl))
        {
	        return;
        }

        // External link — open in system browser
        e.Cancel = true;
        _logger.LogInformation("Blocking navigation to {Uri}, opening in browser", e.Uri);
        Process.Start(new ProcessStartInfo(e.Uri) { UseShellExecute = true });
    }

    private void CoreWebView2_SourceChanged(object? sender, CoreWebView2SourceChangedEventArgs e)
    {
        var src = WebView.CoreWebView2.Source;
        if (!Uri.TryCreate(src, UriKind.Absolute, out var uri))
        {
	        return;
        }

        // Persist relative part (path + query + fragment)
        _currentRoute = uri.PathAndQuery + uri.Fragment;
        _routes.SaveRoute(_windowIndex, _currentRoute, GetCurrentGeometry());
    }

    private void CoreWebView2_NewWindowRequested(object? sender, CoreWebView2NewWindowRequestedEventArgs e)
    {
        e.Handled = true;

        if (!Uri.TryCreate(e.Uri, UriKind.Absolute, out var uri) || !_navigation.IsAllowedOrigin(uri, _baseUrl))
        {
            Process.Start(new ProcessStartInfo(e.Uri) { UseShellExecute = true });
            return;
        }

        var route = uri.PathAndQuery + uri.Fragment;
        _windowManager.OpenMainWindow(route, null);
    }

    private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        _routes.RemoveRoute(_windowIndex);
    }

    private void MainWindow_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.F12)
        {
            WebView.CoreWebView2?.OpenDevToolsWindow();
            e.Handled = true;
        }
    }

    public void Reload()
    {
        Dispatcher.Invoke(() => WebView.CoreWebView2?.Reload());
    }

    private SavedWindowState GetCurrentGeometry() => new()
    {
        Route = _currentRoute,
        Left = Left,
        Top = Top,
        Width = ActualWidth,
        Height = ActualHeight,
        IsMaximized = WindowState == WindowState.Maximized
    };

    // ── Menu handlers ──────────────────────────────────────────────────────────

    private void NewWindow_Click(object sender, RoutedEventArgs e)
        => _windowManager.OpenMainWindow(null, null);

    private void ReloadAll_Click(object sender, RoutedEventArgs e)
        => _windowManager.ReloadAll();

    private async void EditFile_Click(object sender, RoutedEventArgs e)
    {
        if (_settings.Current.Mode == Models.RuntimeMode.Local)
        {
            // Forward Ctrl+E keystroke to the web app
            await WebView.CoreWebView2.ExecuteScriptAsync(
                "document.dispatchEvent(new KeyboardEvent('keydown', { key: 'e', ctrlKey: true, bubbles: true }))");
            return;
        }

        // Remote: download and open
        var src = WebView.CoreWebView2?.Source;
        if (src == null)
        {
	        return;
        }

        if (!Uri.TryCreate(src, UriKind.Absolute, out var uri))
        {
	        return;
        }

        var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
        var filePath = query["f"];
        if (string.IsNullOrEmpty(filePath))
        {
            new ErrorWindow("No file selected (no 'f' parameter in URL).").ShowDialog();
            return;
        }

        try
        {
            await _fileDownload.DownloadAndOpenAsync(filePath, _baseUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "EditFile failed");
            new ErrorWindow($"Failed to open file:\n{ex.Message}").ShowDialog();
        }
    }

    private void ConnectionSettings_Click(object sender, RoutedEventArgs e)
    {
        var w = new SettingsWindow(
            _settings,
            new RemoteUrlValidator(Microsoft.Extensions.Logging.Abstractions.NullLogger<RemoteUrlValidator>.Instance),
            _windowManager)
        { Owner = this };
        w.Show();
    }

    private async void AppSettings_Click(object sender, RoutedEventArgs e)
    {
        await WebView.CoreWebView2.ExecuteScriptAsync(
            "document.dispatchEvent(new KeyboardEvent('keydown', { key: 'k', ctrlKey: true, shiftKey: true, bubbles: true }))");
    }

    private void DevTools_Click(object sender, RoutedEventArgs e)
        => WebView.CoreWebView2?.OpenDevToolsWindow();

    private void OpenInBrowser_Click(object sender, RoutedEventArgs e)
    {
        var url = WebView.CoreWebView2?.Source;
        if (!string.IsNullOrEmpty(url))
        {
	        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
    }

    private void Documentation_Click(object sender, RoutedEventArgs e)
        => Process.Start(new ProcessStartInfo(DocsUrl) { UseShellExecute = true });

    private void ReleaseOverview_Click(object sender, RoutedEventArgs e)
        => Process.Start(new ProcessStartInfo(ReleasesUrl) { UseShellExecute = true });
}
