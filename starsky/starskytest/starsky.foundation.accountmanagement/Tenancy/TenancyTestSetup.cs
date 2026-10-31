using System;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.accountmanagement.Services;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models;
using starsky.foundation.database.Tenancy;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Tenancy;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.accountmanagement.Tenancy;

/// <summary>
///     One shared in-memory SQLite database; contexts, user managers and services per tenant
/// </summary>
internal sealed class TenancyTestSetup : IAsyncDisposable
{
	private readonly SqliteConnection _connection = new("Filename=:memory:");
	private readonly string _root =
		System.IO.Path.Combine(System.IO.Path.GetTempPath(),
			"starsky-tenancy-" + Guid.NewGuid().ToString("N"));

	public readonly AppSettings AppSettings;
	public readonly IMemoryCache? Cache;
	public readonly DatabaseTenantRegistry Registry;
	private readonly ServiceProvider _provider;

	private TenancyTestSetup(bool withCache)
	{
		AppSettings = new AppSettings
		{
			StorageFolder = System.IO.Path.Combine(_root, "default", "storage"),
			ThumbnailTempFolder = System.IO.Path.Combine(_root, "default", "thumbs"),
			TempFolder = System.IO.Path.Combine(_root, "default", "temp"),
			TenantsRoot = System.IO.Path.Combine(_root, "tenants"),
			AccountRegisterFirstRoleAdmin = true
		};
		Cache = withCache ? new MemoryCache(new MemoryCacheOptions()) : null;

		var services = new ServiceCollection();
		services.AddScoped<ITenantContext, TenantContext>();
		services.AddDbContext<ApplicationDbContext>(o => o.UseSqlite(_connection));
		_provider = services.BuildServiceProvider();
		Registry = new DatabaseTenantRegistry(_provider.GetRequiredService<IServiceScopeFactory>());
		SettingsProvider = new TenantSettingsProvider(AppSettings, Registry);
	}

	public ITenantSettingsProvider SettingsProvider { get; }

	public static async Task<TenancyTestSetup> Create(bool withCache = false)
	{
		var setup = new TenancyTestSetup(withCache);
		await setup._connection.OpenAsync();
		await using var context = setup.ContextFor("default");
		await context.Database.EnsureCreatedAsync();
		return setup;
	}

	public ApplicationDbContext ContextFor(string tenantId)
	{
		var tenant = new TenantContext();
		tenant.SetTenant(tenantId);
		return new ApplicationDbContext(
			new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(_connection).Options,
			tenant);
	}

	public UserManager UserManagerFor(ApplicationDbContext context, string tenantId)
	{
		var tenant = new TenantContext();
		tenant.SetTenant(tenantId);
		return new UserManager(context, AppSettings, new FakeIWebLogger(), Cache, tenant);
	}

	public async Task AddActiveTenant(string tenantId, TenantState state = TenantState.Active)
	{
		await using var context = ContextFor("default");
		context.Tenants.Add(new TenantItem
		{
			Id = tenantId, DisplayName = tenantId.ToUpperInvariant(), State = state
		});
		await context.SaveChangesAsync();
		Registry.Invalidate();
	}

	public async ValueTask DisposeAsync()
	{
		await _provider.DisposeAsync();
		await _connection.DisposeAsync();
		if ( System.IO.Directory.Exists(_root) )
		{
			System.IO.Directory.Delete(_root, true);
		}
	}
}
