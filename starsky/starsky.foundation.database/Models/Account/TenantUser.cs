using System;

namespace starsky.foundation.database.Models.Account;

public class TenantUser
{
	public int TenantId { get; set; }
	public int UserId { get; set; }
	public TenantRole Role { get; set; } = TenantRole.User;
	public DateTime Created { get; set; } = DateTime.UtcNow;

	public virtual Tenant? Tenant { get; set; }
	public virtual User? User { get; set; }
}
