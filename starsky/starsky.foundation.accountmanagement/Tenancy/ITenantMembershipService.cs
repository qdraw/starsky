using System.Collections.Generic;
using System.Threading.Tasks;

namespace starsky.foundation.accountmanagement.Tenancy;

/// <param name="TenantId">tenant</param>
/// <param name="RoleCode">role of the user in this tenant, e.g. Administrator</param>
/// <param name="Permissions">permission codes of that role</param>
public sealed record TenantMembership(
	string TenantId,
	string RoleCode,
	IReadOnlyList<string> Permissions);

/// <param name="UserId">user</param>
/// <param name="IsPlatformAdmin">may create and disable tenants</param>
/// <param name="Membership">null: the user is not a member of the tenant</param>
public sealed record TenantAccess(int UserId, bool IsPlatformAdmin, TenantMembership? Membership);

public sealed record TenantListItem(string TenantId, string DisplayName, string RoleCode);

public sealed record TenantMemberItem(int UserId, string? Name, string? Identifier, string RoleCode);

public enum MembershipResult
{
	Ok,
	TenantNotFound,
	UserNotFound,
	RoleNotFound,
	AlreadyMember,
	NotAMember,

	/// <summary>
	///     A tenant must always keep at least one administrator
	/// </summary>
	LastAdministrator
}

public interface ITenantMembershipService
{
	/// <summary>
	///     Authorization data of a user for a tenant. Cached for 30 seconds
	/// </summary>
	Task<TenantAccess> GetAccessAsync(int userId, string tenantId);

	Task<List<TenantListItem>> GetTenantsForUserAsync(int userId);

	Task<List<TenantMemberItem>> GetMembersAsync(string tenantId);

	/// <summary>
	///     Give an existing user a role in a tenant (by credential identifier, e.g. e-mail)
	/// </summary>
	Task<MembershipResult> AddMemberAsync(string tenantId, string identifier, string roleCode);

	Task<MembershipResult> SetRoleAsync(string tenantId, int userId, string roleCode);

	Task<MembershipResult> RemoveMemberAsync(string tenantId, int userId);

	void Invalidate(int userId, string tenantId);
}
