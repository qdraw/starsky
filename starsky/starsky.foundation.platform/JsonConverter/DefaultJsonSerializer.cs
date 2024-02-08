using System.Text.Json;
using System.Text.Json.Serialization;

namespace starsky.foundation.platform.JsonConverter
{
	public static class DefaultJsonSerializer
	{
		/// <summary>
		/// No enters in output
		/// </summary>
		public static JsonSerializerOptions CamelCaseNoEnters =>
			new JsonSerializerOptions
			{
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
				PropertyNameCaseInsensitive = true,
				AllowTrailingCommas = true,
				WriteIndented = false
			};

		/// <summary>
		/// Write with enters in output
		/// </summary>
		public static JsonSerializerOptions CamelCase =>
			new JsonSerializerOptions
			{
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
				PropertyNameCaseInsensitive = true,
				AllowTrailingCommas = true,
				WriteIndented = true
			};

		/// <summary>
		/// PascalCase (No Naming policy)
		/// Bool is written as quoted string "true" or "false"
		/// </summary>
		public static JsonSerializerOptions NoNamingPolicyBoolAsString =>
			new JsonSerializerOptions
			{
				PropertyNamingPolicy = null,
				DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
				PropertyNameCaseInsensitive = true,
				AllowTrailingCommas = true,
				WriteIndented = true,
				Converters = { new JsonBoolQuotedConverter(), },
			};

		/// <summary>
		/// PascalCase (No Naming policy)
		/// Bool as normal bool
		/// </summary>
		public static JsonSerializerOptions NoNamingPolicy =>
			new JsonSerializerOptions
			{
				PropertyNamingPolicy = null,
				DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
				PropertyNameCaseInsensitive = true,
				AllowTrailingCommas = true,
				WriteIndented = true
			};
	}
}
