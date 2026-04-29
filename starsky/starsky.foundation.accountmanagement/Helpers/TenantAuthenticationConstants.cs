namespace starsky.foundation.accountmanagement.Helpers;

public static class TenantAuthenticationConstants
{
	public const string SessionCookieName = ".Starsky.Session";
	public const string TenantSlugItemKey = "tenant_slug";
	public const string TenantIdClaimType = "tenant_id";
	public const string TenantSlugClaimType = "tenant";
	public const string GlobalAdminClaimType = "global_admin";
	public const string TenantRoleClaimType = "tenant_role";
}
