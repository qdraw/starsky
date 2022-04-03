using System.Text.Json;

namespace starsky.foundation.platform.JsonConverter
{
	public static class JsonClone
	{
		public static T CloneViaJson<T>(this T source)
		{
			var serialized = JsonSerializer.Serialize(
				source,
				DefaultJsonSerializer.CamelCase);

			return JsonSerializer.Deserialize<T>(serialized);
		}
	}	
}
