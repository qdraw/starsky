using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using starsky.feature.cloudimport.Clients;
using starsky.feature.cloudimport.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.import.Interfaces;
using starsky.foundation.import.Models;
using starsky.foundation.injection;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;

namespace starsky.feature.cloudimport.Services;

[Service(typeof(ICloudImportService), InjectionLifetime = InjectionLifetime.Singleton)]
public class CloudImportService(
	IServiceScopeFactory serviceScopeFactory,
	IWebLogger logger,
	AppSettings appSettings)
	: ICloudImportService
{
	private readonly Dictionary<string, CloudImportResult> _lastSyncResults = new();
	private readonly ConcurrentDictionary<string, DateTime> _processedFiles = new();
	private readonly ConcurrentDictionary<string, SemaphoreSlim> _providerLocks = new();
	private readonly object _resultsLock = new();

	public bool IsSyncInProgress { get; private set; }

	public Dictionary<string, CloudImportResult> LastSyncResults
	{
		get
		{
			lock ( _resultsLock )
			{
				return new Dictionary<string, CloudImportResult>(_lastSyncResults);
			}
		}
	}

	public async Task<List<CloudImportResult>> SyncAllAsync(CloudImportTriggerType triggerType)
	{
		var results = new List<CloudImportResult>();
		var enabledProviders =
			appSettings.CloudImport?.Providers.Where(p => p.Enabled).ToList() ?? [];

		if ( enabledProviders.Count == 0 )
		{
			logger.LogInformation("No enabled Cloud Import providers found");
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
				results.Add(new CloudImportResult
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

	public async Task<CloudImportResult> SyncAsync(string[] args)
	{
		var importProvider = ArgsHelper.GetCloudImportProvider(args);
		if ( string.IsNullOrEmpty(importProvider) )
		{
			return new CloudImportResult
			{
				TriggerType = CloudImportTriggerType.CommandLineInterface,
				Errors = ["No Cloud Import provider specified in arguments"],
				SkippedNoInput = true
			};
		}

		return await SyncAsync(importProvider, CloudImportTriggerType.CommandLineInterface);
	}

	public async Task<CloudImportResult> SyncAsync(string providerId,
		CloudImportTriggerType triggerType)
	{
		var providerSettings =
			appSettings.CloudImport?.Providers.FirstOrDefault(p => p.Id == providerId);
		if ( providerSettings == null )
		{
			logger.LogError($"Provider with ID '{providerId}' not found in configuration");
			return new CloudImportResult
			{
				ProviderId = providerId,
				StartTime = DateTime.UtcNow,
				EndTime = DateTime.UtcNow,
				TriggerType = triggerType,
				Errors = [$"Provider '{providerId}' not found"],
				SkippedNoInput = true
			};
		}

		if ( !providerSettings.Enabled )
		{
			logger.LogInformation($"Cloud Import provider '{providerId}' is disabled");
			return new CloudImportResult
			{
				ProviderId = providerId,
				ProviderName = providerSettings.Provider,
				StartTime = DateTime.UtcNow,
				EndTime = DateTime.UtcNow,
				TriggerType = triggerType,
				Errors = ["Provider is disabled"],
				SkippedNoInput = true
			};
		}

		// Get or create a lock for this provider
		var providerLock = _providerLocks.GetOrAdd(providerId, _ => new SemaphoreSlim(1, 1));

		var lockAcquired = false;
		try
		{
			// Prevent overlapping sync executions for this provider
			if ( !await providerLock.WaitAsync(0) )
			{
				logger.LogError(
					$"Cloud Import already in progress for provider '{providerId}', skipping this execution");
				return new CloudImportResult
				{
					ProviderId = providerId,
					ProviderName = providerSettings.Provider,
					StartTime = DateTime.UtcNow,
					EndTime = DateTime.UtcNow,
					TriggerType = triggerType,
					Errors = ["Sync already in progress for this provider"]
				};
			}

			lockAcquired = true;

			try
			{
				IsSyncInProgress = true;
				var result = new CloudImportResult
				{
					ProviderId = providerId,
					ProviderName = providerSettings.Provider,
					StartTime = DateTime.UtcNow,
					TriggerType = triggerType
				};

				logger.LogInformation(
					$"Starting Cloud Import (Provider ID: {providerId}, Trigger: {triggerType}, Provider: {providerSettings.Provider}, Folder: {providerSettings.RemoteFolder})");

				// Get the Cloud Import client
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

				if ( cloudClient is DropboxCloudImportClient dropboxClient )
				{
					var credentials = providerSettings.Credentials;
					await dropboxClient.InitializeClient(credentials.RefreshToken,
						credentials.AppKey,
						credentials.AppSecret);
				}

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
				var (cloudFiles, cloudImportResult) =
					await GetCloudFiles(cloudClient, result,
						providerSettings, providerId);
				if ( cloudImportResult != null )
				{
					return cloudImportResult;
				}

				// Process each file
				var import = scope.ServiceProvider.GetRequiredService<IImport>();
				var tempFolder = GetTempFolder(providerId);

				try
				{
					await ProcessFileLoopAsync(cloudClient, import, cloudFiles, tempFolder, result,
						providerSettings);
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
					$"Cloud Import completed for provider '{providerId}': {result.FilesImportedSuccessfully} imported, {result.FilesSkipped} skipped, {result.FilesFailed} failed");

				return result;
			}
			finally
			{
				providerLock.Release();
				if ( _providerLocks.Values.All(l => l.CurrentCount != 0) )
				{
					IsSyncInProgress = false;
				}
			}
		}
		catch
		{
			if ( lockAcquired )
			{
				providerLock.Release();
			}

			throw;
		}
	}

	private async Task<(IEnumerable<CloudFile>, CloudImportResult?)> GetCloudFiles(
		ICloudImportClient cloudClient,
		CloudImportResult result, CloudImportProviderSettings providerSettings, string providerId)
	{
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
			return ( [], result );
		}

		return ( cloudFiles, null );
	}

	private static string GetTempFolder(string providerId)
	{
		var tempFolder = Path.Combine(Path.GetTempPath(),
			"starsky-cloud-import",
			providerId,
			Guid.NewGuid().ToString());
		Directory.CreateDirectory(tempFolder);
		return tempFolder;
	}

	private void UpdateLastSyncResult(string providerId, CloudImportResult result)
	{
		lock ( _resultsLock )
		{
			_lastSyncResults[providerId] = result;
		}
	}

	private async Task ProcessFileLoopAsync(ICloudImportClient cloudClient,
		IImport import,
		IEnumerable<CloudFile> cloudFiles,
		string tempFolder,
		CloudImportResult result,
		CloudImportProviderSettings providerSettings)
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

	private async Task ProcessFileAsync(
		ICloudImportClient cloudClient,
		IImport import,
		CloudFile file,
		string tempFolder,
		CloudImportResult result,
		CloudImportProviderSettings providerSettings)
	{
		// Check if already processed (idempotency)
		var fileKey = $"{file.Path}_{file.Hash}_{file.Size}";
		if ( _processedFiles.TryGetValue(fileKey, out var processedDate) &&
		     DateTime.UtcNow - processedDate < TimeSpan.FromHours(24) )
		{
			logger.LogInformation($"Skipping already processed file: {file.Name}");
			result.FilesSkipped++;
			return;
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
			if ( importResult.Count != 0 && importResult.All(i => i.Status == ImportStatus.Ok) )
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

	private static ICloudImportClient? GetCloudClient(IServiceScope scope, string providerName)
	{
		var clients = scope.ServiceProvider.GetServices<ICloudImportClient>();
		return clients.FirstOrDefault(c =>
			c.Name.Equals(providerName, StringComparison.OrdinalIgnoreCase));
	}
}
