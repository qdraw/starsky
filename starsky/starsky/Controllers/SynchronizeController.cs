using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.platform.JsonConverter;
using starsky.foundation.realtime.Interfaces;
using starsky.foundation.sync.SyncInterfaces;
using starsky.foundation.worker.Services;

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
		/// Experimental/Alpha API to sync data! Please use /api/sync 
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
