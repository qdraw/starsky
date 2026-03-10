using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using starsky.feature.geolookup.Interfaces;
using starsky.feature.realtime.Interface;
using starsky.foundation.database.Models;
using starsky.foundation.injection;
using starsky.foundation.metaupdate.Interfaces;
using starsky.foundation.metaupdate.Models;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.worker.Interfaces;

namespace starsky.Helpers;

public static class ControllerBackgroundJobTypes
{
	public const string MetaUpdate = "Controller.MetaUpdate.v1";
	public const string MetaReplace = "Controller.MetaReplace.v1";
	public const string GeoSync = "Controller.GeoSync.v1";
	public const string MetaTimeCorrect = "Controller.MetaTimeCorrect.v1";
}

public sealed class MetaUpdateBackgroundPayload
{
	public Dictionary<string, List<string>> ChangedFileIndexItemName { get; set; } = new();
	public List<FileIndexItem> FileIndexResultsList { get; set; } = [];
	public bool Collections { get; set; }
	public bool Append { get; set; }
	public int RotateClock { get; set; }
}

public sealed class MetaReplaceBackgroundPayload
{
	public Dictionary<string, List<string>> ChangedFileIndexItemName { get; set; } = new();
	public List<FileIndexItem> ResultsOkOrDeleteList { get; set; } = [];
	public bool Collections { get; set; }
}

public sealed class GeoSyncBackgroundPayload
{
	public string SubPath { get; set; } = "/";
	public bool Index { get; set; }
	public bool OverwriteLocationNames { get; set; }
}

public sealed class MetaTimeCorrectBackgroundPayload
{
	public List<ExifTimezoneCorrectionResult> ValidateResults { get; set; } = [];
	public string RequestType { get; set; } = string.Empty;
	public string RequestJson { get; set; } = string.Empty;
	public string CorrectionType { get; set; } = string.Empty;
}

[Service(typeof(IBackgroundJobHandler), InjectionLifetime = InjectionLifetime.Scoped)]
public sealed class MetaUpdateBackgroundJobHandler(IServiceScopeFactory scopeFactory)
	: IBackgroundJobHandler
{
	public string JobType => ControllerBackgroundJobTypes.MetaUpdate;

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

[Service(typeof(IBackgroundJobHandler), InjectionLifetime = InjectionLifetime.Scoped)]
public sealed class MetaReplaceBackgroundJobHandler(IServiceScopeFactory scopeFactory)
	: IBackgroundJobHandler
{
	public string JobType => ControllerBackgroundJobTypes.MetaReplace;

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

[Service(typeof(IBackgroundJobHandler), InjectionLifetime = InjectionLifetime.Scoped)]
public sealed class GeoSyncBackgroundJobHandler(
	IServiceScopeFactory scopeFactory,
	IWebLogger logger) : IBackgroundJobHandler
{
	public string JobType => ControllerBackgroundJobTypes.GeoSync;

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
