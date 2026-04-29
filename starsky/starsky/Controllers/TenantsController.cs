using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using starsky.foundation.accountmanagement.Helpers;
using starsky.foundation.accountmanagement.Interfaces;
using starsky.foundation.database.Data;

namespace starsky.Controllers;

public sealed class TenantsController(ApplicationDbContext dbContext, ITenantSessionStore sessionStore)
	: Controller
{
	[HttpGet("/api/tenants/mine")]
	[AllowAnonymous]
	[Produces("application/json")]
	public async Task<IActionResult> Mine()
	{
		if (!Request.Cookies.TryGetValue(TenantAuthenticationConstants.SessionCookieName, out var sessionId) ||
			string.IsNullOrWhiteSpace(sessionId))
		{
			return Unauthorized("Missing session");
		}

		var session = await sessionStore.GetValidSessionAsync(sessionId);
		if (session == null)
		{
			return Unauthorized("Invalid session");
		}

		var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == session.UserId);
		if (user == null)
		{
			return Unauthorized("Invalid session user");
		}

		var memberships = await dbContext.TenantUsers
			.Where(m => m.UserId == user.Id)
			.Join(dbContext.Tenants,
				m => m.TenantId,
				t => t.Id,
				(m, t) => new
				{
					t.Slug,
					t.Name,
					t.IsEnabled,
					TenantRole = m.Role.ToString()
				})
			.OrderBy(t => t.Slug)
			.ToListAsync();

		var anyTenantExists = await dbContext.Tenants.AnyAsync();
		return Json(new
		{
			tenants = memberships,
			isGlobalAdmin = user.IsGlobalAdmin,
			isEmpty = memberships.Count == 0,
			canCreateFirstTenant = user.IsGlobalAdmin && !anyTenantExists
		});
	}
}
