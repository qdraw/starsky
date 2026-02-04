using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.feature.cloudimport;
using starsky.feature.cloudimport.Interfaces;

namespace starskytest.FakeMocks;

public class FakeCloudImportService : ICloudImportService
{
	public CloudImportResult? LastSyncResult { get; set; }
	public Func<CloudImportTriggerType, Task<CloudImportResult>>? SyncAsyncFunc { get; set; }

	public List<CloudImportTriggerType> SyncCalls { get; } = new();
	public bool IsSyncInProgress { get; set; }
	public Dictionary<string, CloudImportResult> LastSyncResults { get; set; } = new();
	public bool ThrowOnSync { get; set; }

	public Task<CloudImportResult> SyncAsync(string[] args)
	{
		throw new NotImplementedException();
	}

	public Task<CloudImportResult> SyncAsync(string providerId, CloudImportTriggerType triggerType)
	{
		// For test, just call the main SyncAsync
		return SyncAsync(triggerType);
	}

	public Task<List<CloudImportResult>> SyncAllAsync(CloudImportTriggerType triggerType)
	{
		// For test, just call the main SyncAsync and wrap in a list
		return Task.FromResult(new List<CloudImportResult> { SyncAsync(triggerType).Result });
	}

	public Task<CloudImportResult> SyncAsync(CloudImportTriggerType triggerType)
	{
		if ( IsSyncInProgress )
		{
			return Task.FromResult(new CloudImportResult
			{
				ProviderId = string.Empty,
				StartTime = DateTime.UtcNow,
				EndTime = DateTime.UtcNow,
				TriggerType = triggerType,
				Errors = ["Provider not found"],
				SkippedNoInput = true
			});
		}

		SyncCalls.Add(triggerType);

		if ( ThrowOnSync )
		{
			throw new InvalidOperationException("Sync failed");
		}

		if ( SyncAsyncFunc != null )
		{
			return SyncAsyncFunc(triggerType);
		}

		return Task.FromResult(new CloudImportResult
		{
			StartTime = DateTime.UtcNow,
			EndTime = DateTime.UtcNow,
			TriggerType = triggerType,
			FilesFound = 5,
			FilesImportedSuccessfully = 5
		});
	}
}
