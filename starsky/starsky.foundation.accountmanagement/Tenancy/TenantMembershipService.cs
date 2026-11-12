using System;
using System.Collections.Generic;
using System.Data;
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
using starsky.foundation.realtime.Interfaces;

namespace starsky.foundation.accountmanagement.Tenancy;

[Service(typeof(ITenantMembershipService), InjectionLifetime = InjectionLifetime.Scoped)]
public sealed class TenantMembershipService(
	ApplicationDbContext dbContext,
	IMemoryCache? cache = null,
	IWebSocketConnectionsService? sockets = null) : ITenantMembershipService
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

	public async Task<string?> GetLastUsedAsync(int userId)
	{
		return await dbContext.Users.AsNoTracking().Where(u => u.Id == userId)
			.Select(u => u.LastTenantId).FirstOrDefaultAsync();
	}

	public async Task<MembershipResult> SetLastUsedAsync(int userId, string tenantId)
	{
		if ( !await dbContext.UserRoles.AnyAsync(p =>
			    p.UserId == userId && p.TenantId == tenantId) )
		{
			return MembershipResult.NotAMember;
		}

		var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
		if ( user == null )
		{
			return MembershipResult.UserNotFound;
		}

		// no write when nothing changes: every page load reports its tenant
		if ( user.LastTenantId != tenantId )
		{
			user.LastTenantId = tenantId;
			await dbContext.SaveChangesAsync();
		}

		return MembershipResult.Ok;
	}

	/// <summary>
	///     Where a login lands without a target: the last used tenant when the user is still a
	///     member, the only tenant when there is one, otherwise null (the user must choose, or
	///     has no access at all)
	/// </summary>
	public static string? ResolveLanding(IReadOnlyCollection<TenantListItem> tenants,
		string? lastUsed)
	{
		if ( lastUsed != null && tenants.Any(t => t.TenantId == lastUsed) )
		{
			return lastUsed;
		}

		return tenants.Count == 1 ? tenants.First().TenantId : null;
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

	public Task<MembershipResult> AddMemberAsync(string tenantId, string identifier,
		string roleCode)
	{
		return InSerializableTransaction(() => AddMemberCore(tenantId, identifier, roleCode));
	}

	public Task<MembershipResult> SetRoleAsync(string tenantId, int userId, string roleCode)
	{
		return InSerializableTransaction(() => SetRoleCore(tenantId, userId, roleCode));
	}

	public async Task<MembershipResult> RemoveMemberAsync(string tenantId, int userId)
	{
		var result = await InSerializableTransaction(() => RemoveMemberCore(tenantId, userId));

		// a socket of this user in this tenant is authorised at connect only: close it
		if ( result == MembershipResult.Ok && sockets != null )
		{
			await sockets.CloseConnectionsAsync(tenantId, userId, "Membership removed");
		}

		return result;
	}

	/// <summary>
	///     The checks (last administrator, already a member) and the change belong together:
	///     two administrators that demote each other at the same time would leave none
	/// </summary>
	private async Task<MembershipResult> InSerializableTransaction(
		Func<Task<MembershipResult>> work)
	{
		// the caller may already have a transaction
		if ( dbContext.Database.CurrentTransaction != null )
		{
			return await work();
		}

		// MySQL has a retrying execution strategy: a transaction must be run through it (as one unit
		// that can be repeated), otherwise it refuses to start. The work reads its state again
		var attempt = 0;
		return await dbContext.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
		{
			if ( attempt++ > 0 )
			{
				// a repeat: what the failed attempt tracked is not valid anymore
				dbContext.ChangeTracker.Clear();
			}

			await using var transaction =
				await dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable);
			var result = await work();
			if ( result == MembershipResult.Ok )
			{
				await transaction.CommitAsync();
			}

			// everything else is rolled back (nothing was written) when the transaction is disposed
			return result;
		});
	}

	private async Task<MembershipResult> AddMemberCore(string tenantId, string identifier,
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

	private async Task<MembershipResult> SetRoleCore(string tenantId, int userId, string roleCode)
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

		// the role is part of the key: change it in one statement (one membership per tenant)
		await dbContext.UserRoles
			.Where(p => p.UserId == userId && p.TenantId == tenantId)
			.ExecuteUpdateAsync(setters => setters.SetProperty(p => p.RoleId, role.Id));
		Invalidate(userId, tenantId);
		return MembershipResult.Ok;
	}

	private async Task<MembershipResult> RemoveMemberCore(string tenantId, int userId)
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
