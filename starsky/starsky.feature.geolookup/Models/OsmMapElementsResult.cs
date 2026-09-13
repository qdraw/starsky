using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace starsky.feature.geolookup.Models;

public class OsmMapElementsResult
{
	[JsonPropertyName("error")] public string? Error { get; set; }

	[JsonPropertyName("nearbyObjects")]
	public List<OsmMapElementItem> NearbyObjects { get; set; } = [];

	[JsonPropertyName("enclosingObjects")]
	public List<OsmMapElementItem> EnclosingObjects { get; set; } = [];
}

public class OsmMapElementItem
{
	[JsonPropertyName("id")] public string? Id { get; set; }

	[JsonPropertyName("elementType")] public string? ElementType { get; set; }

	[JsonPropertyName("label")] public string? Label { get; set; }

	[JsonPropertyName("category")] public string? Category { get; set; }

	[JsonPropertyName("type")] public string? Type { get; set; }

	[JsonPropertyName("description")] public string? Description { get; set; }

	[JsonPropertyName("copyText")] public string? CopyText { get; set; }

	[JsonPropertyName("distanceMeters")] public double? DistanceMeters { get; set; }
}

internal class OverpassResponse
{
	[JsonPropertyName("elements")]
	public List<OverpassElement>? Elements { get; set; }
}

internal class OverpassElement
{
	[JsonPropertyName("type")] public string? Type { get; set; }

	[JsonPropertyName("id")] public long Id { get; set; }

	[JsonPropertyName("lat")] public double? Lat { get; set; }

	[JsonPropertyName("lon")] public double? Lon { get; set; }

	[JsonPropertyName("center")] public OverpassCenter? Center { get; set; }

	[JsonPropertyName("tags")]
	public Dictionary<string, string>? Tags { get; set; }
}

internal class OverpassCenter
{
	[JsonPropertyName("lat")] public double Lat { get; set; }

	[JsonPropertyName("lon")] public double Lon { get; set; }
}