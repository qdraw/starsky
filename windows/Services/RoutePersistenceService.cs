using Starsky.Desktop.Models;

namespace Starsky.Desktop.Services;

public class RoutePersistenceService
{
    private readonly SettingsService _settings;

    public RoutePersistenceService(SettingsService settings)
    {
        _settings = settings;
    }

    public List<SavedWindowState> GetRoutes() => _settings.Current.Windows;

    public void SaveRoute(int index, string route, SavedWindowState? geometry = null)
    {
        var windows = _settings.Current.Windows;
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

        _settings.Save();
    }

    public void RemoveRoute(int index)
    {
        var windows = _settings.Current.Windows;
        if (index >= 0 && index < windows.Count)
        {
            windows.RemoveAt(index);
            _settings.Save();
        }
    }

    public void ClearAll()
    {
        _settings.Current.Windows.Clear();
        _settings.Save();
    }
}
