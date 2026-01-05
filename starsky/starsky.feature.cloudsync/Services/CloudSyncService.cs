using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.cloudsync.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.import.Interfaces;
using starsky.foundation.import.Models;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;

namespace starsky.foundation.cloudsync.Services;

[Service(typeof(ICloudSyncService), InjectionLifetime = InjectionLifetime.Singleton)]
public class CloudSyncService(
	IServiceScopeFactory serviceScopeFactory,
	IWebLogger logger,
	CloudSyncSettings settings)
	: ICloudSyncService
{
	private readonly ConcurrentDictionary<string, DateTime> _processedFiles = new();
	private readonly SemaphoreSlim _syncLock = new(1, 1);

	public bool IsSyncInProgress { get; private set; }

	public CloudSyncResult? LastSyncResult { get; private set; }

	public async Task<CloudSyncResult> SyncAsync(CloudSyncTriggerType triggerType)
	{
		if ( !settings.Enabled )
		{
			logger.LogInformation("Cloud sync is disabled");
			return new CloudSyncResult
			{
				StartTime = DateTime.UtcNow,
				EndTime = DateTime.UtcNow,
				TriggerType = triggerType,
				Errors = new List<string> { "Cloud sync is disabled" }
			};
		}

		// Prevent overlapping sync executions
		if ( !await _syncLock.WaitAsync(0) )
		{
			logger.LogError("Cloud sync already in progress, skipping this execution");
			return new CloudSyncResult
			{
				StartTime = DateTime.UtcNow,
				EndTime = DateTime.UtcNow,
				TriggerType = triggerType,
				Errors = new List<string> { "Sync already in progress" }
			};
		}

		try
		{
			IsSyncInProgress = true;
			var result = new CloudSyncResult
			{
				StartTime = DateTime.UtcNow, TriggerType = triggerType
			};

			logger.LogInformation(
				$"Starting cloud sync (Trigger: {triggerType}, Provider: {settings.Provider}, Folder: {settings.RemoteFolder})");

			// Get the cloud sync client
			using var scope = serviceScopeFactory.CreateScope();
			var cloudClient = GetCloudClient(scope);

			if ( cloudClient is not { Enabled: true } )
			{
				var error =
					$"Cloud provider '{settings.Provider}' is not available or not enabled";
				logger.LogError(error);
				result.Errors.Add(error);
				result.EndTime = DateTime.UtcNow;
				LastSyncResult = result;
				return result;
			}

			// Test connection
			if ( !await cloudClient.TestConnectionAsync() )
			{
				const string error = "Failed to connect to cloud storage provider";
				logger.LogError(error);
				result.Errors.Add(error);
				result.EndTime = DateTime.UtcNow;
				LastSyncResult = result;
				return result;
			}

			// List files
			IEnumerable<CloudFile> cloudFiles;
			try
			{
				cloudFiles = await cloudClient.ListFilesAsync(settings.RemoteFolder);
				result.FilesFound = cloudFiles.Count();
				logger.LogInformation($"Found {result.FilesFound} files in cloud storage");
			}
			catch ( Exception ex )
			{
				var error = $"Failed to list files from cloud storage: {ex.Message}";
				logger.LogError(ex, error);
				result.Errors.Add(error);
				result.EndTime = DateTime.UtcNow;
				LastSyncResult = result;
				return result;
			}

			// Process each file
			var import = scope.ServiceProvider.GetRequiredService<IImport>();
			var tempFolder = Path.Combine(Path.GetTempPath(), "starsky-cloudsync",
				Guid.NewGuid().ToString());
			Directory.CreateDirectory(tempFolder);

			try
			{
				foreach ( var file in cloudFiles )
				{
					try
					{
						await ProcessFileAsync(cloudClient, import, file, tempFolder, result);
					}
					catch ( Exception ex )
					{
						logger.LogError(ex, $"Error processing file {file.Name}: {ex.Message}");
						result.FilesFailed++;
						result.FailedFiles.Add(file.Name);
						result.Errors.Add($"{file.Name}: {ex.Message}");
					}
				}
			}
			finally
			{
				try
				{
					if ( Directory.Exists(tempFolder) )
					{
						Directory.Delete(tempFolder, true);
					}
				}
				catch ( Exception ex )
				{
					logger.LogError(ex, $"Failed to cleanup temp folder: {ex.Message}");
				}
			}

			result.EndTime = DateTime.UtcNow;
			LastSyncResult = result;

			logger.LogInformation(
				$"Cloud sync completed: {result.FilesImportedSuccessfully} imported, {result.FilesSkipped} skipped, {result.FilesFailed} failed");

			return result;
		}
		finally
		{
			IsSyncInProgress = false;
			_syncLock.Release();
		}
	}

	private async Task ProcessFileAsync(
		ICloudSyncClient cloudClient,
		IImport import,
		CloudFile file,
		string tempFolder,
		CloudSyncResult result)
	{
		// Check if already processed (idempotency)
		var fileKey = $"{file.Path}_{file.Hash}_{file.Size}";
		if ( _processedFiles.TryGetValue(fileKey, out var processedDate) )
		{
			// Skip if processed within last 24 hours
			if ( DateTime.UtcNow - processedDate < TimeSpan.FromHours(24) )
			{
				logger.LogInformation($"Skipping already processed file: {file.Name}");
				result.FilesSkipped++;
				return;
			}
		}

		logger.LogInformation($"Processing file: {file.Name} (Size: {file.Size} bytes)");

		// Download file
		string localPath;
		try
		{
			localPath = await cloudClient.DownloadFileAsync(file, tempFolder);
			logger.LogInformation($"Downloaded file to: {localPath}");
		}
		catch ( Exception ex )
		{
			logger.LogError(ex, $"Failed to download file {file.Name}: {ex.Message}");
			result.FilesFailed++;
			result.FailedFiles.Add(file.Name);
			result.Errors.Add($"Download failed: {file.Name} - {ex.Message}");
			return;
		}

		// Import file
		var importSuccess = false;
		try
		{
			var importSettings = new ImportSettingsModel
			{
				DeleteAfter = true, // Delete from temp folder after import
				IndexMode = true, // Check if exists in db
				RecursiveDirectory = false
			};

			var importResult = await import.Importer(new[] { localPath }, importSettings);

			// Check if import was successful
			if ( importResult.Any() && importResult.All(i => i.Status == ImportStatus.Ok) )
			{
				importSuccess = true;
				result.FilesImportedSuccessfully++;
				result.SuccessfulFiles.Add(file.Name);
				_processedFiles[fileKey] = DateTime.UtcNow;
				logger.LogInformation($"Successfully imported: {file.Name}");
			}
			else
			{
				var status = importResult.FirstOrDefault()?.Status.ToString() ?? "Unknown";
				logger.LogError($"Import failed for {file.Name}: {status}");
				result.FilesFailed++;
				result.FailedFiles.Add(file.Name);
				result.Errors.Add($"Import failed: {file.Name} - Status: {status}");
			}
		}
		catch ( Exception ex )
		{
			logger.LogError(ex, $"Failed to import file {file.Name}: {ex.Message}");
			result.FilesFailed++;
			result.FailedFiles.Add(file.Name);
			result.Errors.Add($"Import failed: {file.Name} - {ex.Message}");
		}

		// Delete from cloud storage if import was successful and setting is enabled
		if ( importSuccess && settings.DeleteAfterImport )
		{
			try
			{
				var deleted = await cloudClient.DeleteFileAsync(file);
				if ( deleted )
				{
					logger.LogInformation($"Deleted file from cloud storage: {file.Name}");
				}
				else
				{
					logger.LogError($"Failed to delete file from cloud storage: {file.Name}");
				}
			}
			catch ( Exception ex )
			{
				logger.LogError(ex,
					$"Error deleting file from cloud storage {file.Name}: {ex.Message}");
				// Don't fail the whole operation if delete fails
			}
		}
	}

	private ICloudSyncClient? GetCloudClient(IServiceScope scope)
	{
		var clients = scope.ServiceProvider.GetServices<ICloudSyncClient>();
		return clients.FirstOrDefault(c =>
			c.Name.Equals(settings.Provider, StringComparison.OrdinalIgnoreCase));
	}
}
