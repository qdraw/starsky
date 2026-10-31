using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models;
using starsky.foundation.database.Models.Account;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Tenancy;

namespace starsky.foundation.accountmanagement.Tenancy;

[Service(typeof(ITenantAdminService), InjectionLifetime = InjectionLifetime.Scoped)]
public sealed class TenantAdminService(
	ApplicationDbContext dbContext,
	ITenantRegistry registry,
	ITenantSettingsProvider settingsProvider,
	IWebLogger logger) : ITenantAdminService
{
	public async Task<List<TenantOverviewItem>> ListAsync()
	{
		return ( await dbContext.Tenants.AsNoTracking().OrderBy(t => t.Id).ToListAsync() )
			.Select(t => new TenantOverviewItem(t.Id, t.DisplayName, t.State)).ToList();
	}

	public async Task<TenantAdminResult> CreateAsync(string tenantId, string displayName,
		int creatorUserId, TenantRoots? roots = null)
	{
		if ( tenantId == TenantIdHelper.DefaultTenantId )
		{
			return TenantAdminResult.DefaultTenant;
		}

		if ( !TenantIdHelper.IsValid(tenantId) )
		{
			return TenantAdminResult.InvalidId;
		}

		if ( await dbContext.Tenants.AnyAsync(t => t.Id == tenantId) )
		{
			return TenantAdminResult.AlreadyExists;
		}

		var settings = settingsProvider.Build(tenantId, roots);
		if ( !AreFoldersValid(settings) )
		{
			return TenantAdminResult.InvalidFolders;
		}

		var tenant = new TenantItem
		{
			Id = tenantId,
			DisplayName = string.IsNullOrWhiteSpace(displayName) ? tenantId : displayName.Trim(),
			State = TenantState.Provisioning,
			StorageFolder = roots?.StorageFolder,
			ThumbnailTempFolder = roots?.ThumbnailTempFolder,
			TempFolder = roots?.TempFolder
		};
		dbContext.Tenants.Add(tenant);
		await dbContext.SaveChangesAsync();

		try
		{
			Directory.CreateDirectory(settings.StorageFolder);
			Directory.CreateDirectory(settings.ThumbnailTempFolder);
			Directory.CreateDirectory(settings.TempFolder);
		}
		catch ( Exception exception ) when ( exception is IOException or UnauthorizedAccessException )
		{
			// not Active: nobody can use a tenant without folders
			logger.LogError(exception, $"[TenantAdmin] provisioning failed for {tenantId}");
			return TenantAdminResult.ProvisioningFailed;
		}

		tenant.State = TenantState.Active;
		await GiveAdministratorRole(tenantId, creatorUserId);
		await dbContext.SaveChangesAsync();
		registry.Invalidate();
		return TenantAdminResult.Ok;
	}

	public async Task<TenantAdminResult> SetEnabledAsync(string tenantId, bool enabled)
	{
		var (tenant, early) = await Find(tenantId);
		if ( tenant == null )
		{
			return early;
		}

		if ( tenant.State is not (TenantState.Active or TenantState.Disabled) )
		{
			return TenantAdminResult.InvalidState;
		}

		tenant.State = enabled ? TenantState.Active : TenantState.Disabled;
		await dbContext.SaveChangesAsync();
		registry.Invalidate();
		return TenantAdminResult.Ok;
	}

	public async Task<TenantAdminResult> DeleteAsync(string tenantId)
	{
		var (tenant, early) = await Find(tenantId);
		if ( tenant == null )
		{
			return early;
		}

		if ( tenant.State is TenantState.Deleted )
		{
			return TenantAdminResult.InvalidState;
		}

		tenant.State = TenantState.Deleting;
		await dbContext.SaveChangesAsync();
		registry.Invalidate();
		return TenantAdminResult.Ok;
	}

	private async Task<(TenantItem? Tenant, TenantAdminResult Early)> Find(string tenantId)
	{
		if ( tenantId == TenantIdHelper.DefaultTenantId )
		{
			return ( null, TenantAdminResult.DefaultTenant );
		}

		var tenant = await dbContext.Tenants.FirstOrDefaultAsync(t => t.Id == tenantId);
		return tenant == null ? ( null, TenantAdminResult.NotFound ) : ( tenant, TenantAdminResult.Ok );
	}

	/// <summary>
	///     The new folders must be absolute and not overlap with the active tenants
	/// </summary>
	private bool AreFoldersValid(ITenantSettings newTenant)
	{
		try
		{
			var others = registry.ActiveTenantIds.Select(settingsProvider.GetSettings);
			TenantRootsValidator.Validate(others.Append(newTenant));
			return true;
		}
		catch ( InvalidOperationException )
		{
			return false;
		}
	}

	private async Task GiveAdministratorRole(string tenantId, int userId)
	{
		var administrator = AccountRoles.AppAccountRoles.Administrator.ToString();
		var role = await dbContext.Roles.FirstOrDefaultAsync(r => r.Code == administrator);
		if ( role == null || !await dbContext.Users.AnyAsync(u => u.Id == userId) )
		{
			return;
		}

		dbContext.UserRoles.Add(new UserRole
		{
			UserId = userId, RoleId = role.Id, TenantId = tenantId
		});
	}
}
