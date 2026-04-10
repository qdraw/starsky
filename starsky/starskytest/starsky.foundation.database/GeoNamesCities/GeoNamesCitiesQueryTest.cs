using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Data;
using starsky.foundation.database.GeoNamesCities;
using starsky.foundation.database.Models;

namespace starskytest.starsky.foundation.database.GeoNamesCities;

[TestClass]
public class GeoNamesCitiesQueryTest
{
	[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
	public TestContext TestContext { get; set; }

	private static (ApplicationDbContext, IServiceScopeFactory) CreateDbContextAndScopeFactory(
		string dbName)
	{
		var services = new ServiceCollection();
		services.AddDbContext<ApplicationDbContext>(options =>
			options.UseInMemoryDatabase(dbName));
		var provider = services.BuildServiceProvider();
		return ( provider.GetRequiredService<ApplicationDbContext>(),
			provider.GetRequiredService<IServiceScopeFactory>() );
	}

	private static GeoNameCity SampleCity(int id, string name = "Sample")
	{
		return new GeoNameCity
		{
			GeonameId = id,
			Name = name,
			AsciiName = name,
			AlternateNames = name,
			Latitude = 1.0,
			Longitude = 1.0,
			FeatureClass = "P",
			FeatureCode = "PPL",
			CountryCode = "XX",
			Cc2 = "",
			Admin1Code = "01",
			Admin2Code = "02",
			Admin3Code = "03",
			Admin4Code = "04",
			Population = 1000,
			Elevation = 10,
			Dem = 10,
			TimeZoneId = "UTC",
			ModificationDate = DateOnly.FromDateTime(DateTime.UtcNow),
			Province = "Test"
		};
	}

	[TestMethod]
	public async Task AddRange_DisposedContext_UsesNewScope()
	{
		var (db, scopeFactory) = CreateDbContextAndScopeFactory("AddRange_Disposed");
		var query = new GeoNamesCitiesQuery(db, scopeFactory);
		var cities = new List<GeoNameCity> { SampleCity(1, "A"), SampleCity(2, "B") };
		await query.AddRange(cities);
		// Dispose the context
		await db.DisposeAsync();
		// Add more, should use new scope
		var moreCities = new List<GeoNameCity> { SampleCity(3, "C") };
		await query.AddRange(moreCities);
		// Use a new context to verify
		using var scope = scopeFactory.CreateScope();
		var db2 = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
		var all = await db2.GeoNameCities.ToListAsync(TestContext.CancellationToken);
		Assert.HasCount(3, all);
		Assert.Contains(x => x.GeonameId == 1 && x.Name == "A", all);
		Assert.Contains(x => x.GeonameId == 2 && x.Name == "B", all);
		Assert.Contains(x => x.GeonameId == 3 && x.Name == "C", all);
	}

	[TestMethod]
	public async Task GetItem_DisposedContext_UsesNewScope()
	{
		var (db, scopeFactory) = CreateDbContextAndScopeFactory("GetItem_Disposed");
		var query = new GeoNamesCitiesQuery(db, scopeFactory);
		var city = SampleCity(10, "X");
		db.GeoNameCities.Add(city);
		await db.SaveChangesAsync(TestContext.CancellationToken);
		await db.DisposeAsync();
		var result = await query.GetItem(10);
		Assert.IsNotNull(result);
		Assert.AreEqual("X", result.Name);
	}

	[TestMethod]
	public async Task Search_DisposedContext_UsesNewScope()
	{
		var (db, scopeFactory) = CreateDbContextAndScopeFactory("Search_Disposed");
		var query = new GeoNamesCitiesQuery(db, scopeFactory);
		var city1 = SampleCity(20, "Alpha");
		var city2 = SampleCity(21, "Beta");
		db.GeoNameCities.AddRange(city1, city2);
		await db.SaveChangesAsync(TestContext.CancellationToken);
		await db.DisposeAsync();
		var results = await query.Search("Alpha", 10, nameof(GeoNameCity.Name));
		Assert.HasCount(1, results);
		Assert.AreEqual("Alpha", results[0].Name);
	}

	[TestMethod]
	public async Task Search_Empty_NoResults()
	{
		var (db, scopeFactory) = CreateDbContextAndScopeFactory("Search1");
		var query = new GeoNamesCitiesQuery(db, scopeFactory);
		var results = await query.Search(string.Empty, 10, nameof(GeoNameCity.Name));
		Assert.HasCount(0, results);
	}

	[TestMethod]
	public async Task Search_Empty_NoFields()
	{
		var (db, scopeFactory) = CreateDbContextAndScopeFactory("Search1");
		var query = new GeoNamesCitiesQuery(db, scopeFactory);
		var results = await query.Search("test", 10);
		Assert.HasCount(0, results);
	}

	[TestMethod]
	public async Task Search_InValidField_NoResults()
	{
		var (db, scopeFactory) = CreateDbContextAndScopeFactory("Search1");
		var query = new GeoNamesCitiesQuery(db, scopeFactory);
		var results = await query.Search("test", 10, "invalid-field");
		Assert.HasCount(0, results);
	}
}
