using starsky.foundation.platform.Helpers;

namespace starsky.feature.desktop.Models;

public class PathImageFormatExistsAppPathModel
{
	public string SubPath { get; set; } = string.Empty;

	public string FullFilePath { get; set; } = string.Empty;

	public ExtensionRolesHelper.ImageFormat ImageFormat { get; set; } =
		ExtensionRolesHelper.ImageFormat.notfound;

	public bool Exists { get; set; } = false;

	public string AppPath { get; set; } = string.Empty;
}
