using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using starsky.feature.geolookup.Interfaces;
using starsky.feature.geolookup.Models;
using starsky.foundation.http.Interfaces;
using starsky.foundation.injection;
using starsky.foundation.platform.Helpers;

namespace starsky.feature.geolookup.Services;

[Service(typeof(IOsmMapElementsService), InjectionLifetime = InjectionLifetime.Scoped)]
public class OsmMapElementsService(IHttpClientHelper httpClientHelper) : IOsmMapElementsService
{
	private const string HttpsPrefix = "https://";
	private static readonly string[] OverpassBaseUrls =
	[
		"overpass-api.de/api/interpreter",
		"z.overpass-api.de/api/interpreter",
		"lz4.overpass-api.de/api/interpreter"
	];
	private const double NearbyRadiusInMeters = 20.0;
	private const int MaxItemsPerSection = 8;
	private const string ErrorMessage = "Failed to parse OSM map elements response";

	private static readonly string[] DescriptorTagKeys =
	[
		"amenity", "shop", "tourism", "historic", "leisure", "man_made", "building",
		"highway", "waterway", "railway", "natural", "landuse", "boundary", "place"
	];

	private static readonly Dictionary<string, int> NearbyPriority = new()
	{
		{ "waterway", 1 },
		{ "highway", 2 },
		{ "railway", 3 },
		{ "building", 4 },
		{ "leisure", 5 },
		{ "amenity", 6 }
	};

	private static readonly string[] NameTagKeys = ["name", "official_name", "brand", "operator", "ref"];

	public async Task<OsmMapElementsResult> LookupAsync(double latitude, double longitude)
	{
		if ( !ValidateLocation.ValidateLatitudeLongitude(latitude, longitude) )
		{
			return new OsmMapElementsResult { Error = "Non-valid location" };
		}

		string? lastError = null;
		var hadValidResponse = false;

		foreach ( var baseUrl in OverpassBaseUrls )
		{
			var nearbyResponse = await QueryElementsAsync(baseUrl, BuildNearbyQuery(latitude, longitude));
			var enclosingResponse = await QueryElementsAsync(baseUrl, BuildEnclosingQuery(latitude, longitude));

			if ( string.IsNullOrWhiteSpace(nearbyResponse.Error) )
			{
				hadValidResponse = true;
			}
			else
			{
				lastError = ChoosePreferredError(lastError, nearbyResponse.Error);
			}

			if ( string.IsNullOrWhiteSpace(enclosingResponse.Error) )
			{
				hadValidResponse = true;
			}
			else
			{
				lastError = ChoosePreferredError(lastError, enclosingResponse.Error);
			}

			var nearbyObjects = nearbyResponse.Elements
				.Select(element => ToMapElementItem(element, latitude, longitude))
				.Where(item => item != null)
				.Cast<OsmMapElementItem>()
				.OrderBy(item => GetNearbyPriority(item.Category))
				.ThenBy(item => item.DistanceMeters ?? double.MaxValue)
				.GroupBy(item => item.CopyText)
				.Select(group => group.First())
				.Take(MaxItemsPerSection)
				.ToList();

			var enclosingObjects = enclosingResponse.Elements
				.Select(element => ToMapElementItem(element, latitude, longitude))
				.Where(item => item != null)
				.Cast<OsmMapElementItem>()
				.OrderBy(item => item.DistanceMeters ?? double.MaxValue)
				.GroupBy(item => item.CopyText)
				.Select(group => group.First())
				.Take(MaxItemsPerSection)
				.ToList();

			if ( nearbyObjects.Count > 0 || enclosingObjects.Count > 0 )
			{
				return new OsmMapElementsResult
				{
					NearbyObjects = nearbyObjects,
					EnclosingObjects = enclosingObjects
				};
			}
		}

		if ( !hadValidResponse && !string.IsNullOrWhiteSpace(lastError) )
		{
			return new OsmMapElementsResult { Error = lastError };
		}

		return new OsmMapElementsResult();
	}

	private async Task<ParsedOverpassResponse> QueryElementsAsync(string baseUrl, string query)
	{
		var url = BuildUrl(baseUrl, query);
		var response = await httpClientHelper.ReadString(url);
		if ( !response.Key || string.IsNullOrWhiteSpace(response.Value) )
		{
			return new ParsedOverpassResponse([], ErrorMessage);
		}

		try
		{
			var overpassResponse = JsonSerializer.Deserialize<OverpassResponse>(response.Value);
			if ( overpassResponse?.Elements != null )
			{
				return new ParsedOverpassResponse(overpassResponse.Elements, null);
			}

			using var json = JsonDocument.Parse(response.Value);
			if ( json.RootElement.TryGetProperty("remark", out var remark) &&
			     remark.ValueKind == JsonValueKind.String )
			{
				return new ParsedOverpassResponse([], remark.GetString());
			}

			return new ParsedOverpassResponse([], null);
		}
		catch
		{
			return new ParsedOverpassResponse([], ErrorMessage);
		}
	}

