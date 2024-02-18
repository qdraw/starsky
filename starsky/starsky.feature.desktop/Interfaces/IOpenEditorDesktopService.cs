using starsky.feature.desktop.Models;

namespace starsky.feature.desktop.Interfaces;

public interface IOpenEditorDesktopService
{
	Task<(bool?, string, List<PathImageFormatExistsAppPathModel>)> OpenAsync(string f,
		bool collections);
}
