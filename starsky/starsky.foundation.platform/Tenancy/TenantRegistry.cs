using System.Collections.Generic;

namespace starsky.foundation.platform.Tenancy;

/// <summary>
///     Only the default tenant is known. Not registered: the application uses the registry
///     that reads the tenant table (DatabaseTenantRegistry); this one is for code without a database
/// </summary>
public sealed class TenantRegistry : ITenantRegistry
{
	public IReadOnlyCollection<string> ActiveTenantIds { get; } =
		[TenantIdHelper.DefaultTenantId];

	public bool IsActive(string tenantId)
	{
		return tenantId == TenantIdHelper.DefaultTenantId;
	}

	public TenantRoots? GetRoots(string tenantId)
	{
		return null;
	}

	public void Invalidate()
	{
		// nothing to reload
	}
}
