using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Identity;

namespace starsky.foundation.database.Models
{
	// Add profile data for application users by adding properties to the ApplicationUser class
	[SuppressMessage("Usage", "S2094: Remove this empty class, write its code or make it an interface")]
	public sealed class ApplicationUser : IdentityUser
	{
	}
}
