using System.Threading;

namespace starsky.foundation.platform.Tenancy;

/// <summary>
///     The tenant of the current async flow (request or job), for code that has no scoped
///     ITenantContext: cache keys and jobs created deep in a call chain. Set by
///     TenantContext.SetTenant, so it always equals the scoped context of the same flow. It is
///     AsyncLocal, not a singleton field: concurrent requests never see each other's value
/// </summary>
public static class TenantAmbient
{
	private static readonly AsyncLocal<string?> CurrentValue = new();

	public static string Current => CurrentValue.Value ?? TenantIdHelper.DefaultTenantId;

	/// <summary>
	///     For the code that starts the work of a tenant: the middleware (via TenantContext) and
	///     the job runners. Not for use inside a tenant's own work
	/// </summary>
	public static void Set(string tenantId)
	{
		CurrentValue.Value = tenantId;
	}

	/// <summary>
	///     Back to the default tenant (tests, end of a job)
	/// </summary>
	public static void Reset()
	{
		CurrentValue.Value = null;
	}
}
