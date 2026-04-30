using System;

namespace starsky.foundation.platform.Helpers;

public static class TenantPathHelper
{
	public static string NormalizeForTenantScopedStorage(string sourcePath, string? tenantSlug)
	{
		if ( string.IsNullOrWhiteSpace(sourcePath) || string.IsNullOrWhiteSpace(tenantSlug) )
		{
			return sourcePath;
		}

		var tenantPrefix = PathHelper.PrefixDbSlash(tenantSlug);
		if ( sourcePath.Equals(tenantPrefix, StringComparison.OrdinalIgnoreCase) )
		{
			return "/";
		}

		if ( sourcePath.StartsWith(tenantPrefix + "/", StringComparison.OrdinalIgnoreCase) )
		{
			return sourcePath.Substring(tenantPrefix.Length);
		}

		return sourcePath;
	}

	public static string ToTenantScopedPath(string filePath, string? tenantSlug)
	{
		if ( string.IsNullOrWhiteSpace(filePath) || string.IsNullOrWhiteSpace(tenantSlug) )
		{
			return filePath;
		}

		var normalized = PathHelper.PrefixDbSlash(filePath);
		var tenantPrefix = PathHelper.PrefixDbSlash(tenantSlug);
		if ( normalized.Equals(tenantPrefix, StringComparison.OrdinalIgnoreCase) ||
		     normalized.StartsWith(tenantPrefix + "/", StringComparison.OrdinalIgnoreCase) )
		{
			return normalized;
		}

		return tenantPrefix + normalized;
	}
}
