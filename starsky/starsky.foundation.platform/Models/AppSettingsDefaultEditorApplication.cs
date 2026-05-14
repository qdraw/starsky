using System.Collections.Generic;
using System.Text.Json.Serialization;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.JsonConverter;

namespace starsky.foundation.platform.Models;

public class AppSettingsDefaultEditorApplication
{
	/// <summary>
	///     For what type of files
	/// </summary>
	[JsonConverter(typeof(EnumListConverter<ExtensionRolesHelper.ImageFormat>))]
	public List<ExtensionRolesHelper.ImageFormat> ImageFormats { get; set; } = [];

	/// <summary>
	///     Path to .exe on Windows and .app on macOS
	///     No check if exists here
	///     Null Or Empty string means using default app
	/// </summary>
	public string? ApplicationPath { get; set; } = string.Empty;
}
