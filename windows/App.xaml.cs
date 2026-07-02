using Microsoft.UI.Xaml;

namespace Starsky.Windows;

public partial class App : Application
{
    private Services.AppController? _controller;

    public App()
    {
        InitializeComponent();
        UnhandledException += OnUnhandledException;
    }

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        base.OnLaunched(args);

        _controller = new Services.AppController();
        await _controller.StartAsync();
    }

    private void OnUnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        _controller?.Logger.Error("Unhandled exception", e.Exception);
        _controller?.ShowError(e.Exception.ToString());
        e.Handled = true;
    }
}