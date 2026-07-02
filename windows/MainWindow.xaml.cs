using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using Starsky.Windows.Services;

namespace Starsky.Windows;

public sealed partial class MainWindow : Window
{
    private readonly AppController _controller;
    private readonly string _initialRoute;
    private bool _initialized;
    private bool _forceClose;

    public MainWindow(AppController controller, string initialRoute)
    {
        _controller = controller;
        _initialRoute = string.IsNullOrWhiteSpace(initialRoute) ? "?f=/" : initialRoute;
        Title = "Starsky";
        InitializeComponent();
        Closed += OnClosed;
    }

    public string? CurrentUri => Browser.Source?.ToString();

    public async Task InitializeAsync()
    {
        if (_initialized)
        {
            return;
        }

        await Browser.EnsureCoreWebView2Async();

        Browser.CoreWebView2.NavigationStarting += CoreWebView2OnNavigationStarting;
        Browser.CoreWebView2.NewWindowRequested += CoreWebView2OnNewWindowRequested;
        Browser.NavigationCompleted += BrowserOnNavigationCompleted;
        Browser.CoreWebView2.Settings.IsZoomControlEnabled = true;
        Browser.CoreWebView2.Settings.AreDefaultContextMenusEnabled = true;

        _initialized = true;
    }

    public async Task ReloadAsync()
    {
        await NavigateWithWarmupAsync(_initialRoute);
    }

    public async Task ForwardShortcutAsync(string key, bool ctrl, bool shift)
    {
        if (!_initialized)
        {
            return;
        }

        var lowerKey = key.ToLowerInvariant();
                var script = "(() => {"
                        + $"const options = {{ key: '{lowerKey}', ctrlKey: {ctrl.ToString().ToLowerInvariant()}, shiftKey: {shift.ToString().ToLowerInvariant()}, bubbles: true }};"
                        + "document.dispatchEvent(new KeyboardEvent('keydown', options));"
                        + "document.dispatchEvent(new KeyboardEvent('keyup', options));"
                        + "})();";
        await Browser.ExecuteScriptAsync(script);
    }

    public void ForceClose()
    {
        _forceClose = true;
        Close();
    }

    private async void RootGrid_OnLoaded(object sender, RoutedEventArgs e)
    {
        await NavigateWithWarmupAsync(_initialRoute);
    }

    private async Task NavigateWithWarmupAsync(string route)
    {
        await InitializeAsync();
        Browser.NavigateToString(_controller.BuildWarmupHtml("Loading Starsky", "Checking service health and version compatibility..."));

        using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(60));
        var baseUri = _controller.EffectiveBaseUri;
        var isHealthy = await _controller.Warmup.WaitForServerAsync(baseUri, cancellationTokenSource.Token);
        if (!isHealthy)
        {
            Browser.NavigateToString(_controller.BuildWarmupHtml(
                "Starsky could not be reached",
                $"There was an error loading {baseUri}",
                isError: true));
            return;
        }

        var versionResult = await _controller.Warmup.CheckVersionAsync(baseUri, cancellationTokenSource.Token);
        if (versionResult == false)
        {
            Browser.NavigateToString(_controller.BuildWarmupHtml(
                "Desktop version is outdated",
                "Go to Help -> Release Overview and download a newer version.",
                isUpgrade: true));
            return;
        }

        if (versionResult is null)
        {
            Browser.NavigateToString(_controller.BuildWarmupHtml(
                "Version check failed",
                "The app could not verify compatibility with the target server.",
                isError: true));
            return;
        }

        Browser.Source = new Uri(AppController.CombineBaseUriAndRoute(baseUri, route));
    }

    private void CoreWebView2OnNavigationStarting(CoreWebView2 sender, CoreWebView2NavigationStartingEventArgs args)
    {
        if (!_controller.IsNavigationAllowed(args.Uri))
        {
            args.Cancel = true;
        }
    }

    private async void CoreWebView2OnNewWindowRequested(CoreWebView2 sender, CoreWebView2NewWindowRequestedEventArgs args)
    {
        args.Handled = true;

        if (!Uri.TryCreate(args.Uri, UriKind.Absolute, out var targetUri))
        {
            return;
        }

        var route = targetUri.PathAndQuery;
        await _controller.OpenMainWindowAsync(new Models.WindowStateInfo { Route = route });
    }

    private async void BrowserOnNavigationCompleted(WebView2 sender, CoreWebView2NavigationCompletedEventArgs args)
    {
        if (Browser.Source is null)
        {
            return;
        }

        if (Browser.Source.Scheme is "about" or "file")
        {
            return;
        }

        var route = Browser.Source.PathAndQuery;
        await _controller.PersistMainWindowStateAsync(this, route);
    }

    private async void OnClosed(object sender, WindowEventArgs args)
    {
        if (!_forceClose)
        {
            await _controller.PersistMainWindowStateAsync(this, Browser.Source?.PathAndQuery ?? _initialRoute);
        }

        _controller.RemoveMainWindow(this);
    }

    private async void NewWindow_Click(object sender, RoutedEventArgs e)
    {
        await _controller.OpenMainWindowAsync(new Models.WindowStateInfo { Route = "?f=/" }, 20);
    }

    private async void EditFile_Click(object sender, RoutedEventArgs e)
    {
        await _controller.EditCurrentFileAsync(this);
    }

    private async void ConnectionSettings_Click(object sender, RoutedEventArgs e)
    {
        await _controller.OpenSettingsWindowAsync();
    }

    private async void AppSettingsShortcut_Click(object sender, RoutedEventArgs e)
    {
        await _controller.ForwardShortcutAsync(this, "k", true, true);
    }

    private async void Refresh_Click(object sender, RoutedEventArgs e)
    {
        await ReloadAsync();
    }

    private void DevTools_Click(object sender, RoutedEventArgs e)
    {
        Browser.CoreWebView2?.OpenDevToolsWindow();
    }

    private async void OpenInBrowser_Click(object sender, RoutedEventArgs e)
    {
        await _controller.OpenCurrentPageInBrowserAsync(this);
    }

    private async void Documentation_Click(object sender, RoutedEventArgs e)
    {
        await _controller.OpenDocumentationAsync();
    }

    private async void ReleaseOverview_Click(object sender, RoutedEventArgs e)
    {
        await _controller.OpenReleasesAsync();
    }
}