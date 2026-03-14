using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using starsky.feature.webhtmlpublish.Interfaces;
using starsky.feature.webhtmlpublish.Models;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.worker.Interfaces;

namespace starsky.feature.webhtmlpublish.Services;

[Service(typeof(IBackgroundJobHandler), InjectionLifetime = InjectionLifetime.Scoped)]
public sealed class PublishCreateBackgroundJobHandler(
	IWebHtmlPublishService publishService,
	AppSettings appSettings,
	IWebLogger logger) : IBackgroundJobHandler
{
	public const string JobTypeValue = "Publish.Create.v1";
	public string JobType { get; } = JobTypeValue;

	public async Task ExecuteAsync(string? payloadJson, CancellationToken cancellationToken)
	{
		if ( string.IsNullOrWhiteSpace(payloadJson) )
		{
			throw new ArgumentException("Missing payload", nameof(payloadJson));
		}

		var payload = JsonSerializer.Deserialize<PublishCreateBackgroundJobPayload>(payloadJson) ??
		              throw new ArgumentException("Invalid payload", nameof(payloadJson));

		var renderCopyResult = await publishService.RenderCopy(payload.Info,
			payload.PublishProfileName, payload.ItemName, payload.Location);
		await publishService.GenerateZip(appSettings.TempFolder, payload.ItemName,
			renderCopyResult);
		logger.LogInformation($"[/api/publish/create] done: {payload.ItemName} {DateTime.UtcNow}");
	}
}
