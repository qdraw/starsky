using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using starsky.foundation.database.Models;

namespace starsky.foundation.database.JsonConverters;

/// <summary>
///     Custom JSON converter for FileIndexItem that includes the Id field.
///     Uses reflection to serialize all properties dynamically, overriding the [JsonIgnore] on Id.
/// </summary>
public sealed class FileIndexItemWithIdJsonConverter : JsonConverter<FileIndexItem>
{
	public override FileIndexItem Read(ref Utf8JsonReader reader, Type typeToConvert,
		JsonSerializerOptions options)
	{
		// Create a copy of options but ensure the Id is not ignored and avoid recursion
		var readOptions = new JsonSerializerOptions(options)
		{
			DefaultIgnoreCondition = JsonIgnoreCondition.Never
		};

		// Remove this converter from the copy to avoid recursion
		for ( var i = readOptions.Converters.Count - 1; i >= 0; i-- )
		{
			var conv = readOptions.Converters[i];
			if ( conv?.GetType() == typeof(FileIndexItemWithIdJsonConverter) )
			{
				readOptions.Converters.RemoveAt(i);
			}
		}

		using var jsonDoc = JsonDocument.ParseValue(ref reader);
		var element = jsonDoc.RootElement;
		var json = element.GetRawText();
		return JsonSerializer.Deserialize<FileIndexItem>(json, readOptions) ?? new FileIndexItem();
	}

	public override void Write(Utf8JsonWriter writer, FileIndexItem value,
		JsonSerializerOptions options)
	{
		// Create safe options that do NOT contain this converter to avoid recursion
		var safeOptions = new JsonSerializerOptions(options);

		for ( var i = options.Converters.Count - 1; i >= 0; i-- )
		{
			var conv = options.Converters[i];
			if ( conv?.GetType() == typeof(FileIndexItemWithIdJsonConverter) )
			{
				// skip adding this converter
				continue;
			}
			// Only add converters that aren't already present on the safe copy
			if ( !safeOptions.Converters.Contains(conv) )
			{
				safeOptions.Converters.Add(conv);
			}
		}

		// Serialize to JSON element first using the safe options
		var jsonString = JsonSerializer.Serialize(value, safeOptions);
		using var jsonDoc = JsonDocument.Parse(jsonString);
		var element = jsonDoc.RootElement;

		writer.WriteStartObject();

		// First, write the Id field explicitly
		writer.WriteNumber("id", value.Id);

		// Then write all other properties from the serialized JSON
		foreach ( var property in element.EnumerateObject() )
		{
			property.Value.WriteTo(writer);
		}

		writer.WriteEndObject();
	}
}
