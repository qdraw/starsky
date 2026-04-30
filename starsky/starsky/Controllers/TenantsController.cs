using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using starsky.foundation.accountmanagement.Helpers;
using starsky.foundation.accountmanagement.Interfaces;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models.Account;

namespace starsky.Controllers;

public sealed class TenantsController(
	ApplicationDbContext dbContext,
	ITenantSessionStore sessionStore,
	ITenantSlugValidator tenantSlugValidator)
	: Controller
{
	[HttpGet("/api/tenants/mine")]
	[AllowAnonymous]
	[Produces("application/json")]
	public async Task<IActionResult> Mine()
	{
		if ( !Request.Cookies.TryGetValue(TenantAuthenticationConstants.SessionCookieName,
			     out var sessionId) ||
		     string.IsNullOrWhiteSpace(sessionId) )
		{
			return Unauthorized("Missing session");
		}

		var session = await sessionStore.GetValidSessionAsync(sessionId);
		if ( session == null )
		{
			return Unauthorized("Invalid session");
		}

		var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == session.UserId);
		if ( user == null )
		{
			return Unauthorized("Invalid session user");
		}

		var memberships = await dbContext.TenantUsers
			.Where(m => m.UserId == user.Id)
			.Join(dbContext.Tenants,
				m => m.TenantId,
				t => t.Id,
				(m, t) => new { t.Slug, t.Name, t.IsEnabled, TenantRole = m.Role.ToString() })
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

	[HttpPost("/api/tenants/create")]
	[AllowAnonymous]
	[Produces("application/json")]
	public async Task<IActionResult> Create([FromBody] CreateTenantRequest request)
	{
		if ( !ModelState.IsValid )
		{
			return BadRequest("Invalid request");
		}

		if ( string.IsNullOrWhiteSpace(request.Slug) || string.IsNullOrWhiteSpace(request.Name) )
		{
			return BadRequest("Slug and Name are required");
		}

		if ( !Request.Cookies.TryGetValue(TenantAuthenticationConstants.SessionCookieName,
			     out var sessionId) ||
		     string.IsNullOrWhiteSpace(sessionId) )
		{
			return Unauthorized("Missing session");
		}

		var session = await sessionStore.GetValidSessionAsync(sessionId);
		if ( session == null )
		{
			return Unauthorized("Invalid session");
		}

		var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == session.UserId);
		if ( user == null )
		{
			return Unauthorized("Invalid session user");
		}

		// Only global admins can create tenants
		if ( !user.IsGlobalAdmin )
		{
			return Forbid("Only global administrators can create tenants");
		}

		// Validate slug
		if ( !tenantSlugValidator.IsValid(request.Slug) )
		{
			return BadRequest(
				"Invalid tenant slug. Must be lowercase alphanumeric with hyphens, 3-50 characters");
		}

		// Check if slug already exists
		if ( await dbContext.Tenants.AnyAsync(t => t.Slug == request.Slug) )
		{
			return BadRequest("Tenant slug already exists");
		}

		// Create new tenant
		var newTenant = new Tenant { Slug = request.Slug, Name = request.Name, IsEnabled = true };

		dbContext.Tenants.Add(newTenant);
		await dbContext.SaveChangesAsync();

		// Check if this is the first tenant being created
		var tenantCount = await dbContext.Tenants.CountAsync();
		var isFirstTenant = tenantCount == 1;

		// Add creator as admin of the new tenant
		var tenantUser = new TenantUser
		{
			TenantId = newTenant.Id, UserId = user.Id, Role = TenantRole.Admin
		};

		dbContext.TenantUsers.Add(tenantUser);

		// If this is the first tenant, add all existing global admins as admins of this tenant
		if ( isFirstTenant )
		{
			var globalAdmins = await dbContext.Users
				.Where(u => u.IsGlobalAdmin && u.Id != user.Id)
				.ToListAsync();

			foreach ( var globalAdmin in globalAdmins )
			{
				dbContext.TenantUsers.Add(new TenantUser
				{
					TenantId = newTenant.Id, UserId = globalAdmin.Id, Role = TenantRole.Admin
				});
			}
		}

		await dbContext.SaveChangesAsync();

		return Json(new
		{
			success = true,
			message = "Tenant created successfully",
			tenant = new
			{
				slug = newTenant.Slug,
				name = newTenant.Name,
				role = "Admin",
				isEnabled = newTenant.IsEnabled
			}
		});
	}
}

[AllowAnonymous]
public class CreateTenantRequest
{
	[Required]
	[StringLength(50, MinimumLength = 3)]
	[RegularExpression(@"^[a-z0-9][a-z0-9-]{1,48}[a-z0-9]$", ErrorMessage = "Invalid slug format")]
	public required string Slug { get; set; }

	[Required] [StringLength(100)] public required string Name { get; set; }
}
