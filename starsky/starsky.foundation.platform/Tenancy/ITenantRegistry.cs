using System.Collections.Generic;

namespace starsky.foundation.platform.Tenancy;

public interface ITenantRegistry
{
	IReadOnlyCollection<string> ActiveTenantIds { get; }

	/// <summary>
	///     Only active tenants resolve for requests
	/// </summary>
	bool IsActive(string tenantId);

	/// <summary>
	///     Explicit folders of a tenant, null when the tenant uses the TenantsRoot layout
	/// </summary>
	TenantRoots? GetRoots(string tenantId);

	/// <summary>
	///     Reload on next use, call after the tenant table changed
	/// </summary>
	void Invalidate();
}
