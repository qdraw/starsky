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
using starsky.foundation.realtime.Interfaces;
using System.Linq.Expressions;

namespace starsky.foundation.accountmanagement.Tenancy;

[Service(typeof(ITenantAdminService), InjectionLifetime = InjectionLifetime.Scoped)]
public sealed class TenantAdminService(
	ApplicationDbContext dbContext,
	ITenantRegistry registry,
	ITenantSettingsProvider settingsProvider,
	IWebLogger logger,
	IWebSocketConnectionsService? sockets = null,
	AppSettings? appSettings = null) : ITenantAdminService
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
		if ( !await AreFoldersValid(settings, roots) )
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

		if ( enabled && !await AreFoldersValid(settingsProvider.Build(tenant.Id,
			    new TenantRoots(tenant.StorageFolder, tenant.ThumbnailTempFolder,
				    tenant.TempFolder)), null) )
		{
			// the folders of another tenant were made while this one was disabled
			return TenantAdminResult.InvalidFolders;
		}

		tenant.State = enabled ? TenantState.Active : TenantState.Disabled;
		await dbContext.SaveChangesAsync();
		registry.Invalidate();
		await CloseSocketsOfInactiveTenant(tenantId, enabled);
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
		await CloseSocketsOfInactiveTenant(tenantId, false);
		return TenantAdminResult.Ok;
	}

	public async Task<TenantAdminResult> PurgeAsync(string tenantId)
	{
		var (tenant, early) = await Find(tenantId);
		if ( tenant == null )
		{
			return early;
		}

		// only after Delete: a tenant that is in use can not be purged by a typo
		if ( tenant.State != TenantState.Deleting )
		{
			return TenantAdminResult.InvalidState;
		}

		await dbContext.RunCrossTenantAsync(async () =>
		{
			await dbContext.FileIndex.Where(OfTenant<FileIndexItem>(tenantId)).ExecuteDeleteAsync();
			await dbContext.ImportIndex.Where(OfTenant<ImportIndexItem>(tenantId))
				.ExecuteDeleteAsync();
			await dbContext.Notifications.Where(OfTenant<NotificationItem>(tenantId))
				.ExecuteDeleteAsync();
			await dbContext.Settings.Where(OfTenant<SettingsItem>(tenantId)).ExecuteDeleteAsync();
			await dbContext.Thumbnails.Where(OfTenant<ThumbnailItem>(tenantId))
				.ExecuteDeleteAsync();
			await dbContext.QueueItems.Where(OfTenant<QueueItem>(tenantId)).ExecuteDeleteAsync();
			await dbContext.UserRoles.Where(p => p.TenantId == tenantId).ExecuteDeleteAsync();
			return 0;
		});

		DeleteFoldersOfTheTenantsRootLayout(tenant);

		tenant.State = TenantState.Deleted;
		await dbContext.SaveChangesAsync();
		registry.Invalidate();
		logger.LogInformation($"[TenantAdmin] purged tenant {tenantId}");
		return TenantAdminResult.Ok;
	}

	private static Expression<Func<T, bool>> OfTenant<T>(string tenantId) where T : class
	{
		return entity => EF.Property<string>(entity, ITenantEntity.TenantPropertyName) == tenantId;
	}

	/// <summary>
	///     Only the folder TenantsRoot/{id} that the application made itself. A tenant with
	///     explicit folders keeps its files: those are the files of whoever pointed to them
	/// </summary>
	private void DeleteFoldersOfTheTenantsRootLayout(TenantItem tenant)
	{
		if ( appSettings == null || tenant.StorageFolder != null ||
		     tenant.ThumbnailTempFolder != null || tenant.TempFolder != null )
		{
			return;
		}

		var tenantFolder = Path.Combine(appSettings.TenantsRoot, tenant.Id);
		// a valid id has no separators or dots: the folder is always directly in TenantsRoot
		if ( TenantIdHelper.IsValid(tenant.Id) && Directory.Exists(tenantFolder) )
		{
			Directory.Delete(tenantFolder, true);
		}
	}

	/// <summary>
	///     Nobody can use a tenant that is not active: also not with a socket that is open
	/// </summary>
	private async Task CloseSocketsOfInactiveTenant(string tenantId, bool isActive)
	{
		if ( !isActive && sockets != null )
		{
			await sockets.CloseConnectionsAsync(tenantId, null, "Tenant is not active");
		}
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
	///     The folders must be absolute (explicit ones: not a volume root or a top level folder), may
	///     not overlap each other, and may not overlap the folders of any other tenant that is not
	///     deleted. A disabled tenant counts: it can be enabled again
	/// </summary>
	private async Task<bool> AreFoldersValid(ITenantSettings candidate, TenantRoots? explicitRoots)
	{
		if ( explicitRoots != null && new[]
		    {
			    explicitRoots.StorageFolder, explicitRoots.ThumbnailTempFolder,
			    explicitRoots.TempFolder
		    }.Any(folder => folder != null &&
		                    !TenantRootsValidator.IsAcceptableExplicitRoot(folder)) )
		{
			return false;
		}

		try
		{
			TenantRootsValidator.ValidateOwnFolders(candidate);

			var others = new List<ITenantSettings>
			{
				settingsProvider.Build(TenantIdHelper.DefaultTenantId, null)
			};
			foreach ( var tenant in await dbContext.Tenants.AsNoTracking()
				         .Where(t => t.Id != candidate.TenantId && t.State != TenantState.Deleted)
				         .ToListAsync() )
			{
				others.Add(settingsProvider.Build(tenant.Id,
					new TenantRoots(tenant.StorageFolder, tenant.ThumbnailTempFolder,
						tenant.TempFolder)));
			}

			TenantRootsValidator.Validate(others.Append(candidate));
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
