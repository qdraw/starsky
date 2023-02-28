using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using starsky.feature.trash.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;

namespace starsky.Controllers;

[Authorize]
public class TrashController : Controller
{
	private readonly IMoveToTrashService _moveToTrashService;

	public TrashController(IMoveToTrashService moveToTrashService)
	{
		_moveToTrashService = moveToTrashService;
	}

	/// <summary>
	/// Is the system trash supported
	/// </summary>
	/// <returns>bool with json (IActionResult Result)</returns>
	/// <response code="200">the item including the updated content</response>
	/// <response code="401">User unauthorized</response>
	[ProducesResponseType(typeof(bool), 200)]
	[HttpGet("/api/trash/detect-to-use-system-trash")]
	[Produces("application/json")]
	public IActionResult DetectToUseSystemTrash()
	{
		return Json(_moveToTrashService.DetectToUseSystemTrash());
	}
	
	/// <summary>
	/// (beta) Move a file to the trash
	/// </summary>
	/// <param name="f">subPath filepath to file, split by dot comma (;)</param>
	/// <param name="collections">stack collections</param>
	/// <returns>update json (IActionResult Update)</returns>
	/// <response code="200">the item including the updated content</response>
	/// <response code="400">parameter `f` is empty and that results in no input files</response>
	/// <response code="404">item not found in the database or on disk</response>
	/// <response code="401">User unauthorized</response>
	[ProducesResponseType(typeof(List<FileIndexItem>), 200)]
	[ProducesResponseType(typeof(List<FileIndexItem>), 404)]
	[ProducesResponseType(typeof(string), 400)]
	[HttpPost("/api/trash/move-to-trash")]
	[Produces("application/json")]
	public async Task<IActionResult> TrashMoveAsync(string f, bool collections = false)
	{
		var inputFilePaths = PathHelper.SplitInputFilePaths(f);
		if ( !inputFilePaths.Any() )
		{
			return BadRequest("No input files");
		}

		var fileIndexResultsList = await _moveToTrashService.MoveToTrashAsync(inputFilePaths, collections);
		
		return Json(fileIndexResultsList);
	}
}
