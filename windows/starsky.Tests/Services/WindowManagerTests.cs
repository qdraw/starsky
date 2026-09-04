using Microsoft.Extensions.Logging.Abstractions;
using Starsky.Desktop.Models;
using Starsky.Desktop.Services;

namespace starsky.Tests.Services;

[TestClass]
public class WindowManagerTests
{
    private static WindowManager CreateManager()
    {
        var settings = new SettingsService(NullLogger<SettingsService>.Instance);
        var routes = new RoutePersistenceService(settings);
        var navigation = new NavigationService(settings);
        var webViewEnv = new WebViewEnvironmentService();
        var fileDownload = new FileDownloadService(NullLogger<FileDownloadService>.Instance);
        var watcher = new FileWatcherService(NullLogger<FileWatcherService>.Instance);
        var updateService = new UpdateService(settings, NullLogger<UpdateService>.Instance);
        return new WindowManager(settings, routes, navigation, webViewEnv, fileDownload, watcher,
            updateService, NullLogger<WindowManager>.Instance);
    }

    [TestMethod]
    public void SetLocalPort_DoesNotThrow()
    {
        var wm = CreateManager();
        Exception? ex = null;
        try { wm.SetLocalPort(9999); } catch (Exception e) { ex = e; }
        Assert.IsNull(ex);
    }

    [TestMethod]
    public void CloseAll_WithNoWindows_DoesNotThrow()
    {
        var wm = CreateManager();
        Exception? ex = null;
        try { wm.CloseAll(); } catch (Exception e) { ex = e; }
        Assert.IsNull(ex);
    }

    [TestMethod]
    public void ReloadAll_WithNoWindows_DoesNotThrow()
    {
        var wm = CreateManager();
        Exception? ex = null;
        try { wm.ReloadAll(); } catch (Exception e) { ex = e; }
        Assert.IsNull(ex);
    }

    [TestMethod]
    public void MainWindowOptions_Properties_AreAccessible()
    {
        var settings = new SettingsService(NullLogger<SettingsService>.Instance);
        var routes = new RoutePersistenceService(settings);
        var nav = new NavigationService(settings);
        var webViewEnv = new WebViewEnvironmentService();
        var fileDownload = new FileDownloadService(NullLogger<FileDownloadService>.Instance);
        var watcher = new FileWatcherService(NullLogger<FileWatcherService>.Instance);
        var updateService = new UpdateService(settings, NullLogger<UpdateService>.Instance);
        var wm = new WindowManager(settings, routes, nav, webViewEnv, fileDownload, watcher,
            updateService, NullLogger<WindowManager>.Instance);
        var geometry = new SavedWindowState { Left = 10, Top = 20, Width = 800, Height = 600 };

        var opts = new MainWindowOptions
        {
            Settings = settings,
            Routes = routes,
            WebViewEnv = webViewEnv,
            FileDownload = fileDownload,
            Watcher = watcher,
            WindowManager = wm,
            UpdateService = updateService,
            Logger = NullLogger.Instance,
            BaseUrl = "http://localhost:5000",
            InitialRoute = "?f=/photos",
            Geometry = geometry,
            WindowIndex = 3
        };

        Assert.AreSame(settings, opts.Settings);
        Assert.AreSame(routes, opts.Routes);
        Assert.AreSame(webViewEnv, opts.WebViewEnv);
        Assert.AreSame(fileDownload, opts.FileDownload);
        Assert.AreSame(wm, opts.WindowManager);
        Assert.AreEqual("http://localhost:5000", opts.BaseUrl);
        Assert.AreEqual("?f=/photos", opts.InitialRoute);
        Assert.AreSame(geometry, opts.Geometry);
        Assert.AreEqual(3, opts.WindowIndex);
    }

    private static SavedWindowState OnScreen(double left = 200, double top = 200,
        double width = 1200, double height = 800, bool maximized = false) =>
        new() { Left = left, Top = top, Width = width, Height = height, IsMaximized = maximized };

