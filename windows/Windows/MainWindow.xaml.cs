using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Web.WebView2.Core;
using Starsky.Desktop.Models;
using Starsky.Desktop.Services;

namespace Starsky.Desktop.Windows;

[ExcludeFromCodeCoverage]
public partial class MainWindow : Window
{
    [SuppressMessage("Style", "S1075:URIs should not be hardcoded", Justification = "used")]
    private const string DocsUrl = "https://docs.qdraw.nl/"; 
    [SuppressMessage("Style", "S1075:URIs should not be hardcoded", Justification = "used")]
    private const string ReleasesUrl = "https://github.com/qdraw/starsky/releases"; 

    private readonly SettingsService _settings;
    private readonly RoutePersistenceService _routes;
    private readonly WebViewEnvironmentService _webViewEnv;
    private readonly FileDownloadService _fileDownload;
    private readonly FileWatcherService _watcher;
    private readonly WindowManager _windowManager;
    private readonly UpdateService _updateService;
    private readonly ILogger _logger;

    private readonly string _baseUrl;
    private readonly int _windowIndex;
    private string _currentRoute;

    public MainWindow(MainWindowOptions options)
    {
        InitializeComponent();

        _settings = options.Settings;
        _routes = options.Routes;
        _webViewEnv = options.WebViewEnv;
        _fileDownload = options.FileDownload;
        _watcher = options.Watcher;
        _windowManager = options.WindowManager;
        _updateService = options.UpdateService;
        _logger = options.Logger;
        _baseUrl = options.BaseUrl;
        _windowIndex = options.WindowIndex;
        _currentRoute = options.InitialRoute;

        Left = options.Geometry.Left;
        Top = options.Geometry.Top;
        Width = options.Geometry.Width;
        Height = options.Geometry.Height;
        if (options.Geometry.IsMaximized)
        {
	        WindowState = WindowState.Maximized;
        }

        var updateVisibility = _updateService.IsVelopackAvailable ? Visibility.Visible : Visibility.Collapsed;
        CheckForUpdatesMenuItem.Visibility = updateVisibility;
        CheckForUpdatesSeparator.Visibility = updateVisibility;

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
            WebView.CoreWebView2.Settings.UserAgent = $"{uaBase} starsky/{ApplicationInfo.Version}";

            WebView.CoreWebView2.NavigationStarting += CoreWebView2_NavigationStarting;
            WebView.CoreWebView2.SourceChanged += CoreWebView2_SourceChanged;
            WebView.CoreWebView2.NewWindowRequested += CoreWebView2_NewWindowRequested;

            var startUrl = NavigationService.BuildStartUrl(_baseUrl, _currentRoute);
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

        if (NavigationService.IsAllowedOrigin(uri, _baseUrl))
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

        if (!Uri.TryCreate(e.Uri, UriKind.Absolute, out var uri) || !NavigationService.IsAllowedOrigin(uri, _baseUrl))
        {
            Process.Start(new ProcessStartInfo(e.Uri) { UseShellExecute = true });
            return;
        }

        var route = uri.PathAndQuery + uri.Fragment;
        _windowManager.OpenMainWindow(route, null);
    }

    private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        _routes.SaveRoute(_windowIndex, _currentRoute, GetCurrentGeometry());
    }

    private void MainWindow_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.F12)
        {
            WebView.CoreWebView2?.OpenDevToolsWindow();
            e.Handled = true;
        }
        else if (e.Key == Key.F11)
        {
            ToggleFullScreen_Click(sender, e);
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
            var webViewCookies = await WebView.CoreWebView2!.CookieManager.GetCookiesAsync(_baseUrl);
            var cookieHeader = webViewCookies is { Count: > 0 }
                ? string.Join("; ", webViewCookies.Select(c => $"{c.Name}={c.Value}"))
                : null;
            _watcher.SetUploadContext(_baseUrl, cookieHeader);
            await _fileDownload.DownloadAndOpenAsync(filePath, _baseUrl, cookieHeader: cookieHeader);
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

    // ── Edit menu ─────────────────────────────────────────────────────────────

    private async void Undo_Click(object sender, RoutedEventArgs e)
        => await ExecCommandAsync("undo");

    private async void Redo_Click(object sender, RoutedEventArgs e)
        => await ExecCommandAsync("redo");

    private async void Cut_Click(object sender, RoutedEventArgs e)
        => await ExecCommandAsync("cut");

    private async void Copy_Click(object sender, RoutedEventArgs e)
        => await ExecCommandAsync("copy");

    private async void Paste_Click(object sender, RoutedEventArgs e)
        => await ExecCommandAsync("paste");

    private async void SelectAll_Click(object sender, RoutedEventArgs e)
        => await ExecCommandAsync("selectAll");

    private async Task ExecCommandAsync(string command)
    {
        if (WebView.CoreWebView2 != null)
            await WebView.CoreWebView2.ExecuteScriptAsync($"document.execCommand('{command}')");
    }

    // ── View menu ─────────────────────────────────────────────────────────────

    private void ActualSize_Click(object sender, RoutedEventArgs e)
        => WebView.ZoomFactor = 1.0;

    private void ZoomIn_Click(object sender, RoutedEventArgs e)
        => WebView.ZoomFactor = Math.Min(WebView.ZoomFactor * 1.25, 5.0);

    private void ZoomOut_Click(object sender, RoutedEventArgs e)
        => WebView.ZoomFactor = Math.Max(WebView.ZoomFactor / 1.25, 0.25);

    private bool _isFullScreen;
    private WindowStyle _savedWindowStyle;
    private WindowState _savedWindowState;

    private void ToggleFullScreen_Click(object sender, RoutedEventArgs e)
    {
        if (_isFullScreen)
        {
            WindowStyle = _savedWindowStyle;
            WindowState = _savedWindowState;
            _isFullScreen = false;
        }
        else
        {
            _savedWindowStyle = WindowStyle;
            _savedWindowState = WindowState;
            WindowStyle = WindowStyle.None;
            WindowState = WindowState.Maximized;
            _isFullScreen = true;
        }
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

    private async void CheckForUpdates_Click(object sender, RoutedEventArgs e)
    {
        CheckForUpdatesMenuItem.IsEnabled = false;
        try
        {
            if (await _updateService.CheckAsync())
            {
                new UpdateWindow(_updateService).Show();
            }
            else
            {
                MessageBox.Show("You are running the latest version.", "Check for Updates",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        finally
        {
            CheckForUpdatesMenuItem.IsEnabled = true;
        }
    }
}
