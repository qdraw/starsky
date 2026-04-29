using System;

namespace starsky.foundation.database.Models.Account;

public class WebSessionTenant
{
	public int WebSessionId { get; set; }
	public int TenantId { get; set; }
	public DateTime Created { get; set; } = DateTime.UtcNow;
	public DateTime LastSeen { get; set; } = DateTime.UtcNow;

	public virtual WebSession? WebSession { get; set; }
	public virtual Tenant? Tenant { get; set; }
}
