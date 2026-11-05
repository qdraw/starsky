using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Tenancy;

namespace starsky.foundation.database.Tenancy;

/// <summary>
///     Tenants from the Tenants table, as a snapshot that is reloaded every 30 seconds (and after
///     Invalidate). The default tenant is always active and not stored.
///     A disabled tenant therefore stops resolving within 30 seconds at worst
/// </summary>
[Service(typeof(ITenantRegistry), InjectionLifetime = InjectionLifetime.Singleton)]
public sealed class DatabaseTenantRegistry(
	IServiceScopeFactory scopeFactory,
	IWebLogger? logger = null) : ITenantRegistry
{
	internal static readonly TimeSpan MaxAge = TimeSpan.FromSeconds(30);

	private readonly object _lock = new();
	private Snapshot _snapshot = Snapshot.Empty;

	public IReadOnlyCollection<string> ActiveTenantIds =>
		[TenantIdHelper.DefaultTenantId, ..Current().Active.Keys];

	public bool IsActive(string tenantId)
	{
		return tenantId == TenantIdHelper.DefaultTenantId || Current().Active.ContainsKey(tenantId);
	}

	public TenantRoots? GetRoots(string tenantId)
	{
		return Current().Active.GetValueOrDefault(tenantId);
	}

	public void Invalidate()
	{
		lock ( _lock )
		{
			_snapshot = _snapshot with { LoadedAtUtc = DateTime.MinValue };
		}
	}

	private Snapshot Current()
	{
		var snapshot = _snapshot;
		if ( DateTime.UtcNow - snapshot.LoadedAtUtc < MaxAge )
		{
			return snapshot;
		}

		lock ( _lock )
		{
			snapshot = _snapshot;
			if ( DateTime.UtcNow - snapshot.LoadedAtUtc < MaxAge )
			{
				return snapshot;
			}

			_snapshot = Load(snapshot);
			return _snapshot;
		}
	}

	private Snapshot Load(Snapshot previous)
	{
		try
		{
			using var scope = scopeFactory.CreateScope();
			var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
			var active = context.Tenants.AsNoTracking()
				.Where(t => t.State == TenantState.Active).ToList()
				.ToDictionary(t => t.Id,
					t => new TenantRoots(t.StorageFolder, t.ThumbnailTempFolder, t.TempFolder));
			return new Snapshot(active, DateTime.UtcNow);
		}
		catch ( Exception exception ) when ( exception is not OutOfMemoryException )
		{
			// the table does not exist yet (before the migration) or the database is down:
			// keep what we know, only the default tenant when we know nothing, and retry later
			logger?.LogError(exception, "[DatabaseTenantRegistry] load failed");
			return previous with { LoadedAtUtc = DateTime.UtcNow };
		}
	}

	private sealed record Snapshot(
		IReadOnlyDictionary<string, TenantRoots> Active,
		DateTime LoadedAtUtc)
	{
		public static readonly Snapshot Empty =
			new(new Dictionary<string, TenantRoots>(), DateTime.MinValue);
	}
}
