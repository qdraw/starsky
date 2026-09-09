using System.Diagnostics.CodeAnalysis;
using System.Windows;
using Microsoft.Extensions.Logging;
using Starsky.Desktop.Models;
using Starsky.Desktop.Windows;

namespace Starsky.Desktop.Services;

[SuppressMessage("Sonar",
	"S107: Methods should not have too many parameters",
	Justification = "Needed")]
public class WindowManager(
	SettingsService settings,
	RoutePersistenceService routes,
	NavigationService navigation,
	WebViewEnvironmentService webViewEnv,
	FileDownloadService fileDownload,
	FileWatcherService watcher,
	UpdateService updateService,
	ILogger<WindowManager> logger,
	IMountWatcherService? mountWatcherService = null)
{
	private readonly List<MainWindow> _mainWindows = [];
    private int? _localPort;

    public bool IsTerminating { get; private set; }

    // True when this is the only window left; used by MainWindow_Closing to decide
    // whether to save itself (last window = app is exiting, keep the state).
    internal bool IsLastOpenWindow => _mainWindows.Count == 1;

    public void SetLocalPort(int port) => _localPort = port;

    internal static SavedWindowState ResolveGeometry(
        SavedWindowState? geometry, int offset, ILogger? logger = null)
    {
        if (geometry != null && IsOnScreen(geometry))
        {
	        return geometry;
        }

        if (geometry != null)
        {
	        logger?.LogWarning("Saved window geometry is off-screen; resetting to default position");
        }

        return new SavedWindowState
        {
            Left = 100 + offset,
            Top = 100 + offset,
            Width = 1200,
            Height = 800,
            Route = geometry?.Route ?? "?f=/"
        };
    }

    public void OpenMainWindow(string? route, SavedWindowState? geometry)
    {
        var baseUrl = navigation.GetEffectiveBaseUrl(_localPort);
        var offset = _mainWindows.Count * 24;
        var state = ResolveGeometry(geometry, offset, logger);

        var window = new MainWindow(new MainWindowOptions
        {
            Settings = settings,
            WebViewEnv = webViewEnv,
            FileDownload = fileDownload,
            Watcher = watcher,
            WindowManager = this,
            Logger = logger,
            BaseUrl = baseUrl,
            InitialRoute = route ?? "?f=/",
            Geometry = state,
            UpdateService = updateService,
            MountWatcherService = mountWatcherService
        });

        _mainWindows.Add(window);
        window.Closed += (_, _) =>
        {
            _mainWindows.Remove(window);
            if (_mainWindows.Count == 0 && !IsTerminating && Application.Current != null)
            {
	            Application.Current.Shutdown();
            }
        };

        window.Show();
    }

    public void RestoreWindows()
    {
        var saved = routes.GetRoutes();
        if (saved.Count == 0)
        {
            OpenMainWindow(null, null);
            return;
        }

        foreach (var state in saved)
        {
	        OpenMainWindow(state.Route, state);
        }
    }

    private List<SavedWindowState> CollectStates(MainWindow? exclude = null) =>
        _mainWindows
            .Where(w => w != exclude)
            .Select(w => w.GetCurrentState())
            .ToList();

    internal void PersistCurrentState(MainWindow? exclude = null) =>
        routes.SaveAll(CollectStates(exclude));

    public void CloseAll(bool saveState = true)
    {
        IsTerminating = true;
        // Skip when _mainWindows is already empty: the last window saved its own
        // state in MainWindow_Closing before triggering Shutdown(), so overwriting
        // with an empty list here would discard it.
        if (saveState && _mainWindows.Count > 0)
        {
            routes.SaveAll(CollectStates());
        }

        foreach (var w in _mainWindows.ToList())
        {
            try { w.Close(); } catch { /* best-effort */ }
        }
        _mainWindows.Clear();
    }

    public void ReopenAll()
    {
        CloseAll(saveState: false);
        routes.ClearAll();
        IsTerminating = false;
        OpenMainWindow(null, null);
    }

    public void ReloadAll()
    {
        foreach (var w in _mainWindows)
        {
	        w.Reload();
        }
    }

    // Maximized windows are always valid — WPF snaps them to the nearest screen.
    // For normal windows, require that a usable strip of the title bar (≥100 px wide,
    // ≥1 px of height) intersects the virtual desktop so the user can grab and move it.
    internal static bool IsOnScreen(SavedWindowState state)
    {
        if (state.IsMaximized)
        {
	        return true;
        }

        if (state.Width < 200 || state.Height < 100)
        {
	        return false;
        }

        var vLeft   = SystemParameters.VirtualScreenLeft;
        var vTop    = SystemParameters.VirtualScreenTop;
        var vRight  = vLeft + SystemParameters.VirtualScreenWidth;
        var vBottom = vTop  + SystemParameters.VirtualScreenHeight;

        // Window right edge must be far enough right, and left edge far enough left,
        // that at least 100 px of the title bar is reachable.
        const double minTitleBarVisible = 100;
        return state.Left + state.Width  > vLeft  + minTitleBarVisible
            && state.Left                < vRight - minTitleBarVisible
            && state.Top                 < vBottom
            && state.Top + 30            > vTop;
    }
}
