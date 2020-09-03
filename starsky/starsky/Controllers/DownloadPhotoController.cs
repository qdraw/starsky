using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starsky.foundation.thumbnailgeneration.Services;
using starsky.Helpers;
using starskycore.Helpers;

namespace starsky.Controllers
{
	[Authorize]
	public class DownloadPhotoController : Controller
	{
		private readonly IQuery _query;
		private readonly IStorage _iStorage;
		private readonly IStorage _thumbnailStorage;
		
		public DownloadPhotoController(IQuery query, ISelectorStorage selectorStorage)
		{
			_query = query;
			_iStorage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
			_thumbnailStorage = selectorStorage.Get(SelectorStorage.StorageServices.Thumbnail);
		}
		
		/// <summary>
        /// Select manualy the orginal or thumbnail
        /// </summary>
        /// <param name="f">string, 'sub path' to find the file</param>
        /// <param name="isThumbnail">true = 1000px thumb (if supported)</param>
        /// <param name="cache">true = send client headers to cache</param>
        /// <returns>FileStream with image</returns>
        /// <response code="200">returns content of the file or when json is true, "OK"</response>
        /// <response code="404">source image missing</response>
        /// <response code="500">"Thumbnail generation failed"</response>
        /// <response code="401">User unauthorized</response>
        [HttpGet("/api/downloadPhoto")]
        [ProducesResponseType(200)] // file
        [ProducesResponseType(404)] // not found
        [ProducesResponseType(500)] // "Thumbnail generation failed"
        public async Task<IActionResult> DownloadPhoto(string f, bool isThumbnail = true, bool cache = true)
        {
            // f = subpath/filepath
            if (f.Contains("?isthumbnail")) return NotFound("please use &isthumbnail = "+
                                                            "instead of ?isthumbnail= ");

            var singleItem = _query.SingleItem(f);
            if (singleItem == null) return NotFound("not in index " + f);

            if (!_iStorage.ExistFile(singleItem.FileIndexItem.FilePath))
                return NotFound($"source image missing {singleItem.FileIndexItem.FilePath}" );

            // Return full image
            if (!isThumbnail)
            {
	            if ( cache ) CacheControlOverwrite.SetExpiresResponseHeaders(Request);
	            var fs = _iStorage.ReadStream(singleItem.FileIndexItem.FilePath);
                // Return the right mime type (enableRangeProcessing = needed for safari and mp4)
                return File(fs, MimeHelper.GetMimeTypeByFileName(singleItem.FileIndexItem.FilePath),true);
            }

            if (!_thumbnailStorage.ExistFolder("/"))
            {
	            return NotFound("ThumbnailTempFolder not found");
            }
            
            // Return Thumbnail
            var existThumbnailFile = _thumbnailStorage.ExistFile(singleItem.FileIndexItem.FileHash);

            if (!existThumbnailFile)
            {
                var searchItem = new FileIndexItem(singleItem.FileIndexItem.FilePath)
                {
	                FileHash =
		                singleItem.FileIndexItem
			                .FileHash // not loading it from disk to make it faster
                };
                
                var isCreateAThumb = new Thumbnail(_iStorage,_thumbnailStorage).CreateThumb(searchItem.FilePath, searchItem.FileHash);
                if (!isCreateAThumb)
                {
                    Response.StatusCode = 500;
                    return Json("Thumbnail generation failed");
                }
            }
            
            var thumbnailFs = _thumbnailStorage.ReadStream(singleItem.FileIndexItem.FileHash);
            return File(thumbnailFs, "image/jpeg");
        }
	}
}
