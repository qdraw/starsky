using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using starsky.feature.metaupdate.Interfaces;
using starsky.foundation.database.Models;

namespace starsky.Controllers
{
	public class DeleteController : Controller
	{
		private readonly IDeleteItem _deleteItem;

		public DeleteController(IDeleteItem deleteItem)
		{
			_deleteItem = deleteItem;
		}
		
		/// <summary>
        /// Remove files from the disk, but the file must contain the !delete! tag
        /// </summary>
        /// <param name="f">subPaths, separated by dot comma</param>
        /// <param name="collections">true is to update files with the same name before the extenstion,
        /// not recommend to use</param>
        /// <returns>list of deleted files</returns>
        /// <response code="200">file is gone</response>
        /// <response code="404">item not found on disk or !delete! tag is missing</response>
        /// <response code="401">User unauthorized</response>
        [HttpDelete("/api/delete")]
        [ProducesResponseType(typeof(List<FileIndexItem>),200)]
        [ProducesResponseType(typeof(List<FileIndexItem>),404)]
        [Produces("application/json")]
        public IActionResult Delete(string f, bool collections = false)
		{
			var fileIndexResultsList = _deleteItem.Delete(f, collections);
            // When all items are not found
	        // ok = file is deleted
            if (fileIndexResultsList.All(p => p.Status != FileIndexItem.ExifStatus.Ok))
                return NotFound(fileIndexResultsList);
     
            return Json(fileIndexResultsList);
        }
	}
}
