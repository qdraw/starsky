using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.cloudsync.Clients;
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
	AppSettings appSettings)
	: ICloudSyncService
{
	private readonly Dictionary<string, CloudSyncResult> _lastSyncResults = new();
	private readonly ConcurrentDictionary<string, DateTime> _processedFiles = new();
	private readonly ConcurrentDictionary<string, SemaphoreSlim> _providerLocks = new();
	private readonly object _resultsLock = new();

	public bool IsSyncInProgress { get; private set; }

	public Dictionary<string, CloudSyncResult> LastSyncResults
	{
		get
		{
			lock ( _resultsLock )
			{
				return new Dictionary<string, CloudSyncResult>(_lastSyncResults);
			}
		}
	}

	public async Task<List<CloudSyncResult>> SyncAllAsync(CloudSyncTriggerType triggerType)
	{
		var results = new List<CloudSyncResult>();
		var enabledProviders =
			appSettings.CloudSync?.Providers.Where(p => p.Enabled).ToList() ?? [];

		if ( enabledProviders.Count == 0 )
		{
			logger.LogInformation("No enabled cloud sync providers found");
			return results;
		}

		logger.LogInformation(
			$"Starting sync for {enabledProviders.Count} enabled provider(s)");

		foreach ( var providerSettings in enabledProviders )
		{
			try
			{
				var result = await SyncAsync(providerSettings.Id, triggerType);
				results.Add(result);
			}
			catch ( Exception ex )
			{
				logger.LogError(ex,
					$"Error syncing provider {providerSettings.Id}: {ex.Message}");
				results.Add(new CloudSyncResult
				{
					ProviderId = providerSettings.Id,
					ProviderName = providerSettings.Provider,
					StartTime = DateTime.UtcNow,
					EndTime = DateTime.UtcNow,
					TriggerType = triggerType,
					Errors = new List<string> { $"Sync failed: {ex.Message}" }
				});
			}
		}

		return results;
	}

	public async Task<CloudSyncResult> SyncAsync(string providerId,
		CloudSyncTriggerType triggerType)
	{
		var providerSettings =
			appSettings.CloudSync?.Providers.FirstOrDefault(p => p.Id == providerId);
		if ( providerSettings == null )
		{
			logger.LogError($"Provider with ID '{providerId}' not found in configuration");
			return new CloudSyncResult
			{
				ProviderId = providerId,
				StartTime = DateTime.UtcNow,
				EndTime = DateTime.UtcNow,
				TriggerType = triggerType,
				Errors = [$"Provider '{providerId}' not found"]
			};
		}

		if ( !providerSettings.Enabled )
		{
			logger.LogInformation($"Cloud sync provider '{providerId}' is disabled");
			return new CloudSyncResult
			{
				ProviderId = providerId,
				ProviderName = providerSettings.Provider,
				StartTime = DateTime.UtcNow,
				EndTime = DateTime.UtcNow,
				TriggerType = triggerType,
				Errors = ["Provider is disabled"]
			};
		}

		// Get or create a lock for this provider
		var providerLock = _providerLocks.GetOrAdd(providerId, _ => new SemaphoreSlim(1, 1));

		// Prevent overlapping sync executions for this provider
		if ( !await providerLock.WaitAsync(0) )
		{
			logger.LogError(
				$"Cloud sync already in progress for provider '{providerId}', skipping this execution");
			return new CloudSyncResult
			{
				ProviderId = providerId,
				ProviderName = providerSettings.Provider,
				StartTime = DateTime.UtcNow,
				EndTime = DateTime.UtcNow,
				TriggerType = triggerType,
				Errors = ["Sync already in progress for this provider"]
			};
		}

		try
		{
			IsSyncInProgress = true;
			var result = new CloudSyncResult
			{
				ProviderId = providerId,
				ProviderName = providerSettings.Provider,
				StartTime = DateTime.UtcNow,
				TriggerType = triggerType
			};

			logger.LogInformation(
				$"Starting cloud sync (Provider ID: {providerId}, Trigger: {triggerType}, Provider: {providerSettings.Provider}, Folder: {providerSettings.RemoteFolder})");

			// Get the cloud sync client
			using var scope = serviceScopeFactory.CreateScope();
			var cloudClient = GetCloudClient(scope, providerSettings.Provider);

			if ( cloudClient is not { Enabled: true } )
			{
				var error =
					$"Cloud provider '{providerSettings.Provider}' is not available or not enabled";
				logger.LogError(error);
				result.Errors.Add(error);
				result.EndTime = DateTime.UtcNow;
				UpdateLastSyncResult(providerId, result);
				return result;
			}

			// Initialize client with provider-specific credentials
			if ( cloudClient is DropboxCloudSyncClient dropboxClient )
			{
				// if ( string.IsNullOrWhiteSpace(providerSettings.Credentials.AccessToken) )
				// {
				// 	const string error = "Dropbox access token is not configured for this provider";
				// 	logger.LogError(error);
				// 	result.Errors.Add(error);
				// 	result.EndTime = DateTime.UtcNow;
				// 	UpdateLastSyncResult(providerId, result);
				// 	return result;
				// }

				var credentials = providerSettings.Credentials;
				await dropboxClient.InitializeClient(credentials.RefreshToken, credentials.AppKey,
					credentials.AppSecret);
			}

			// Test connection
			if ( !await cloudClient.TestConnectionAsync() )
			{
				const string error = "Failed to connect to cloud storage provider";
				logger.LogError(error);
				result.Errors.Add(error);
				result.EndTime = DateTime.UtcNow;
				UpdateLastSyncResult(providerId, result);
				return result;
			}

			// List files
			IEnumerable<CloudFile> cloudFiles;
			try
			{
				cloudFiles = await cloudClient.ListFilesAsync(providerSettings.RemoteFolder);
				result.FilesFound = cloudFiles.Count();
				logger.LogInformation(
					$"Found {result.FilesFound} files in cloud storage for provider '{providerId}'");
			}
			catch ( Exception ex )
			{
				var error = $"Failed to list files from cloud storage: {ex.Message}";
				logger.LogError(ex, error);
				result.Errors.Add(error);
				result.EndTime = DateTime.UtcNow;
				UpdateLastSyncResult(providerId, result);
				return result;
			}

			// Process each file
			var import = scope.ServiceProvider.GetRequiredService<IImport>();
			var tempFolder = Path.Combine(Path.GetTempPath(), "starsky-cloudsync", providerId,
				Guid.NewGuid().ToString());
			Directory.CreateDirectory(tempFolder);

			try
			{
				foreach ( var file in cloudFiles )
				{
					try
					{
						await ProcessFileAsync(cloudClient, import, file, tempFolder, result,
							providerSettings);
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
			UpdateLastSyncResult(providerId, result);

			logger.LogInformation(
				$"Cloud sync completed for provider '{providerId}': {result.FilesImportedSuccessfully} imported, {result.FilesSkipped} skipped, {result.FilesFailed} failed");

			return result;
		}
		finally
		{
			IsSyncInProgress = _providerLocks.Values.Any(l => l.CurrentCount == 0);
			providerLock.Release();
		}
	}

	private void UpdateLastSyncResult(string providerId, CloudSyncResult result)
	{
		lock ( _resultsLock )
		{
			_lastSyncResults[providerId] = result;
		}
	}

	private async Task ProcessFileAsync(
		ICloudSyncClient cloudClient,
		IImport import,
		CloudFile file,
		string tempFolder,
		CloudSyncResult result,
		CloudSyncProviderSettings providerSettings)
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
				RecursiveDirectory = false,
				Origin = providerSettings.Id
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
		if ( importSuccess && providerSettings.DeleteAfterImport )
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

	private ICloudSyncClient? GetCloudClient(IServiceScope scope, string providerName)
	{
		var clients = scope.ServiceProvider.GetServices<ICloudSyncClient>();
		return clients.FirstOrDefault(c =>
			c.Name.Equals(providerName, StringComparison.OrdinalIgnoreCase));
	}
}
