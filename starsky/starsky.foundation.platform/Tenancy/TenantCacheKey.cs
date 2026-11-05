namespace starsky.foundation.platform.Tenancy;

public static class TenantCacheKey
{
	/// <summary>
	///     Cache keys of file paths and hashes are the same in every tenant, and the cache is
	///     shared. The default tenant keeps the original keys
	/// </summary>
	public static string Prefix(string key, string tenantId)
	{
		return tenantId == TenantIdHelper.DefaultTenantId ? key : $"tenant-{tenantId}:{key}";
	}

	/// <summary>
	///     For the tenant of the current request or job
	/// </summary>
	public static string Prefix(string key)
	{
		return Prefix(key, TenantAmbient.Current);
	}
}
