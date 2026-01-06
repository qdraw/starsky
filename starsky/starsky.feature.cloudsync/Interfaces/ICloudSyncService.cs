namespace starsky.foundation.cloudsync.Interfaces;

public interface ICloudSyncService
{
	/// <summary>
	///     Check if a sync is currently in progress
	/// </summary>
	bool IsSyncInProgress { get; }

	/// <summary>
	///     Get the last sync results for all providers
	/// </summary>
	Dictionary<string, CloudSyncResult> LastSyncResults { get; }

	/// <summary>
	///     Execute a cloud sync operation for all enabled providers
	/// </summary>
	/// <param name="triggerType">Whether this sync was manually triggered or scheduled</param>
	/// <returns>List of results for each provider</returns>
	Task<List<CloudSyncResult>> SyncAllAsync(CloudSyncTriggerType triggerType);

	Task<CloudSyncResult> SyncAsync(string[] args);

	/// <summary>
	///     Execute a cloud sync operation for a specific provider
	/// </summary>
	/// <param name="providerId">The ID of the provider to sync</param>
	/// <param name="triggerType">Whether this sync was manually triggered or scheduled</param>
	/// <returns>Result of the sync operation</returns>
	Task<CloudSyncResult> SyncAsync(string providerId,
		CloudSyncTriggerType triggerType);
}
