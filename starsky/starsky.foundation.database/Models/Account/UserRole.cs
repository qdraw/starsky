using starsky.foundation.platform.Tenancy;

namespace starsky.foundation.database.Models.Account;

/// <summary>
///     The membership of a user in a tenant, with the role in that tenant. Users are global, so
///     this is deliberately not an ITenantEntity (no query filter): it is read across tenants
/// </summary>
public class UserRole
{
	public int UserId { get; set; }
	public int RoleId { get; set; }

	public string TenantId { get; set; } = TenantIdHelper.DefaultTenantId;

	public virtual User? User { get; set; }
	public virtual Role? Role { get; set; }
}
