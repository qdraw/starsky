using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace starsky.foundation.database.Models.Account;

public class WebSession
{
	[Key]
	[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public int Id { get; set; }

	[Required]
	[MaxLength(128)]
	public string SessionId { get; set; } = string.Empty;

	public int UserId { get; set; }
	public DateTime ExpiresAt { get; set; }
	public DateTime? RevokedAt { get; set; }
	public DateTime Created { get; set; } = DateTime.UtcNow;
	public DateTime LastSeen { get; set; } = DateTime.UtcNow;

	public virtual User? User { get; set; }
	public virtual ICollection<WebSessionTenant>? WebSessionTenants { get; set; }
}
