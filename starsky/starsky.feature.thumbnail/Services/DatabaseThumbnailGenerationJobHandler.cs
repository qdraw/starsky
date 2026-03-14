using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using starsky.feature.thumbnail.Interfaces;
using starsky.foundation.injection;
using starsky.foundation.worker.Interfaces;

namespace starsky.feature.thumbnail.Services;

[Service(typeof(IBackgroundJobHandler), InjectionLifetime = InjectionLifetime.Scoped)]
public sealed class DatabaseThumbnailGenerationJobHandler(
	IDatabaseThumbnailGenerationService service) : IBackgroundJobHandler
{
	public string JobType => DatabaseThumbnailGenerationService.DatabaseThumbnailGenerationJobType;

	public async Task ExecuteAsync(string? payloadJson, CancellationToken cancellationToken)
	{
		// Payload reserved for future versioning. Parse to validate known schema now.
		if ( !string.IsNullOrWhiteSpace(payloadJson) )
		{
			_ = JsonSerializer.Deserialize<DatabaseThumbnailGenerationPayload>(payloadJson);
		}

		if ( service is DatabaseThumbnailGenerationService concrete )
		{
			await concrete.ExecuteQueuedJobAsync();
			return;
		}

		// Fallback for alternate implementations.
		await service.StartBackgroundQueue();
	}
}

