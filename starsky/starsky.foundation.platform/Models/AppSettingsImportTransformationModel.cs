using System.Collections.Generic;
using starsky.foundation.platform.Helpers;

namespace starsky.foundation.platform.Models;

public class AppSettingsImportTransformationModel
{
	public ExtensionRolesHelper.ImageFormat ImageFormat { get; set; } =
		ExtensionRolesHelper.ImageFormat.unknown;

	public string Source { get; set; } = string.Empty;

	public List<TransformationRule> TransformationRules { get; set; } = new();
}

public class TransformationRule
{
	public int ColorClass { get; set; }
	public List<string> Tags { get; set; } = new();
}
