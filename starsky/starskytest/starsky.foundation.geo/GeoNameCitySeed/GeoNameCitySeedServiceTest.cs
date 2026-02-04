using System;
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
using starsky.foundation.storage.Interfaces;
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

	private static GeoNameCitySeedService CreateSut(ApplicationDbContext dbContext,
		IStorage? storage = null)
	{
		storage ??= new ReadLinesFakeIStorage();
		var appSettings = new AppSettings { DependenciesFolder = "." };
		var fakeSelectorStorage = new FakeSelectorStorage(storage);
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
		Assert.HasCount(68, all);

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
		Assert.HasCount(68, all);

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
	
	[TestMethod]
	public async Task Setup_ReturnsFalse_NoMemoryCacheNull()
	{
		await using var dbContext = CreateDbContext("nocache");
		var appSettings = new AppSettings { DependenciesFolder = "." };
		var fakeSelectorStorage = new FakeSelectorStorage(new ReadLinesFakeIStorage());
		var fakeGeoFileDownload = new FakeIGeoFileDownload();
		var fakeLogger = new FakeIWebLogger();
		var fakeScopeFactory = new FakeIServiceScopeFactory();
		var geoNamesCitiesQuery = new GeoNamesCitiesQuery(dbContext, fakeScopeFactory);
		var sut = new GeoNameCitySeedService(
			fakeSelectorStorage,
			appSettings,
			fakeGeoFileDownload,
			geoNamesCitiesQuery,
			fakeLogger,
			null
		);

		// No cache set
		var awaitedResult = await sut.Setup();
		Assert.IsFalse(awaitedResult,
			"Setup should return false when cache is not set and file exists");
	}

	[TestMethod]
	public async Task ParseCityAsync_ThrowsFormatException_WhenLineIsInvalid()
	{
		await using var dbContext = CreateDbContext("invalid");
		var sut = CreateSut(dbContext);
		const string invalidLine = "1\tAmsterdam\tAmsterdam"; // Less than 19 fields
		await Assert.ThrowsExactlyAsync<FormatException>(async () =>
		{
			await sut.ParseCityAsync(invalidLine);
		});
	}

	[TestMethod]
	public async Task ParseCityAsync_ParsesValidLine()
	{
		await using var dbContext = CreateDbContext("valid");
		var sut = CreateSut(dbContext);
		// Add missing fields for population, elevation, dem, timezone, date
		const string validLineFull =
			"1\tAmsterdam\tAmsterdam\tAms,Amsterdam\t52.3702\t4.8952\tP\tPPL" +
			"\tNL\t\t07\t\t\t\t821752\t\t2\tEurope/Amsterdam\t2024-02-01";
		var city = await sut.ParseCityAsync(validLineFull);
		Assert.AreEqual(1, city.GeonameId);
		Assert.AreEqual("Amsterdam", city.Name);
		Assert.AreEqual("NL", city.CountryCode);
		Assert.AreEqual("Europe/Amsterdam", city.TimeZoneId);
		Assert.AreEqual(new DateOnly(2024, 2, 1), city.ModificationDate);
	}

	[TestMethod]
	public async Task GetProvince_ReturnsProvinceName_WhenExists()
	{
		await using var dbContext = CreateDbContext("province");
		var sut = CreateSut(dbContext);

		// NL.03 is mapped to "Gelderland"
		var result = await sut.GetProvince("NL", "03");
		Assert.AreEqual("Gelderland", result);
	}

	[TestMethod]
	public async Task GetProvince_ReturnsEmpty_WhenNotExists()
	{
		await using var dbContext = CreateDbContext("province2");
		var sut = CreateSut(dbContext);

		// Unknown code
		var result = await sut.GetProvince("NL", "99");
		Assert.AreEqual(string.Empty, result);
	}

	[TestMethod]
	public async Task GetProvince_ReturnsEmpty_WhenAdminFileNotExistsFile()
	{
		await using var dbContext = CreateDbContext("province2");
		// dont use the ReadLinesFakeIStorage here
		var sut = CreateSut(dbContext, new FakeIStorage());

		var result = await sut.GetProvince("NL", "03");
		Assert.AreEqual(string.Empty, result);
	}

	private sealed class ReadLinesFakeIStorage : FakeIStorage
	{
		public override IAsyncEnumerable<string> ReadLinesAsync(string path,
			CancellationToken cancellationToken)
		{
			var lines = CreateAnGeoNameCitySeedData.SampleLines;
			if ( path is "./admin1CodesASCII.txt" or ".\\admin1CodesASCII.txt" )
			{
				lines = CreateAnGeoNameCitySeedData.Admin1Data;
			}

			return GetLines();

			async IAsyncEnumerable<string> GetLines()
			{
				foreach ( var line in lines )
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
