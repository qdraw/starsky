using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.geolookup.Services;
using starsky.foundation.http.Services;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.geolookup.Services;

/// <summary>
///     Calls the real Overpass API. Disabled by default: needs internet, the public servers are
///     rate limited and OpenStreetMap data changes. Overpass only accepts TLS 1.3, which .NET
///     does not support on macOS, there CurlPostHelper takes over so curl must be installed.
///     Remove the [Ignore] to run them, for example to refresh the expectations.
/// </summary>
[TestClass]
[Ignore("Calls the live Overpass API, remove this attribute to run")]
public class OsmMapElementsLiveTest
{
	private static OsmMapElementsService CreateService()
	{
		var factory = new ServiceCollection().AddHttpClient().BuildServiceProvider()
			.GetRequiredService<IHttpClientFactory>();
		var httpClientHelper = new HttpClientHelper(new HttpProvider(factory, new CurlPostHelper()), null,
			new FakeIWebLogger());
		return new OsmMapElementsService(httpClientHelper);
	}

	[TestMethod]
	[DynamicData(nameof(PlaceNames))]
	public async Task LookupAsync_LiveOverpass_ReturnsExpectedObjects(string name)
	{
		var place = OsmMapElementsExpectations.Places.Single(p => p.Name == name);

		var result = await CreateService().LookupAsync(place.Latitude, place.Longitude);

		OsmMapElementsTestHelper.AssertPlace(place, result);
		// pause between places, the public servers are rate limited
		await Task.Delay(3000);
	}

	[TestMethod]
	public async Task LookupAsync_LiveOverpass_OpenSeaHasNoNearbyObjects()
	{
		var result = await CreateService().LookupAsync(54.0, 3.0);

		Assert.IsNull(result.Error);
		Assert.IsEmpty(result.NearbyObjects);
	}

	public static System.Collections.Generic.IEnumerable<object[]> PlaceNames =>
		OsmMapElementsExpectations.PlaceNames();
}
