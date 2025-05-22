using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using starsky.foundation.database.Models;
using starsky.foundation.sync.SyncInterfaces;

namespace starsky.Controllers;

[Authorize]
public sealed class SynchronizeController(IManualBackgroundSyncService manualBackgroundSyncService)
	: Controller
{
	/// <summary>
	///     Faster API to Check if directory is changed (not recursive)
	/// </summary>
	/// <param name="f">subPaths split by dot comma</param>
	/// <returns>list of changed files</returns>
	/// <response code="200">started sync as background job</response>
	/// <response code="401">User unauthorized</response>
	[HttpPost("/api/synchronize")]
	[ProducesResponseType(typeof(string), 200)]
	[ProducesResponseType(typeof(string), 401)]
	[Produces("application/json")]
	public async Task<IActionResult> Index(string f)
	{
		if ( !ModelState.IsValid )
		{
			return BadRequest("Model invalid");
		}

		var status = await manualBackgroundSyncService.ManualSync(f);
		return status switch
		{
			FileIndexItem.ExifStatus.NotFoundNotInIndex => NotFound("Failed"),
			FileIndexItem.ExifStatus.OperationNotSupported => BadRequest("Already started"),
			_ => Ok("Job created")
		};
	}
}
