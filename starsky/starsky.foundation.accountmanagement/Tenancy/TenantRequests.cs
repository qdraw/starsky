using System.Collections.Generic;

namespace starsky.foundation.accountmanagement.Tenancy;

public sealed record AddMemberRequest(string Identifier, string Role);

public sealed record SetRoleRequest(string Role);

public sealed record CreateTenantRequest(string Id, string DisplayName);

public sealed record SetLastTenantRequest(string TenantId);

/// <param name="Current">tenant of the url of this request</param>
/// <param name="Tenants">memberships of the user (active tenants)</param>
/// <param name="LastUsed">tenant the user used last, or null</param>
/// <param name="Landing">where a login without target goes, null: choose or no access</param>
/// <param name="IsPlatformAdmin">may create and delete tenants (the screen shows it as such)</param>
public sealed record CurrentTenantResponse(
	string Current,
	List<TenantListItem> Tenants,
	string? LastUsed = null,
	string? Landing = null,
	bool IsPlatformAdmin = false);
