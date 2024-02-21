using starsky.feature.desktop.Models;

namespace starsky.feature.desktop.Interfaces;

public interface IOpenEditorDesktopService
{
	/// <summary>
	/// Check if the file is less then the amount of files that are allowed to open
	/// If there are more files to open it will return false and the front-end will ask for confirmation
	/// </summary>
	/// <param name="f">dot comma list of paths</param>
	/// <returns>true is no confirmation and false ask are you sure</returns>
	bool OpenAmountConfirmationChecker(string f);

	/// <summary>
	/// Is supported and enabled in the feature toggle
	/// </summary>
	/// <returns>Should you use it?</returns>
	bool IsEnabled();

	/// <summary>
	/// Open a file in the default editor or specific editor which is set in the app settings
	/// </summary>
	/// <param name="f">dot comma split list with subPaths</param>
	/// <param name="collections">should pick raw/jpeg file even its not specified</param>
	/// <returns>files done and list of results</returns>
	Task<(bool?, string, List<PathImageFormatExistsAppPathModel>)> OpenAsync(string f,
		bool collections);
}
