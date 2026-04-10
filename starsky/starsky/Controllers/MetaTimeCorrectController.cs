using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using starsky.foundation.metaupdate.Interfaces;
using starsky.foundation.metaupdate.Models;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;

namespace starsky.Controllers;

/// <summary>
///     Controller for EXIF timezone correction operations
///     Supports both timezone-based and custom offset corrections
/// </summary>
[Authorize]
public class MetaTimeCorrectController(
	IExifTimezoneCorrectionService exifTimezoneCorrectionService,
	IWebLogger logger)
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
			logger.LogDebug("Validation Result: {0}", validationResult);
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

		await exifTimezoneCorrectionService.QueueCorrectionTask(validateResults, request,
			"timezone");

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

		await exifTimezoneCorrectionService.QueueCorrectionTask(validateResults, request,
			"custom offset");

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
}
