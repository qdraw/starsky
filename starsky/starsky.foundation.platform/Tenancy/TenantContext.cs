using starsky.foundation.injection;

namespace starsky.foundation.platform.Tenancy;

[Service(typeof(ITenantContext), InjectionLifetime = InjectionLifetime.Scoped)]
public sealed class TenantContext : ITenantContext
{
	private string? _tenantId;

	/// <summary>
	///     The tenant that was set (request, job). A scope that was opened inside the work of a
	///     tenant without setting one (a plain CreateScope in a job) has the tenant of that flow
	///     (TenantAmbient), not the default tenant
	/// </summary>
	public string TenantId => _tenantId ?? TenantAmbient.Current;

	/// <summary>
	///     Set once per scope by the middleware or by a tenant job
	/// </summary>
	public void SetTenant(string tenantId, bool updateAmbient = true)
	{
		if ( !TenantIdHelper.IsValid(tenantId) )
		{
			throw new System.ArgumentException("Invalid tenant id", nameof(tenantId));
		}

		_tenantId = tenantId;
		if ( updateAmbient )
		{
			TenantAmbient.Set(tenantId);
		}
	}
}
