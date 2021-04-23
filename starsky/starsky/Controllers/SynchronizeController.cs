using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using starsky.foundation.database.Models;
using starsky.foundation.sync.SyncInterfaces;

namespace starsky.Controllers
{
	[Authorize]
	public class SynchronizeController : Controller
	{
		private readonly IManualBackgroundSyncService _manualBackgroundSyncService;

		public SynchronizeController(IManualBackgroundSyncService manualBackgroundSyncService)
		{
			_manualBackgroundSyncService = manualBackgroundSyncService;
		}

		/// <summary>
		/// Faster API to Check if directory is changed (not recursive)
		/// </summary>
		/// <param name="f">subPaths split by dot comma</param>
		/// <returns>list of changed files</returns>
		/// <response code="200">started sync as background job</response>
		/// <response code="401">User unauthorized</response>
		[HttpPost("/api/synchronize")]
		[HttpGet("/api/synchronize")] // < = = = = = = = = subject to change!
		[ProducesResponseType(typeof(string),200)]
		[ProducesResponseType(typeof(string),401)]
		[Produces("application/json")]	   
		public async Task<IActionResult> Index(string f)
		{
			var status = await _manualBackgroundSyncService.ManualSync(f);
			switch ( status )
			{
				case FileIndexItem.ExifStatus.NotFoundNotInIndex:
					return NotFound("Failed");
				case FileIndexItem.ExifStatus.OperationNotSupported:
					return BadRequest("Already started");
			}
			return Ok("Job created");
		}
	}
}
