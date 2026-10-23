using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.geolookup.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.geolookup.Services;

/// <summary>
///     Shared by the fixture and live tests of OsmMapElementsService
/// </summary>
internal static class OsmMapElementsTestHelper
{
	internal const string PreferredHost = "overpass-api.de/api/interpreter";

	/// <summary>
	///     Read a recorded Overpass response, see OsmMapElementsFixtures/capture-fixtures.js
	/// </summary>
	internal static string ReadFixture(string name, string kind)
	{
		var fileName = $"{name}.{kind}.json";
		var assembly = Assembly.GetExecutingAssembly();
		var resourceName = assembly.GetManifestResourceNames()
			.Single(p => p.EndsWith("OsmMapElementsFixtures." + fileName, StringComparison.Ordinal));
		using var stream = assembly.GetManifestResourceStream(resourceName)!;
		using var reader = new StreamReader(stream);
		return reader.ReadToEnd();
	}

	/// <summary>
	///     Http client that answers with the recorded nearby and enclosing response of one place
	/// </summary>
	internal static FakeIHttpClientHelper FakeHttpWithFixture(string name, double latitude,
		double longitude)
	{
		return new FakeIHttpClientHelper(null!, new Dictionary<string, KeyValuePair<bool, string>>
		{
			{
				BuildUrl(NearbyQuery(latitude, longitude)),
				new KeyValuePair<bool, string>(true, ReadFixture(name, "nearby"))
			},
			{
				BuildUrl(EnclosingQuery(latitude, longitude)),
				new KeyValuePair<bool, string>(true, ReadFixture(name, "enclosing"))
			}
		});
	}

	private static string NearbyQuery(double latitude, double longitude)
	{
		var lat = latitude.ToString(CultureInfo.InvariantCulture);
		var lon = longitude.ToString(CultureInfo.InvariantCulture);
		return "[timeout:10][out:json];" +
		       "(" +
		       $"node(around:20,{lat},{lon});" +
		       $"way(around:20,{lat},{lon});" +
		       $"relation(around:20,{lat},{lon});" +
		       ");" +
		       "out center tags qt;";
	}

	private static string EnclosingQuery(double latitude, double longitude)
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

	private static string BuildUrl(string query)
	{
		return $"https://{PreferredHost}?data={Uri.EscapeDataString(query)}";
	}

	/// <summary>
	///     Checks the result of one place against what a user expects
	/// </summary>
	internal static void AssertPlace(OsmMapElementsExpectations.Place place,
		OsmMapElementsResult result)
	{
		Assert.IsNull(result.Error, place.Name);
		var nearby = result.NearbyObjects.Select(p => p.Label).ToList();
		var enclosing = result.EnclosingObjects.Select(p => p.Label).ToList();

		if ( place.NearbyEmpty )
		{
			Assert.IsEmpty(nearby, place.Name);
		}

		if ( place.NearbyFirst != null )
		{
			Assert.AreEqual(place.NearbyFirst, nearby.FirstOrDefault(), place.Name);
		}

		if ( place.EnclosingFirst != null )
		{
			Assert.AreEqual(place.EnclosingFirst, enclosing.FirstOrDefault(), place.Name);
		}

		foreach ( var expected in place.NearbyContains ?? [] )
		{
			Assert.Contains(expected, nearby, $"{place.Name} nearby: {string.Join(", ", nearby)}");
		}

		foreach ( var expected in place.EnclosingContains ?? [] )
		{
			Assert.Contains(expected, enclosing,
				$"{place.Name} enclosing: {string.Join(", ", enclosing)}");
		}

		AssertGeneralRules(place.Name, result.NearbyObjects);
		AssertGeneralRules(place.Name, result.EnclosingObjects);
	}

	/// <summary>
	///     Rules that apply to every place
	/// </summary>
	internal static void AssertGeneralRules(string name, List<OsmMapElementItem> items)
	{
		Assert.IsLessThanOrEqualTo(8, items.Count, name);
		Assert.IsFalse(items.Any(p => string.IsNullOrWhiteSpace(p.Label)), name);
		// generic unnamed objects like building=yes are skipped
		Assert.IsFalse(
			items.Any(p => !p.IsNamed && string.Equals(p.Type, "yes", StringComparison.OrdinalIgnoreCase)),
			name);
		// no duplicates in what the user can copy
		Assert.AreEqual(items.Count, items.Select(p => p.CopyText).Distinct().Count(), name);
		// named objects are always ranked before unnamed ones
		var firstUnnamed = items.FindIndex(p => !p.IsNamed);
		if ( firstUnnamed >= 0 )
		{
			Assert.IsFalse(items.Skip(firstUnnamed).Any(p => p.IsNamed), name);
		}
	}
}
