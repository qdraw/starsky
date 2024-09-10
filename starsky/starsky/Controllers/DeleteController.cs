using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using starsky.feature.metaupdate.Interfaces;
using starsky.foundation.database.Models;

namespace starsky.Controllers;

[Authorize]
public sealed class DeleteController : Controller
{
	private readonly IDeleteItem _deleteItem;

	public DeleteController(IDeleteItem deleteItem)
	{
		_deleteItem = deleteItem;
	}

	/// <summary>
	///     Remove files from the disk, but the file must contain the !delete!
	///     (TrashKeyword.TrashKeywordString) tag
	/// </summary>
	/// <param name="f">subPaths, separated by dot comma</param>
	/// <param name="collections">true is to update files with the same name before the extenstion</param>
	/// <returns>list of deleted files</returns>
	/// <response code="200">file is gone</response>
	/// <response code="404">
	///     item not found on disk or !delete! (TrashKeyword.TrashKeywordString) tag is
	///     missing
	/// </response>
	/// <response code="401">User unauthorized</response>
	[HttpDelete("/api/delete")]
	[ProducesResponseType(typeof(List<FileIndexItem>), 200)]
	[ProducesResponseType(typeof(List<FileIndexItem>), 404)]
	[Produces("application/json")]
	public async Task<IActionResult> Delete(string f, bool collections = false)
	{
		if ( !ModelState.IsValid )
		{
			return BadRequest("ModelState is not valid");
		}

		var fileIndexResultsList = await _deleteItem.DeleteAsync(f, collections);
		// When all items are not found
		// ok = file is deleted
		if ( fileIndexResultsList.TrueForAll(p =>
			    p.Status != FileIndexItem.ExifStatus.Ok) )
		{
			return NotFound(fileIndexResultsList);
		}


		return Json(fileIndexResultsList);
	}
}
