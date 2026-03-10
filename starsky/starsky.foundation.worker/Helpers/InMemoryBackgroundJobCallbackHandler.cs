using System.Threading;
using System.Threading.Tasks;
using starsky.foundation.injection;
using starsky.foundation.worker.Interfaces;

namespace starsky.foundation.worker.Helpers;

[Service(typeof(IBackgroundJobHandler), InjectionLifetime = InjectionLifetime.Scoped)]
public sealed class InMemoryBackgroundJobCallbackHandler : IBackgroundJobHandler
{
	public string JobType => InMemoryBackgroundJobCallbackRegistry.CallbackJobType;

	public async Task ExecuteAsync(string? payloadJson, CancellationToken cancellationToken)
	{
		if ( string.IsNullOrWhiteSpace(payloadJson) )
		{
			return;
		}

		await InMemoryBackgroundJobCallbackRegistry.TryExecuteAsync(
			new Models.BackgroundTaskQueueJob
			{
				JobType = JobType,
				PayloadJson = payloadJson
			},
			cancellationToken);
	}
}

