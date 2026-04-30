namespace starsky.foundation.platform.Helpers;

/// <summary>
///     Shared tenant claim-type constants that must be accessible to
///     foundation layers below starsky.foundation.accountmanagement to
///     avoid circular project references.
/// </summary>
public static class TenantConstants
{
	/// <summary>Claim type used to carry the tenant slug inside a ClaimsPrincipal.</summary>
	public const string TenantSlugClaimType = "tenant";

	/// <summary>Claim type used to carry the numeric tenant ID inside a ClaimsPrincipal.</summary>
	public const string TenantIdClaimType = "tenant_id";

	/// <summary>Key used to store the tenant slug in HttpContext.Items.</summary>
	public const string TenantSlugItemKey = "tenant_slug";
}

