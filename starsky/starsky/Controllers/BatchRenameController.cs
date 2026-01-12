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
[Route("api/[controller]")]
public class BatchRenameController : ControllerBase
{
	private readonly RenameService _renameService;

	public BatchRenameController(IQuery query, 
		ISelectorStorage selectorStorage, 
		IWebLogger logger)
	{
		var storage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
		_renameService = new RenameService(query, storage, logger);
	}

	/// <summary>
	///     Preview batch rename mappings for a list of file paths and a rename pattern.
	/// </summary>
	/// <param name="filePaths">List of file paths to preview rename for.</param>
	/// <param name="pattern">Rename pattern string
	/// (e.g. {yyyy}{MM}{dd}_{filenamebase}{seqn}.{ext}).</param>
	/// <returns>List of BatchRenameMapping preview results.</returns>
	[HttpPost("preview")]
	public ActionResult<List<BatchRenameMapping>> PreviewBatchRename(
		[FromBody] BatchRenameRequest request)
	{
		if ( string.IsNullOrEmpty(request.Pattern) )
		{
			return BadRequest("Invalid request");
		}

		var result = _renameService
			.PreviewBatchRename(request.FilePaths, request.Pattern);
		return Ok(result);
	}

	/// <summary>
	///     Execute batch rename for a list of mappings.
	/// </summary>
	/// <param name="request">Request containing mappings and collections flag.</param>
	/// <returns>List of FileIndexItem results.</returns>
	[HttpPost("execute")]
	public async Task<ActionResult<List<FileIndexItem>>> ExecuteBatchRenameAsync(
		[FromBody] BatchRenameRequest request)
	{
		var mappings = _renameService
			.PreviewBatchRename(request.FilePaths, request.Pattern);
		var result = await _renameService
			.ExecuteBatchRenameAsync(mappings, request.Collections);
		return Ok(result);
	}
}
