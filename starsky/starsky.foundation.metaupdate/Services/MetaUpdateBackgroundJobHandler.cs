using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.injection;
using starsky.foundation.metaupdate.Interfaces;
using starsky.foundation.metaupdate.Models;
using starsky.foundation.worker.Interfaces;

namespace starsky.foundation.metaupdate.Services;

[Service(typeof(IBackgroundJobHandler), InjectionLifetime = InjectionLifetime.Scoped)]
public sealed class MetaUpdateBackgroundJobHandler(IServiceScopeFactory scopeFactory)
	: IBackgroundJobHandler
{
	public const string MetaUpdate = "MetaUpdate.v1";
	public string JobType => MetaUpdate;

	public async Task ExecuteAsync(string? payloadJson, CancellationToken cancellationToken)
	{
		if ( string.IsNullOrWhiteSpace(payloadJson) )
		{
			throw new ArgumentException("Missing payload");
		}

		var payload = JsonSerializer.Deserialize<MetaUpdateBackgroundPayload>(payloadJson)
		              ?? throw new ArgumentException("Invalid payload");
		var metaUpdateService = scopeFactory.CreateScope().ServiceProvider
			.GetRequiredService<IMetaUpdateService>();
		await metaUpdateService.UpdateAsync(payload.ChangedFileIndexItemName,
			payload.FileIndexResultsList,
			null,
			payload.Collections,
			payload.Append,
			payload.RotateClock);
	}
}
