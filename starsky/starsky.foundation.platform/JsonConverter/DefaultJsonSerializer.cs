using System.Text.Json;

namespace starsky.foundation.platform.JsonConverter
{
	public static class DefaultJsonSerializer
	{
		public static JsonSerializerOptions CamelCase =>
			new JsonSerializerOptions
			{
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
				PropertyNameCaseInsensitive = true,
			};
	}
}
