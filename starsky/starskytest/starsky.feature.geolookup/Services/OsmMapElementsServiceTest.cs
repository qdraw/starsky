using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.geolookup.Services;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.geolookup.Services;

[TestClass]
public class OsmMapElementsServiceTest
{
	[TestMethod]
	public async Task LookupAsync_ReturnsNormalizedNearbyAndEnclosingObjects()
	{
		// building (priority 4) ranks before amenity (priority 6), regardless of distance
		const string nearbyJson = "{\"elements\":[" +
		                          "{\"type\":\"node\",\"id\":1,\"lat\":52.52,\"lon\":13.405,\"tags\":{\"name\":\"Cafe Central\",\"amenity\":\"cafe\"}}," +
		                          "{\"type\":\"way\",\"id\":2,\"center\":{\"lat\":52.5202,\"lon\":13.4052},\"tags\":{\"name\":\"Block B\",\"building\":\"apartments\"}}]}";

		const string enclosingJson = "{\"elements\":[" +
		                             "{\"type\":\"relation\",\"id\":47811,\"tags\":{\"name\":\"Amsterdam\",\"boundary\":\"administrative\"}}]}";

		var nearbyUrl = BuildNearbyUrl("overpass-api.de/api/interpreter", 52.52, 13.405);
		var enclosingUrl = BuildEnclosingUrl("overpass-api.de/api/interpreter", 52.52, 13.405);
		var fakeHttp = new FakeIHttpClientHelper(null!, new Dictionary<string, KeyValuePair<bool, string>>
		{
			{ nearbyUrl, new KeyValuePair<bool, string>(true, nearbyJson) },
			{ enclosingUrl, new KeyValuePair<bool, string>(true, enclosingJson) }
		});

		var service = new OsmMapElementsService(fakeHttp);
		var result = await service.LookupAsync(52.52, 13.405);

		Assert.IsNull(result.Error);
		Assert.AreEqual(2, result.NearbyObjects.Count);
		Assert.AreEqual(1, result.EnclosingObjects.Count);
		Assert.AreEqual("Block B", result.NearbyObjects[0].Label);
		Assert.AreEqual("Cafe Central", result.NearbyObjects[1].Label);
		Assert.AreEqual("Amsterdam", result.EnclosingObjects[0].Label);
	}

	[TestMethod]
	public async Task LookupAsync_InvalidLocation_ReturnsError()
	{
		var service = new OsmMapElementsService(new FakeIHttpClientHelper(null!,
			new Dictionary<string, KeyValuePair<bool, string>>()));
		var result = await service.LookupAsync(999, 999);
		Assert.AreEqual("Non-valid location", result.Error);
	}

	[TestMethod]
	public async Task LookupAsync_InvalidJson_ReturnsError()
	{
		var nearbyUrl = BuildNearbyUrl("overpass-api.de/api/interpreter", 52.52, 13.405);
		var enclosingUrl = BuildEnclosingUrl("overpass-api.de/api/interpreter", 52.52, 13.405);
		var fakeHttp = new FakeIHttpClientHelper(null!, new Dictionary<string, KeyValuePair<bool, string>>
		{
			{ nearbyUrl, new KeyValuePair<bool, string>(true, "{invalid") },
			{ enclosingUrl, new KeyValuePair<bool, string>(true, "{invalid") }
		});

		var service = new OsmMapElementsService(fakeHttp);
		var result = await service.LookupAsync(52.52, 13.405);
		Assert.AreEqual("Failed to parse OSM map elements response", result.Error);
	}

