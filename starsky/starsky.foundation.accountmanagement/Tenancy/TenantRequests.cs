using System.Collections.Generic;

namespace starsky.foundation.accountmanagement.Tenancy;

public sealed record AddMemberRequest(string Identifier, string Role);

public sealed record SetRoleRequest(string Role);

public sealed record CreateTenantRequest(string Id, string DisplayName);

public sealed record CurrentTenantResponse(
	string Current,
	List<TenantListItem> Tenants);
