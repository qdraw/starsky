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
		const string nearbyJson = "{\"elements\":[" +
		                          "{\"type\":\"node\",\"id\":1,\"lat\":52.52,\"lon\":13.405,\"tags\":{\"name\":\"Cafe Central\",\"amenity\":\"cafe\"}}," +
		                          "{\"type\":\"way\",\"id\":2,\"center\":{\"lat\":52.5202,\"lon\":13.4052},\"tags\":{\"building\":\"apartments\"}}]}";

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
		Assert.AreEqual("Cafe Central", result.NearbyObjects[0].Label);
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
		var fallbackNearby = BuildNearbyUrl("overpass.osm.ch/api/interpreter", 52.37430, 4.90879);
		var fallbackEnclosing = BuildEnclosingUrl("overpass.osm.ch/api/interpreter", 52.37430, 4.90879);

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

	private static string BuildNearbyUrl(string baseUrl, double latitude, double longitude)
	{
		var lat = latitude.ToString(CultureInfo.InvariantCulture);
		var lon = longitude.ToString(CultureInfo.InvariantCulture);
		var radius = 33.75.ToString(CultureInfo.InvariantCulture);
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
		            "way(pivot.a);" +
		            "relation(pivot.a);" +
		            "out center tags qt;";

		return BuildUrl(baseUrl, query);
	}

	private static string BuildUrl(string baseUrl, string query)
	{
		return $"https://{baseUrl}?data={System.Uri.EscapeDataString(query)}";
	}
}