using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.database.Models;
using starsky.foundation.injection;
using starsky.foundation.metaupdate.Interfaces;
using starsky.foundation.metaupdate.Models;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.worker.Interfaces;

namespace starsky.feature.metaupdate.Services;


[Service(typeof(IBackgroundJobHandler), InjectionLifetime = InjectionLifetime.Scoped)]
public sealed class MetaTimeCorrectBackgroundJobHandler(
	IServiceScopeFactory scopeFactory,
	IWebLogger logger,
	IRealtimeConnectionsService realtimeConnectionsService) : IBackgroundJobHandler
{
	public string JobType => ControllerBackgroundJobTypes.MetaTimeCorrect;

	public async Task ExecuteAsync(string? payloadJson, CancellationToken cancellationToken)
	{
		if ( string.IsNullOrWhiteSpace(payloadJson) )
		{
			throw new ArgumentException("Missing payload");
		}

		var payload = JsonSerializer.Deserialize<MetaTimeCorrectBackgroundPayload>(payloadJson)
		              ?? throw new ArgumentException("Invalid payload");
		using var scope = scopeFactory.CreateScope();
		var scopedService =
			scope.ServiceProvider.GetRequiredService<IExifTimezoneCorrectionService>();
		var fileIndexResultsList = payload.ValidateResults
			.Where(r => r.FileIndexItem != null)
			.Select(r => r.FileIndexItem!)
			.ToList();
		IExifTimeCorrectionRequest request = payload.RequestType switch
		{
			"timezone" => JsonSerializer.Deserialize<ExifTimezoneBasedCorrectionRequest>(
				              payload.RequestJson)
			              ?? throw new ArgumentException("Invalid timezone request payload"),
			"offset" => JsonSerializer.Deserialize<ExifCustomOffsetCorrectionRequest>(
				            payload.RequestJson)
			            ?? throw new ArgumentException("Invalid offset request payload"),
			_ => throw new ArgumentException("Unknown request type")
		};

		logger.LogInformation(
			$"[MetaTimeCorrectController] Starting {payload.CorrectionType} correction for {fileIndexResultsList.Count} files");
		var results = await scopedService.CorrectTimezoneAsync(fileIndexResultsList, request);
		logger.LogInformation(
			$"[MetaTimeCorrectController] Completed {payload.CorrectionType} correction: {results.Count(r => r.Success)} succeeded, {results.Count(r => !r.Success)} failed");
		var webSocketResponse = new ApiNotificationResponseModel<List<FileIndexItem>>(
			fileIndexResultsList,
			ApiNotificationType.MetaTimeCorrect);
		await realtimeConnectionsService.NotificationToAllAsync(webSocketResponse,
			CancellationToken.None);
	}
}
