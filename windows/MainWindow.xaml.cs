using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using Starsky.Windows.Services;

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
