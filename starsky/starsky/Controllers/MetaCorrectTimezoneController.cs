using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using starsky.feature.realtime.Interface;
using starsky.foundation.database.Models;
using starsky.foundation.metaupdate.Interfaces;
using starsky.foundation.metaupdate.Models;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.worker.Interfaces;

namespace starsky.Controllers;

[Authorize]
public class MetaCorrectTimezoneController(
	IExifTimezoneCorrectionService exifTimezoneCorrectionService,
	IUpdateBackgroundTaskQueue queue,
	IWebLogger logger,
	IServiceScopeFactory scopeFactory)
	: Controller
{
	/// <summary>
	///     Preview timezone correction for batch of images (dry-run)
	/// </summary>
	/// <param name="f">subPath filepath to file, split by dot comma (;)</param>
	/// <param name="request">Timezone correction request with RecordedTimezone and CorrectTimezone</param>
	/// <returns>Preview of corrections without modifying files</returns>
	/// <response code="200">Preview results with corrections that would be applied</response>
	/// <response code="400">Invalid parameters or file paths</response>
	/// <response code="401">User unauthorized</response>
	[ProducesResponseType(200)]
	[ProducesResponseType(typeof(string), 400)]
	[HttpPost("/api/exif/correct-timezone/preview")]
	[Produces("application/json")]
	public async Task<IActionResult> PreviewTimezoneCorrectionAsync(
		string f,
		bool? collections,
		[FromBody] ExifTimezoneCorrectionRequest request)
	{
		if ( !ModelState.IsValid || string.IsNullOrWhiteSpace(f) || collections == null )
		{
			return BadRequest("Model is not valid");
		}

		var subPaths = PathHelper.SplitInputFilePaths(f);
		if ( subPaths.Length == 0 )
		{
			return BadRequest("No input files");
		}

		var results = await
			exifTimezoneCorrectionService.Validate(subPaths,
				collections.Value,
				request);

		return Ok(results);
	}

	/// <summary>
	///     Execute timezone correction for batch of images
	/// </summary>
	/// <param name="f">subPath filepath to file, split by dot comma (;)</param>
	/// <param name="request">Timezone correction request with RecordedTimezone and CorrectTimezone</param>
	/// <returns>Results of the correction execution</returns>
	/// <response code="200">Correction results with success/error status per file</response>
	/// <response code="400">Invalid parameters or file paths</response>
	/// <response code="401">User unauthorized</response>
	[ProducesResponseType(200)]
	[ProducesResponseType(typeof(string), 400)]
	[HttpPost("/api/meta-correct-timezone/execute")]
	[Produces("application/json")]
	public async Task<IActionResult> ExecuteTimezoneCorrectionAsync(
		string f,
		bool? collections,
		[FromBody] ExifTimezoneCorrectionRequest request)
	{
		if ( !ModelState.IsValid || string.IsNullOrWhiteSpace(f) || collections == null )
		{
			return BadRequest("Model is not valid");
		}

		var subPaths = PathHelper.SplitInputFilePaths(f);
		if ( subPaths.Length == 0 )
		{
			return BadRequest("No input files");
		}

		var stopwatch = StopWatchLogger.StartUpdateReplaceStopWatch();

		var validateResults = await
			exifTimezoneCorrectionService.Validate(subPaths,
				collections.Value,
				request);

		// Queue background task for batch correction
		await queue.QueueBackgroundWorkItemAsync(async _ =>
		{
			var scope = scopeFactory.CreateScope();
			var service = scope.ServiceProvider
				.GetRequiredService<IExifTimezoneCorrectionService>();

			var fileIndexItems = validateResults
				.Where(x => x.FileIndexItem != null)
				.Select(p => p.FileIndexItem).Cast<FileIndexItem>().ToList();
			await service.CorrectTimezoneAsync(fileIndexItems, request);
			await UpdateWebSocketTaskRun(fileIndexItems);
		}, "TimezoneCorrectionExecute", Activity.Current?.Id);

		new StopWatchLogger(logger).StopUpdateReplaceStopWatch("timezone-correction", f, false,
			stopwatch);

		logger.LogInformation(
			$"[TimezoneCorrectionController] Queued correction for {subPaths.Length} files: " +
			$"{request.RecordedTimezone} -> {request.CorrectTimezone}");

		return Json(validateResults);
	}

	private async Task UpdateWebSocketTaskRun(List<FileIndexItem> fileIndexResultsList)
	{
		var webSocketResponse =
			new ApiNotificationResponseModel<List<FileIndexItem>>(fileIndexResultsList,
				ApiNotificationType.MetaCorrectTimezone);
		var realtimeConnectionsService = scopeFactory.CreateScope()
			.ServiceProvider.GetRequiredService<IRealtimeConnectionsService>();
		await realtimeConnectionsService.NotificationToAllAsync(webSocketResponse,
			CancellationToken.None);
	}

	/// <summary>
	///     Get all available system timezones
	/// </summary>
	/// <returns>List of available timezone identifiers</returns>
	/// <response code="200">List of timezone identifiers</response>
	/// <response code="401">User unauthorized</response>
	[ProducesResponseType(200)]
	[HttpGet("/api/meta-correct-timezone/timezones")]
	[Produces("application/json")]
	public IActionResult GetAvailableTimezones()
	{
		var timezones = new List<dynamic>();

		// Add standard system timezones
		// foreach (var tz in TimeZoneInfo.GetSystemTimeZones())
		// {
		// 	timezones.Add(new
		// 	{
		// 		id = tz.Id,
		// 		displayName = tz.DisplayName,
		// 		standardName = tz.StandardName,
		// 		daylightName = tz.DaylightName
		// 	});
		// }

		// Add Etc/GMT timezones (fixed offset, no DST)
		// GMT offset format: Etc/GMT+X where X is hours behind UTC (opposite sign)
		for (var i = -12; i <= 14; i++)
		{
			if (i == 0)
			{
				continue; // Skip GMT, already in system timezones
			}

			var gmtId = i > 0 ? $"Etc/GMT+{i}" : $"Etc/GMT{i}";
			var offset = i > 0 ? -i : Math.Abs(i);
			var offsetStr = offset >= 0 ? $"+{offset:D2}:00" : $"-{Math.Abs(offset):D2}:00";
			var displayName = $"(UTC{offsetStr}) {gmtId}";

			timezones.Add(new
			{
				id = gmtId,
				displayName,
				standardName = gmtId,
				daylightName = gmtId
			});
		}

		return Ok(timezones.OrderBy(tz => tz.displayName).ToList());
	}
}