	[TestMethod]
	public async Task LookupAsync_ValidJsonWithoutElements_ReturnsEmptyResultWithoutError()
	{
		var nearbyUrl = BuildNearbyUrl("overpass-api.de/api/interpreter", 52.52, 13.405);
		var enclosingUrl = BuildEnclosingUrl("overpass-api.de/api/interpreter", 52.52, 13.405);
		var fakeHttp = new FakeIHttpClientHelper(null!, new Dictionary<string, KeyValuePair<bool, string>>
		{
			{ nearbyUrl, new KeyValuePair<bool, string>(true, "{\"version\":0.6}") },
			{ enclosingUrl, new KeyValuePair<bool, string>(true, "{\"version\":0.6}") }
		});

		var service = new OsmMapElementsService(fakeHttp);
		var result = await service.LookupAsync(52.52, 13.405);

		Assert.IsNull(result.Error);
		Assert.AreEqual(0, result.NearbyObjects.Count);
		Assert.AreEqual(0, result.EnclosingObjects.Count);
	}

	[TestMethod]
	public async Task LookupAsync_OverpassRemark_ReturnsRemarkAsError()
	{
		var nearbyUrl = BuildNearbyUrl("overpass-api.de/api/interpreter", 52.52, 13.405);
		var enclosingUrl = BuildEnclosingUrl("overpass-api.de/api/interpreter", 52.52, 13.405);
		var fakeHttp = new FakeIHttpClientHelper(null!, new Dictionary<string, KeyValuePair<bool, string>>
		{
			{ nearbyUrl, new KeyValuePair<bool, string>(true, "{\"remark\":\"runtime error: Query timed out\"}") },
			{ enclosingUrl, new KeyValuePair<bool, string>(true, "{\"remark\":\"runtime error: Query timed out\"}") }
		});

		var service = new OsmMapElementsService(fakeHttp);
		var result = await service.LookupAsync(52.52, 13.405);

		Assert.AreEqual("runtime error: Query timed out", result.Error);
	}

	[TestMethod]
	public async Task LookupAsync_UsesFallbackHost_WhenPreferredHostReturnsNoData()
	{
		const string empty = "{\"elements\":[]}";
		const string nearbyJson = "{\"elements\":[" +
		                          "{\"type\":\"node\",\"id\":99,\"lat\":52.3743,\"lon\":4.90879,\"tags\":{\"name\":\"Damrak\",\"highway\":\"bus_stop\"}}]}";
		const string enclosingJson = "{\"elements\":[" +
		                             "{\"type\":\"relation\",\"id\":271110,\"tags\":{\"name\":\"Amsterdam\",\"boundary\":\"administrative\"}}]}";

		var preferredNearby = BuildNearbyUrl("overpass-api.de/api/interpreter", 52.37430, 4.90879);
		var preferredEnclosing = BuildEnclosingUrl("overpass-api.de/api/interpreter", 52.37430, 4.90879);
		var fallbackNearby = BuildNearbyUrl("z.overpass-api.de/api/interpreter", 52.37430, 4.90879);
		var fallbackEnclosing = BuildEnclosingUrl("z.overpass-api.de/api/interpreter", 52.37430, 4.90879);

		var fakeHttp = new FakeIHttpClientHelper(null!, new Dictionary<string, KeyValuePair<bool, string>>
		{
			{ preferredNearby, new KeyValuePair<bool, string>(true, empty) },
			{ preferredEnclosing, new KeyValuePair<bool, string>(true, empty) },
			{ fallbackNearby, new KeyValuePair<bool, string>(true, nearbyJson) },
			{ fallbackEnclosing, new KeyValuePair<bool, string>(true, enclosingJson) }
		});

		var service = new OsmMapElementsService(fakeHttp);
		var result = await service.LookupAsync(52.37430, 4.90879);

		Assert.IsNull(result.Error);
		Assert.AreEqual(1, result.NearbyObjects.Count);
		Assert.AreEqual(1, result.EnclosingObjects.Count);
		Assert.AreEqual("Damrak", result.NearbyObjects[0].Label);
		Assert.AreEqual("Amsterdam", result.EnclosingObjects[0].Label);
		CollectionAssert.AreEqual(
			new[] { preferredNearby, preferredEnclosing, fallbackNearby, fallbackEnclosing },
			fakeHttp.UrlsCalled);
	}

