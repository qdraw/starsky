namespace starsky.foundation.cloudsync.Interfaces;

public interface ICloudSyncService
{
	/// <summary>
	/// Execute a cloud sync operation
	/// </summary>
	/// <param name="triggerType">Whether this sync was manually triggered or scheduled</param>
	/// <returns>Result of the sync operation</returns>
	Task<CloudSyncResult> SyncAsync(CloudSyncTriggerType triggerType);

	/// <summary>
	/// Check if a sync is currently in progress
	/// </summary>
	bool IsSyncInProgress { get; }

	/// <summary>
	/// Get the last sync result
	/// </summary>
	CloudSyncResult? LastSyncResult { get; }
}

