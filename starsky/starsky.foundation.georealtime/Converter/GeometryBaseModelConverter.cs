using System.Collections.Generic;
using starsky.foundation.georealtime.Models.GeoJson;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace starsky.foundation.georealtime.Converter;
public class GeometryBaseModelConverter : JsonConverter<GeometryBaseModel>
{
	public override GeometryBaseModel Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		using var doc = JsonDocument.ParseValue(ref reader);
		var root = doc.RootElement;

		if ( !root.TryGetProperty("type", out var typeElement) ||
		     !root.TryGetProperty("coordinates", out var coordinates) ||
		     coordinates.ValueKind == JsonValueKind.Null ||
			 coordinates.ValueKind == JsonValueKind.Number )
		{
			throw new JsonException("Invalid JSON for GeometryBaseModel");
		}
			
		var geometryType = typeElement.GetString();
		var coordinatesRawText = coordinates.GetRawText();

		switch (geometryType)
		{
			case "Point":
				return new GeometryPointModel()
				{
					Coordinates =
						JsonSerializer.Deserialize<List<double>>(
							coordinatesRawText)
				};
			case "LineString":
				return new GeometryLineStringModel()
				{
					Coordinates =
						JsonSerializer.Deserialize<List<List<double>>>(
							coordinatesRawText)
				};
		}

		throw new JsonException("Invalid JSON for GeometryBaseModel");
	}

	public override void Write(Utf8JsonWriter writer, GeometryBaseModel value, JsonSerializerOptions options)
	{
		JsonSerializer.Serialize(writer, value, value.GetType(), options);
	}
}

