using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.geolookup.Models;
using starsky.feature.geolookup.Services;
using starsky.foundation.database.Data;
using starsky.foundation.database.Thumbnails;
using starsky.foundation.platform.Tenancy;
using starskytest.FakeMocks;

namespace starskytest.Tenancy;

[TestClass]
public sealed class TenantCacheKeysTest
{
	[TestCleanup]
	public void Cleanup()
	{
		TenantAmbient.Reset();
	}

	[TestMethod]
	[DataRow("default", "key", "key")]
	[DataRow("acme", "key", "tenant-acme:key")]
	public void Prefix_DefaultKeepsTheKey(string tenantId, string key, string expected)
	{
		Assert.AreEqual(expected, TenantCacheKey.Prefix(key, tenantId));
	}

	[TestMethod]
	public void Prefix_UsesTheAmbientTenant()
	{
		TenantAmbient.Set("beta");

		Assert.AreEqual("tenant-beta:x", TenantCacheKey.Prefix("x"));

		TenantAmbient.Reset();
		Assert.AreEqual("x", TenantCacheKey.Prefix("x"));
	}

	[TestMethod]
	public void ThumbnailQuery_RunningJobFlag_IsPerTenant()
	{
		using var connection = new SqliteConnection("Filename=:memory:");
		using var context = new ApplicationDbContext(
			new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(connection).Options);
		using var cache = new MemoryCache(new MemoryCacheOptions());
		var query = new ThumbnailQuery(context, null, new FakeIWebLogger(), cache);

		TenantAmbient.Set("acme");
		query.SetRunningJob(true);

		Assert.IsTrue(query.IsRunningJob());
		TenantAmbient.Set("beta");
		Assert.IsFalse(query.IsRunningJob());
		TenantAmbient.Reset();
		Assert.IsFalse(query.IsRunningJob());
	}

	[TestMethod]
	public void GeoCacheStatus_IsPerTenant()
	{
		using var cache = new MemoryCache(new MemoryCacheOptions());
		var service = new GeoCacheStatusService(cache);

		TenantAmbient.Set("acme");
		service.StatusUpdate("/folder", 5, StatusType.Total);
		Assert.AreEqual(5, service.Status("/folder").Total);

		TenantAmbient.Set("beta");
		Assert.AreEqual(0, service.Status("/folder").Total);
	}
}
