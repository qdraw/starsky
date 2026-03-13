using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using starsky.feature.geolookup.Interfaces;
using starsky.feature.geolookup.Models;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.worker.Interfaces;

namespace starsky.feature.geolookup.Services;

[Service(typeof(IBackgroundJobHandler), InjectionLifetime = InjectionLifetime.Scoped)]
public sealed class GeoSyncBackgroundJobHandler(
	IServiceScopeFactory scopeFactory,
	IWebLogger logger) : IBackgroundJobHandler
{
	public const string GeoSync = "GeoSync.v1";

	public string JobType => GeoSync;

	public async Task ExecuteAsync(string? payloadJson, CancellationToken cancellationToken)
	{
		if ( string.IsNullOrWhiteSpace(payloadJson) )
		{
			throw new ArgumentException("Missing payload");
		}

		var payload = JsonSerializer.Deserialize<GeoSyncBackgroundPayload>(payloadJson)
		              ?? throw new ArgumentException("Invalid payload");
		logger.LogInformation(
			$"GeoSyncFolder started {payload.SubPath} {DateTime.UtcNow.ToShortTimeString()}");
		var geoBackgroundTask = scopeFactory.CreateScope().ServiceProvider
			.GetRequiredService<IGeoBackgroundTask>();
		var result = await geoBackgroundTask.GeoBackgroundTaskAsync(payload.SubPath,
			payload.Index,
			payload.OverwriteLocationNames);
		logger.LogInformation($"GeoSyncFolder end {payload.SubPath} {result.Count}");
	}
}
