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

		var payload = JsonSerializer.Deserialize<SmallThumbnailBackgroundPayload>(payloadJson);
		if ( payload == null )
		{
			throw new ArgumentException("Invalid payload", nameof(payloadJson));
		}

		if ( service is not SmallThumbnailBackgroundJobService concrete )
		{
			throw new InvalidOperationException(
				"SmallThumbnailBackgroundJobService implementation mismatch");
		}

		await concrete.WorkThumbnailGenerationLoop(payload.Path);
	}
}
