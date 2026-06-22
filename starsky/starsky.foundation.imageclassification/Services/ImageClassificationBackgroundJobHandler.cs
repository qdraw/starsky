using System.Text.Json;
using starsky.foundation.database.Interfaces;
using starsky.foundation.imageclassification.Interfaces;
using starsky.foundation.imageclassification.Models;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.worker.Interfaces;

namespace starsky.foundation.imageclassification.Services;

[Service(typeof(IBackgroundJobHandler), InjectionLifetime = InjectionLifetime.Scoped)]
public sealed class ImageClassificationBackgroundJobHandler(
	IImageClassificationService imageClassificationService,
	IQuery query,
	IWebLogger logger) : IBackgroundJobHandler
{
	public const string ImageClassificationJobType = "ImageClassification.v1";
	public string JobType => ImageClassificationJobType;

	public async Task ExecuteAsync(string? payloadJson, CancellationToken cancellationToken)
	{
		if ( string.IsNullOrWhiteSpace(payloadJson) )
		{
			throw new ArgumentException("Missing payload", nameof(payloadJson));
		}

		var payload = JsonSerializer.Deserialize<ImageClassificationQueuePayload>(payloadJson) ??
		              throw new ArgumentException("Invalid payload", nameof(payloadJson));

		if ( string.IsNullOrWhiteSpace(payload.FilePath) )
		{
			throw new ArgumentException("Missing file path", nameof(payloadJson));
		}

		var item = await query.GetObjectByFilePathAsync(payload.FilePath);
		if ( item == null )
		{
			logger.LogInformation(
				$"[ImageClassificationBackgroundJobHandler] skip missing item {payload.FilePath}");
			return;
		}

		await imageClassificationService.ClassifyAndUpdateAsync(item, cancellationToken);
	}
}

