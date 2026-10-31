using System;
using System.Collections.Generic;
using System.IO;

namespace starsky.foundation.platform.Tenancy;

public static class TenantRootsValidator
{
	/// <summary>
	///     A tenant folder inside another tenant's folder would be indexed, synced and watched
	///     by that other tenant
	/// </summary>
	public static void Validate(IEnumerable<ITenantSettings> tenants)
	{
		var roots = new List<(string TenantId, string Name, string Path)>();
		foreach ( var tenant in tenants )
		{
			roots.Add(( tenant.TenantId, "storage", Canonical(tenant.TenantId, tenant.StorageFolder) ));
			roots.Add(( tenant.TenantId, "thumbnails",
				Canonical(tenant.TenantId, tenant.ThumbnailTempFolder) ));
			roots.Add(( tenant.TenantId, "temp", Canonical(tenant.TenantId, tenant.TempFolder) ));
		}

		for ( var i = 0; i < roots.Count; i++ )
		{
			for ( var j = i + 1; j < roots.Count; j++ )
			{
				var (idA, nameA, pathA) = roots[i];
				var (idB, nameB, pathB) = roots[j];
				if ( idA == idB || !( IsInsideOrEqual(pathA, pathB) || IsInsideOrEqual(pathB, pathA) ) )
				{
					continue;
				}

				throw new InvalidOperationException(
					$"Tenant folders overlap: {idA} {nameA} '{pathA}' and {idB} {nameB} '{pathB}'");
			}
		}
	}

	private static string Canonical(string tenantId, string folder)
	{
		if ( string.IsNullOrWhiteSpace(folder) || !Path.IsPathRooted(folder) )
		{
			throw new InvalidOperationException(
				$"Tenant '{tenantId}' folder '{folder}' must be an absolute path");
		}

		return Path.GetFullPath(folder)
			.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
	}

	private static bool IsInsideOrEqual(string path, string possibleParent)
	{
		return path.Equals(possibleParent, StringComparison.OrdinalIgnoreCase) ||
		       path.StartsWith(possibleParent + Path.DirectorySeparatorChar,
			       StringComparison.OrdinalIgnoreCase);
	}
}
