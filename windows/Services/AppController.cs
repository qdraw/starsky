using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Starsky.Windows.Models;
using Starsky.Windows.Views;
using System.Collections.Concurrent;

namespace Starsky.Windows.Services;

public sealed class AppController
{
    private readonly DispatcherQueue _dispatcherQueue;
    private readonly HttpClient _httpClient = new();
    private readonly List<MainWindow> _mainWindows = new();
    private readonly List<SettingsWindow> _settingsWindows = new();
    private readonly ConcurrentDictionary<MainWindow, WindowStateInfo> _trackedMainWindows = new();

    private SplashWindow? _splashWindow;
    private UpdateWarningWindow? _updateWarningWindow;

    public AppController()
    {
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
        Paths = new AppPaths();
        Logger = new FileLogger(Paths);
        SettingsStore = new SettingsStore(Paths);
        BackendProcess = new BackendProcessService(Paths, Logger);
        Warmup = new BrowserWarmupService(_httpClient);
        BackendApi = new BackendApiClient(_httpClient);
        Workspace = new FileWorkspaceService(Paths);
        FileWatcher = new FileWatcherService(Logger);
        ExternalOpen = new ExternalOpenService();
    }

    public AppPaths Paths { get; }

    public FileLogger Logger { get; }

    public SettingsStore SettingsStore { get; }

    public BackendProcessService BackendProcess { get; }

    public BrowserWarmupService Warmup { get; }

    public BackendApiClient BackendApi { get; }

    public FileWorkspaceService Workspace { get; }

    public FileWatcherService FileWatcher { get; }

    public ExternalOpenService ExternalOpen { get; }

    public AppSettings Settings { get; private set; } = new();

    public bool IsRemoteMode => Settings.Mode == LocationMode.Remote;

    public Uri EffectiveBaseUri => IsRemoteMode && !string.IsNullOrWhiteSpace(Settings.RemoteUrl)
        ? new Uri(Settings.RemoteUrl!)
        : BackendProcess.LocalBaseUri;

