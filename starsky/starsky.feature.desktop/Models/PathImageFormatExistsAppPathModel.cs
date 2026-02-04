using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;

namespace starsky.feature.desktop.Models;

public class PathImageFormatExistsAppPathModel
{
	public string SubPath { get; set; } = string.Empty;

	public string FullFilePath { get; set; } = string.Empty;

	public ExtensionRolesHelper.ImageFormat ImageFormat { get; set; } =
		ExtensionRolesHelper.ImageFormat.notfound;

	public FileIndexItem.ExifStatus Status { get; set; } = FileIndexItem.ExifStatus.Default;

	public string AppPath { get; set; } = string.Empty;
}
