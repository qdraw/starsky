using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using starsky.feature.metaupdate.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;

namespace starsky.Controllers
{
	[Authorize]
	public class MetaInfoController : Controller
	{
		private readonly IMetaInfo _metaInfo;

		public MetaInfoController(IMetaInfo metaInfo)
		{
			_metaInfo = metaInfo;
		}
		
		/// <summary>
        /// Get realtime (cached a few minutes) about the file
        /// </summary>
        /// <param name="f">subPaths split by dot comma</param>
        /// <param name="collections">true is to update files with the same name before the extenstion</param>
        /// <returns>info of object</returns>
        /// <response code="200">the item on disk</response>
        /// <response code="404">item not found on disk</response>
        /// <response code="203">you are not allowed to edit this item</response>
        /// <response code="401">User unauthorized</response>
        [HttpGet("/api/info")]
        [ProducesResponseType(typeof(List<FileIndexItem>),200)]
        [ProducesResponseType(typeof(List<FileIndexItem>),404)]
        [ProducesResponseType(typeof(List<FileIndexItem>),203)]
        [Produces("application/json")]
        public IActionResult Info(string f, bool collections = true)
        {
            var inputFilePaths = PathHelper.SplitInputFilePaths(f).ToList();

            var fileIndexResultsList = _metaInfo.GetInfo(inputFilePaths, collections);
            
            // returns read only
            if (fileIndexResultsList.All(p => p.Status == FileIndexItem.ExifStatus.ReadOnly))
            {
                Response.StatusCode = 203; // is readonly
                return Json(fileIndexResultsList);
            }
                
            // When all items are not found
            if (fileIndexResultsList.All(p => (p.Status != FileIndexItem.ExifStatus.Ok && p.Status != FileIndexItem.ExifStatus.Deleted)))
                return NotFound(fileIndexResultsList);
            
            return Json(fileIndexResultsList);
        }

	}
}


