namespace starsky.foundation.platform.Tenancy;

/// <summary>
///     The tenant of the current request or job. Always scoped, never a singleton:
///     a shared instance would leak the tenant between concurrent requests
/// </summary>
public interface ITenantContext
{
	string TenantId { get; }
}
