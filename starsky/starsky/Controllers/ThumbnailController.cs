using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using starsky.foundation.database.Interfaces;
using starsky.foundation.platform.Helpers;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starskycore.Helpers;

namespace starsky.Controllers
{
	[Authorize]
	public class ThumbnailController : Controller
	{
		private readonly IQuery _query;
		private readonly IStorage _iStorage;
		private readonly IStorage _thumbnailStorage;
		
		public ThumbnailController(IQuery query, ISelectorStorage selectorStorage)
		{
			_query = query;
			_iStorage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
			_thumbnailStorage = selectorStorage.Get(SelectorStorage.StorageServices.Thumbnail);
		}
		
        /// <summary>
        /// Http Endpoint to get full size image or thumbnail
        /// </summary>
        /// <param name="f">one single file</param>
        /// <param name="isSingleitem">true = load orginal</param>
        /// <param name="json">text as output</param>
        /// <returns>thumbnail or status (IActionResult Thumbnail)</returns>
        /// <response code="200">returns content of the file or when json is true, "OK"</response>
        /// <response code="404">item not found on disk</response>
        /// <response code="409">Conflict, you did try get for example a thumbnail of a raw file</response>
        /// <response code="209">"Thumbnail is not ready yet"</response>
        /// <response code="401">User unauthorized</response>
        [HttpGet("/api/thumbnail/{f}")]
        [ProducesResponseType(200)] // file
        [ProducesResponseType(404)] // not found
        [ProducesResponseType(409)] // raw
        [ProducesResponseType(209)] // "Thumbnail is not ready yet"
        [IgnoreAntiforgeryToken]
        [AllowAnonymous] // <=== ALLOW FROM EVERYWHERE
        [ResponseCache(Duration = 29030400)] // 4 weeks
        public async Task<IActionResult> Thumbnail(
            string f, 
            bool isSingleitem = false, 
            bool json = false)
        {
            // f is Hash
            // isSingleItem => detailView
            // Retry thumbnail => is when you press reset thumbnail
            // json, => to don't waste the users bandwidth.

	        // For serving jpeg files
	        f = FilenamesHelper.GetFileNameWithoutExtension(f);
	        
	        // Restrict the fileHash to letters and digits only
	        // I/O function calls should not be vulnerable to path injection attacks
	        if (!Regex.IsMatch(f, "^[a-zA-Z0-9_-]+$") )
	        {
		        return BadRequest();
	        }
	        
            if (_thumbnailStorage.ExistFile(f))
            {
                // When a file is corrupt show error
                var stream = _thumbnailStorage.ReadStream(f,50);
                var imageFormat = ExtensionRolesHelper.GetImageFormat(stream);
                if ( imageFormat == ExtensionRolesHelper.ImageFormat.unknown )
                {
	                SetExpiresResponseHeadersToZero();
	                return NoContent(); // 204
                }

                // When using the api to check using javascript
                // use the cached version of imageFormat, otherwise you have to check if it deleted
                if (json) return Json("OK");

                // thumbs are always in jpeg
                stream = _thumbnailStorage.ReadStream(f);
                Response.Headers.Add("x-filename", FilenamesHelper.GetFileName(f + ".jpg"));
                return File(stream, "image/jpeg", f + ".jpg");
            }
            
            // Cached view of item
            var sourcePath = _query.GetSubPathByHash(f);
            if (sourcePath == null) return NotFound("not in index");
            
	        // Need to check again for recently moved files
	        if (!_iStorage.ExistFile(sourcePath))
	        {
		        // remove from cache
		        _query.ResetItemByHash(f);
		        // query database again
		        sourcePath = _query.GetSubPathByHash(f);
		        if (sourcePath == null) return NotFound("not in index");
	        }

	        if ( !_iStorage.ExistFile(sourcePath) )
		        return NotFound("There is no thumbnail image " + f + " and no source image " +
		                        sourcePath);
	        
	        if (!isSingleitem)
	        {
		        // "Photo exist in database but " + "isSingleItem flag is Missing"
		        SetExpiresResponseHeadersToZero();
		        Response.StatusCode = 202; // A conflict, that the thumb is not generated yet
		        return Json("Thumbnail is not ready yet");
	        }
                
	        if (ExtensionRolesHelper.IsExtensionThumbnailSupported(sourcePath))
	        {
		        var fs1 = _iStorage.ReadStream(sourcePath);

		        var fileExt = FilenamesHelper.GetFileExtensionWithoutDot(sourcePath);
		        Response.Headers.Add("x-filename", FilenamesHelper.GetFileName(sourcePath));
		        return File(fs1, MimeHelper.GetMimeType(fileExt));
	        }
	        Response.StatusCode = 409; // A conflict, that the thumb is not generated yet
	        return Json("Thumbnail is not supported; for example you try to view a raw file");

	        // When you have duplicate files and one of them is removed and there is no thumbnail
            // generated yet you might get an false error
        }
        
        /// <summary>
        /// Force Http context to no browser cache
        /// </summary>
        public void SetExpiresResponseHeadersToZero()
        {
	        Request.HttpContext.Response.Headers.Remove("Cache-Control");
	        Request.HttpContext.Response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate");

	        Request.HttpContext.Response.Headers.Remove("Pragma");
	        Request.HttpContext.Response.Headers.Add("Pragma", "no-cache");

	        Request.HttpContext.Response.Headers.Remove("Expires");
	        Request.HttpContext.Response.Headers.Add("Expires", "0");
        }
	}
}
