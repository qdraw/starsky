using Microsoft.Extensions.Logging.Abstractions;
using Starsky.Desktop.Models;
using Starsky.Desktop.Services;

namespace starsky.Tests.Services;

public class WindowManagerTests
{
    private static WindowManager CreateManager()
    {
        var settings = new SettingsService(NullLogger<SettingsService>.Instance);
        var routes = new RoutePersistenceService(settings);
        var navigation = new NavigationService(settings);
        var webViewEnv = new WebViewEnvironmentService();
        var fileDownload = new FileDownloadService(NullLogger<FileDownloadService>.Instance);
        return new WindowManager(settings, routes, navigation, webViewEnv, fileDownload,
            NullLogger<WindowManager>.Instance);
    }

    [Fact]
    public void SetLocalPort_DoesNotThrow()
    {
        var wm = CreateManager();
        var ex = Record.Exception(() => wm.SetLocalPort(9999));
        Assert.Null(ex);
    }

    [Fact]
    public void CloseAll_WithNoWindows_DoesNotThrow()
    {
        var wm = CreateManager();
        var ex = Record.Exception(() => wm.CloseAll());
        Assert.Null(ex);
    }

    [Fact]
    public void ReloadAll_WithNoWindows_DoesNotThrow()
    {
        var wm = CreateManager();
        var ex = Record.Exception(() => wm.ReloadAll());
        Assert.Null(ex);
    }

    private static SavedWindowState OnScreen(double left = 200, double top = 200,
        double width = 1200, double height = 800, bool maximized = false) =>
        new() { Left = left, Top = top, Width = width, Height = height, IsMaximized = maximized };

    [Fact]
    public void IsOnScreen_NormalWindowOnScreen_ReturnsTrue()
    {
        Assert.True(WindowManager.IsOnScreen(OnScreen()));
    }

    [Fact]
    public void IsOnScreen_Maximized_AlwaysReturnsTrue()
    {
        // Maximized windows may have any stored position; WPF snaps to nearest screen.
        var offscreen = OnScreen(left: -99999, top: -99999, maximized: true);
        Assert.True(WindowManager.IsOnScreen(offscreen));
    }

    [Fact]
    public void IsOnScreen_WindowFarOffLeftEdge_ReturnsFalse()
    {
        var state = OnScreen(left: -5000, top: 200);
        Assert.False(WindowManager.IsOnScreen(state));
    }

    [Fact]
    public void IsOnScreen_WindowFarOffRightEdge_ReturnsFalse()
    {
        var state = OnScreen(left: 999_999, top: 200);
        Assert.False(WindowManager.IsOnScreen(state));
    }

    [Fact]
    public void IsOnScreen_WindowFarAboveTopEdge_ReturnsFalse()
    {
        var state = OnScreen(left: 200, top: -5000);
        Assert.False(WindowManager.IsOnScreen(state));
    }

    [Fact]
    public void IsOnScreen_WindowFarBelowBottomEdge_ReturnsFalse()
    {
        var state = OnScreen(left: 200, top: 999_999);
        Assert.False(WindowManager.IsOnScreen(state));
    }

    [Fact]
    public void IsOnScreen_TooNarrow_ReturnsFalse()
    {
        var state = OnScreen(left: 200, top: 200, width: 50, height: 800);
        Assert.False(WindowManager.IsOnScreen(state));
    }

    [Fact]
    public void IsOnScreen_TooShort_ReturnsFalse()
    {
        var state = OnScreen(left: 200, top: 200, width: 1200, height: 50);
        Assert.False(WindowManager.IsOnScreen(state));
    }
}
