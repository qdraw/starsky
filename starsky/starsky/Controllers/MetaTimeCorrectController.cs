using System;
using System.Collections.Generic;
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

/// <summary>
///     Controller for EXIF timezone correction operations
///     Supports both timezone-based and custom offset corrections
/// </summary>
[Authorize]
public class MetaTimeCorrectController(
	IExifTimezoneCorrectionService exifTimezoneCorrectionService,
	IUpdateBackgroundTaskQueue queue,
	IWebLogger logger,
	IServiceScopeFactory scopeFactory)
	: Controller
{
	private const string ModelNotValidError = "Model is not valid";
	private const string NoInputFilesError = "No input files";

	/// <summary>
	///     Preview timezone correction for batch of images (dry-run)
	///     Uses timezone IDs to calculate offset differences (DST-aware)
	/// </summary>
	/// <param name="f">subPath filepath to file, split by dot comma (;)</param>
	/// <param name="collections">Include collections or not</param>
	/// <param name="request">Timezone correction request with RecordedTimezone and CorrectTimezone</param>
	/// <returns>Preview of corrections without modifying files</returns>
	/// <response code="200">Preview results with corrections that would be applied</response>
	/// <response code="400">Invalid parameters or file paths</response>
	/// <response code="401">User unauthorized</response>
	[ProducesResponseType(200)]
	[ProducesResponseType(typeof(string), 400)]
	[HttpPost("/api/meta-time-correct/timezone-preview")]
	[Produces("application/json")]
	public async Task<IActionResult> PreviewTimezoneCorrectionAsync(
		string f,
		bool? collections,
		[FromBody] ExifTimezoneBasedCorrectionRequest request)
	{
		var validationResult = ValidateRequest(ModelState.IsValid, f, collections);
		if ( validationResult != null )
		{
			return validationResult;
		}

		var results = await
			exifTimezoneCorrectionService.Validate(f,
				collections!.Value,
				request);

		return Ok(results);
	}

	/// <summary>
	///     Execute timezone correction for batch of images
	///     Uses timezone IDs to calculate offset differences (DST-aware)
	/// </summary>
	/// <param name="f">subPath filepath to file, split by dot comma (;)</param>
	/// <param name="collections">Include collections or not</param>
	/// <param name="request">Timezone correction request with RecordedTimezone and CorrectTimezone</param>
	/// <returns>Results of the correction execution</returns>
	/// <response code="200">Correction results with success/error status per file</response>
	/// <response code="400">Invalid parameters or file paths</response>
	/// <response code="401">User unauthorized</response>
	[ProducesResponseType(200)]
	[ProducesResponseType(typeof(string), 400)]
	[HttpPost("/api/meta-time-correct/timezone-execute")]
	[Produces("application/json")]
	public async Task<IActionResult> ExecuteTimezoneCorrectionAsync(
		string f,
		bool? collections,
		[FromBody] ExifTimezoneBasedCorrectionRequest request)
	{
		var validationResult = ValidateRequest(ModelState.IsValid, f, collections);
		if ( validationResult != null )
		{
			return validationResult;
		}

		var validateResults = await
			exifTimezoneCorrectionService.Validate(f,
				collections!.Value,
				request);

		await QueueCorrectionTask(validateResults, request, "timezone");

		return new JsonResult(validateResults);
	}

	/// <summary>
	///     Preview custom offset correction for batch of images (dry-run)
	///     Uses custom time/date offsets (years, months, days, hours, minutes, seconds)
	/// </summary>
	/// <param name="f">subPath filepath to file, split by dot comma (;)</param>
	/// <param name="collections">Include collections or not</param>
	/// <param name="request">Custom offset correction request with offset values</param>
	/// <returns>Preview of corrections without modifying files</returns>
	/// <response code="200">Preview results with corrections that would be applied</response>
	/// <response code="400">Invalid parameters or file paths</response>
	/// <response code="401">User unauthorized</response>
	[ProducesResponseType(200)]
	[ProducesResponseType(typeof(string), 400)]
	[HttpPost("/api/meta-time-correct/offset-preview")]
	[Produces("application/json")]
	public async Task<IActionResult> PreviewCustomOffsetCorrectionAsync(
		string f,
		bool? collections,
		[FromBody] ExifCustomOffsetCorrectionRequest request)
	{
		var validationResult = ValidateRequest(ModelState.IsValid, f, collections);
		if ( validationResult != null )
		{
			return validationResult;
		}

		var results = await
			exifTimezoneCorrectionService.Validate(f,
				collections!.Value,
				request);

		return Ok(results);
	}

	/// <summary>
	///     Execute custom offset correction for batch of images
	///     Uses custom time/date offsets (years, months, days, hours, minutes, seconds)
	/// </summary>
	/// <param name="f">subPath filepath to file, split by dot comma (;)</param>
	/// <param name="collections">Include collections or not</param>
	/// <param name="request">Custom offset correction request with offset values</param>
	/// <returns>Results of the correction execution</returns>
	/// <response code="200">Correction results with success/error status per file</response>
	/// <response code="400">Invalid parameters or file paths</response>
	/// <response code="401">User unauthorized</response>
	[ProducesResponseType(200)]
	[ProducesResponseType(typeof(string), 400)]
	[HttpPost("/api/meta-time-correct/offset-execute")]
	[Produces("application/json")]
	public async Task<IActionResult> ExecuteCustomOffsetCorrectionAsync(
		string f,
		bool? collections,
		[FromBody] ExifCustomOffsetCorrectionRequest request)
	{
		var validationResult = ValidateRequest(ModelState.IsValid, f, collections);
		if ( validationResult != null )
		{
			return validationResult;
		}

		var subPaths = PathHelper.SplitInputFilePaths(f);
		var validateResults = await
			exifTimezoneCorrectionService.Validate(subPaths,
				collections!.Value,
				request);

		await QueueCorrectionTask(validateResults, request, "custom offset");

		return new JsonResult(validateResults);
	}

	/// <summary>
	///     Validate common request parameters
	/// </summary>
	private BadRequestObjectResult? ValidateRequest(bool modelStateIsValid,
		string f, bool? collections)
	{
		if ( !modelStateIsValid || string.IsNullOrWhiteSpace(f) || collections == null )
		{
			return BadRequest(ModelNotValidError);
		}

		var subPaths = PathHelper.SplitInputFilePaths(f);
		return subPaths.Length == 0 ? BadRequest(NoInputFilesError) : null;
	}

	/// <summary>
	///     Queue background task for correction
	/// </summary>
	private async Task QueueCorrectionTask(
		List<ExifTimezoneCorrectionResult> validateResults,
		IExifTimeCorrectionRequest request,
		string correctionType)
	{
		await queue.QueueBackgroundWorkItemAsync(async _ =>
		{
			using var scope = scopeFactory.CreateScope();
			var scopedService =
				scope.ServiceProvider.GetRequiredService<IExifTimezoneCorrectionService>();

			var fileIndexResultsList = validateResults
				.Where(r => r.FileIndexItem != null)
				.Select(r => r.FileIndexItem!)
				.ToList();

			logger.LogInformation(
				$"[MetaTimeCorrectController] Starting {correctionType} correction for {fileIndexResultsList.Count} files");

			var results = await scopedService.CorrectTimezoneAsync(
				fileIndexResultsList,
				request);

			logger.LogInformation(
				$"[MetaTimeCorrectController] Completed {correctionType} correction: " +
				$"{results.Count(r => r.Success)} succeeded, {results.Count(r => !r.Success)} failed");

			await UpdateWebSocketTaskRun(fileIndexResultsList);
		});
	}

	private async Task UpdateWebSocketTaskRun(List<FileIndexItem> fileIndexResultsList)
	{
		// Update via websocket
		var webSocketResponse =
			new ApiNotificationResponseModel<List<FileIndexItem>>(fileIndexResultsList,
				ApiNotificationType.MetaTimeCorrect);

		var realtimeConnectionsService = scopeFactory.CreateScope()
			.ServiceProvider.GetRequiredService<IRealtimeConnectionsService>();

		await realtimeConnectionsService.NotificationToAllAsync(webSocketResponse,
			CancellationToken.None);
	}
}
