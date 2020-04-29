using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;

namespace starsky.Controllers
{
	[Authorize]
	public class ImportHistoryController : Controller
	{
		private readonly IImportQuery _importQuery;

		public ImportHistoryController(IImportQuery importQuery)
		{
			_importQuery = importQuery;
		}
		
		/// <summary>
		/// Today's imported files
		/// </summary>
		/// <returns>list of files</returns>
		/// <response code="200">done</response>
		[HttpGet("/api/import/history")]
		[ProducesResponseType(typeof(List<ImportIndexItem>),200)] // yes
		[Produces("application/json")]
		public IActionResult History()
		{
			return Json(_importQuery.History());
		}

	}
}
