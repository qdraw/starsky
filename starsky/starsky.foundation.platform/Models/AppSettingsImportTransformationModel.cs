using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.JsonConverter;

namespace starsky.foundation.platform.Models;

public class AppSettingsImportTransformationModel
{
	public List<TransformationRule> Rules { get; set; } = [];

	/// <summary>
	///     Display the import transformation model as a string
	/// </summary>
	/// <returns>A string like this: Rules: [No Rules] or Rules: [Condition: Origin=/test, Formats=jpg,png]</returns>
	public override string ToString()
	{
		var rulesDisplay = Rules.Count > 0
			? string.Join(", ", Rules.Select(r =>
				$"Condition: Origin={r.Conditions.Origin}, Formats={string.Join(",", r.Conditions.ImageFormats)}"))
			: "No Rules";

		return $"Rules: [{rulesDisplay}]";
	}
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
