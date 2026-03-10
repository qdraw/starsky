using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using starsky.foundation.injection;
using starsky.foundation.worker.Interfaces;

namespace starsky.feature.syncbackground.Helpers;

[Service(typeof(IBackgroundJobHandler), InjectionLifetime = InjectionLifetime.Scoped)]
public sealed class OnStartupSyncJobHandler(OnStartupSync onStartupSync) : IBackgroundJobHandler
{
	public string JobType => OnStartupSync.JobType;

	public async Task ExecuteAsync(string? payloadJson, CancellationToken cancellationToken)
	{
		if ( !string.IsNullOrWhiteSpace(payloadJson) )
		{
			_ = JsonSerializer.Deserialize<OnStartupSyncPayload>(payloadJson);
		}

		await onStartupSync.StartUpSyncTask();
	}
}

