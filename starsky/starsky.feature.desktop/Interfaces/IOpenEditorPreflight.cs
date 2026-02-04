using starsky.feature.desktop.Models;

namespace starsky.feature.desktop.Interfaces;

public interface IOpenEditorPreflight
{
	Task<List<PathImageFormatExistsAppPathModel>> PreflightAsync(
		List<string> inputFilePaths, bool collections);
}
