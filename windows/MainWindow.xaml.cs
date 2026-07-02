using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using Starsky.Windows.Services;
using System.IO;

namespace Starsky.Windows;

public sealed partial class MainWindow : Window
{
    private readonly AppController _controller;
    private readonly string _initialRoute;
    private readonly SemaphoreSlim _initializeGate = new(1, 1);
    private bool _forceClose;
    private bool _isInitialized;
    private string? _currentUri;

    public MainWindow(AppController controller, string initialRoute)
    {
        _controller = controller;
        _initialRoute = string.IsNullOrWhiteSpace(initialRoute) ? "?f=/" : initialRoute;
        _controller.Logger.Info("MainWindow ctor: setting title");
        Title = "Starsky";
        _controller.Logger.Info("MainWindow ctor: InitializeComponent start");
        InitializeComponent();
        _controller.Logger.Info("MainWindow ctor: InitializeComponent done");
        BrowserView.NavigationStarting += OnNavigationStarting;
        BrowserView.NavigationCompleted += OnNavigationCompleted;
        Closed += OnClosed;
    }

    public void ForceClose()
    {
        _forceClose = true;
        Close();
    }

    public string? CurrentUri => _currentUri;

    public async Task ReloadAsync()
    {
        await InitializeAsync();
        BrowserView.CoreWebView2?.Reload();
    }

    public async Task ForwardShortcutAsync(string key, bool ctrl, bool shift)
    {
        await InitializeAsync();
        if (BrowserView.CoreWebView2 is null)
        {
            return;
        }

        var escapedKey = key.Replace("\\", "\\\\").Replace("'", "\\'");
        var script = $@"(function() {{
            const options = {{
                key: '{escapedKey}',
                ctrlKey: {ctrl.ToString().ToLowerInvariant()},
                shiftKey: {shift.ToString().ToLowerInvariant()},
                bubbles: true,
                cancelable: true
            }};
            document.dispatchEvent(new KeyboardEvent('keydown', options));
            document.dispatchEvent(new KeyboardEvent('keyup', options));
        }})();";

        await BrowserView.CoreWebView2.ExecuteScriptAsync(script);
    }

    public async Task InitializeAsync()
    {
        if (_isInitialized)
        {
            return;
        }

        await _initializeGate.WaitAsync();
        try
        {
            if (_isInitialized)
            {
                return;
            }

            await BrowserView.EnsureCoreWebView2Async();

            var navigationTarget = AppController.CombineBaseUriAndRoute(_controller.EffectiveBaseUri, _initialRoute);
            _controller.Logger.Info($"MainWindow.InitializeAsync: navigating to {navigationTarget}");
            _currentUri = navigationTarget;
            BrowserView.Source = new Uri(navigationTarget);
            _isInitialized = true;
        }
        finally
        {
            _initializeGate.Release();
        }
    }

    private async void NewWindow_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            await _controller.OpenMainWindowAsync();
        }
        catch (Exception exception)
        {
            _controller.Logger.Error("NewWindow_Click failed", exception);
            _controller.ShowError(exception.ToString());
        }
    }

    private async void EditFile_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            await _controller.EditCurrentFileAsync(this);
        }
        catch (Exception exception)
        {
            _controller.Logger.Error("EditFile_Click failed", exception);
            _controller.ShowError(exception.ToString());
        }
    }

    private async void ConnectionSettings_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            await _controller.OpenSettingsWindowAsync();
        }
        catch (Exception exception)
        {
            _controller.Logger.Error("ConnectionSettings_Click failed", exception);
            _controller.ShowError(exception.ToString());
        }
    }

    private async void AppSettingsShortcut_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var settingsPath = _controller.Paths.BackendAppSettingsLocalPath;
            _controller.Logger.Info($"AppSettingsShortcut_Click: opening {settingsPath}");
            if (File.Exists(settingsPath))
            {
                await _controller.ExternalOpen.OpenFileAsync(settingsPath);
            }
            else
            {
                _controller.ShowError($"Settings file not found at: {settingsPath}");
            }
        }
        catch (Exception exception)
        {
            _controller.Logger.Error("AppSettingsShortcut_Click failed", exception);
            _controller.ShowError(exception.ToString());
        }
    }

    private void Refresh_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (BrowserView.CoreWebView2 is not null)
            {
                BrowserView.CoreWebView2.Reload();
            }
        }
        catch (Exception exception)
        {
            _controller.Logger.Error("Refresh_Click failed", exception);
            _controller.ShowError(exception.ToString());
        }
    }

    private async void DevTools_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            await InitializeAsync();
            if (BrowserView.CoreWebView2 is not null)
            {
                BrowserView.CoreWebView2.OpenDevToolsWindow();
            }
        }
        catch (Exception exception)
        {
            _controller.Logger.Error("DevTools_Click failed", exception);
            _controller.ShowError(exception.ToString());
        }
    }

    private async void OpenInBrowser_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            await _controller.OpenCurrentPageInBrowserAsync(this);
        }
        catch (Exception exception)
        {
            _controller.Logger.Error("OpenInBrowser_Click failed", exception);
            _controller.ShowError(exception.ToString());
        }
    }

    private async void Documentation_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            await _controller.OpenDocumentationAsync();
        }
        catch (Exception exception)
        {
            _controller.Logger.Error("Documentation_Click failed", exception);
            _controller.ShowError(exception.ToString());
        }
    }

    private async void ReleaseOverview_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            await _controller.OpenReleasesAsync();
        }
        catch (Exception exception)
        {
            _controller.Logger.Error("ReleaseOverview_Click failed", exception);
            _controller.ShowError(exception.ToString());
        }
    }

    private void OnNavigationStarting(WebView2 sender, CoreWebView2NavigationStartingEventArgs args)
    {
        if (string.IsNullOrWhiteSpace(args.Uri))
        {
            return;
        }

        if (_controller.IsNavigationAllowed(args.Uri))
        {
            return;
        }

        args.Cancel = true;
        _ = _controller.ExternalOpen.OpenUriAsync(args.Uri);
    }

    private void OnNavigationCompleted(WebView2 sender, CoreWebView2NavigationCompletedEventArgs args)
    {
        if (sender.Source is not null)
        {
            _currentUri = sender.Source.AbsoluteUri;
        }

        if (!args.IsSuccess)
        {
            _controller.Logger.Warn($"MainWindow.OnNavigationCompleted failed: {args.WebErrorStatus}");
        }
    }

    private async void OnClosed(object sender, WindowEventArgs args)
    {
        _controller.Logger.Info($"MainWindow.OnClosed fired. forceClose={_forceClose}");
        BrowserView.NavigationStarting -= OnNavigationStarting;
        BrowserView.NavigationCompleted -= OnNavigationCompleted;
        if (!_forceClose)
        {
            await _controller.PersistMainWindowStateAsync(this, _initialRoute);
        }

        _controller.RemoveMainWindow(this);
    }
}
