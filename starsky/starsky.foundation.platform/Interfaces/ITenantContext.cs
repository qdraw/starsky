namespace starsky.foundation.platform.Interfaces;

/// <summary>
///     Scoped ambient context that carries the current tenant's identity through both
///     HTTP-request scopes (filled from JWT/cookie claims via IHttpContextAccessor) and
///     explicit background-job scopes (filled by setting the properties directly).
/// </summary>
public interface ITenantContext
{
	/// <summary>Primary key of the current tenant. Null = no active tenant (global scope).</summary>
	int? TenantId { get; set; }

	/// <summary>Slug of the current tenant (e.g. "main"). Null = no active tenant.</summary>
	string? TenantSlug { get; set; }
}

