namespace starsky.feature.cloudimport.Interfaces;

public interface ICloudImportService
{
	/// <summary>
	///     Check if a sync is currently in progress
	/// </summary>
	bool IsSyncInProgress { get; }

	/// <summary>
	///     Get the last sync results for all providers
	/// </summary>
	Dictionary<string, CloudImportResult> LastSyncResults { get; }

	/// <summary>
	///     Execute a cloud sync operation for all enabled providers
	/// </summary>
	/// <param name="triggerType">Whether this sync was manually triggered or scheduled</param>
	/// <returns>List of results for each provider</returns>
	Task<List<CloudImportResult>> SyncAllAsync(CloudImportTriggerType triggerType);

	Task<CloudImportResult> SyncAsync(string[] args);

	/// <summary>
	///     Execute a cloud sync operation for a specific provider
	/// </summary>
	/// <param name="providerId">The ID of the provider to sync</param>
	/// <param name="triggerType">Whether this sync was manually triggered or scheduled</param>
	/// <returns>Result of the sync operation</returns>
	Task<CloudImportResult> SyncAsync(string providerId,
		CloudImportTriggerType triggerType);
}
