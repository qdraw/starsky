using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.cloudsync;
using starsky.foundation.cloudsync.Interfaces;

namespace starskytest.FakeMocks;

public class FakeCloudSyncService : ICloudSyncService
{
	public bool IsSyncInProgress { get; set; }
	public CloudSyncResult? LastSyncResult { get; set; }
	public Func<CloudSyncTriggerType, Task<CloudSyncResult>>? SyncAsyncFunc { get; set; }
	public Dictionary<string, CloudSyncResult> LastSyncResults { get; } = new();

	public List<CloudSyncTriggerType> SyncCalls { get; } = new();

	public Task<CloudSyncResult> SyncAsync(CloudSyncTriggerType triggerType)
	{
		SyncCalls.Add(triggerType);

		if ( SyncAsyncFunc != null )
		{
			return SyncAsyncFunc(triggerType);
		}

		return Task.FromResult(new CloudSyncResult
		{
			StartTime = DateTime.UtcNow,
			EndTime = DateTime.UtcNow,
			TriggerType = triggerType,
			FilesFound = 5,
			FilesImportedSuccessfully = 5
		});
	}

	public Task<CloudSyncResult> SyncAsync(string providerId, CloudSyncTriggerType triggerType)
	{
		// For test, just call the main SyncAsync
		return SyncAsync(triggerType);
	}

	public Task<List<CloudSyncResult>> SyncAllAsync(CloudSyncTriggerType triggerType)
	{
		// For test, just call the main SyncAsync and wrap in a list
		return Task.FromResult(new List<CloudSyncResult> { SyncAsync(triggerType).Result });
	}
}
