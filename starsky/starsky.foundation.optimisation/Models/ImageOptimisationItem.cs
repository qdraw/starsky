using System.Diagnostics.CodeAnalysis;
using starsky.foundation.platform.Helpers;

namespace starsky.foundation.optimisation.Models;

[ExcludeFromCodeCoverage]
public class ImageOptimisationItem
{
	public required string InputPath { get; set; }
	public required string OutputPath { get; set; }
	public ExtensionRolesHelper.ImageFormat ImageFormat { get; set; } =
		ExtensionRolesHelper.ImageFormat.unknown;
}
