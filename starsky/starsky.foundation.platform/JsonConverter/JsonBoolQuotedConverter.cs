using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace starsky.foundation.platform.JsonConverter
{
	public sealed class JsonBoolQuotedConverter : JsonConverter<bool>
	{
		public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			var isString = reader.TokenType is JsonTokenType.String or JsonTokenType.Null;
			if ( !isString )
			{
				return reader.GetBoolean();
			}

			var stringValue = reader.GetString();
			return stringValue?.Equals("true", StringComparison.InvariantCultureIgnoreCase) == true;
		}

		public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
		{
			writer.WriteStringValue(value.ToString().ToLowerInvariant());
		}
	}
}