	[TestMethod]
	public async Task LookupAsync_EnclosingObjects_SortedByDistanceFromCentre()
	{
		// Oosterdok multipolygon centre is ~0 m away; Amsterdam municipality centre is ~2 km away.
		// Closer centre = smaller/more specific area → should appear first.
		const double lat = 52.37432;
		const double lon = 4.90882;

		const string enclosingJson = "{\"elements\":[" +
		                             // Nederland — centre is in the Caribbean, very far
		                             "{\"type\":\"relation\",\"id\":47796," +
		                             "\"center\":{\"lat\":32.787,\"lon\":-30.699}," +
		                             "\"tags\":{\"name\":\"Nederland\",\"boundary\":\"administrative\",\"admin_level\":\"3\"}}," +
		                             // Amsterdam — centre ~2 km away
		                             "{\"type\":\"relation\",\"id\":47811," +
		                             "\"center\":{\"lat\":52.3545,\"lon\":4.9182}," +
		                             "\"tags\":{\"name\":\"Amsterdam\",\"boundary\":\"administrative\",\"admin_level\":\"8\"}}," +
		                             // Oosterdok water multipolygon — centre right on the query point (~0 m)
		                             "{\"type\":\"relation\",\"id\":8878552," +
		                             "\"center\":{\"lat\":52.37432,\"lon\":4.90882}," +
		                             "\"tags\":{\"name\":\"Oosterdok\",\"natural\":\"water\",\"water\":\"canal\"}}]}";

		var nearbyUrl = BuildNearbyUrl("overpass-api.de/api/interpreter", lat, lon);
		var enclosingUrl = BuildEnclosingUrl("overpass-api.de/api/interpreter", lat, lon);
		var fakeHttp = new FakeIHttpClientHelper(null!, new Dictionary<string, KeyValuePair<bool, string>>
		{
			{ nearbyUrl, new KeyValuePair<bool, string>(true, "{\"elements\":[]}") },
			{ enclosingUrl, new KeyValuePair<bool, string>(true, enclosingJson) }
		});

		var service = new OsmMapElementsService(fakeHttp);
		var result = await service.LookupAsync(lat, lon);

		Assert.AreEqual(3, result.EnclosingObjects.Count);
		Assert.AreEqual("Oosterdok", result.EnclosingObjects[0].Label);
		Assert.AreEqual("Amsterdam", result.EnclosingObjects[1].Label);
		Assert.AreEqual("Nederland", result.EnclosingObjects[2].Label);
	}

	[TestMethod]
	public async Task LookupAsync_WaterwayRanksBeforeCloserBuilding()
	{
		// A building (5 m away) should NOT beat a canal (45 m away) in position 0
		const double lat = 52.37427;
		const double lon = 4.90883;

		const string nearbyJson = "{\"elements\":[" +
		                          // Building closer (5 m away)
		                          "{\"type\":\"way\",\"id\":1," +
		                          "\"center\":{\"lat\":52.37431,\"lon\":4.90884}," +
		                          "\"tags\":{\"name\":\"Some Building\",\"building\":\"yes\"}}," +
		                          // Canal further (45 m away) — higher priority
		                          "{\"type\":\"way\",\"id\":29474883," +
		                          "\"center\":{\"lat\":52.37467,\"lon\":4.90883}," +
		                          "\"tags\":{\"name\":\"Oosterdok\",\"waterway\":\"canal\"}}]}";

		var nearbyUrl = BuildNearbyUrl("overpass-api.de/api/interpreter", lat, lon);
		var enclosingUrl = BuildEnclosingUrl("overpass-api.de/api/interpreter", lat, lon);
		var fakeHttp = new FakeIHttpClientHelper(null!, new Dictionary<string, KeyValuePair<bool, string>>
		{
			{ nearbyUrl, new KeyValuePair<bool, string>(true, nearbyJson) },
			{ enclosingUrl, new KeyValuePair<bool, string>(true, "{\"elements\":[]}") }
		});

		var service = new OsmMapElementsService(fakeHttp);
		var result = await service.LookupAsync(lat, lon);

		Assert.AreEqual(2, result.NearbyObjects.Count);
		Assert.AreEqual("Oosterdok", result.NearbyObjects[0].Label);
		Assert.AreEqual("Some Building", result.NearbyObjects[1].Label);
	}

