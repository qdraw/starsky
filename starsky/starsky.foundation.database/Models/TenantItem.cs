using System;
using System.ComponentModel.DataAnnotations;

namespace starsky.foundation.database.Models;

public enum TenantState
{
	/// <summary>
	///     Being created: folders are not ready, nobody can use it
	/// </summary>
	Provisioning = 0,
	Active = 1,

	/// <summary>
	///     No user access, nothing is removed
	/// </summary>
	Disabled = 2,

	/// <summary>
	///     Cleanup in progress
	/// </summary>
	Deleting = 3,
	Deleted = 4
}

/// <summary>
///     The tenant registry. The default tenant is not stored here: it always exists and uses the
///     existing folders. Not an ITenantEntity: it describes tenants, it does not belong to one
/// </summary>
public sealed class TenantItem
{
	/// <summary>
	///     Slug, see TenantIdHelper
	/// </summary>
	[Key]
	[MaxLength(63)]
	public string Id { get; set; } = string.Empty;

	[MaxLength(150)] public string DisplayName { get; set; } = string.Empty;

	public TenantState State { get; set; } = TenantState.Provisioning;

	/// <summary>
	///     Explicit absolute folders, null uses the TenantsRoot layout
	/// </summary>
	[MaxLength(1024)]
	public string? StorageFolder { get; set; }

	[MaxLength(1024)] public string? ThumbnailTempFolder { get; set; }

	[MaxLength(1024)] public string? TempFolder { get; set; }

	public DateTime Created { get; set; } = DateTime.UtcNow;
}