    [TestMethod]
    public void IsOnScreen_NormalWindowOnScreen_ReturnsTrue()
    {
        Assert.IsTrue(WindowManager.IsOnScreen(OnScreen()));
    }

    [TestMethod]
    public void IsOnScreen_Maximized_AlwaysReturnsTrue()
    {
        // Maximized windows may have any stored position; WPF snaps to nearest screen.
        var offscreen = OnScreen(left: -99999, top: -99999, maximized: true);
        Assert.IsTrue(WindowManager.IsOnScreen(offscreen));
    }

    [TestMethod]
    public void IsOnScreen_WindowFarOffLeftEdge_ReturnsFalse()
    {
        var state = OnScreen(left: -5000, top: 200);
        Assert.IsFalse(WindowManager.IsOnScreen(state));
    }

    [TestMethod]
    public void IsOnScreen_WindowFarOffRightEdge_ReturnsFalse()
    {
        var state = OnScreen(left: 999_999, top: 200);
        Assert.IsFalse(WindowManager.IsOnScreen(state));
    }

    [TestMethod]
    public void IsOnScreen_WindowFarAboveTopEdge_ReturnsFalse()
    {
        var state = OnScreen(left: 200, top: -5000);
        Assert.IsFalse(WindowManager.IsOnScreen(state));
    }

    [TestMethod]
    public void IsOnScreen_WindowFarBelowBottomEdge_ReturnsFalse()
    {
        var state = OnScreen(left: 200, top: 999_999);
        Assert.IsFalse(WindowManager.IsOnScreen(state));
    }

    [TestMethod]
    public void IsOnScreen_TooNarrow_ReturnsFalse()
    {
        var state = OnScreen(left: 200, top: 200, width: 50, height: 800);
        Assert.IsFalse(WindowManager.IsOnScreen(state));
    }

    [TestMethod]
    public void IsOnScreen_TooShort_ReturnsFalse()
    {
        var state = OnScreen(left: 200, top: 200, width: 1200, height: 50);
        Assert.IsFalse(WindowManager.IsOnScreen(state));
    }

    // ── ResolveGeometry ───────────────────────────────────────────────────────

    [TestMethod]
    public void ResolveGeometry_NullGeometry_ReturnsDefaultState()
    {
        var result = WindowManager.ResolveGeometry(null, 0);

        Assert.AreEqual(100, result.Left);
        Assert.AreEqual(100, result.Top);
        Assert.AreEqual(1200, result.Width);
        Assert.AreEqual(800, result.Height);
        Assert.AreEqual("?f=/", result.Route);
    }

    [TestMethod]
    public void ResolveGeometry_NullGeometry_WithOffset_AppliesOffset()
    {
        var result = WindowManager.ResolveGeometry(null, 48);

        Assert.AreEqual(148, result.Left);
        Assert.AreEqual(148, result.Top);
    }

    [TestMethod]
    public void ResolveGeometry_OnScreenGeometry_ReturnsOriginal()
    {
        var geometry = OnScreen(left: 200, top: 200, width: 1200, height: 800);

        var result = WindowManager.ResolveGeometry(geometry, 0);

        Assert.AreSame(geometry, result);
    }

    [TestMethod]
    public void ResolveGeometry_OffScreenGeometry_ReturnsDefaultWithRoutePreserved()
    {
        var geometry = new SavedWindowState
        {
            Left = -99999,
            Top = -99999,
            Width = 1200,
            Height = 800,
            Route = "?f=/photos"
        };

        var result = WindowManager.ResolveGeometry(geometry, 0);

        Assert.AreEqual("?f=/photos", result.Route);
        Assert.AreEqual(100, result.Left);
    }

    [TestMethod]
    public void ResolveGeometry_NullGeometry_RouteIsDefault()
    {
        var result = WindowManager.ResolveGeometry(null, 0);

        Assert.AreEqual("?f=/", result.Route);
    }
}
