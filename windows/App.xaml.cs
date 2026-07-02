using Microsoft.UI.Xaml;

namespace Starsky.Windows;

public partial class App : Application
{
    private Services.AppController? _controller;

    public App()
    {
        InitializeComponent();
        UnhandledException += OnUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnCurrentDomainUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
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

    private void OnCurrentDomainUnhandledException(object sender, System.UnhandledExceptionEventArgs e)
    {
        if (_controller is null)
        {
            return;
        }

        if (e.ExceptionObject is Exception exception)
        {
            _controller.Logger.Error($"AppDomain unhandled exception. IsTerminating={e.IsTerminating}", exception);
            return;
        }

        _controller.Logger.Error($"AppDomain unhandled non-exception object. IsTerminating={e.IsTerminating}");
    }

    private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        _controller?.Logger.Error("Unobserved task exception", e.Exception);
        e.SetObserved();
    }
}
