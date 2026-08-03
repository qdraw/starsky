using System.Collections.Generic;
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
		const string expectedJson = "{\"elements\":[" +
		                            "{\"type\":\"node\",\"id\":1,\"lat\":52.52,\"lon\":13.405,\"tags\":{\"name\":\"Cafe Central\",\"amenity\":\"cafe\"}}," +
		                            "{\"type\":\"way\",\"id\":2,\"center\":{\"lat\":52.5202,\"lon\":13.4052},\"tags\":{\"building\":\"apartments\"}}," +
		                            "{\"type\":\"area\",\"id\":3601,\"tags\":{\"name\":\"Amsterdam\",\"boundary\":\"administrative\"}}]}";

		var url = new[]
		{
			"https://overpass-api.de/api/interpreter?data="
		}.Single();
		var fakeHttp = new FakeIHttpClientHelper(null!, new Dictionary<string, KeyValuePair<bool, string>>
		{
			{ url, new KeyValuePair<bool, string>(true, expectedJson) }
		});

		var service = new OsmMapElementsServiceWithFixedUrl(fakeHttp, url);
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
		const string url = "https://overpass-api.de/api/interpreter?data=";
		var fakeHttp = new FakeIHttpClientHelper(null!, new Dictionary<string, KeyValuePair<bool, string>>
		{
			{ url, new KeyValuePair<bool, string>(true, "{invalid") }
		});

		var service = new OsmMapElementsServiceWithFixedUrl(fakeHttp, url);
		var result = await service.LookupAsync(52.52, 13.405);
		Assert.AreEqual("Failed to parse OSM map elements response", result.Error);
	}

	private sealed class OsmMapElementsServiceWithFixedUrl(FakeIHttpClientHelper httpClientHelper, string url)
		: OsmMapElementsService(httpClientHelper)
	{
		public new async Task<starsky.feature.geolookup.Models.OsmMapElementsResult> LookupAsync(double latitude,
			double longitude)
		{
			httpClientHelper.UrlsCalled.Clear();
			return await base.LookupAsync(latitude, longitude);
		}

		protected internal static string FixedUrl(string _) => url;
	}
}