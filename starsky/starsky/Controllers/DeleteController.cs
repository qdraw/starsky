using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using starsky.foundation.database.Helpers;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;

namespace starsky.Controllers
{
	public class DeleteController : Controller
	{
		private readonly IQuery _query;
		private readonly AppSettings _appSettings;
		private readonly IStorage _iStorage;
		private readonly IStorage _thumbnailStorage;

		public DeleteController(IQuery query, AppSettings appSettings, ISelectorStorage selectorStorage)
		{
			_query = query;
			_appSettings = appSettings;
			_iStorage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
			_thumbnailStorage = selectorStorage.Get(SelectorStorage.StorageServices.Thumbnail);
		}
		
		/// <summary>
        /// Remove files from the disk, but the file must contain the !delete! tag
        /// </summary>
        /// <param name="f">subPaths, separated by dot comma</param>
        /// <param name="collections">true is to update files with the same name before the extenstion</param>
        /// <returns>list of deleted files</returns>
        /// <response code="200">file is gone</response>
        /// <response code="404">item not found on disk or !delete! tag is missing</response>
        /// <response code="401">User unauthorized</response>
        [HttpDelete("/api/delete")]
        [ProducesResponseType(typeof(List<FileIndexItem>),200)]
        [ProducesResponseType(typeof(List<FileIndexItem>),404)]
        [Produces("application/json")]
        public IActionResult Delete(string f, bool collections = true)
        {
            var inputFilePaths = PathHelper.SplitInputFilePaths(f);
            // the result list
            var fileIndexResultsList = new List<FileIndexItem>();
     
            foreach (var subPath in inputFilePaths)
            {
                var detailView = _query.SingleItem(subPath, null, collections, false);
                
                // todo check if file exist
                
                // todo remove directories
                
                // Dir is readonly / don't edit
                if ( new StatusCodesHelper(_appSettings).IsReadOnlyStatus(detailView) 
                     == FileIndexItem.ExifStatus.ReadOnly)
                {
	                new StatusCodesHelper().ReturnExifStatusError(detailView.FileIndexItem, 
		                FileIndexItem.ExifStatus.ReadOnly,
		                fileIndexResultsList);
	                continue; 
                }
				
                // Deleted is allowed but the status need be updated
                if ( new StatusCodesHelper(_appSettings).IsDeletedStatus(detailView) 
                     == FileIndexItem.ExifStatus.Deleted)
                {
	                new StatusCodesHelper().ReturnExifStatusError(detailView.FileIndexItem, 
		                FileIndexItem.ExifStatus.ReadOnly,
		                fileIndexResultsList);
                }

                var collectionSubPathList = detailView.GetCollectionSubPathList(detailView, collections, subPath);
     
                // display the to delete items
                for (int i = 0; i < collectionSubPathList.Count; i++)
                {
                    var collectionSubPath = collectionSubPathList[i];
                    var detailViewItem = _query.SingleItem(collectionSubPath, null, collections, false);
     
                    // Allow only files that contains the delete tag
                    if (!detailViewItem.FileIndexItem.Tags.Contains("!delete!"))
                    {
                        detailViewItem.FileIndexItem.Status = FileIndexItem.ExifStatus.Unauthorized;
                        fileIndexResultsList.Add(detailViewItem.FileIndexItem.Clone());
                        continue;
                    }
	                // return a Ok, which means the file is deleted
	                detailViewItem.FileIndexItem.Status = FileIndexItem.ExifStatus.Ok;
     
					// remove thumbnail from disk
					_thumbnailStorage.FileDelete(detailViewItem.FileIndexItem.FileHash);
     
                    fileIndexResultsList.Add(detailViewItem.FileIndexItem.Clone());
	                
                    // remove item from db
                    _query.RemoveItem(detailViewItem.FileIndexItem);
     
	                // remove the sidecar file (if exist)
	                if ( ExtensionRolesHelper.IsExtensionForceXmp(detailViewItem.FileIndexItem
		                .FileName) )
	                {
		                _iStorage.FileDelete(
			                ExtensionRolesHelper.ReplaceExtensionWithXmp(detailViewItem
				                .FileIndexItem.FilePath));
	                }
	                
	                // todo remove .json file
	                
	                // and remove the actual file
	                _iStorage.FileDelete(detailViewItem.FileIndexItem.FilePath);
                }
            }
            
            // When all items are not found
	        // ok = file is deleted
            if (fileIndexResultsList.All(p => p.Status != FileIndexItem.ExifStatus.Ok))
                return NotFound(fileIndexResultsList);
     
            return Json(fileIndexResultsList);
        }
	}
}
