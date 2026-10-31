using System;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models;
using starsky.foundation.database.Query;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Tenancy;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.database.QueryTest;

[TestClass]
public sealed class QueryTenantTest
{
	public TestContext TestContext { get; set; } = null!;

	private sealed class Setup : IAsyncDisposable
	{
		public readonly SqliteConnection Connection = new("Filename=:memory:");
		public ServiceProvider Provider = null!;
		public IMemoryCache Cache = null!;

		public async ValueTask DisposeAsync()
		{
			await Provider.DisposeAsync();
			await Connection.DisposeAsync();
		}

		public static async Task<Setup> Create(TestContext testContext)
		{
			var setup = new Setup();
			await setup.Connection.OpenAsync(testContext.CancellationToken);
			var services = new ServiceCollection();
			services.AddMemoryCache();
			services.AddScoped<ITenantContext, TenantContext>();
			services.AddDbContext<ApplicationDbContext>(o => o.UseSqlite(setup.Connection));
			setup.Provider = services.BuildServiceProvider();
			setup.Cache = setup.Provider.GetRequiredService<IMemoryCache>();

			foreach ( var tenant in new[] { "default", "beta" } )
			{
				await using var context = setup.ContextFor(tenant, out _);
				await context.Database.EnsureCreatedAsync(testContext.CancellationToken);
				context.FileIndex.Add(new FileIndexItem("/same.jpg") { Tags = tenant });
				await context.SaveChangesAsync(testContext.CancellationToken);
			}

			return setup;
		}

		public ApplicationDbContext ContextFor(string tenantId, out TenantContext tenantContext)
		{
			tenantContext = new TenantContext();
			tenantContext.SetTenant(tenantId);
			return new ApplicationDbContext(
				new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(Connection).Options,
				tenantContext);
		}

		public Query QueryFor(string tenantId, ApplicationDbContext? context = null)
		{
			var scopeContext = new TenantContext();
			scopeContext.SetTenant(tenantId);
			return new Query(context ?? ContextFor(tenantId, out _), new AppSettings(),
				Provider.GetRequiredService<IServiceScopeFactory>(), new FakeIWebLogger(), Cache,
				scopeContext);
		}
	}

	[TestMethod]
	public async Task GetObjectByFilePath_UsesTheTenantOfTheQuery()
	{
		await using var setup = await Setup.Create(TestContext);

		var defaultItem = await setup.QueryFor("default").GetObjectByFilePathAsync("/same.jpg");
		var betaItem = await setup.QueryFor("beta").GetObjectByFilePathAsync("/same.jpg");

		Assert.AreEqual("default", defaultItem?.Tags);
		Assert.AreEqual("beta", betaItem?.Tags);
	}

	[TestMethod]
	public async Task FallbackScope_AfterDisposedContext_KeepsTheTenant()
	{
		await using var setup = await Setup.Create(TestContext);
		var disposed = setup.ContextFor("beta", out _);
		await disposed.DisposeAsync();

		var item = setup.QueryFor("beta", disposed).GetObjectByFilePath("/same.jpg");

		// a new scope has the default tenant unless the tenant is carried over
		Assert.AreEqual("beta", item?.Tags);
	}

	[TestMethod]
	public async Task Cache_IsSeparatedPerTenant()
	{
		await using var setup = await Setup.Create(TestContext);
		var cacheTime = TimeSpan.FromMinutes(1);

		var first = await setup.QueryFor("default")
			.GetObjectByFilePathAsync("/same.jpg", cacheTime);
		var fromCacheDefault = await setup.QueryFor("default")
			.GetObjectByFilePathAsync("/same.jpg", cacheTime);
		var beta = await setup.QueryFor("beta").GetObjectByFilePathAsync("/same.jpg", cacheTime);
		var fromCacheBeta = await setup.QueryFor("beta")
			.GetObjectByFilePathAsync("/same.jpg", cacheTime);

		Assert.AreEqual("default", first?.Tags);
		Assert.AreEqual(FileIndexItem.ExifStatus.OkAndSame, fromCacheDefault?.Status);
		Assert.AreEqual("default", fromCacheDefault?.Tags);
		Assert.AreEqual("beta", beta?.Tags);
		Assert.AreEqual(FileIndexItem.ExifStatus.OkAndSame, fromCacheBeta?.Status);
		Assert.AreEqual("beta", fromCacheBeta?.Tags);
	}

	[TestMethod]
	public async Task Cache_DefaultTenantKeepsOriginalKeys()
	{
		await using var setup = await Setup.Create(TestContext);

		await setup.QueryFor("default")
			.GetObjectByFilePathAsync("/same.jpg", TimeSpan.FromMinutes(1));
		await setup.QueryFor("beta")
			.GetObjectByFilePathAsync("/same.jpg", TimeSpan.FromMinutes(1));

		Assert.IsTrue(setup.Cache.TryGetValue(Query.GetObjectByFilePathAsyncCacheName("/same.jpg"),
			out _));
		Assert.IsTrue(setup.Cache.TryGetValue(
			"tenant-beta:" + Query.GetObjectByFilePathAsyncCacheName("/same.jpg"), out _));
	}
}
