using System.Text.RegularExpressions;

namespace starsky.foundation.platform.Tenancy;

public static partial class TenantIdHelper
{
	/// <summary>
	///     The tenant that desktop, CLI and requests without a prefix use
	/// </summary>
	public const string DefaultTenantId = "default";

	/// <summary>
	///     Tenant ids end up in URLs and folder names: lowercase letters, digits and dashes only
	///     (no dots or slashes, so it can never escape a tenant root)
	/// </summary>
	public static bool IsValid(string? tenantId)
	{
		return !string.IsNullOrEmpty(tenantId) && tenantId.Length <= 63 &&
		       ValidTenantIdRegex().IsMatch(tenantId);
	}

	[GeneratedRegex("^[a-z0-9]+(-[a-z0-9]+)*$")]
	private static partial Regex ValidTenantIdRegex();
}
