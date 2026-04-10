using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using starsky.feature.thumbnail.Interfaces;
using starsky.foundation.injection;
using starsky.foundation.worker.Interfaces;

namespace starsky.feature.thumbnail.Services;

[Service(typeof(IBackgroundJobHandler), InjectionLifetime = InjectionLifetime.Scoped)]
public sealed class SmallThumbnailBackgroundJobHandler(
	ISmallThumbnailBackgroundJobService service) : IBackgroundJobHandler
{
	public string JobType => SmallThumbnailBackgroundJobService.JobType;

	public async Task ExecuteAsync(string? payloadJson, CancellationToken cancellationToken)
	{
		if ( string.IsNullOrWhiteSpace(payloadJson) )
		{
			throw new ArgumentException("Missing payload", nameof(payloadJson));
		}

		var payload = JsonSerializer.Deserialize<SmallThumbnailBackgroundPayload>(payloadJson) ??
		              throw new ArgumentException("Invalid payload", nameof(payloadJson));

		await service.WorkThumbnailGenerationLoop(payload.Path);
	}
}
