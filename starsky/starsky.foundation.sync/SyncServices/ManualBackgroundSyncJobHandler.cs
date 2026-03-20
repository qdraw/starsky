using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using starsky.foundation.injection;
using starsky.foundation.sync.SyncInterfaces;
using starsky.foundation.worker.Interfaces;

namespace starsky.foundation.sync.SyncServices;

[Service(typeof(IBackgroundJobHandler), InjectionLifetime = InjectionLifetime.Scoped)]
public sealed class ManualBackgroundSyncJobHandler(
	IManualBackgroundSyncService manualBackgroundSyncService) : IBackgroundJobHandler
{
	public string JobType => ManualBackgroundSyncService.JobType;

	public async Task ExecuteAsync(string? payloadJson, CancellationToken cancellationToken)
	{
		if ( string.IsNullOrWhiteSpace(payloadJson) )
		{
			throw new ArgumentException("Missing payload", nameof(payloadJson));
		}

		var payload = JsonSerializer.Deserialize<ManualBackgroundSyncPayload>(payloadJson) ??
		              throw new ArgumentException("Invalid payload", nameof(payloadJson));

		if ( manualBackgroundSyncService is not ManualBackgroundSyncService concrete )
		{
			throw new InvalidOperationException(
				"ManualBackgroundSyncService implementation mismatch");
		}

		await concrete.BackgroundTaskExceptionWrapper(payload.SubPath);
	}
}
