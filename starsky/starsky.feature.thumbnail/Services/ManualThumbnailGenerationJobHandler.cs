using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using starsky.feature.thumbnail.Interfaces;
using starsky.foundation.injection;
using starsky.foundation.worker.Interfaces;

namespace starsky.feature.thumbnail.Services;

[Service(typeof(IBackgroundJobHandler), InjectionLifetime = InjectionLifetime.Scoped)]
public sealed class ManualThumbnailGenerationJobHandler(
	IManualThumbnailGenerationService service) : IBackgroundJobHandler
{
	public string JobType => ManualThumbnailGenerationService.JobType;

	public async Task ExecuteAsync(string? payloadJson, CancellationToken cancellationToken)
	{
		if ( string.IsNullOrWhiteSpace(payloadJson) )
		{
			throw new ArgumentException("Missing payload", nameof(payloadJson));
		}

		var payload = JsonSerializer.Deserialize<ManualThumbnailGenerationPayload>(payloadJson) ??
		              throw new ArgumentException("Invalid payload", nameof(payloadJson));

		if ( service is not ManualThumbnailGenerationService concrete )
		{
			throw new InvalidOperationException(
				"ManualThumbnailGenerationService implementation mismatch");
		}

		await concrete.WorkThumbnailGeneration(payload.SubPath);
	}
}
