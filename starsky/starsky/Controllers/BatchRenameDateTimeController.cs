using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using starsky.feature.rename.DateTimeRepair.Services;
using starsky.feature.rename.Models;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.metaupdate.Models;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;

namespace starsky.Controllers;

[ApiController]
[Authorize]
[Route("api/batch-rename-datetime")]
public class BatchRenameDateTimeController : ControllerBase
{
	private const string ModelNotValidError = "Model is not valid";
	private readonly FilenameDatetimeRepairService _filenameDatetimeRepairService;

	public BatchRenameDateTimeController(IQuery query,
		ISelectorStorage selectorStorage,
		IWebLogger logger, AppSettings appSettings)
	{
		var storage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);

		_filenameDatetimeRepairService =
			new FilenameDatetimeRepairService(query, storage, logger, appSettings);
	}

	/// <summary>
	///     Preview filename datetime repair for files with datetime patterns in their names.
	///     Detects patterns like YYYYMMDD_HHMMSS and shows how they would be corrected.
	/// </summary>
	/// <param name="request">
	///     Request containing file paths, collections flag, and timezone/offset correction details
	/// </param>
	/// <returns>List of FilenameDatetimeRepairMapping preview results showing before/after filenames</returns>
	[HttpPost("offset-preview")]
	public ActionResult<List<FilenameDatetimeRepairMapping>> PreviewCustomOffsetDatetimeRepair(
		[FromBody] FilenameDatetimeRepairRequest<ExifCustomOffsetCorrectionRequest> request)
	{
		if ( !ModelState.IsValid || request.CorrectionRequest == null! )
		{
			return BadRequest(ModelNotValidError);
		}

		var result = _filenameDatetimeRepairService
			.PreviewRepair(request.FilePaths, request.CorrectionRequest, request.Collections);
		return Ok(result);
	}

	/// <summary>
	///     Execute filename datetime repair to correct datetime patterns in filenames.
	///     Renames files to match corrected timestamps (e.g., fixing timezone offsets in filenames).
	/// </summary>
	/// <param name="request">
	///     Request containing file paths, collections flag, and timezone/offset correction details
	/// </param>
	/// <returns>List of FileIndexItem results with updated file paths</returns>
	[HttpPost("offset-execute")]
	public async Task<ActionResult<List<FileIndexItem>>> ExecuteCustomOffsetDatetimeRepairAsync(
		[FromBody] FilenameDatetimeRepairRequest<ExifCustomOffsetCorrectionRequest> request)
	{
		if ( !ModelState.IsValid || request.CorrectionRequest == null! )
		{
			return BadRequest(ModelNotValidError);
		}

		var result = await _filenameDatetimeRepairService
			.ExecuteRepairAsync(request.FilePaths, request.CorrectionRequest, request.Collections);

		return Ok(result);
	}

	/// <summary>
	///     Preview filename datetime repair for files with datetime patterns in their names.
	///     Detects patterns like YYYYMMDD_HHMMSS and shows how they would be corrected.
	/// </summary>
	/// <param name="request">
	///     Request containing file paths, collections flag, and timezone/offset correction details
	/// </param>
	/// <returns>List of FilenameDatetimeRepairMapping preview results showing before/after filenames</returns>
	[HttpPost("timezone-preview")]
	public ActionResult<List<FilenameDatetimeRepairMapping>> PreviewTimezoneDatetimeRepair(
		[FromBody] FilenameDatetimeRepairRequest<ExifTimezoneBasedCorrectionRequest> request)
	{
		if ( !ModelState.IsValid || request.CorrectionRequest == null! )
		{
			return BadRequest(ModelNotValidError);
		}

		var result = _filenameDatetimeRepairService
			.PreviewRepair(request.FilePaths, request.CorrectionRequest, request.Collections);
		return Ok(result);
	}

	/// <summary>
	///     Execute filename datetime repair to correct datetime patterns in filenames.
	///     Renames files to match corrected timestamps (e.g., fixing timezone offsets in filenames).
	/// </summary>
	/// <param name="request">
	///     Request containing file paths, collections flag, and timezone/offset correction details
	/// </param>
	/// <returns>List of FileIndexItem results with updated file paths</returns>
	[HttpPost("timezone-execute")]
	public async Task<ActionResult<List<FileIndexItem>>> ExecuteTimezoneDatetimeRepairAsync(
		[FromBody] FilenameDatetimeRepairRequest<ExifTimezoneBasedCorrectionRequest> request)
	{
		if ( !ModelState.IsValid || request.CorrectionRequest == null! )
		{
			return BadRequest(ModelNotValidError);
		}

		var result = await _filenameDatetimeRepairService
			.ExecuteRepairAsync(request.FilePaths, request.CorrectionRequest, request.Collections);

		return Ok(result);
	}
}
