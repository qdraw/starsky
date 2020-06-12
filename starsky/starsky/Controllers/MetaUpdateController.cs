using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using starsky.feature.update.Interfaces;
using starsky.feature.update.Services;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;

namespace starsky.Controllers
{
	public class MetaUpdateController : Controller
	{
		private readonly IPreflightUpdate _preflightUpdate;

		public MetaUpdateController(IPreflightUpdate preflightUpdate)
		{
			_preflightUpdate = preflightUpdate;
		}
	    
	    /// <summary>
	    /// Update Exif and Rotation API
	    /// </summary>
	    /// <param name="f">subPath filepath to file, split by dot comma (;)</param>
	    /// <param name="inputModel">tags: use for keywords
	    /// colorClass: int 0-9, the colorClass to fast select images
	    /// description: string to update description/caption abstract, empty will be ignore
	    /// title: edit image title</param>
	    /// <param name="collections">StackCollections bool, default true</param>
	    /// <param name="append">only for stings, add update to existing items</param>
	    /// <param name="rotateClock">relative orientation -1 or 1</param>
	    /// <returns>update json</returns>
	    /// <response code="200">the item including the updated content</response>
	    /// <response code="404">item not found in the database or on disk</response>
	    /// <response code="401">User unauthorized</response>
	    [IgnoreAntiforgeryToken]
	    [ProducesResponseType(typeof(List<FileIndexItem>),200)]
	    [ProducesResponseType(typeof(List<FileIndexItem>),404)]
	    [HttpPost("/api/update")]
	    [Produces("application/json")]
	    public async Task<IActionResult> UpdateAsync(FileIndexItem inputModel, string f, bool append, bool collections = true)
	    {
		    var inputFilePaths = PathHelper.SplitInputFilePaths(f);

			
			// Per file stored key = string[fileHash] item => List <string> FileIndexItem.name (e.g. Tags) that are changed
			var changedFileIndexItemName = new Dictionary<string, List<string>>();

			var fileIndexResultsList =  _preflightUpdate.Preflight(inputModel, inputFilePaths, collections);
			
			
			// // Update >
			// _bgTaskQueue.QueueBackgroundWorkItem(async token =>
			// {
			// 	new UpdateService(_query,_exifTool, _readMeta,_iStorage,_thumbnailStorage)
			// 		.Update(changedFileIndexItemName,fileIndexResultsList,inputModel,collections, append);
			// });
            
            // When all items are not found
            if (fileIndexResultsList.All(p => p.Status != FileIndexItem.ExifStatus.Ok))
                return NotFound(fileIndexResultsList);

            // Clone an new item in the list to display
            var returnNewResultList = new List<FileIndexItem>();
            foreach ( var cloneItem in fileIndexResultsList.Select(item => item.Clone()) )
            {
	            cloneItem.FileHash = null;
	            returnNewResultList.Add(cloneItem);
            }
                        
            return Json(returnNewResultList);
	    }
	}
}
