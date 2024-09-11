using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace starsky.foundation.platform.JsonConverter;

/// <summary>
///     Enum converter for Lists with Enum into Json
/// </summary>
/// <typeparam name="T">Enum</typeparam>
[SuppressMessage("ReSharper", "S6966: Await ReadAsync instead.",
	Justification = "There is no Async jet")]
public class EnumListConverter<T> : JsonConverter<List<T>> where T : struct, Enum
{
	public override List<T> Read(ref Utf8JsonReader reader, Type typeToConvert,
		JsonSerializerOptions options)
	{
		if ( reader.TokenType != JsonTokenType.StartArray )
		{
			throw new JsonException();
		}

		var result = new List<T>();

		while ( reader.Read() )
		{
			if ( reader.TokenType == JsonTokenType.EndArray )
			{
				return result;
			}

			if ( reader.TokenType != JsonTokenType.String )
			{
				throw new JsonException();
			}

			if ( Enum.TryParse<T>(reader.GetString(), out var enumValue) )
			{
				result.Add(enumValue);
			}
			else
			{
				throw new JsonException($"Unknown enum value: {reader.GetString()}");
			}
		}

		throw new JsonException("Unexpected end of JSON input");
	}

	public override void Write(Utf8JsonWriter writer, List<T> value, JsonSerializerOptions options)
	{
		writer.WriteStartArray();

		foreach ( var item in value )
		{
			writer.WriteStringValue(item.ToString());
		}

		writer.WriteEndArray();
	}
}