	private static string BuildNearbyQuery(double latitude, double longitude)
	{
		var lat = latitude.ToString(CultureInfo.InvariantCulture);
		var lon = longitude.ToString(CultureInfo.InvariantCulture);
		var radius = NearbyRadiusInMeters.ToString(CultureInfo.InvariantCulture);

		return "[timeout:10][out:json];" +
		       "(" +
		       $"node(around:{radius},{lat},{lon});" +
		       $"way(around:{radius},{lat},{lon});" +
		       $"relation(around:{radius},{lat},{lon});" +
		       ");" +
		       "out center tags qt;";
	}

	private static string BuildEnclosingQuery(double latitude, double longitude)
	{
		var lat = latitude.ToString(CultureInfo.InvariantCulture);
		var lon = longitude.ToString(CultureInfo.InvariantCulture);

		return "[timeout:10][out:json];" +
		       $"is_in({lat},{lon})->.a;" +
		       "(" +
		       "way(pivot.a);" +
		       "relation(pivot.a);" +
		       ");" +
		       "out center tags qt;";
	}

	private static string BuildUrl(string baseUrl, string query)
	{
		return $"{HttpsPrefix}{baseUrl}?data={Uri.EscapeDataString(query)}";
	}

	private static OsmMapElementItem? ToMapElementItem(OverpassElement element, double latitude,
		double longitude)
	{
		var tags = element.Tags;
		if ( tags == null || tags.Count == 0 )
		{
			return null;
		}

		var category = DescriptorTagKeys.FirstOrDefault(tags.ContainsKey);
		var type = category != null && tags.TryGetValue(category, out var categoryValue)
			? categoryValue
			: null;
		var label = NameTagKeys
			.Select(key => tags.TryGetValue(key, out var value) ? value : null)
			.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));

		label ??= !string.IsNullOrWhiteSpace(type)
			? Beautify(type)
			: $"OSM {element.Type} {element.Id}";

		var description = !string.IsNullOrWhiteSpace(category)
			? $"{Beautify(category)}: {Beautify(type ?? string.Empty)}".TrimEnd(':', ' ')
			: Beautify(element.Type ?? string.Empty);

		var copyText = label == description || string.IsNullOrWhiteSpace(description)
			? label
			: $"{label} ({description})";

		return new OsmMapElementItem
		{
			Id = $"{element.Type}/{element.Id}",
			ElementType = element.Type,
			Label = label,
			Category = category,
			Type = type,
			Description = description,
			CopyText = copyText,
			DistanceMeters = GetDistanceMeters(latitude, longitude, element)
		};
	}

	private static double? GetDistanceMeters(double latitude, double longitude,
		OverpassElement element)
	{
		var pointLat = element.Center?.Lat ?? element.Lat;
		var pointLon = element.Center?.Lon ?? element.Lon;
		if ( pointLat == null || pointLon == null )
		{
			return null;
		}

		const double earthRadius = 6371000;
		var dLat = DegreesToRadians(pointLat.Value - latitude);
		var dLon = DegreesToRadians(pointLon.Value - longitude);
		var lat1 = DegreesToRadians(latitude);
		var lat2 = DegreesToRadians(pointLat.Value);

		var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
		        Math.Cos(lat1) * Math.Cos(lat2) *
		        Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
		var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
		return Math.Round(earthRadius * c, 1);
	}

	private static double DegreesToRadians(double degrees)
	{
		return degrees * Math.PI / 180;
	}

	private static int GetNearbyPriority(string? category) =>
		category != null && NearbyPriority.TryGetValue(category, out var p) ? p : int.MaxValue;

	private static string Beautify(string value)
	{
		return string.Join(" ", value
			.Split('_', StringSplitOptions.RemoveEmptyEntries)
			.Select(part => char.ToUpperInvariant(part[0]) + part[1..]));
	}

	private sealed record ParsedOverpassResponse(
		System.Collections.Generic.List<OverpassElement> Elements,
		string? Error);

	private static string? ChoosePreferredError(string? existingError, string? candidateError)
	{
		if ( string.IsNullOrWhiteSpace(candidateError) )
		{
			return existingError;
		}

		if ( string.IsNullOrWhiteSpace(existingError) )
		{
			return candidateError;
		}

		if ( existingError == ErrorMessage && candidateError != ErrorMessage )
		{
			return candidateError;
		}

		return existingError;
	}
}
