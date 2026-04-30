using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;

namespace starsky.foundation.storage.Services;

/// <summary>
///     One-time startup migration that moves existing files from the legacy flat
///     <c>StorageFolder/</c> layout into the new per-tenant convention
///     <c>StorageFolder/main/</c>.
///
///     The migration is considered necessary when <c>StorageFolder/main/</c> does NOT
///     yet exist but <c>StorageFolder/</c> contains at least one file or directory that
///     is not itself a valid tenant slug directory (i.e. pre-migration data exists).
///
///     After the migration all existing rows in the database will already have
///     <c>TenantId</c> pointing at the "main" tenant because the EF Core migration
///     <c>20260430121000_TenantIsolationPhase1Backfill</c> backfills them.
///
///     This service is safe to run multiple times: once <c>StorageFolder/main/</c>
///     exists it exits immediately without touching anything.
/// </summary>
public sealed class StorageTenantMigrationService : IHostedService
{
	private const string DefaultTenantSlug = "main";

	private readonly AppSettings _appSettings;
	private readonly IWebLogger _logger;

	public StorageTenantMigrationService(AppSettings appSettings, IWebLogger logger)
	{
		_appSettings = appSettings;
		_logger = logger;
	}

	public Task StartAsync(CancellationToken cancellationToken)
	{
		try
		{
			Run(cancellationToken);
		}
		catch ( Exception ex )
		{
			_logger.LogError(
				"[StorageTenantMigrationService] Unhandled error during file migration", ex);
		}

		return Task.CompletedTask;
	}

	public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

	internal void Run(CancellationToken cancellationToken = default)
	{
		var storageFolder = _appSettings.StorageFolder;
		if ( string.IsNullOrEmpty(storageFolder) )
		{
			_logger.LogInformation(
				"[StorageTenantMigrationService] StorageFolder is empty – skipping migration.");
			return;
		}

		var mainTenantRoot = Path.Combine(storageFolder.TrimEnd(Path.DirectorySeparatorChar,
			Path.AltDirectorySeparatorChar), DefaultTenantSlug);

		if ( Directory.Exists(mainTenantRoot) )
		{
			_logger.LogInformation(
				$"[StorageTenantMigrationService] '{mainTenantRoot}' already exists – " +
				"skipping migration.");
			return;
		}

		var storageFolderNormalised =
			storageFolder.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

		if ( !Directory.Exists(storageFolderNormalised) )
		{
			_logger.LogInformation(
				$"[StorageTenantMigrationService] StorageFolder '{storageFolderNormalised}' " +
				"does not exist – nothing to migrate.");
			return;
		}

		// Check whether there is anything to migrate (skip if storage folder is empty).
		var children = Directory.GetFileSystemEntries(storageFolderNormalised);
		if ( children.Length == 0 )
		{
			_logger.LogInformation(
				"[StorageTenantMigrationService] StorageFolder is empty – nothing to migrate.");
			// Still create the 'main' directory so the convention is in place.
			Directory.CreateDirectory(mainTenantRoot);
			return;
		}

		_logger.LogInformation(
			$"[StorageTenantMigrationService] Starting migration: moving " +
			$"{children.Length} item(s) from '{storageFolderNormalised}' into '{mainTenantRoot}'.");

		Directory.CreateDirectory(mainTenantRoot);

		foreach ( var child in children )
		{
			if ( cancellationToken.IsCancellationRequested )
			{
				break;
			}

			var name = Path.GetFileName(child);

			// Skip the 'main' directory itself (just created above).
			if ( string.Equals(name, DefaultTenantSlug, StringComparison.OrdinalIgnoreCase) )
			{
				continue;
			}

			var destination = Path.Combine(mainTenantRoot, name);
			try
			{
				if ( File.Exists(child) )
				{
					File.Move(child, destination);
					_logger.LogInformation(
						$"[StorageTenantMigrationService] Moved file: {name}");
				}
				else if ( Directory.Exists(child) )
				{
					Directory.Move(child, destination);
					_logger.LogInformation(
						$"[StorageTenantMigrationService] Moved directory: {name}");
				}
			}
			catch ( Exception ex )
			{
				_logger.LogError(
					$"[StorageTenantMigrationService] Failed to move '{name}': {ex.Message}",
					ex);
			}
		}

		_logger.LogInformation(
			"[StorageTenantMigrationService] Migration completed.");
	}
}


