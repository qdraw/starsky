using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
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
		/// Get thumbnail for index pages (300 px or 150px or 1000px (based on whats there))
		/// </summary>
		/// <param name="f">one single fileHash (NOT path)</param>
		/// <returns>thumbnail or status (IActionResult ThumbnailFromIndex)</returns>
		/// <response code="200">returns content of the file</response>
		/// <response code="400">string (f) input not allowed to avoid path injection attacks</response>
		/// <response code="404">item not found on disk</response>
		/// <response code="401">User unauthorized</response>
		[HttpGet("/api/thumbnail/small/{f}")]
		[ProducesResponseType(200)] // file
		[ProducesResponseType(400)] // string (f) input not allowed to avoid path injection attacks
		[ProducesResponseType(404)] // not found
		[AllowAnonymous] // <=== ALLOW FROM EVERYWHERE
		[ResponseCache(Duration = 29030400)] // 4 weeks
		public IActionResult ThumbnailSmallOrTinyMeta(string f)
		{
			f = FilenamesHelper.GetFileNameWithoutExtension(f);
			
			// Restrict the fileHash to letters and digits only
			// I/O function calls should not be vulnerable to path injection attacks
			if (!Regex.IsMatch(f, "^[a-zA-Z0-9_-]+$") )
			{
				return BadRequest();
			}
			
			if ( _thumbnailStorage.ExistFile(ThumbnailNameHelper.Combine(f,ThumbnailSize.Small)) )
			{
				var stream = _thumbnailStorage.ReadStream(ThumbnailNameHelper.Combine(f,ThumbnailSize.Small) );
				Response.Headers.TryAdd("x-image-size", new StringValues(ThumbnailSize.Small.ToString()));
				return File(stream, "image/jpeg");
			}

			if ( _thumbnailStorage.ExistFile(ThumbnailNameHelper.Combine(f,ThumbnailSize.TinyMeta) )  )
			{
				var stream = _thumbnailStorage.ReadStream(ThumbnailNameHelper.Combine(f,ThumbnailSize.TinyMeta));
				Response.Headers.TryAdd("x-image-size", new StringValues(ThumbnailSize.TinyMeta.ToString()));
				return File(stream, "image/jpeg");
			}

			if ( !_thumbnailStorage.ExistFile(ThumbnailNameHelper.Combine(f,ThumbnailSize.Large)) )
			{
				return NotFound("hash not found");
			}

			var streamDefaultThumbnail = _thumbnailStorage.ReadStream(ThumbnailNameHelper.Combine(f,ThumbnailSize.Large));
			Response.Headers.TryAdd("x-image-size", new StringValues(ThumbnailSize.Large.ToString()));
			return File(streamDefaultThumbnail, "image/jpeg");
		}


		/// <summary>
		/// Get overview of what exists by name
		/// </summary>
		/// <param name="f">one single fileHash (NOT path)</param>
		/// <returns>thumbnail or status (IActionResult ThumbnailFromIndex)</returns>
		/// <response code="200">returns content of the file</response>
		/// <response code="400">string (f) input not allowed to avoid path injection attacks</response>
		/// <response code="404">no thumbnails yet</response>
		/// <response code="401">User unauthorized</response>
		[HttpGet("/api/thumbnail/list-sizes/{f}")]
		[ProducesResponseType(200)] // file
		[ProducesResponseType(
			400)] // string (f) input not allowed to avoid path injection attacks
		[ProducesResponseType(404)] // not found
		public IActionResult ListSizesByHash(string f)
		{
			// For serving jpeg files
			f = FilenamesHelper.GetFileNameWithoutExtension(f);
	        
			// Restrict the fileHash to letters and digits only
			// I/O function calls should not be vulnerable to path injection attacks
			if (!Regex.IsMatch(f, "^[a-zA-Z0-9_-]+$") )
			{
				return BadRequest();
			}
			
			var data = new { 
				TinyMeta = _thumbnailStorage.ExistFile(ThumbnailNameHelper.Combine(f,ThumbnailSize.TinyMeta)),
				Small = _thumbnailStorage.ExistFile(ThumbnailNameHelper.Combine(f,ThumbnailSize.Small)),
				Large = _thumbnailStorage.ExistFile(ThumbnailNameHelper.Combine(f,ThumbnailSize.Large)),
				ExtraLarge = _thumbnailStorage.ExistFile(ThumbnailNameHelper.Combine(f,ThumbnailSize.ExtraLarge))
			};

			if ( data.TinyMeta || data.Small || data.Large || data.ExtraLarge )
				return Json(data);
			
			var sourcePath = _query.GetSubPathByHash(f);
			if ( sourcePath != null ) return Json(data);
			return NotFound("not in index");
		}

		private IActionResult ReturnThumbnailResult(string f, bool json, ThumbnailSize size)
		{
			Response.Headers.Add("x-image-size", new StringValues(size.ToString()));
			var stream = _thumbnailStorage.ReadStream(ThumbnailNameHelper.Combine(f, size),50);
			var imageFormat = ExtensionRolesHelper.GetImageFormat(stream);
			if ( imageFormat == ExtensionRolesHelper.ImageFormat.unknown )
			{
				SetExpiresResponseHeadersToZero();
				return NoContent(); // 204
			}
			
			// When using the api to check using javascript
			// use the cached version of imageFormat, otherwise you have to check if it deleted
			if (json) return Json("OK");

			stream = _thumbnailStorage.ReadStream(
					ThumbnailNameHelper.Combine(f, size));
			
			// thumbs are always in jpeg
			Response.Headers.Add("x-filename", new StringValues(FilenamesHelper.GetFileName(f + ".jpg")));
			return File(stream, "image/jpeg");
		}
		

		/// <summary>
        /// Get thumbnail with fallback to original source image.
        /// Return source image when IsExtensionThumbnailSupported is true
        /// </summary>
        /// <param name="f">one single fileHash (NOT path)</param>
        /// <param name="isSingleItem">true = load original</param>
        /// <param name="json">text as output</param>
        /// <param name="extraLarge">give preference to extraLarge over large image</param> 
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
            bool json = false,
            bool extraLarge = true)
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

	        var preferredSize = ThumbnailSize.ExtraLarge;
	        var altSize = ThumbnailSize.Large;
	        if ( !extraLarge )
	        {
		        preferredSize = ThumbnailSize.Large;
		        altSize = ThumbnailSize.ExtraLarge;
	        }
	        
            if (_thumbnailStorage.ExistFile(ThumbnailNameHelper.Combine(f, preferredSize)))
            {
                return ReturnThumbnailResult(f, json, preferredSize);
            }

            if ( _thumbnailStorage.ExistFile(ThumbnailNameHelper.Combine(f, altSize)) )
            {
	            return ReturnThumbnailResult(f, json, altSize);
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
        /// <param name="f">one single fileHash (NOT path)</param>
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
