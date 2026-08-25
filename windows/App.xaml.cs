using System.IO;
using System.Net.Http;
using System.Windows;
using Microsoft.Extensions.Logging;
using Starsky.Desktop.Models;
using Starsky.Desktop.Services;
using Starsky.Desktop.Windows;

namespace Starsky.Desktop;

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
        _logger.LogInformation("Starsky Desktop {Version} starting", AppVersion);

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
                await WaitForHealthAsync($"http://localhost:{_localPort}", splash);

                // 6c. Version compatibility
                splash.UpdateStatus("Checking version…");
                await CheckVersionAsync($"http://localhost:{_localPort}");
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
                Dispatcher.Invoke(() => new UpdateWindow(updateService).Show());
            }
        });
    }

    private static async Task WaitForHealthAsync(string baseUrl, SplashWindow splash)
    {
        using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(3) };
        var deadline = DateTime.UtcNow.AddSeconds(60);

        while (DateTime.UtcNow < deadline)
        {
            try
            {
                var resp = await http.GetAsync($"{baseUrl}/api/health");
                if (resp.IsSuccessStatusCode)
                {
	                return;
                }
            }
            catch { /* not yet ready */ }

            splash.UpdateStatus("Waiting for backend…");
            await Task.Delay(1000);
        }

        throw new TimeoutException("Backend did not become ready within 60 seconds.");
    }

    private static async Task CheckVersionAsync(string baseUrl)
    {
        using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
        var resp = await http.PostAsync($"{baseUrl}/api/health/version?version={AppVersion}", null);

        if (resp.StatusCode == System.Net.HttpStatusCode.BadRequest)
        {
	        throw new InvalidOperationException($"This version ({AppVersion}) is incompatible with the server. Please update Starsky Desktop.");
        }
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
}