    public async Task StartAsync()
    {
        try
        {
            Paths.EnsureDirectories();
            Logger.Info("Starting Windows shell");
            Settings = await SettingsStore.LoadAsync();
            Logger.Info("Settings loaded");

            FileWatcher.Start(Workspace.GetWorkspaceRoot());
            FileWatcher.FileChanged += (_, fullPath) => Logger.Info($"Forward file change pipeline for {fullPath}");
            Logger.Info("File watcher started");

            if (!IsRemoteMode)
            {
                Logger.Info($"Local mode: starting backend at {Paths.ResolveBackendExecutablePath()}");
                await BackendProcess.StartAsync();
                Logger.Info($"Backend started on port {BackendProcess.Port}. Waiting for health check.");
                using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(120));
                var healthy = await Warmup.WaitForServerAsync(BackendProcess.LocalBaseUri, cancellationTokenSource.Token);
                Logger.Info($"Health check result: {healthy}");
                if (!healthy)
                {
                    throw new TimeoutException($"Starsky backend on port {BackendProcess.Port} did not become healthy within 120 seconds.");
                }
            }
            else
            {
                Logger.Info("Remote mode: skipping backend startup");
            }

            Logger.Info("Restoring main windows");
            await RestoreMainWindowsAsync();
            Logger.Info("Main windows restored");

            _ = ScheduleUpdateCheckAsync();
        }
        catch (Exception exception)
        {
            Logger.Error($"Startup failed at: {exception.Message}", exception);
            _splashWindow?.Close();
            _splashWindow = null;
            ShowError($"Startup failed.\n\n{exception}");
        }
    }

    public async Task RestoreMainWindowsAsync()
    {
        var windows = Settings.MainWindows.Count > 0
            ? Settings.MainWindows.ToList()
            : new List<WindowStateInfo> { new() };

        for (var i = 0; i < windows.Count; i++)
        {
            await OpenMainWindowAsync(windows[i], i * 20);
        }
    }

    public async Task<MainWindow> OpenMainWindowAsync(WindowStateInfo? state = null, int offset = 0)
    {
        var mainWindow = new MainWindow(this, state?.Route ?? "?f=/");
        _mainWindows.Add(mainWindow);
        _trackedMainWindows[mainWindow] = state ?? new WindowStateInfo();
        mainWindow.Activate();
        WindowPlacementHelper.Apply(mainWindow, state, offset);
        await mainWindow.InitializeAsync();
        await PersistMainWindowsAsync();
        return mainWindow;
    }

    public async Task OpenSettingsWindowAsync()
    {
        var settingsWindow = new SettingsWindow(this);
        _settingsWindows.Add(settingsWindow);
        settingsWindow.Activate();
        WindowPlacementHelper.Apply(settingsWindow, Settings.SettingsWindow);
        await settingsWindow.InitializeAsync();
    }

    public async Task SwitchModeAsync(LocationMode mode)
    {
        if (Settings.Mode == mode)
        {
            return;
        }

        Settings.Mode = mode;
        Settings.MainWindows.Clear();
        await SettingsStore.SaveAsync(Settings);

        FileWatcher.Start(Workspace.GetWorkspaceRoot());

        var settingsWindows = _settingsWindows.ToList();
        foreach (var mainWindow in _mainWindows.ToList())
        {
            mainWindow.ForceClose();
        }

        if (!IsRemoteMode)
        {
            await BackendProcess.StartAsync();
        }

        await OpenMainWindowAsync(new WindowStateInfo { Route = "?f=/" });

        foreach (var settingsWindow in settingsWindows)
        {
            settingsWindow.Activate();
            await settingsWindow.RefreshAsync();
        }
    }

    public async Task<UrlValidationResult> ValidateAndSaveRemoteUrlAsync(string location)
    {
        var result = await BackendApi.ValidateRemoteUrlAsync(location, CancellationToken.None);
        if (result.IsValid)
        {
            Settings.RemoteUrl = result.Location;
            await SettingsStore.SaveAsync(Settings);
        }

        return result;
    }

    public async Task SetUpdatePolicyAsync(bool enabled)
    {
        Settings.UpdatePolicyEnabled = enabled;
        await SettingsStore.SaveAsync(Settings);
    }

    public async Task ReloadAllAsync()
    {
        foreach (var mainWindow in _mainWindows.ToList())
        {
            await mainWindow.ReloadAsync();
        }
    }

    public async Task OpenCurrentPageInBrowserAsync(MainWindow window)
    {
        if (!string.IsNullOrWhiteSpace(window.CurrentUri))
        {
            await ExternalOpen.OpenUriAsync(window.CurrentUri);
        }
    }

    public Task OpenDocumentationAsync()
    {
        return ExternalOpen.OpenUriAsync("https://docs.qdraw.nl/docs/getting-started/first-steps");
    }

    public Task OpenReleasesAsync()
    {
        return ExternalOpen.OpenUriAsync("https://github.com/qdraw/starsky/releases/latest");
    }

    public async Task ForwardShortcutAsync(MainWindow window, string key, bool ctrl, bool shift)
    {
        await window.ForwardShortcutAsync(key, ctrl, shift);
    }

    public async Task EditCurrentFileAsync(MainWindow window)
    {
        if (!IsRemoteMode)
        {
            await window.ForwardShortcutAsync("e", true, false);
            return;
        }

        var filePath = ExtractQueryValue(window.CurrentUri, "f");
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return;
        }

        var detail = await BackendApi.GetDetailViewAsync(EffectiveBaseUri, filePath, CancellationToken.None);
        if (detail is null)
        {
            ShowError("The current page did not return a Starsky detail payload.");
            return;
        }

        var sidecarTarget = Workspace.GetSidecarTargetPath(detail);
        if (!string.IsNullOrWhiteSpace(sidecarTarget) && detail.SidecarExtensionsList.Count > 0)
        {
            var sidecarSubPath = Workspace.BuildSidecarSubPath(detail);
            var sidecarUri = new Uri(EffectiveBaseUri,
                $"/starsky/api/download-sidecar?f={Uri.EscapeDataString(sidecarSubPath)}");
            await BackendApi.DownloadToFileAsync(sidecarUri, sidecarTarget, CancellationToken.None);
        }

        var binaryTarget = Workspace.GetBinaryTargetPath(detail);
        var temporaryBinary = binaryTarget + ".tmp";
        var lastBinaryPath = detail.CollectionPaths.Last(path => !path.EndsWith("xmp", StringComparison.OrdinalIgnoreCase));
        var downloadUri = new Uri(EffectiveBaseUri,
            $"/starsky/api/download-photo?isThumbnail=false&f={Uri.EscapeDataString(lastBinaryPath)}&cache=false");
        await BackendApi.DownloadToFileAsync(downloadUri, temporaryBinary, CancellationToken.None);

        if (File.Exists(binaryTarget))
        {
            File.Delete(binaryTarget);
        }

        File.Move(temporaryBinary, binaryTarget);

        try
        {
            await ExternalOpen.OpenFileAsync(binaryTarget);
        }
        catch (Exception exception)
        {
            ShowError(exception.ToString());
        }
    }

    public async Task PersistMainWindowStateAsync(MainWindow window, string route)
    {
        _trackedMainWindows[window] = WindowPlacementHelper.Capture(window, route);
        await PersistMainWindowsAsync();
    }

    public async Task PersistSettingsWindowStateAsync(SettingsWindow window)
    {
        Settings.SettingsWindow = WindowPlacementHelper.Capture(window, "settings");
        await SettingsStore.SaveAsync(Settings);
    }

    public void RemoveMainWindow(MainWindow window)
    {
        _mainWindows.Remove(window);
        _trackedMainWindows.TryRemove(window, out _);
        _ = PersistMainWindowsAsync();
        TryExitWhenAllWindowsAreClosed();
    }

    public void RemoveSettingsWindow(SettingsWindow window)
    {
        _settingsWindows.Remove(window);
        TryExitWhenAllWindowsAreClosed();
    }

    public void ShowError(string message)
    {
        _dispatcherQueue.TryEnqueue(() =>
        {
            var errorWindow = new ErrorWindow(message);
            errorWindow.Activate();
        });
    }

    public bool IsNavigationAllowed(string uri)
    {
        if (string.IsNullOrWhiteSpace(uri))
        {
            return false;
        }

        if (uri.StartsWith("about:", StringComparison.OrdinalIgnoreCase) ||
            uri.StartsWith("file:", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (!Uri.TryCreate(uri, UriKind.Absolute, out var targetUri))
        {
            return false;
        }

        if (targetUri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return targetUri.GetLeftPart(UriPartial.Authority)
            .Equals(EffectiveBaseUri.GetLeftPart(UriPartial.Authority), StringComparison.OrdinalIgnoreCase);
    }

    public string BuildWarmupHtml(string title, string body, bool isError = false, bool isUpgrade = false)
    {
        var accent = isError ? "#8f1d1d" : isUpgrade ? "#876000" : "#0f172a";
                return $@"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""utf-8"" />
    <title>{title}</title>
    <style>
        body {{ font-family: Segoe UI, sans-serif; background: linear-gradient(160deg, #eff6ff, #ffffff); margin: 0; display: grid; place-items: center; min-height: 100vh; }}
        .card {{ width: min(520px, 88vw); padding: 32px; border-radius: 18px; background: white; box-shadow: 0 18px 50px rgba(15, 23, 42, 0.14); color: #0f172a; }}
        .title {{ margin: 0 0 12px; font-size: 28px; font-weight: 700; color: {accent}; }}
        .body {{ font-size: 16px; line-height: 1.55; white-space: pre-wrap; }}
    </style>
</head>
<body>
    <section class=""card"">
        <h1 class=""title"">{title}</h1>
        <div class=""body"">{body}</div>
    </section>
</body>
</html>";
    }

    public static string CombineBaseUriAndRoute(Uri baseUri, string route)
    {
        var baseText = baseUri.ToString().TrimEnd('/');
        if (string.IsNullOrWhiteSpace(route))
        {
            return baseText + "/?f=/";
        }

        if (route.StartsWith("?", StringComparison.Ordinal))
        {
            return baseText + "/" + route;
        }

        if (route.StartsWith("/", StringComparison.Ordinal))
        {
            return baseText + route;
        }

        return baseText + "/" + route;
    }

    private async Task PersistMainWindowsAsync()
    {
        Settings.MainWindows = _trackedMainWindows.Values.OrderBy(state => state.X).ToList();
        await SettingsStore.SaveAsync(Settings);
    }

    private async Task ScheduleUpdateCheckAsync()
    {
        await Task.Delay(1000);

        if (!Settings.UpdatePolicyEnabled)
        {
            return;
        }

        if (Settings.LastUpdateWarningUtc is not null &&
            DateTimeOffset.UtcNow - Settings.LastUpdateWarningUtc.Value < TimeSpan.FromDays(4))
        {
            return;
        }

        using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var shouldShow = await Warmup.ShouldShowUpdateWarningAsync(EffectiveBaseUri, cancellationTokenSource.Token);
        if (!shouldShow)
        {
            return;
        }

        await _dispatcherQueue.EnqueueAsync(() =>
        {
            _updateWarningWindow = new UpdateWarningWindow(this);
            _updateWarningWindow.Activate();
        });
    }

    public async Task MarkUpdateWarningSeenAsync()
    {
        Settings.LastUpdateWarningUtc = DateTimeOffset.UtcNow;
        await SettingsStore.SaveAsync(Settings);
    }

    public async Task ShutdownAsync()
    {
        Logger.Info("Shutting down Windows shell");
        FileWatcher.Stop();
        BackendProcess.Stop();
        await PersistMainWindowsAsync();
    }

    private void TryExitWhenAllWindowsAreClosed()
    {
        if (_mainWindows.Count > 0 || _settingsWindows.Count > 0 || _updateWarningWindow is not null)
        {
            return;
        }

        _ = ShutdownAndExitAsync();
    }

    private async Task ShutdownAndExitAsync()
    {
        await ShutdownAsync();
        await _dispatcherQueue.EnqueueAsync(() => Application.Current.Exit());
    }

    private static string? ExtractQueryValue(string? absoluteUri, string key)
    {
        if (string.IsNullOrWhiteSpace(absoluteUri) || !Uri.TryCreate(absoluteUri, UriKind.Absolute, out var uri))
        {
            return null;
        }

        foreach (var pair in uri.Query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = pair.Split('=', 2);
            if (parts.Length == 2 && parts[0] == key)
            {
                return Uri.UnescapeDataString(parts[1]);
            }
        }

        return null;
    }

    public void ClearUpdateWarningWindow(UpdateWarningWindow window)
    {
        if (ReferenceEquals(_updateWarningWindow, window))
        {
            _updateWarningWindow = null;
        }

        TryExitWhenAllWindowsAreClosed();
    }
}