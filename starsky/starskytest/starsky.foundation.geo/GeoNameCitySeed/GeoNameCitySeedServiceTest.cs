using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Data;
using starsky.foundation.database.GeoNamesCities;
using starsky.foundation.geo.GeoNameCitySeed;
using starsky.foundation.platform.Models;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;
using VerifyMSTest;

namespace starskytest.starsky.foundation.geo.GeoNameCitySeed;

[TestClass]
public sealed class GeoNameCitySeedServiceTest : VerifyBase
{
	private static ApplicationDbContext CreateDbContext(string connectionString)
	{
		var options = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseInMemoryDatabase("GeoNameCitySeedServiceTestDb" + connectionString)
			.Options;
		return new ApplicationDbContext(options);
	}

	private static GeoNameCitySeedService CreateSut(ApplicationDbContext dbContext)
	{
		var appSettings = new AppSettings { DependenciesFolder = "." };
		var fakeSelectorStorage = new FakeSelectorStorage(new ReadLinesFakeIStorage());
		var fakeGeoFileDownload = new FakeIGeoFileDownload();
		var fakeLogger = new FakeIWebLogger();
		var fakeMemoryCache = new MemoryCache(new MemoryCacheOptions());
		var fakeScopeFactory = new FakeIServiceScopeFactory();
		var geoNamesCitiesQuery = new GeoNamesCitiesQuery(dbContext, fakeScopeFactory);
		return new GeoNameCitySeedService(
			fakeSelectorStorage,
			appSettings,
			fakeGeoFileDownload,
			geoNamesCitiesQuery,
			fakeLogger,
			fakeMemoryCache
		);
	}

	[TestMethod]
	public async Task Seed_InsertsUniqueCities_Verify()
	{
		await using var dbContext = CreateDbContext("1");
		var sut = CreateSut(dbContext);

		var result = await sut.Seed();
		Assert.IsTrue(result);

		var all = await dbContext.GeoNameCities.ToListAsync(TestContext.CancellationToken);
		Assert.HasCount(55, all);

		await Verify(all);

		Assert.IsTrue(all.Any(x => x.GeonameId == 38832 && x.Name == "Vila"));
		Assert.IsTrue(all.Any(x => x.GeonameId == 3039154 && x.Name == "El Tarter"));
		Assert.IsTrue(all.Any(x => x.GeonameId == 3039163 && x.Name.StartsWith("Sant Juli")));
		Assert.IsTrue(all.Any(x => x.GeonameId == 6691091 && x.Name.StartsWith("Al Karama")));
	}

	[TestMethod]
	public async Task Seed_InsertsUniqueCities_RunTwoTimes_Verify()
	{
		await using var dbContext = CreateDbContext("2");
		var sut = CreateSut(dbContext);

		var result1 = await sut.Seed();
		Assert.IsTrue(result1);
		// yes run a second time
		var result2 = await sut.Seed();
		Assert.IsTrue(result2);

		var all = await dbContext.GeoNameCities.ToListAsync(TestContext.CancellationToken);
		Assert.HasCount(55, all);

		await Verify(all);

		Assert.IsTrue(all.Any(x => x.GeonameId == 38832 && x.Name == "Vila"));
		Assert.IsTrue(all.Any(x => x.GeonameId == 3039154 && x.Name == "El Tarter"));
		Assert.IsTrue(all.Any(x => x.GeonameId == 3039163 && x.Name.StartsWith("Sant Juli")));
		Assert.IsTrue(all.Any(x => x.GeonameId == 6691091 && x.Name.StartsWith("Al Karama")));
	}

	[TestMethod]
	public async Task Setup_ReturnsTrue_WhenCacheIsSet()
	{
		await using var dbContext = CreateDbContext("cache");
		var appSettings = new AppSettings { DependenciesFolder = "." };
		var fakeSelectorStorage = new FakeSelectorStorage(new ReadLinesFakeIStorage());
		var fakeGeoFileDownload = new FakeIGeoFileDownload();
		var fakeLogger = new FakeIWebLogger();
		var memoryCache = new MemoryCache(new MemoryCacheOptions());
		var fakeScopeFactory = new FakeIServiceScopeFactory();
		var geoNamesCitiesQuery = new GeoNamesCitiesQuery(dbContext, fakeScopeFactory);
		var sut = new GeoNameCitySeedService(
			fakeSelectorStorage,
			appSettings,
			fakeGeoFileDownload,
			geoNamesCitiesQuery,
			fakeLogger,
			memoryCache
		);

		// Set the cache key to true
		const string cacheKey = "GeoNameCitySeedService.Setup";
		memoryCache.Set(cacheKey, true);

		var awaitedResult = await sut.Setup();

		Assert.IsTrue(awaitedResult, "Setup should return true when cache is set");
	}

	[TestMethod]
	public async Task Setup_ReturnsFalse_WhenCacheIsNotSet()
	{
		await using var dbContext = CreateDbContext("nocache");
		var appSettings = new AppSettings { DependenciesFolder = "." };
		var fakeSelectorStorage = new FakeSelectorStorage(new ReadLinesFakeIStorage());
		var fakeGeoFileDownload = new FakeIGeoFileDownload();
		var fakeLogger = new FakeIWebLogger();
		var memoryCache = new MemoryCache(new MemoryCacheOptions());
		var fakeScopeFactory = new FakeIServiceScopeFactory();
		var geoNamesCitiesQuery = new GeoNamesCitiesQuery(dbContext, fakeScopeFactory);
		var sut = new GeoNameCitySeedService(
			fakeSelectorStorage,
			appSettings,
			fakeGeoFileDownload,
			geoNamesCitiesQuery,
			fakeLogger,
			memoryCache
		);

		// No cache set
		var awaitedResult = await sut.Setup();
		Assert.IsFalse(awaitedResult,
			"Setup should return false when cache is not set and file exists");
	}

	private sealed class ReadLinesFakeIStorage : FakeIStorage
	{
		public override IAsyncEnumerable<string> ReadLinesAsync(string path,
			CancellationToken cancellationToken)
		{
			return GetLines();

			static async IAsyncEnumerable<string> GetLines()
			{
				foreach ( var line in CreateAnGeoNameCitySeedData.SampleLines )
				{
					await Task.Yield();
					yield return line;
				}
			}
		}

		public override bool ExistFile(string path)
		{
			return true;
		}
	}
}
