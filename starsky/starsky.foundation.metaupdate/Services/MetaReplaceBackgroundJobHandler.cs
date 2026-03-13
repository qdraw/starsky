using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.injection;
using starsky.foundation.metaupdate.Interfaces;
using starsky.foundation.metaupdate.Models;
using starsky.foundation.worker.Interfaces;

namespace starsky.foundation.metaupdate.Services;

[Service(typeof(IBackgroundJobHandler), InjectionLifetime = InjectionLifetime.Scoped)]
public sealed class MetaReplaceBackgroundJobHandler(IServiceScopeFactory scopeFactory)
	: IBackgroundJobHandler
{
	public const string MetaReplace = "MetaReplace.v1";

	public string JobType => MetaReplace;

	public async Task ExecuteAsync(string? payloadJson, CancellationToken cancellationToken)
	{
		if ( string.IsNullOrWhiteSpace(payloadJson) )
		{
			throw new ArgumentException("Missing payload");
		}

		var payload = JsonSerializer.Deserialize<MetaReplaceBackgroundPayload>(payloadJson)
		              ?? throw new ArgumentException("Invalid payload");
		var metaUpdateService = scopeFactory.CreateScope().ServiceProvider
			.GetRequiredService<IMetaUpdateService>();
		await metaUpdateService.UpdateAsync(payload.ChangedFileIndexItemName,
			payload.ResultsOkOrDeleteList,
			null,
			payload.Collections,
			false,
			0);
	}
}
