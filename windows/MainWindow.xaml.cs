using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Text;
using Microsoft.Web.WebView2.Core;
using Starsky.Windows.Services;

namespace Starsky.Windows;

public sealed partial class MainWindow
{
    private sealed class BrowserTab
    {
        public required Border TabBorder { get; init; }
        public required TextBlock TitleText { get; init; }
        public required Button CloseButton { get; init; }
        public required WebView2 Browser { get; init; }
    }

    private readonly AppController _controller;
    private readonly string _initialRoute;
    private readonly SemaphoreSlim _initializeGate = new(1, 1);
    private readonly List<BrowserTab> _tabs = new();
    private BrowserTab? _activeTab;
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
        Closed += OnClosed;
    }

    private WebView2? ActiveBrowser => _activeTab?.Browser;

    // Tab styling helpers
    private static readonly Microsoft.UI.Xaml.Media.SolidColorBrush ActiveTabBgBrush =
        new(Microsoft.UI.ColorHelper.FromArgb(255, 243, 243, 243));
    private static readonly Microsoft.UI.Xaml.Media.SolidColorBrush InactiveTabBgBrush =
        new(Microsoft.UI.ColorHelper.FromArgb(255, 225, 228, 232));
    private static readonly Microsoft.UI.Xaml.Media.SolidColorBrush ActiveIndicatorBrush =
        new(Microsoft.UI.ColorHelper.FromArgb(255, 0, 120, 215));
    private static readonly Microsoft.UI.Xaml.Media.SolidColorBrush TransparentBrush =
        new(Microsoft.UI.ColorHelper.FromArgb(0, 0, 0, 0));

    public void ForceClose()
    {
        _forceClose = true;
        Close();
    }

    public string? CurrentUri => _currentUri;

    public async Task ReloadAsync()
    {
        await InitializeAsync();
        ActiveBrowser?.CoreWebView2?.Reload();
    }

    public async Task ForwardShortcutAsync(string key, bool ctrl, bool shift)
    {
        await InitializeAsync();
        var activeBrowser = ActiveBrowser;
        if (activeBrowser?.CoreWebView2 is null)
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

        await activeBrowser.CoreWebView2.ExecuteScriptAsync(script);
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

            await OpenTabAsync(_initialRoute, selectTab: true, treatAsRoute: true);
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
            await OpenTabAsync("?f=/", selectTab: true, treatAsRoute: true);
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

    private void Refresh_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (ActiveBrowser?.CoreWebView2 is not null)
            {
                ActiveBrowser.CoreWebView2.Reload();
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
            if (ActiveBrowser?.CoreWebView2 is not null)
            {
                ActiveBrowser.CoreWebView2.OpenDevToolsWindow();
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

    private async Task OpenTabAsync(string routeOrUri, bool selectTab, bool treatAsRoute)
    {
        var browser = new WebView2();
        browser.NavigationStarting += OnNavigationStarting;
        browser.NavigationCompleted += OnNavigationCompleted;
        browser.Visibility = Visibility.Collapsed;

        // Title text block
        var titleText = new TextBlock
        {
            Text = "New Tab",
            VerticalAlignment = VerticalAlignment.Center,
            FontSize = 12,
            MaxWidth = 180,
            TextTrimming = TextTrimming.CharacterEllipsis,
            Margin = new Thickness(10, 0, 6, 0),
        };

        // Close button (×)
        var closeButton = new Button
        {
            Content = "✕",
            Padding = new Thickness(4, 0, 4, 0),
            MinWidth = 20,
            MinHeight = 20,
            FontSize = 10,
            Background = TransparentBrush,
            BorderBrush = TransparentBrush,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 6, 0),
        };

        // Inner row: [title] [×]
        var row = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
        row.Children.Add(titleText);
        row.Children.Add(closeButton);

        // Active indicator bar at bottom
        var indicator = new Border
        {
            Height = 3,
            CornerRadius = new CornerRadius(2, 2, 0, 0),
            Background = TransparentBrush,
            VerticalAlignment = VerticalAlignment.Bottom,
        };

        // Tab outer stack: [row] [indicator]
        var tabStack = new StackPanel { Orientation = Orientation.Vertical };
        tabStack.Children.Add(row);
        tabStack.Children.Add(indicator);

        // Outer border gives the tab its background + right separator
        var tabBorder = new Border
        {
            Child = tabStack,
            Background = InactiveTabBgBrush,
            BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(60, 0, 0, 0)),
            BorderThickness = new Thickness(0, 0, 1, 0),
            MinWidth = 80,
            Padding = new Thickness(0, 4, 0, 0),
        };

        var tab = new BrowserTab
        {
            TabBorder = tabBorder,
            TitleText = titleText,
            CloseButton = closeButton,
            Browser = browser,
        };

        // Click anywhere on the tab border to activate
        tabBorder.Tapped += (_, _) => ActivateTab(tab);
        closeButton.Click += (_, _) => CloseTab(tab);

        BrowserHost.Children.Add(browser);
        TabStripPanel.Children.Add(tabBorder);
        _tabs.Add(tab);

        if (_tabs.Count == 1)
        {
            closeButton.Visibility = Visibility.Collapsed;
        }

        await browser.EnsureCoreWebView2Async();
        browser.CoreWebView2.NewWindowRequested += OnNewWindowRequested;
        browser.CoreWebView2.DocumentTitleChanged += (_, _) =>
        {
            var title = browser.CoreWebView2.DocumentTitle
	            .Replace("- Starsky App", string.Empty);
            if (string.IsNullOrWhiteSpace(title))
            {
                return;
            }

            // Must dispatch to UI thread
            DispatcherQueue.TryEnqueue(() => tab.TitleText.Text = title);
        };

        var navigationTarget = treatAsRoute || !Uri.TryCreate(routeOrUri, UriKind.Absolute, out _)
            ? AppController.CombineBaseUriAndRoute(_controller.EffectiveBaseUri, routeOrUri)
            : routeOrUri;

        _controller.Logger.Info($"MainWindow.InitializeAsync: navigating to {navigationTarget}");
        browser.Source = new Uri(navigationTarget);

        if (selectTab || _activeTab is null)
        {
            ActivateTab(tab);
        }

        if (_tabs.Count > 1)
        {
            foreach (var entry in _tabs)
            {
                entry.CloseButton.Visibility = Visibility.Visible;
            }
        }
    }

    private void ApplyTabStyle(BrowserTab tab, bool isActive)
    {
        tab.TabBorder.Background = isActive ? ActiveTabBgBrush : InactiveTabBgBrush;

        // Find the indicator border (last child of the tabStack)
        if (tab.TabBorder.Child is StackPanel tabStack && tabStack.Children.Last() is Border indicator)
        {
            indicator.Background = isActive ? ActiveIndicatorBrush : TransparentBrush;
        }

        tab.TitleText.FontWeight = isActive ? FontWeights.SemiBold : FontWeights.Normal;
    }

    private void ActivateTab(BrowserTab tab)
    {
        _activeTab = tab;
        foreach (var entry in _tabs)
        {
            var isActive = ReferenceEquals(entry, tab);
            entry.Browser.Visibility = isActive ? Visibility.Visible : Visibility.Collapsed;
            ApplyTabStyle(entry, isActive);
        }

        if (tab.Browser.Source is not null)
        {
            _currentUri = tab.Browser.Source.AbsoluteUri;
        }
    }

    private void CloseTab(BrowserTab tab)
    {
        if (_tabs.Count <= 1)
        {
            return;
        }

        tab.Browser.NavigationStarting -= OnNavigationStarting;
        tab.Browser.NavigationCompleted -= OnNavigationCompleted;
        if (tab.Browser.CoreWebView2 is not null)
        {
            tab.Browser.CoreWebView2.NewWindowRequested -= OnNewWindowRequested;
        }

        BrowserHost.Children.Remove(tab.Browser);
        TabStripPanel.Children.Remove(tab.TabBorder);
        _tabs.Remove(tab);

        if (_tabs.Count == 1)
        {
            _tabs[0].CloseButton.Visibility = Visibility.Collapsed;
        }

        if (ReferenceEquals(_activeTab, tab) && _tabs.Count > 0)
        {
            ActivateTab(_tabs[^1]);
        }
    }

    private async void OnNewWindowRequested(CoreWebView2 sender, CoreWebView2NewWindowRequestedEventArgs args)
    {
	    try
	    {
		    using var deferral = args.GetDeferral();
		    try
		    {
			    if (string.IsNullOrWhiteSpace(args.Uri))
			    {
				    return;
			    }

			    args.Handled = true;
			    if (!_controller.IsNavigationAllowed(args.Uri))
			    {
				    await _controller.ExternalOpen.OpenUriAsync(args.Uri);
				    return;
			    }

			    await OpenTabAsync(args.Uri, selectTab: true, treatAsRoute: false);
		    }
		    catch (Exception exception)
		    {
			    _controller.Logger.Error("OnNewWindowRequested failed", exception);
			    _controller.ShowError(exception.ToString());
		    }
	    }
	    catch (Exception)
	    {
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
            if (ReferenceEquals(sender, ActiveBrowser))
            {
                _currentUri = sender.Source.AbsoluteUri;
            }

            // Update tab title as fallback (DocumentTitleChanged handles the real title)
            foreach (var tab in _tabs)
            {
                if (!ReferenceEquals(tab.Browser, sender))
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(tab.TitleText.Text) || tab.TitleText.Text == "New Tab")
                {
                    tab.TitleText.Text = sender.Source.Host;
                }

                break;
            }
        }

        if (!args.IsSuccess)
        {
            _controller.Logger.Warn($"MainWindow.OnNavigationCompleted failed: {args.WebErrorStatus}");
        }
    }

    private async void OnClosed(object sender, WindowEventArgs args)
    {
        _controller.Logger.Info($"MainWindow.OnClosed fired. forceClose={_forceClose}");
        foreach (var tab in _tabs)
        {
            tab.Browser.NavigationStarting -= OnNavigationStarting;
            tab.Browser.NavigationCompleted -= OnNavigationCompleted;
            if (tab.Browser.CoreWebView2 is not null)
            {
                tab.Browser.CoreWebView2.NewWindowRequested -= OnNewWindowRequested;
            }
        }

        if (!_forceClose)
        {
            await _controller.PersistMainWindowStateAsync(this, _initialRoute);
        }

        _controller.RemoveMainWindow(this);
    }
}
