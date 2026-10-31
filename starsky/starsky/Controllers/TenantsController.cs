using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using starsky.Attributes;
using starsky.foundation.accountmanagement.Interfaces;
using starsky.foundation.accountmanagement.Tenancy;
using starsky.foundation.platform.Tenancy;

namespace starsky.Controllers;

/// <summary>
///     Memberships and the tenant lifecycle. Not behind the membership check of the middleware:
///     the attributes decide (tenant administrator, platform administrator)
/// </summary>
[Authorize]
public sealed class TenantsController(
	ITenantMembershipService membershipService,
	ITenantAdminService adminService,
	IUserManager userManager,
	ITenantContext tenantContext) : Controller
{
	/// <summary>
	///     The tenants of the current user, for the tenant switcher
	/// </summary>
	/// <response code="200">tenants with the role of the user in each</response>
	/// <response code="401">please login first</response>
	[HttpGet("/api/tenants/mine")]
	[Produces("application/json")]
	[ProducesResponseType(typeof(CurrentTenantResponse), 200)]
	public async Task<IActionResult> Mine()
	{
		var userId = userManager.GetCurrentUserId(HttpContext);
		var tenants = await membershipService.GetTenantsForUserAsync(userId);
		var lastUsed = await membershipService.GetLastUsedAsync(userId);
		return Json(new CurrentTenantResponse(tenantContext.TenantId, tenants, lastUsed,
			TenantMembershipService.ResolveLanding(tenants, lastUsed)));
	}

	/// <summary>
	///     The client reports the tenant of the page it loaded; the next login can go there
	/// </summary>
	/// <response code="200">saved</response>
	/// <response code="404">not a member of that tenant</response>
	// No antiforgery token: the page that reports its tenant is still the one of before the login
	// (its token was made for an anonymous user), and the only thing a forged call can do is
	// change the landing tenant between tenants the user is a member of anyway
	[HttpPost("/api/tenants/last-used")]
	public async Task<IActionResult> LastUsed([FromBody] SetLastTenantRequest request)
	{
		var userId = userManager.GetCurrentUserId(HttpContext);
		return ToResult(await membershipService.SetLastUsedAsync(userId, request.TenantId));
	}

	/// <summary>
	///     Members of the current tenant
	/// </summary>
	[HttpGet("/api/tenants/members")]
	[TenantAdmin]
	[Produces("application/json")]
	[ProducesResponseType(typeof(List<TenantMemberItem>), 200)]
	public async Task<IActionResult> Members()
	{
		return Json(await membershipService.GetMembersAsync(tenantContext.TenantId));
	}

	/// <summary>
	///     Give an existing user a role in the current tenant
	/// </summary>
	[HttpPost("/api/tenants/members")]
	[ValidateAntiForgeryToken]
	[TenantAdmin]
	public async Task<IActionResult> AddMember([FromBody] AddMemberRequest request)
	{
		return ToResult(await membershipService.AddMemberAsync(tenantContext.TenantId,
			request.Identifier, request.Role));
	}

	[HttpPut("/api/tenants/members/{userId:int}")]
	[ValidateAntiForgeryToken]
	[TenantAdmin]
	public async Task<IActionResult> SetRole(int userId, [FromBody] SetRoleRequest request)
	{
		return ToResult(await membershipService.SetRoleAsync(tenantContext.TenantId, userId,
			request.Role));
	}

	[HttpDelete("/api/tenants/members/{userId:int}")]
	[ValidateAntiForgeryToken]
	[TenantAdmin]
	public async Task<IActionResult> RemoveMember(int userId)
	{
		return ToResult(await membershipService.RemoveMemberAsync(tenantContext.TenantId, userId));
	}

	/// <summary>
	///     All tenants with their state
	/// </summary>
	[HttpGet("/api/tenants")]
	[PlatformAdmin]
	[Produces("application/json")]
	public async Task<IActionResult> List()
	{
		return Json(await adminService.ListAsync());
	}

	[HttpPost("/api/tenants")]
	[ValidateAntiForgeryToken]
	[PlatformAdmin]
	public async Task<IActionResult> Create([FromBody] CreateTenantRequest request)
	{
		var userId = userManager.GetCurrentUserId(HttpContext);
		return ToResult(await adminService.CreateAsync(request.Id, request.DisplayName, userId));
	}

	[HttpPost("/api/tenants/{tenantId}/disable")]
	[ValidateAntiForgeryToken]
	[PlatformAdmin]
	public async Task<IActionResult> Disable(string tenantId)
	{
		return ToResult(await adminService.SetEnabledAsync(tenantId, false));
	}

	[HttpPost("/api/tenants/{tenantId}/enable")]
	[ValidateAntiForgeryToken]
	[PlatformAdmin]
	public async Task<IActionResult> Enable(string tenantId)
	{
		return ToResult(await adminService.SetEnabledAsync(tenantId, true));
	}

	/// <summary>
	///     Marks the tenant as deleting; files and rows are removed by a separate cleanup
	/// </summary>
	[HttpDelete("/api/tenants/{tenantId}")]
	[ValidateAntiForgeryToken]
	[PlatformAdmin]
	public async Task<IActionResult> Delete(string tenantId)
	{
		return ToResult(await adminService.DeleteAsync(tenantId));
	}

	/// <summary>
	///     IRREVERSIBLE: remove the data of a tenant that is deleting. Confirm with the id of the
	///     tenant, so a wrong id in the url alone does nothing
	/// </summary>
	/// <param name="tenantId">tenant in the deleting state</param>
	/// <param name="confirm">the same id again</param>
	[HttpPost("/api/tenants/{tenantId}/purge")]
	[ValidateAntiForgeryToken]
	[PlatformAdmin]
	public async Task<IActionResult> Purge(string tenantId, string? confirm)
	{
		if ( confirm != tenantId )
		{
			return BadRequest("confirm must be the id of the tenant");
		}

		return ToResult(await adminService.PurgeAsync(tenantId));
	}

	private IActionResult ToResult(MembershipResult result)
	{
		return result switch
		{
			MembershipResult.Ok => Ok(),
			MembershipResult.TenantNotFound or MembershipResult.UserNotFound
				or MembershipResult.NotAMember => NotFound(result.ToString()),
			MembershipResult.RoleNotFound => BadRequest(result.ToString()),
			_ => Conflict(result.ToString())
		};
	}

	private IActionResult ToResult(TenantAdminResult result)
	{
		return result switch
		{
			TenantAdminResult.Ok => Ok(),
			TenantAdminResult.NotFound => NotFound(result.ToString()),
			TenantAdminResult.InvalidId or TenantAdminResult.InvalidFolders
				or TenantAdminResult.DefaultTenant => BadRequest(result.ToString()),
			TenantAdminResult.ProvisioningFailed => StatusCode(500, result.ToString()),
			_ => Conflict(result.ToString())
		};
	}
}
