using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using starsky.foundation.database.Models;

namespace starsky.foundation.database.JsonConverters;

/// <summary>
///     Custom JSON converter for FileIndexItem that includes the `Id` field.
///     Uses reflection to serialize all properties dynamically, overriding the [JsonIgnore] on Id.
/// </summary>
public sealed class FileIndexItemWithIdJsonConverter : JsonConverter<FileIndexItem>
{
	public override FileIndexItem Read(ref Utf8JsonReader reader, Type typeToConvert,
		JsonSerializerOptions options)
	{
		// Build read options based on provided options but ensure Id is not ignored
		var readOptions = new JsonSerializerOptions
		{
			PropertyNamingPolicy = options.PropertyNamingPolicy,
			DefaultIgnoreCondition = JsonIgnoreCondition.Never,
			PropertyNameCaseInsensitive = options.PropertyNameCaseInsensitive,
			AllowTrailingCommas = options.AllowTrailingCommas,
			WriteIndented = options.WriteIndented
		};

		// Copy converters except this one to avoid recursion
		foreach ( var conv in options.Converters )
		{
			// Skip this converter and the factory to avoid recursion when building safe options
			if ( conv is FileIndexItemWithIdJsonConverter
			    or FileIndexItemWithIdJsonConverterFactory )
			{
				continue;
			}

			readOptions.Converters.Add(conv);
		}

		using var jsonDoc = JsonDocument.ParseValue(ref reader);
		var element = jsonDoc.RootElement;
		var json = element.GetRawText();
		return JsonSerializer.Deserialize<FileIndexItem>(json, readOptions) ?? new FileIndexItem();
	}

	public override void Write(Utf8JsonWriter writer, FileIndexItem value,
		JsonSerializerOptions options)
	{
		// Build safe options that copy key settings but exclude this converter to avoid recursion
		var safeOptions = new JsonSerializerOptions
		{
			PropertyNamingPolicy = options.PropertyNamingPolicy,
			DefaultIgnoreCondition = options.DefaultIgnoreCondition,
			PropertyNameCaseInsensitive = options.PropertyNameCaseInsensitive,
			AllowTrailingCommas = options.AllowTrailingCommas,
			WriteIndented = options.WriteIndented
		};

		foreach ( var conv in options.Converters )
		{
			// Skip this converter and the factory to avoid recursion when building safe options
			if ( conv is FileIndexItemWithIdJsonConverter
			    or FileIndexItemWithIdJsonConverterFactory )
			{
				continue;
			}

			safeOptions.Converters.Add(conv);
		}

		// Serialize to JSON element first using the safe options
		var jsonString = JsonSerializer.Serialize(value, safeOptions);
		using var jsonDoc = JsonDocument.Parse(jsonString);
		var element = jsonDoc.RootElement;

		writer.WriteStartObject();

		// First, write the Id field explicitly
		writer.WriteNumber("id", value.Id);

		// Then write all other properties from the serialized JSON (write name and value)
		foreach ( var property in element.EnumerateObject() )
		{
			writer.WritePropertyName(property.Name);
			property.Value.WriteTo(writer);
		}

		writer.WriteEndObject();
	}
}
