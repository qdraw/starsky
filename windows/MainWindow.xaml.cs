using Microsoft.UI.Xaml;
using Starsky.Windows.Services;

namespace Starsky.Windows;

public sealed partial class MainWindow : Window
{
    private readonly AppController _controller;
    private readonly string _initialRoute;
    private bool _forceClose;

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

    public void ForceClose()
    {
        _forceClose = true;
        Close();
    }

    public string? CurrentUri => null;

    public Task ReloadAsync() => Task.CompletedTask;

    public Task ForwardShortcutAsync(string key, bool ctrl, bool shift) => Task.CompletedTask;

    public Task InitializeAsync() => Task.CompletedTask;

    private async void OnClosed(object sender, WindowEventArgs args)
    {
        _controller.Logger.Info($"MainWindow.OnClosed fired. forceClose={_forceClose}");
        if (!_forceClose)
        {
            await _controller.PersistMainWindowStateAsync(this, _initialRoute);
        }

        _controller.RemoveMainWindow(this);
    }
}
