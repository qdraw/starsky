using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace starsky.foundation.database.Models.Account;

public class User
{
	[Key]
	[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public int Id { get; set; }

	public string? Name { get; set; }
	public DateTime Created { get; set; }

	public DateTime LockoutEnd { get; set; }

	public bool LockoutEnabled { get; set; }

	public int AccessFailedCount { get; set; }

	/// <summary>
	///     May create and disable tenants. Not a tenant role: it gives no access to the
	///     photos of a tenant
	/// </summary>
	public bool IsPlatformAdmin { get; set; }

	/// <summary>
	///     The tenant the user used last: where the login lands when there is no other target.
	///     A preference of the user (global), not a setting of a tenant
	/// </summary>
	public string? LastTenantId { get; set; }

	// menu settings in the future

	public virtual ICollection<Credential>? Credentials { get; set; }
}