	[TestMethod]
	public async Task LookupAsync_OosterdokCanal_ReturnsCorrectLabel()
	{
		// lat=52.37427 lon=4.90883 → nearby should return Oosterdok canal
		const double lat = 52.37427;
		const double lon = 4.90883;

		const string nearbyJson = "{\"elements\":[" +
		                          "{\"type\":\"way\",\"id\":29474883," +
		                          "\"center\":{\"lat\":52.37497,\"lon\":4.90701}," +
		                          "\"tags\":{\"name\":\"Oosterdok\",\"waterway\":\"canal\"," +
		                          "\"wikidata\":\"Q2302371\"}}]}";

		const string enclosingJson = "{\"elements\":[" +
		                             "{\"type\":\"relation\",\"id\":11956771," +
		                             "\"tags\":{\"name\":\"Oosterdokseiland\",\"boundary\":\"place\"," +
		                             "\"place\":\"neighbourhood\",\"type\":\"boundary\"," +
		                             "\"wikidata\":\"Q2538727\"}}]}";

		var nearbyUrl = BuildNearbyUrl("overpass-api.de/api/interpreter", lat, lon);
		var enclosingUrl = BuildEnclosingUrl("overpass-api.de/api/interpreter", lat, lon);
		var fakeHttp = new FakeIHttpClientHelper(null!, new Dictionary<string, KeyValuePair<bool, string>>
		{
			{ nearbyUrl, new KeyValuePair<bool, string>(true, nearbyJson) },
			{ enclosingUrl, new KeyValuePair<bool, string>(true, enclosingJson) }
		});

		var service = new OsmMapElementsService(fakeHttp);
		var result = await service.LookupAsync(lat, lon);

		Assert.IsNull(result.Error);
		Assert.AreEqual(1, result.NearbyObjects.Count);
		Assert.AreEqual("Oosterdok", result.NearbyObjects[0].Label);
		Assert.AreEqual("waterway", result.NearbyObjects[0].Category);
		Assert.AreEqual("canal", result.NearbyObjects[0].Type);
		Assert.AreEqual(1, result.EnclosingObjects.Count);
		Assert.AreEqual("Oosterdokseiland", result.EnclosingObjects[0].Label);
	}

	private static string BuildNearbyUrl(string baseUrl, double latitude, double longitude)
	{
		var lat = latitude.ToString(CultureInfo.InvariantCulture);
		var lon = longitude.ToString(CultureInfo.InvariantCulture);
		var radius = 20.0.ToString(CultureInfo.InvariantCulture);
		var query = "[timeout:10][out:json];" +
		            "(" +
		            $"node(around:{radius},{lat},{lon});" +
		            $"way(around:{radius},{lat},{lon});" +
		            $"relation(around:{radius},{lat},{lon});" +
		            ");" +
		            "out center tags qt;";

		return BuildUrl(baseUrl, query);
	}

	private static string BuildEnclosingUrl(string baseUrl, double latitude, double longitude)
	{
		var lat = latitude.ToString(CultureInfo.InvariantCulture);
		var lon = longitude.ToString(CultureInfo.InvariantCulture);
		var query = "[timeout:10][out:json];" +
		            $"is_in({lat},{lon})->.a;" +
		            "(" +
		            "way(pivot.a);" +
		            "relation(pivot.a);" +
		            ");" +
		            "out center tags qt;";

		return BuildUrl(baseUrl, query);
	}

	private static string BuildUrl(string baseUrl, string query)
	{
		return $"https://{baseUrl}?data={System.Uri.EscapeDataString(query)}";
	}
}