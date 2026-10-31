using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models;
using starsky.foundation.database.Models.Account;
using starsky.foundation.injection;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Tenancy;

namespace starsky.foundation.accountmanagement.Tenancy;

[Service(typeof(ITenantMembershipService), InjectionLifetime = InjectionLifetime.Scoped)]
public sealed class TenantMembershipService(
	ApplicationDbContext dbContext,
	IMemoryCache? cache = null) : ITenantMembershipService
{
	/// <summary>
	///     Revoking a membership works within this time, also on another instance
	/// </summary>
	internal static readonly TimeSpan CacheTime = TimeSpan.FromSeconds(30);

	private static readonly string AdministratorCode =
		AccountRoles.AppAccountRoles.Administrator.ToString();

	public async Task<TenantAccess> GetAccessAsync(int userId, string tenantId)
	{
		var key = CacheKey(userId, tenantId);
		if ( cache?.TryGetValue(key, out TenantAccess? cached) == true && cached != null )
		{
			return cached;
		}

		var access = await LoadAccessAsync(userId, tenantId);
		cache?.Set(key, access, CacheTime);
		return access;
	}

	public void Invalidate(int userId, string tenantId)
	{
		cache?.Remove(CacheKey(userId, tenantId));
	}

	public async Task<List<TenantListItem>> GetTenantsForUserAsync(int userId)
	{
		var memberships = await dbContext.UserRoles.AsNoTracking()
			.Where(p => p.UserId == userId)
			.Select(p => new { p.TenantId, RoleCode = p.Role!.Code })
			.ToListAsync();

		var names = await dbContext.Tenants.AsNoTracking()
			.Where(t => t.State == TenantState.Active)
			.ToDictionaryAsync(t => t.Id, t => t.DisplayName);

		var result = new List<TenantListItem>();
		foreach ( var membership in memberships.OrderBy(p => p.TenantId) )
		{
			if ( membership.TenantId == TenantIdHelper.DefaultTenantId )
			{
				result.Add(new TenantListItem(membership.TenantId, "Default",
					membership.RoleCode ?? string.Empty));
			}
			else if ( names.TryGetValue(membership.TenantId, out var displayName) )
			{
				// only active tenants: a disabled tenant is not offered in the switcher
				result.Add(new TenantListItem(membership.TenantId, displayName,
					membership.RoleCode ?? string.Empty));
			}
		}

		return result;
	}

	public async Task<List<TenantMemberItem>> GetMembersAsync(string tenantId)
	{
		var rows = await dbContext.UserRoles.AsNoTracking()
			.Where(p => p.TenantId == tenantId)
			.Select(p => new
			{
				p.UserId,
				p.User!.Name,
				Identifier = dbContext.Credentials
					.Where(c => c.UserId == p.UserId).Select(c => c.Identifier).FirstOrDefault(),
				RoleCode = p.Role!.Code
			}).ToListAsync();

		return rows.Select(r =>
			new TenantMemberItem(r.UserId, r.Name, r.Identifier, r.RoleCode ?? string.Empty))
			.OrderBy(r => r.Identifier).ToList();
	}

	public async Task<MembershipResult> AddMemberAsync(string tenantId, string identifier,
		string roleCode)
	{
		if ( !await TenantExists(tenantId) )
		{
			return MembershipResult.TenantNotFound;
		}

		var role = await FindRole(roleCode);
		if ( role == null )
		{
			return MembershipResult.RoleNotFound;
		}

		var credential = await dbContext.Credentials.AsNoTracking()
			.FirstOrDefaultAsync(c => c.Identifier == identifier);
		if ( credential == null )
		{
			return MembershipResult.UserNotFound;
		}

		if ( await dbContext.UserRoles.AnyAsync(p =>
			    p.UserId == credential.UserId && p.TenantId == tenantId) )
		{
			return MembershipResult.AlreadyMember;
		}

		dbContext.UserRoles.Add(new UserRole
		{
			UserId = credential.UserId, RoleId = role.Id, TenantId = tenantId
		});
		await dbContext.SaveChangesAsync();
		Invalidate(credential.UserId, tenantId);
		return MembershipResult.Ok;
	}

	public async Task<MembershipResult> SetRoleAsync(string tenantId, int userId, string roleCode)
	{
		var role = await FindRole(roleCode);
		if ( role == null )
		{
			return MembershipResult.RoleNotFound;
		}

		var current = await dbContext.UserRoles.Include(p => p.Role)
			.FirstOrDefaultAsync(p => p.UserId == userId && p.TenantId == tenantId);
		if ( current == null )
		{
			return MembershipResult.NotAMember;
		}

		if ( current.RoleId == role.Id )
		{
			return MembershipResult.Ok;
		}

		if ( current.Role?.Code == AdministratorCode && role.Code != AdministratorCode &&
		     await IsLastAdministrator(tenantId) )
		{
			return MembershipResult.LastAdministrator;
		}

		// the role is part of the key: replace the row
		dbContext.UserRoles.Remove(current);
		dbContext.UserRoles.Add(new UserRole
		{
			UserId = userId, RoleId = role.Id, TenantId = tenantId
		});
		await dbContext.SaveChangesAsync();
		Invalidate(userId, tenantId);
		return MembershipResult.Ok;
	}

	public async Task<MembershipResult> RemoveMemberAsync(string tenantId, int userId)
	{
		var current = await dbContext.UserRoles.Include(p => p.Role)
			.FirstOrDefaultAsync(p => p.UserId == userId && p.TenantId == tenantId);
		if ( current == null )
		{
			return MembershipResult.NotAMember;
		}

		if ( current.Role?.Code == AdministratorCode && await IsLastAdministrator(tenantId) )
		{
			return MembershipResult.LastAdministrator;
		}

		dbContext.UserRoles.Remove(current);
		await dbContext.SaveChangesAsync();
		Invalidate(userId, tenantId);
		return MembershipResult.Ok;
	}

	private async Task<TenantAccess> LoadAccessAsync(int userId, string tenantId)
	{
		var user = await dbContext.Users.AsNoTracking()
			.Where(u => u.Id == userId)
			.Select(u => new { u.IsPlatformAdmin }).FirstOrDefaultAsync();
		if ( user == null )
		{
			return new TenantAccess(userId, false, null);
		}

		var userRole = await dbContext.UserRoles.AsNoTracking()
			.Include(p => p.Role)
			.FirstOrDefaultAsync(p => p.UserId == userId && p.TenantId == tenantId);
		if ( userRole?.Role?.Code == null )
		{
			return new TenantAccess(userId, user.IsPlatformAdmin, null);
		}

		var permissions = await dbContext.RolePermissions.AsNoTracking()
			.Where(rp => rp.RoleId == userRole.RoleId)
			.Select(rp => dbContext.Permissions.Where(p => p.Id == rp.PermissionId)
				.Select(p => p.Code).FirstOrDefault())
			.ToListAsync();

		return new TenantAccess(userId, user.IsPlatformAdmin,
			new TenantMembership(tenantId, userRole.Role.Code,
				permissions.Where(p => p != null).Select(p => p!).ToList()));
	}

	private async Task<bool> TenantExists(string tenantId)
	{
		return tenantId == TenantIdHelper.DefaultTenantId ||
		       await dbContext.Tenants.AnyAsync(t => t.Id == tenantId &&
		                                             t.State != TenantState.Deleted);
	}

	private async Task<Role?> FindRole(string roleCode)
	{
		return AccountRoles.GetAllRoles().Contains(roleCode)
			? await dbContext.Roles.AsNoTracking().FirstOrDefaultAsync(r => r.Code == roleCode)
			: null;
	}

	private async Task<bool> IsLastAdministrator(string tenantId)
	{
		return await dbContext.UserRoles.CountAsync(p =>
			p.TenantId == tenantId && p.Role!.Code == AdministratorCode) <= 1;
	}

	private static string CacheKey(int userId, string tenantId)
	{
		return $"tenant-membership:{tenantId}:{userId}";
	}
}
