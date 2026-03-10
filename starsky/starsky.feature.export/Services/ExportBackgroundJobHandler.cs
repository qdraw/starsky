using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using starsky.feature.export.Interfaces;
using starsky.feature.export.Models;
using starsky.foundation.injection;
using starsky.foundation.worker.Interfaces;

namespace starsky.feature.export.Services;

[Service(typeof(IBackgroundJobHandler), InjectionLifetime = InjectionLifetime.Scoped)]
public sealed class ExportBackgroundJobHandler(IExport exportService) : IBackgroundJobHandler
{
	public const string Export = "Controller.Export.v1";

	public string JobType => Export;

	public async Task ExecuteAsync(string? payloadJson, CancellationToken cancellationToken)
	{
		if ( string.IsNullOrWhiteSpace(payloadJson) )
		{
			throw new ArgumentException("Missing payload");
		}

		var payload = JsonSerializer.Deserialize<ExportBackgroundPayload>(payloadJson)
		              ?? throw new ArgumentException("Invalid payload");
		await exportService.CreateZip(payload.FileIndexResultsList, payload.Thumbnail,
			payload.ZipOutputName);
	}
}
