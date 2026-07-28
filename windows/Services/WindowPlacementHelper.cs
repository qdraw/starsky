using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Starsky.Windows.Models;
using Windows.Graphics;
using WinRT.Interop;

namespace Starsky.Windows.Services;

public static class WindowPlacementHelper
{
    public static WindowStateInfo Capture(Window window, string route)
    {
        var appWindow = GetAppWindow(window);
        return new WindowStateInfo
        {
            Route = route,
            X = appWindow.Position.X,
            Y = appWindow.Position.Y,
            Width = appWindow.Size.Width,
            Height = appWindow.Size.Height,
        };
    }

    public static void Apply(Window window, WindowStateInfo? state, int offset = 0)
    {
        if (state is null)
        {
            return;
        }

        var appWindow = GetAppWindow(window);
        appWindow.MoveAndResize(new RectInt32(
            state.X + offset,
            state.Y + offset,
            Math.Max(480, state.Width),
            Math.Max(360, state.Height)));
    }

    private static AppWindow GetAppWindow(Window window)
    {
        var hwnd = WindowNative.GetWindowHandle(window);
        var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hwnd);
        return AppWindow.GetFromWindowId(windowId);
    }
}