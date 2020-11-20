using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.sync.SyncInterfaces;

namespace starsky.Controllers
{
	[Authorize]
	public class SynchronizeController : Controller
	{
		private readonly ISynchronize _synchronize;
		private readonly IQuery _query;

		public SynchronizeController(ISynchronize synchronize, IQuery query )
		{
			_synchronize = synchronize;
			_query = query;
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
			var fileIndexItem = _query.SingleItem(f)?.FileIndexItem;
			if ( fileIndexItem == null )
			{
				return NotFound(new List<FileIndexItem>
				{
					new FileIndexItem(f)
					{
						Status = FileIndexItem.ExifStatus.NotFoundNotInIndex
					}
				});
			}
			return Ok(await _synchronize.Sync(fileIndexItem.FilePath));
		}
	}
}
