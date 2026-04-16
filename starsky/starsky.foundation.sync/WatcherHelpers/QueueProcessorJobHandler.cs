using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.injection;
using starsky.foundation.worker.Interfaces;

namespace starsky.foundation.sync.WatcherHelpers;

[Service(typeof(IBackgroundJobHandler), InjectionLifetime = InjectionLifetime.Scoped)]
public sealed class QueueProcessorJobHandler(
	IServiceScopeFactory scopeFactory) : IBackgroundJobHandler
{
	public string JobType => QueueProcessor.JobType;

	public async Task ExecuteAsync(string? payloadJson, CancellationToken cancellationToken)
	{
		if ( string.IsNullOrWhiteSpace(payloadJson) )
		{
			throw new ArgumentException("Missing payload", nameof(payloadJson));
		}

		var payload = JsonSerializer.Deserialize<QueueProcessorPayload>(payloadJson) ??
		              throw new ArgumentException("Invalid payload", nameof(payloadJson));

		var connector = new SyncWatcherConnector(scopeFactory);
		await connector.Sync(new Tuple<string, string?, WatcherChangeTypes>(
			payload.FilePath,
			payload.ToPath,
			payload.ChangeTypes));
	}
}
