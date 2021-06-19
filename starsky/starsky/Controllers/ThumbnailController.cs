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
		/// Get thumbnail for index pages (300 px)
		/// </summary>
		/// <param name="f">one single file</param>
		/// <returns>thumbnail or status (IActionResult ThumbnailFromIndex)</returns>
		/// <response code="200">returns content of the file</response>
		/// <response code="400">string (f) input not allowed to avoid path injection attacks</response>
		/// <response code="404">item not found on disk</response>
		/// <response code="401">User unauthorized</response>
		[HttpGet("/api/thumbnail/index/{f}")]
		[ProducesResponseType(200)] // file
		[ProducesResponseType(
			400)] // string (f) input not allowed to avoid path injection attacks
		[ProducesResponseType(404)] // not found
		[IgnoreAntiforgeryToken]
		[AllowAnonymous] // <=== ALLOW FROM EVERYWHERE
		[ResponseCache(Duration = 29030400)] // 4 weeks
		public IActionResult ThumbnailFromIndex(string f)
		{
			f = FilenamesHelper.GetFileNameWithoutExtension(f);
			
			// Restrict the fileHash to letters and digits only
			// I/O function calls should not be vulnerable to path injection attacks
			if (!Regex.IsMatch(f, "^[a-zA-Z0-9_-]+$") )
			{
				return BadRequest();
			}
			
			if ( _thumbnailStorage.ExistFile(f + "@300") )
			{
				var stream = _thumbnailStorage.ReadStream(f+ "@300");
				return File(stream, "image/jpeg");
			}

			if ( _thumbnailStorage.ExistFile(ThumbnailNameHelper.Combine(f,ThumbnailSize.TinyMeta) )  )
			{
				var stream = _thumbnailStorage.ReadStream(ThumbnailNameHelper.Combine(f,ThumbnailSize.TinyMeta));
				return File(stream, "image/jpeg");
			}

			if ( !_thumbnailStorage.ExistFile(f) )
			{
				return NotFound("hash not found");
			}

			var streamDefaultThumbnail = _thumbnailStorage.ReadStream(f);
			return File(streamDefaultThumbnail, "image/jpeg");
		}

		/// <summary>
        /// Get thumbnail with fallback to original source image.
        /// Return source image when IsExtensionThumbnailSupported is true
        /// </summary>
        /// <param name="f">one single file</param>
        /// <param name="isSingleItem">true = load original</param>
        /// <param name="json">text as output</param>
        /// <returns>thumbnail or status (IActionResult Thumbnail)</returns>
        /// <response code="200">returns content of the file or when json is true, "OK"</response>
        /// <response code="204">thumbnail is corrupt</response>
        /// <response code="400">string (f) input not allowed to avoid path injection attacks</response>
        /// <response code="404">item not found on disk</response>
        /// <response code="210">Conflict, you did try get for example a thumbnail of a raw file</response>
        /// <response code="209">"Thumbnail is not ready yet"</response>
        /// <response code="401">User unauthorized</response>
        [HttpGet("/api/thumbnail/{f}")]
        [ProducesResponseType(200)] // file
        [ProducesResponseType(204)] // thumbnail is corrupt
		[ProducesResponseType(400)] // string (f) input not allowed to avoid path injection attacks
        [ProducesResponseType(404)] // not found
        [ProducesResponseType(210)] // raw
        [ProducesResponseType(209)] // "Thumbnail is not ready yet"
        [IgnoreAntiforgeryToken]
        [AllowAnonymous] // <=== ALLOW FROM EVERYWHERE
        [ResponseCache(Duration = 29030400)] // 4 weeks
        public async Task<IActionResult> Thumbnail(
            string f, 
            bool isSingleItem = false, 
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

	        IActionResult ReturnResult(ThumbnailSize size)
	        {
		        // When using the api to check using javascript
		        // use the cached version of imageFormat, otherwise you have to check if it deleted
		        if (json) return Json("OK");

		        // thumbs are always in jpeg
		        var stream = _thumbnailStorage.ReadStream(ThumbnailNameHelper.Combine(f,size));
		        Response.Headers.Add("x-filename", FilenamesHelper.GetFileName(f + ".jpg"));
		        return File(stream, "image/jpeg");
	        }
	        
            if (_thumbnailStorage.ExistFile(ThumbnailNameHelper.Combine(f, ThumbnailSize.ExtraLarge)))
            {
                // When a file is corrupt show error
                var stream = _thumbnailStorage.ReadStream(ThumbnailNameHelper.Combine(f, ThumbnailSize.ExtraLarge),50);
                var imageFormat = ExtensionRolesHelper.GetImageFormat(stream);
                if ( imageFormat == ExtensionRolesHelper.ImageFormat.unknown )
                {
	                SetExpiresResponseHeadersToZero();
	                return NoContent(); // 204
                }
                return ReturnResult(ThumbnailSize.ExtraLarge);
            }

            if ( _thumbnailStorage.ExistFile(ThumbnailNameHelper.Combine(f, ThumbnailSize.Large)) )
            {
	            return ReturnResult(ThumbnailSize.Large);
            }

            // Cached view of item
            var sourcePath = _query.GetSubPathByHash(f);
            if ( sourcePath == null )
            {
	            SetExpiresResponseHeadersToZero();
	            return NotFound("not in index");
            }
            
	        // Need to check again for recently moved files
	        if (!_iStorage.ExistFile(sourcePath))
	        {
		        // remove from cache
		        _query.ResetItemByHash(f);
		        // query database again
		        sourcePath = _query.GetSubPathByHash(f);
		        SetExpiresResponseHeadersToZero();
		        if (sourcePath == null) return NotFound("not in index");
	        }

	        if ( !_iStorage.ExistFile(sourcePath) )
		        return NotFound("There is no thumbnail image " + f + " and no source image " +
		                        sourcePath);
	        
	        if (!isSingleItem)
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
	        
	        Response.StatusCode = 210; // A conflict, that the thumb is not generated yet
	        return Json("Thumbnail is not supported; for example you try to view a raw file");
        }

        /// <summary>
        /// Get zoomed in image by fileHash.
        /// At the moment this is the source image
        /// </summary>
        /// <param name="f">one single file</param>
        /// <param name="z">zoom factor? </param>
        /// <returns>Image</returns>
        /// <response code="200">returns content of the file or when json is true, "OK"</response>
        /// <response code="400">string (f) input not allowed to avoid path injection attacks</response>
        /// <response code="404">item not found on disk</response>
        /// <response code="210">Conflict, you did try get for example a thumbnail of a raw file</response>
        /// <response code="401">User unauthorized</response>
        [HttpGet("/api/thumbnail/zoom/{f}@{z}")]
        [ProducesResponseType(200)] // file
        [ProducesResponseType(400)] // string (f) input not allowed to avoid path injection attacks
        [ProducesResponseType(404)] // not found
        [ProducesResponseType(210)] // raw
        public async Task<IActionResult> ByZoomFactor(
	        string f,
	        int z = 0)
        {
	        // For serving jpeg files
	        f = FilenamesHelper.GetFileNameWithoutExtension(f);
	        
	        // Restrict the fileHash to letters and digits only
	        // I/O function calls should not be vulnerable to path injection attacks
	        if (!Regex.IsMatch(f, "^[a-zA-Z0-9_-]+$") )
	        {
		        return BadRequest();
	        }
	        
	        // Cached view of item
	        var sourcePath = _query.GetSubPathByHash(f);
	        if (sourcePath == null) return NotFound("not in index");
	        
	        if (ExtensionRolesHelper.IsExtensionThumbnailSupported(sourcePath))
	        {
		        var fs1 = _iStorage.ReadStream(sourcePath);

		        var fileExt = FilenamesHelper.GetFileExtensionWithoutDot(sourcePath);
		        Response.Headers.Add("x-filename", FilenamesHelper.GetFileName(sourcePath));
		        return File(fs1, MimeHelper.GetMimeType(fileExt));
	        }
	        
	        Response.StatusCode = 210; // A conflict, that the thumb is not generated yet
	        return Json("Thumbnail is not supported; for example you try to view a raw file");
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
