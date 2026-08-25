using System.Diagnostics.CodeAnalysis;
using System.Windows;
using Microsoft.Extensions.Logging;
using Starsky.Desktop.Models;
using Starsky.Desktop.Services;
using Starsky.Desktop.Windows;

namespace Starsky.Desktop;

[ExcludeFromCodeCoverage]
public partial class App : Application
{
    private const string AppVersion = "0.8.1";

    private ILogger<App>? _logger;
    private BackendService? _backend;
    private FileWatcherService? _watcher;
    private WindowManager? _windowManager;
    private int _localPort;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // 1. Ensure directories
        ApplicationPaths.EnsureDirectories();

        // 2. Initialize logging
        var logFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            builder.AddProvider(new DailyFileLoggerProvider(ApplicationPaths.LogsDir));
            builder.SetMinimumLevel(LogLevel.Information);
        });
        _logger = logFactory.CreateLogger<App>();
        _logger.LogInformation("Starsky Desktop starting");

        // 3. Load settings
        var settingsService = new SettingsService(logFactory.CreateLogger<SettingsService>());
        settingsService.Load();

        // 4. Initialize services
        _backend = new BackendService(logFactory.CreateLogger<BackendService>());
        _watcher = new FileWatcherService(logFactory.CreateLogger<FileWatcherService>());
        var webViewEnv = new WebViewEnvironmentService();
        var navigation = new NavigationService(settingsService);
        var routes = new RoutePersistenceService(settingsService);
        var fileDownload = new FileDownloadService(logFactory.CreateLogger<FileDownloadService>());
        var updateService = new UpdateService(settingsService, logFactory.CreateLogger<UpdateService>());

        _windowManager = new WindowManager(
            settingsService, routes, navigation, webViewEnv, fileDownload,
            logFactory.CreateLogger<WindowManager>());

        // 5. Show splash
        var splash = new SplashWindow();
        splash.Show();

        try
        {
            if (settingsService.Current.Mode == RuntimeMode.Local)
            {
                // 6a. Start local backend
                _localPort = PortFinder.FindFreePort();
                _windowManager.SetLocalPort(_localPort);
                splash.UpdateStatus("Starting backend…");
                _logger.LogInformation("Using port {Port}", _localPort);
                await _backend.StartAsync(_localPort);

                // 6b. Wait for health
                splash.UpdateStatus("Waiting for backend…");
                using var healthHttp = new HttpClient { Timeout = TimeSpan.FromSeconds(3) };
                await BackendService.WaitForHealthAsync(
                    healthHttp, $"http://localhost:{_localPort}",
                    msg => splash.UpdateStatus(msg));

                // 6c. Version compatibility
                splash.UpdateStatus("Checking version…");
                using var versionHttp = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
                await BackendService.CheckVersionCompatibilityAsync(
                    versionHttp, $"http://localhost:{_localPort}", AppVersion);
            }
            else
            {
                // 6d. Remote mode — validate URL exists
                var remoteUrl = settingsService.Current.RemoteBaseUrl;
                if (string.IsNullOrWhiteSpace(remoteUrl))
                {
                    splash.Close();
                    new ErrorWindow("No remote server URL configured.\nOpen Settings → Connection Settings to set one.")
                        .ShowDialog();
                    Shutdown();
                    return;
                }
            }

            // 7. Start file watcher
            _watcher.Start();

            // 8. Restore windows
            splash.UpdateStatus("Opening Starsky…");
            _windowManager.RestoreWindows();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Startup failed");
            splash.Close();
            new ErrorWindow($"Startup failed:\n{ex.Message}").ShowDialog();
            Shutdown();
            return;
        }

        // 9. Close splash
        splash.Close();

        // 10. Schedule update check
        _ = Task.Delay(5000).ContinueWith(async _ =>
        {
            if (await updateService.CheckAsync())
            {
                await Dispatcher.InvokeAsync(() => new UpdateWindow(updateService).Show());
            }
        });
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        _logger?.LogInformation("Starsky Desktop shutting down");
        _watcher?.Stop();
        _windowManager?.CloseAll();
        if (_backend != null)
        {
	        await _backend.StopAsync();
        }

        _logger?.LogInformation("Shutdown complete");
        base.OnExit(e);
    }

    [SuppressMessage("Style", "S2325:Remove unused parameter", Justification = "Required by WPF")]
    public void Connect(int connectionId, object target)
    {
	    throw new NotImplementedException();
    }
}
