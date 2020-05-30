using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace starsky.foundation.platform.JsonConverter
{
	public class JsonBoolQuotedConverter : JsonConverter<bool>
	{
		public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			return reader.GetBoolean();
		}

		public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
		{
			writer.WriteStringValue(value.ToString().ToLowerInvariant());
		}
	}
}
