using System.Text.Json;

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
	}
}
