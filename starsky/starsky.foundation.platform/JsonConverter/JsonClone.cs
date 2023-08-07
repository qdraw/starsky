using System.Text.Json;

namespace starsky.foundation.platform.JsonConverter
{
	public static class JsonClone
	{
		/// <summary>
		/// Clone via JsonConverter
		/// </summary>
		/// <param name="source">The object</param>
		/// <typeparam name="T">type to clone from and to</typeparam>
		/// <returns>typed object</returns>
		public static T CloneViaJson<T>(this T source)
		{
			var serialized = JsonSerializer.Serialize(
				source, DefaultJsonSerializer.CamelCase);
			return JsonSerializer.Deserialize<T>(serialized, DefaultJsonSerializer.CamelCase);
		}
	}	
}
