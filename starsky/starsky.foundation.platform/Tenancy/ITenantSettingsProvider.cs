namespace starsky.foundation.platform.Tenancy;

/// <summary>
///     Singletons and hosted services use this: they have no request scope to get the tenant from
/// </summary>
public interface ITenantSettingsProvider
{
	/// <exception cref="System.ArgumentException">invalid tenant id</exception>
	/// <exception cref="System.InvalidOperationException">tenant is not active</exception>
	ITenantSettings GetSettings(string tenantId);

	/// <summary>
	///     The settings a tenant would have, also for a tenant that is not active yet (to create
	///     its folders and to validate them). Not cached
	/// </summary>
	ITenantSettings Build(string tenantId, TenantRoots? roots);

	/// <summary>
	///     Throws when tenant folders are relative, overlap or are nested in each other
	/// </summary>
	void ValidateRoots();
}
