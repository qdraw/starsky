using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using starsky.foundation.injection;
using starsky.foundation.platform.Models;

namespace starsky.foundation.platform.Tenancy;

[Service(typeof(ITenantSettingsProvider), InjectionLifetime = InjectionLifetime.Singleton)]
public sealed class TenantSettingsProvider(AppSettings appSettings, ITenantRegistry registry)
	: ITenantSettingsProvider
{
	private readonly ConcurrentDictionary<string, ITenantSettings> _cache = new();

	public ITenantSettings GetSettings(string tenantId)
	{
		if ( !TenantIdHelper.IsValid(tenantId) )
		{
			throw new ArgumentException("Invalid tenant id", nameof(tenantId));
		}

		if ( !registry.IsActive(tenantId) )
		{
			throw new InvalidOperationException($"Tenant '{tenantId}' is not active");
		}

		return _cache.GetOrAdd(tenantId, id => Build(id, registry.GetRoots(id)));
	}

	public void ValidateRoots()
	{
		TenantRootsValidator.Validate(registry.ActiveTenantIds.Select(GetSettings));
	}

	public ITenantSettings Build(string tenantId, TenantRoots? roots)
	{
		if ( !TenantIdHelper.IsValid(tenantId) )
		{
			throw new ArgumentException("Invalid tenant id", nameof(tenantId));
		}

		// the default tenant keeps the existing folders untouched
		if ( tenantId == TenantIdHelper.DefaultTenantId )
		{
			return new TenantSettings(tenantId, appSettings);
		}

		var tenantFolder = Path.Combine(appSettings.TenantsRoot, tenantId);
		return TenantSettings.ForFolders(tenantId, appSettings,
			roots?.StorageFolder ?? Path.Combine(tenantFolder, "storage"),
			roots?.ThumbnailTempFolder ?? Path.Combine(tenantFolder, "thumbnails"),
			roots?.TempFolder ?? Path.Combine(tenantFolder, "temp"));
	}
}
