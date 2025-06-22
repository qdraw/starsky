using System.Collections.Generic;
using System.Text.Json.Serialization;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.JsonConverter;

namespace starsky.foundation.platform.Models;

public class AppSettingsImportTransformationModel
{
	public List<TransformationRule> Rules { get; set; } = [];
}

public class TransformationRule
{
	public TransformationConditions Conditions { get; set; } = new();

	public ColorClassParser.Color? ColorClass { get; set; }
}

public class TransformationConditions
{
	[JsonConverter(typeof(EnumListConverter<ExtensionRolesHelper.ImageFormat>))]
	public List<ExtensionRolesHelper.ImageFormat> ImageFormats { get; set; } = [];

	public string Origin { get; set; } = string.Empty;
}
