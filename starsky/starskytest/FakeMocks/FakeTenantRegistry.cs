using System.Collections.Generic;
using System.Linq;
using starsky.foundation.platform.Tenancy;

namespace starskytest.FakeMocks;

public sealed class FakeTenantRegistry(params string[] activeTenants) : ITenantRegistry
{
	private readonly Dictionary<string, TenantRoots> _roots = new();

	public IReadOnlyCollection<string> ActiveTenantIds { get; } = activeTenants.ToList();

	public bool IsActive(string tenantId)
	{
		return ActiveTenantIds.Contains(tenantId);
	}

	public TenantRoots? GetRoots(string tenantId)
	{
		return _roots.GetValueOrDefault(tenantId);
	}

	public void Invalidate()
	{
		// nothing to reload
	}

	public FakeTenantRegistry WithRoots(string tenantId, TenantRoots roots)
	{
		_roots[tenantId] = roots;
		return this;
	}
}
