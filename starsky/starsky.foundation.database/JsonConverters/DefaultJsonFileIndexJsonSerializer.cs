using System.Text.Json;
using System.Text.Json.Serialization;

namespace starsky.foundation.database.JsonConverters;

public class DefaultJsonFileIndexJsonSerializer
{
	/// <summary>
	///     PascalCase (No Naming policy)
	///     Bool is written as quoted string "true" or "false"
	/// </summary>
	public static JsonSerializerOptions WithIdConverter =>
		new()
		{
			PropertyNamingPolicy = null,
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
			PropertyNameCaseInsensitive = true,
			AllowTrailingCommas = true,
			WriteIndented = false,
			Converters = { new FileIndexItemWithIdJsonConverterFactory() }
		};
}
