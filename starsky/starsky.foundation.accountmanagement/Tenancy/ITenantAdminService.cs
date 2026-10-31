using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Tenancy;

namespace starsky.foundation.accountmanagement.Tenancy;

public enum TenantAdminResult
{
	Ok,
	InvalidId,
	AlreadyExists,
	NotFound,

	/// <summary>
	///     The default tenant always exists with the existing folders
	/// </summary>
	DefaultTenant,

	/// <summary>
	///     Folders are relative, or overlap with the folders of another tenant
	/// </summary>
	InvalidFolders,

	/// <summary>
	///     The folders could not be created; the tenant stays in Provisioning
	/// </summary>
	ProvisioningFailed,
	InvalidState
}

public sealed record TenantOverviewItem(
	string TenantId,
	string DisplayName,
	TenantState State);

/// <summary>
///     Tenant lifecycle. The caller must be a platform administrator
/// </summary>
public interface ITenantAdminService
{
	Task<List<TenantOverviewItem>> ListAsync();

	/// <summary>
	///     Provisioning -> folders -> Active. The creator becomes Administrator of the new tenant
	/// </summary>
	Task<TenantAdminResult> CreateAsync(string tenantId, string displayName, int creatorUserId,
		TenantRoots? roots = null);

	/// <summary>
	///     No user access, nothing is removed. Active and Disabled only
	/// </summary>
	Task<TenantAdminResult> SetEnabledAsync(string tenantId, bool enabled);

	/// <summary>
	///     Marks the tenant Deleting. Does not remove files or rows: that is a separate cleanup
	/// </summary>
	Task<TenantAdminResult> DeleteAsync(string tenantId);
}
