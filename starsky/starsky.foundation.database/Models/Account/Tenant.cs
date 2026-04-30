using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace starsky.foundation.database.Models.Account;

public class Tenant
{
	[Key]
	[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public int Id { get; set; }

	[Required]
	[MaxLength(50)]
	public string Slug { get; set; } = string.Empty;

	[Required]
	[MaxLength(100)]
	public string Name { get; set; } = string.Empty;

	public bool IsEnabled { get; set; } = true;

	public DateTime Created { get; set; } = DateTime.UtcNow;

	public virtual ICollection<TenantUser>? TenantUsers { get; set; }
	public virtual ICollection<WebSessionTenant>? WebSessionTenants { get; set; }
}
