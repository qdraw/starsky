namespace starsky.foundation.platform.Tenancy;

/// <summary>
///     Explicit absolute folders of a tenant. Null values fall back to
///     TenantsRoot/{tenantId}/{storage,thumbnails,temp}
/// </summary>
public sealed record TenantRoots(
	string? StorageFolder = null,
	string? ThumbnailTempFolder = null,
	string? TempFolder = null);
