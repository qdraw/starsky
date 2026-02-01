using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using starsky.feature.rename.Models;
using starsky.feature.rename.Services;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;

namespace starsky.Controllers;

[ApiController]
[Authorize]
[Route("api/batch-rename-datetime")]
public class BatchRenameDateTimeController : Controller
{
	private readonly FilenameDatetimeRepairService _filenameDatetimeRepairService;

	public BatchRenameDateTimeController(IQuery query,
		ISelectorStorage selectorStorage,
		IWebLogger logger)
	{
		var storage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);

		_filenameDatetimeRepairService = new FilenameDatetimeRepairService(query, storage, logger);
	}

	/// <summary>
	///     Preview filename datetime repair for files with datetime patterns in their names.
	///     Detects patterns like YYYYMMDD_HHMMSS and shows how they would be corrected.
	/// </summary>
	/// <param name="request">
	///     Request containing file paths, collections flag, and timezone/offset correction details
	/// </param>
	/// <returns>List of FilenameDatetimeRepairMapping preview results showing before/after filenames</returns>
	[HttpPost("preview")]
	public ActionResult<List<FilenameDatetimeRepairMapping>> PreviewDatetimeRepair(
		[FromBody] FilenameDatetimeRepairRequest request)
	{
		if ( request.CorrectionRequest == null )
		{
			return BadRequest("CorrectionRequest is required and must be valid");
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
	[HttpPost("execute")]
	public async Task<ActionResult<List<FileIndexItem>>> ExecuteDatetimeRepairAsync(
		[FromBody] FilenameDatetimeRepairRequest request)
	{
		if ( request.CorrectionRequest == null )
		{
			return BadRequest("CorrectionRequest is required and must be valid");
		}

		var mappings = _filenameDatetimeRepairService
			.PreviewRepair(request.FilePaths, request.CorrectionRequest, request.Collections);

		var result = await _filenameDatetimeRepairService
			.ExecuteRepairAsync(mappings, request.Collections);
		return Ok(result);
	}
}
