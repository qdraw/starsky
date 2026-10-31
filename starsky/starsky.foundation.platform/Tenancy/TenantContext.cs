using starsky.foundation.injection;

namespace starsky.foundation.platform.Tenancy;

[Service(typeof(ITenantContext), InjectionLifetime = InjectionLifetime.Scoped)]
public sealed class TenantContext : ITenantContext
{
	public string TenantId { get; private set; } = TenantIdHelper.DefaultTenantId;

	/// <summary>
	///     Set once per scope by the middleware or by a tenant job
	/// </summary>
	public void SetTenant(string tenantId)
	{
		if ( !TenantIdHelper.IsValid(tenantId) )
		{
			throw new System.ArgumentException("Invalid tenant id", nameof(tenantId));
		}

		TenantId = tenantId;
	}
}
