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
	private const string OverpassBaseUrl = "overpass-api.de/api/interpreter";
	private const int NearbyRadiusInMeters = 30;
	private const int MaxItemsPerSection = 8;

	private static readonly string[] DescriptorTagKeys =
	[
		"amenity", "shop", "tourism", "historic", "leisure", "man_made", "building",
		"highway", "natural", "landuse", "boundary", "place"
	];

	private static readonly string[] NameTagKeys = ["name", "official_name", "brand", "operator", "ref"];

	public async Task<OsmMapElementsResult> LookupAsync(double latitude, double longitude)
	{
		if ( !ValidateLocation.ValidateLatitudeLongitude(latitude, longitude) )
		{
			return new OsmMapElementsResult { Error = "Non-valid location" };
		}

		const string errorMessage = "Failed to parse OSM map elements response";
		var url = BuildUrl(latitude, longitude);
		var response = await httpClientHelper.ReadString(url);
		if ( !response.Key || string.IsNullOrWhiteSpace(response.Value) )
		{
			return new OsmMapElementsResult { Error = errorMessage };
		}

		try
		{
			var overpassResponse = JsonSerializer.Deserialize<OverpassResponse>(response.Value);
			if ( overpassResponse?.Elements == null )
			{
				return new OsmMapElementsResult { Error = errorMessage };
			}

			return new OsmMapElementsResult
			{
				NearbyObjects = overpassResponse.Elements
					.Where(element => element.Type != "area")
					.Select(element => ToMapElementItem(element, latitude, longitude))
					.Where(item => item != null)
					.Cast<OsmMapElementItem>()
					.OrderBy(item => item.DistanceMeters ?? double.MaxValue)
					.GroupBy(item => item.CopyText)
					.Select(group => group.First())
					.Take(MaxItemsPerSection)
					.ToList(),
				EnclosingObjects = overpassResponse.Elements
					.Where(element => element.Type == "area")
					.Select(element => ToMapElementItem(element, latitude, longitude))
					.Where(item => item != null)
					.Cast<OsmMapElementItem>()
					.GroupBy(item => item.CopyText)
					.Select(group => group.First())
					.Take(MaxItemsPerSection)
					.ToList()
			};
		}
		catch
		{
			return new OsmMapElementsResult { Error = errorMessage };
		}
	}

	private static string BuildUrl(double latitude, double longitude)
	{
		var query = "[out:json][timeout:25];" +
		            "(" +
		            $"node(around:{NearbyRadiusInMeters},{latitude.ToString(CultureInfo.InvariantCulture)},{longitude.ToString(CultureInfo.InvariantCulture)});" +
		            $"way(around:{NearbyRadiusInMeters},{latitude.ToString(CultureInfo.InvariantCulture)},{longitude.ToString(CultureInfo.InvariantCulture)});" +
		            $"relation(around:{NearbyRadiusInMeters},{latitude.ToString(CultureInfo.InvariantCulture)},{longitude.ToString(CultureInfo.InvariantCulture)});" +
		            ");" +
		            "out center tags qt;" +
		            $"is_in({latitude.ToString(CultureInfo.InvariantCulture)},{longitude.ToString(CultureInfo.InvariantCulture)})->.containing;" +
		            "area.containing;" +
		            "out tags qt;";

		return $"{HttpsPrefix}{OverpassBaseUrl}?data={Uri.EscapeDataString(query)}";
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

	private static string Beautify(string value)
	{
		return string.Join(" ", value
			.Split('_', StringSplitOptions.RemoveEmptyEntries)
			.Select(part => char.ToUpperInvariant(part[0]) + part[1..]));
	}
}