using starsky.foundation.platform.Helpers;

namespace starsky.foundation.optimisation.Models;

public class ImageOptimisationItem
{
	public required string InputPath { get; set; }
	public required string OutputPath { get; set; }
	public ExtensionRolesHelper.ImageFormat ImageFormat { get; set; } =
		ExtensionRolesHelper.ImageFormat.unknown;
}
