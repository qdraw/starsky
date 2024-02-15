using System.Collections.Generic;
using System.Text.Json.Serialization;
using starsky.foundation.platform.Helpers;

namespace starsky.foundation.platform.Models;

public class AppSettingsDefaultEditorApplication
{
	/// <summary>
	/// For what type of files
	/// </summary>
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public List<ExtensionRolesHelper.ImageFormat> ImageFormats { get; set; } = [];

	/// <summary>
	/// Path to .exe on windows and .app on Mac OS
	/// </summary>
	public string ApplicationPath { get; set; }
}
