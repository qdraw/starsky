namespace starsky.feature.desktop.Interfaces;

public interface IOpenEditorDesktopService
{
	Task<(bool?, string)> OpenAsync(string f, bool collections);
}
