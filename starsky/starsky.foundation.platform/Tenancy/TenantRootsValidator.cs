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

	/// <summary>
	///     The folders of ONE tenant may not overlap each other either (the temp folder inside
	///     the storage folder would be indexed, and cleaned up)
	/// </summary>
	public static void ValidateOwnFolders(ITenantSettings tenant)
	{
		var roots = new[]
		{
			( Name: "storage", Path: Canonical(tenant.TenantId, tenant.StorageFolder) ),
			( Name: "thumbnails", Path: Canonical(tenant.TenantId, tenant.ThumbnailTempFolder) ),
			( Name: "temp", Path: Canonical(tenant.TenantId, tenant.TempFolder) )
		};
		for ( var i = 0; i < roots.Length; i++ )
		{
			for ( var j = i + 1; j < roots.Length; j++ )
			{
				if ( IsInsideOrEqual(roots[i].Path, roots[j].Path) ||
				     IsInsideOrEqual(roots[j].Path, roots[i].Path) )
				{
					throw new InvalidOperationException(
						$"Tenant '{tenant.TenantId}' folders overlap: {roots[i].Name} " +
						$"'{roots[i].Path}' and {roots[j].Name} '{roots[j].Path}'");
				}
			}
		}
	}

	/// <summary>
	///     An explicit folder of a tenant (given by an administrator) must be absolute and not a
	///     volume root or a top level folder like /etc
	/// </summary>
	public static bool IsAcceptableExplicitRoot(string? folder)
	{
		if ( string.IsNullOrWhiteSpace(folder) || !Path.IsPathRooted(folder) )
		{
			return false;
		}

		var full = Path.GetFullPath(folder)
			.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
		var rootLength = Path.GetPathRoot(full)?.TrimEnd(Path.DirectorySeparatorChar,
			Path.AltDirectorySeparatorChar).Length ?? 0;
		var below = full[rootLength..]
			.Split([Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar],
				StringSplitOptions.RemoveEmptyEntries);
		return below.Length >= 2;
	}
}
