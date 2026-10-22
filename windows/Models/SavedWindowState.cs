namespace Starsky.Desktop.Models;

public class SavedWindowState
{
    public string Route { get; set; } = "?f=/";
    public double Left { get; set; } = 100;
    public double Top { get; set; } = 100;
    public double Width { get; set; } = 1200;
    public double Height { get; set; } = 800;
    public bool IsMaximized { get; set; }
}
