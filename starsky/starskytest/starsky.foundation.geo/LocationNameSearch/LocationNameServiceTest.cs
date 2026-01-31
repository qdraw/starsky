using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models;
using starsky.foundation.geo.LocationNameSearch;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.geo.LocationNameSearch;

[TestClass]
public class LocationNameServiceTest
{
	[TestMethod]
	public async Task SearchCity_ReturnsOrderedResults()
	{
		var query = new FakeGeoNamesCitiesQuery();
		query.Cities.AddRange([
			new GeoNameCity { Name = "Amsterdam", Population = 1000000 },
			new GeoNameCity { Name = "Amersfoort", Population = 200000 },
			new GeoNameCity { Name = "Rotterdam", Population = 800000 },
			new GeoNameCity { Name = "Amstel", Population = 10000 }
		]);
		var seed = new FakeGeoNameCitySeedService();
		var logger = new FakeIWebLogger();
		var service = new LocationNameService(query, seed, logger);
		var result = await service.SearchCity("Am");
		Assert.HasCount(3, result); // Amsterdam, Amersfoort, Amstel
		Assert.AreEqual("Amsterdam", result[0].Name);
		Assert.AreEqual("Amersfoort", result[1].Name);
		Assert.AreEqual("Amstel", result[2].Name);
	}

	[TestMethod]
	public async Task SearchCity_SeedFails_ReturnsEmpty()
	{
		var query = new FakeGeoNamesCitiesQuery();
		var seed = new FakeGeoNameCitySeedService { SeedResult = false };
		var logger = new FakeIWebLogger();
		var service = new LocationNameService(query, seed, logger);
		var result = await service.SearchCity("Am");
		Assert.IsEmpty(result);
	}

	[TestMethod]
	public async Task SearchCity_NoMatches_ReturnsEmpty()
	{
		var query = new FakeGeoNamesCitiesQuery();
		query.Cities.Add(new GeoNameCity { Name = "Rotterdam", Population = 800000 });
		var seed = new FakeGeoNameCitySeedService();
		var logger = new FakeIWebLogger();
		var service = new LocationNameService(query, seed, logger);
		var result = await service.SearchCity("Utrecht");
		Assert.IsEmpty(result);
	}
}
