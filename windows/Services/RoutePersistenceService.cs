using Starsky.Desktop.Models;

namespace Starsky.Desktop.Services;

public class RoutePersistenceService(SettingsService settings)
{
	public List<SavedWindowState> GetRoutes() => settings.Current.Windows;

    public void SaveRoute(int index, string route, SavedWindowState? geometry = null)
    {
        var windows = settings.Current.Windows;
        while (windows.Count <= index)
        {
	        windows.Add(new SavedWindowState());
        }

        windows[index].Route = route;
        if (geometry != null)
        {
            windows[index].Left = geometry.Left;
            windows[index].Top = geometry.Top;
            windows[index].Width = geometry.Width;
            windows[index].Height = geometry.Height;
            windows[index].IsMaximized = geometry.IsMaximized;
        }

        settings.Save();
    }

    public void RemoveRoute(int index)
    {
        var windows = settings.Current.Windows;
        if (index >= 0 && index < windows.Count)
        {
            windows.RemoveAt(index);
            settings.Save();
        }
    }

    public void ClearAll()
    {
        settings.Current.Windows.Clear();
        settings.Save();
    }
}
