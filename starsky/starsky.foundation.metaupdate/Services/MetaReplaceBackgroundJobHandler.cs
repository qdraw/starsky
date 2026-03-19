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
		var scope = scopeFactory.CreateScope();
		var metaUpdateService = scope.ServiceProvider
			.GetRequiredService<IMetaUpdateService>();
		var metaPreflight = scope.ServiceProvider
			.GetRequiredService<IMetaPreflight>();

		var (fileIndexResultsList, changedFileIndexItemName) = await metaPreflight.PreflightAsync(
			null, payload.SubPaths, false, payload.Collections, 0);

		await metaUpdateService.UpdateAsync(changedFileIndexItemName,
			fileIndexResultsList,
			null,
			payload.Collections,
			false,
			0);
	}
}
