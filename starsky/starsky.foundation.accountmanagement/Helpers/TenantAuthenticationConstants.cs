using starsky.foundation.platform.Helpers;

namespace starsky.foundation.accountmanagement.Helpers;

public static class TenantAuthenticationConstants
{
	public const string SessionCookieName = ".Starsky.Session";

	// Re-exported from TenantConstants so existing call-sites are unchanged.
	public const string TenantSlugItemKey = TenantConstants.TenantSlugItemKey;
	public const string TenantIdClaimType = TenantConstants.TenantIdClaimType;
	public const string TenantSlugClaimType = TenantConstants.TenantSlugClaimType;

	public const string GlobalAdminClaimType = "global_admin";
	public const string TenantRoleClaimType = "tenant_role";
}
