using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using starsky.feature.thumbnail.Interfaces;
using starsky.foundation.database.Interfaces;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Thumbnails;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Models;
using starsky.foundation.storage.Storage;
using starsky.project.web.Helpers;

namespace starsky.Controllers;

[Authorize]
public sealed class ThumbnailController : Controller
{
	private const string ModelError = "Model is invalid";
	private readonly ThumbnailImageFormat _imageFormat;
	private readonly IStorage _iStorage;
	private readonly IWebLogger _logger;
	private readonly IQuery _query;
	private readonly ISmallThumbnailBackgroundJobService _thumbnailBgService;
	private readonly IStorage _thumbnailStorage;

	public ThumbnailController(IQuery query, ISelectorStorage selectorStorage,
		AppSettings appSettings, IWebLogger logger,
		ISmallThumbnailBackgroundJobService thumbnailBgService)
	{
		_query = query;
		_iStorage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
		_thumbnailStorage = selectorStorage.Get(SelectorStorage.StorageServices.Thumbnail);
		_imageFormat = appSettings.ThumbnailImageFormat;
		_logger = logger;
		_thumbnailBgService = thumbnailBgService;
	}

	/// <summary>
	///     Get thumbnail for index pages (300 px or 150px or 1000px (based on what's there))
	/// </summary>
	/// <param name="fileHash">one single fileHash (NOT path)</param>
	/// <param name="f">filePath for generation</param>
	/// <returns>thumbnail or status (IActionResult ThumbnailFromIndex)</returns>
	/// <response code="200">returns content of the file</response>
	/// <response code="400">string (f) input not allowed to avoid path injection attacks</response>
	/// <response code="404">item not found on disk</response>
	/// <response code="401">User unauthorized</response>
	[HttpGet("/api/thumbnail/small/{fileHash}")]
	[ProducesResponseType(200)] // file
	[ProducesResponseType(400)] // string (f) input not allowed to avoid path injection attacks
	[ProducesResponseType(404)] // not found
	[AllowAnonymous] // <=== ALLOW FROM EVERYWHERE
	[ResponseCache(Duration = 29030400)] // 4 weeks
	public IActionResult ThumbnailSmallOrTinyMeta(string fileHash, string? f = null)
	{
		if ( !ModelState.IsValid )
		{
			return BadRequest(ModelError);
		}

		const string xImageSizeHeader = "x-image-size";

		fileHash = FilenamesHelper.GetFileNameWithoutExtension(fileHash);

		// Restrict the fileHash to letters and digits only
		// I/O function calls should not be vulnerable to path injection attacks
		if ( !ThumbnailNameHelper.ValidateThumbnailName(fileHash) )
		{
			return BadRequest();
		}

		if ( _thumbnailStorage.ExistFile(
			    ThumbnailNameHelper.Combine(fileHash, ThumbnailSize.Small, _imageFormat)) )
		{
			var stream =
				_thumbnailStorage.ReadStream(
					ThumbnailNameHelper.Combine(fileHash, ThumbnailSize.Small, _imageFormat));
			Response.Headers.TryAdd(xImageSizeHeader,
				new StringValues(nameof(ThumbnailSize.Small)));
			return File(stream, MimeHelper.GetMimeType(_imageFormat.ToString()));
		}

		if ( _thumbnailStorage.ExistFile(
			    ThumbnailNameHelper.Combine(fileHash, ThumbnailSize.TinyMeta, _imageFormat)) )
		{
			var stream =
				_thumbnailStorage.ReadStream(
					ThumbnailNameHelper.Combine(fileHash, ThumbnailSize.TinyMeta, _imageFormat));
			Response.Headers.TryAdd(xImageSizeHeader,
				new StringValues(nameof(ThumbnailSize.TinyMeta)));
			return File(stream, MimeHelper.GetMimeType(_imageFormat.ToString()));
		}

		if ( !_thumbnailStorage.ExistFile(
			    ThumbnailNameHelper.Combine(fileHash, ThumbnailSize.Large, _imageFormat)) )
		{
			_thumbnailBgService.CreateJob(HttpContext.User.Identity?.IsAuthenticated, f);
			SetExpiresResponseHeadersToZero();
			return NotFound("hash not found");
		}

		var streamDefaultThumbnail =
			_thumbnailStorage.ReadStream(
				ThumbnailNameHelper.Combine(fileHash, ThumbnailSize.Large, _imageFormat));
		Response.Headers.TryAdd(xImageSizeHeader,
			new StringValues(nameof(ThumbnailSize.Large)));
		return File(streamDefaultThumbnail, MimeHelper.GetMimeType(_imageFormat.ToString()));
	}

	/// <summary>
	///     Get overview of what exists by name
	/// </summary>
	/// <param name="f">one single fileHash (NOT path)</param>
	/// <returns>thumbnail or status (IActionResult ThumbnailFromIndex)</returns>
	/// <response code="200">ok view content to see what's ready</response>
	/// <response code="202">Thumbnail is not ready yet</response>
	/// <response code="400">string (f) input not allowed to avoid path injection attacks</response>
	/// <response code="404">no thumbnails yet</response>
	/// <response code="401">User unauthorized</response>
	[HttpGet("/api/thumbnail/list-sizes/{f}")]
	[ProducesResponseType(200)] // file
	[ProducesResponseType(202)] // thumbnail can be generated "Thumbnail is not ready yet"
	[ProducesResponseType(210)] // raw
	[ProducesResponseType(
		400)] // string (f) input not allowed to avoid path injection attacks
	[ProducesResponseType(404)] // not found
	public async Task<IActionResult> ListSizesByHash(string f)
	{
		if ( !ModelState.IsValid )
		{
			return BadRequest(ModelError);
		}

		// For serving jpeg files
		f = FilenamesHelper.GetFileNameWithoutExtension(f);

		// Restrict the fileHash to letters and digits only
		// I/O function calls should not be vulnerable to path injection attacks
		if ( !ThumbnailNameHelper.ValidateThumbnailName(f) )
		{
			return BadRequest();
		}

		var data = new ThumbnailSizesExistStatusModel
		{
			TinyMeta =
				_thumbnailStorage.ExistFile(
					ThumbnailNameHelper.Combine(f, ThumbnailSize.TinyMeta, _imageFormat)),
			Small =
				_thumbnailStorage.ExistFile(
					ThumbnailNameHelper.Combine(f, ThumbnailSize.Small, _imageFormat)),
			Large =
				_thumbnailStorage.ExistFile(
					ThumbnailNameHelper.Combine(f, ThumbnailSize.Large, _imageFormat)),
			ExtraLarge =
				_thumbnailStorage.ExistFile(
					ThumbnailNameHelper.Combine(f, ThumbnailSize.ExtraLarge, _imageFormat))
		};

		// Success has all items (except tinyMeta)
		if ( data is { Small: true, Large: true, ExtraLarge: true } )
		{
			return Json(data);
		}

		var sourcePath = await _query.GetSubPathByHashAsync(f);
		var isThumbnailSupported =
			ExtensionRolesHelper.IsExtensionImageSharpThumbnailSupported(sourcePath);
		switch ( isThumbnailSupported )
		{
			case true when !string.IsNullOrEmpty(sourcePath):
				Response.StatusCode = 202;
				return Json(data);
			case false when !string.IsNullOrEmpty(sourcePath):
				Response.StatusCode = 210; // A conflict, that the thumb is not generated yet
				return Json(
					"Thumbnail is not supported; for example you try to view a raw or video file");
			default:
				return NotFound("not in index");
		}
	}

	private IActionResult ReturnThumbnailResult(string f, bool json, ThumbnailSize size)
	{
		Response.Headers.Append("x-image-size", new StringValues(size.ToString()));
		var stream =
			_thumbnailStorage.ReadStream(ThumbnailNameHelper.Combine(f, size, _imageFormat), 50);
		var imageFormat = new ExtensionRolesHelper(_logger).GetImageFormat(stream);
		if ( imageFormat == ExtensionRolesHelper.ImageFormat.unknown )
		{
			SetExpiresResponseHeadersToZero();
			return NoContent(); // 204
		}

		// When using the api to check using javascript
		// use the cached version of imageFormat, otherwise you have to check if it deleted
		if ( json )
		{
			return Json("OK");
		}

		stream = _thumbnailStorage.ReadStream(
			ThumbnailNameHelper.Combine(f, size, _imageFormat));

		// thumbs are always in jpeg or webp
		Response.Headers.Append("x-filename",
			new StringValues(FilenamesHelper.GetFileName($"{f}.{_imageFormat}")));
		return File(stream, MimeHelper.GetMimeType(_imageFormat.ToString()));
	}

	/// <summary>
	///     Get thumbnail with fallback to original source image.
	///     Return source image when IsExtensionThumbnailSupported is true
	/// </summary>
	/// <param name="f">one single fileHash (NOT path)</param>
	/// <param name="filePath">fallback FilePath</param>
	/// <param name="isSingleItem">true = load original</param>
	/// <param name="json">text as output</param>
	/// <param name="extraLarge">give preference to extraLarge over large image</param>
	/// <returns>thumbnail or status (IActionResult Thumbnail)</returns>
	/// <response code="200">returns content of the file or when json is true, "OK"</response>
	/// <response code="202">thumbnail can be generated, Thumbnail is not ready yet</response>
	/// <response code="204">thumbnail is corrupt</response>
	/// <response code="210">Conflict, you did try get for example a thumbnail of a raw file</response>
	/// <response code="400">string (f) input not allowed to avoid path injection attacks</response>
	/// <response code="404">item not found on disk</response>
	/// <response code="401">User unauthorized</response>
	[HttpGet("/api/thumbnail/{f}")]
	[ProducesResponseType(200)] // file
	[ProducesResponseType(202)] // thumbnail can be generated "Thumbnail is not ready yet"
	[ProducesResponseType(204)] // thumbnail is corrupt
	[ProducesResponseType(210)] // raw
	[ProducesResponseType(400)] // string (f) input not allowed to avoid path injection attacks
	[ProducesResponseType(404)] // not found
	[AllowAnonymous] // <=== ALLOW FROM EVERYWHERE
	[ResponseCache(Duration = 29030400)] // 4 weeks
	public async Task<IActionResult> Thumbnail(
		string f,
		string? filePath = null,
		bool isSingleItem = false,
		bool json = false,
		bool extraLarge = true)
	{
		if ( !ModelState.IsValid )
		{
			return BadRequest(ModelError);
		}

		// f is Hash
		// isSingleItem => detailView
		// Retry thumbnail => is when you press reset thumbnail
		// json, => to don't waste the user's bandwidth.

		// For serving jpeg files
		f = FilenamesHelper.GetFileNameWithoutExtension(f);

		// Get the text before at (@) so replace @2000 with nothing to match  fileHash
		var beforeAt = Regex.Match(f, ".*(?=@)", RegexOptions.None,
			TimeSpan.FromSeconds(1)).Value;
		if ( !string.IsNullOrEmpty(beforeAt) )
		{
			f = beforeAt;
		}

		// Restrict the fileHash to letters and digits only
		// I/O function calls should not be vulnerable to path injection attacks
		if ( !ThumbnailNameHelper.ValidateThumbnailName(f) )
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

		if ( _thumbnailStorage.ExistFile(
			    ThumbnailNameHelper.Combine(f, preferredSize, _imageFormat)) )
		{
			return ReturnThumbnailResult(f, json, preferredSize);
		}

		if ( _thumbnailStorage.ExistFile(ThumbnailNameHelper.Combine(f, altSize, _imageFormat)) )
		{
			return ReturnThumbnailResult(f, json, altSize);
		}

		// Cached view of item
		// Need to check again for recently moved files
		var sourcePath = await _query.GetSubPathByHashAsync(f);
		if ( sourcePath == null )
		{
			// remove from cache
			_query.ResetItemByHash(f);

			if ( string.IsNullOrEmpty(filePath) ||
			     await _query.GetObjectByFilePathAsync(filePath) == null )
			{
				SetExpiresResponseHeadersToZero();
				return NotFound("not in index");
			}

			sourcePath = filePath;
		}

		if ( !_iStorage.ExistFile(sourcePath) )
		{
			return NotFound("There is no thumbnail image " + f + " and no source image " +
			                sourcePath);
		}

		if ( !isSingleItem )
		{
			// "Photo exist in database but " + "isSingleItem flag is Missing"
			SetExpiresResponseHeadersToZero();
			Response.StatusCode = 202; // A conflict, that the thumb is not generated yet
			return Json("Thumbnail is not ready yet");
		}

		if ( ExtensionRolesHelper.IsExtensionImageSharpThumbnailSupported(sourcePath) )
		{
			var fs1 = _iStorage.ReadStream(sourcePath);

			var fileExt = FilenamesHelper.GetFileExtensionWithoutDot(sourcePath);
			var fileName = HttpUtility.UrlEncode(FilenamesHelper.GetFileName(sourcePath));
			Response.Headers.TryAdd("x-filename", new StringValues(fileName));
			return File(fs1, MimeHelper.GetMimeType(fileExt));
		}

		Response.StatusCode = 210; // A conflict, that the thumb is not generated yet
		return Json("Thumbnail is not supported; for example you try to view a raw file");
	}

	/// <summary>
	///     Get zoomed in image by fileHash.
	///     At the moment this is the source image
	/// </summary>
	/// <param name="f">one single fileHash (NOT path)</param>
	/// <param name="z">zoom factor? </param>
	/// <param name="filePath">fallback filePath</param>
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
	[SuppressMessage("Usage", "IDE0060:Remove unused parameter")]
	public async Task<IActionResult> ByZoomFactorAsync(
		string f,
		int z = 0,
		string filePath = "")
	{
		if ( !ModelState.IsValid )
		{
			return BadRequest(ModelError);
		}

		// For serving jpeg files
		f = FilenamesHelper.GetFileNameWithoutExtension(f);

		// Restrict the fileHash to letters and digits only
		// I/O function calls should not be vulnerable to path injection attacks
		if ( !ThumbnailNameHelper.ValidateThumbnailName(f) )
		{
			return BadRequest();
		}

		// Cached view of item
		var sourcePath = await _query.GetSubPathByHashAsync(f);
		if ( sourcePath == null )
		{
			if ( await _query.GetObjectByFilePathAsync(filePath) == null )
			{
				return NotFound("not in index");
			}

			sourcePath = filePath;
		}

		if ( ExtensionRolesHelper.IsExtensionImageSharpThumbnailSupported(sourcePath) )
		{
			var fs1 = _iStorage.ReadStream(sourcePath);

			var fileExt = FilenamesHelper.GetFileExtensionWithoutDot(sourcePath);
			Response.Headers.Append("x-filename", FilenamesHelper.GetFileName(sourcePath));
			return File(fs1, MimeHelper.GetMimeType(fileExt));
		}

		Response.StatusCode = 210; // A conflict, that the thumb is not generated yet
		return Json("Thumbnail is not supported; for example you try to view a raw file");
	}

	/// <summary>
	///     Force Http context to no browser cache
	/// </summary>
	public void SetExpiresResponseHeadersToZero()
	{
		Request.HttpContext.Response.Headers.Remove("Cache-Control");
		Request.HttpContext.Response.Headers.Append("Cache-Control",
			"no-cache, no-store, must-revalidate");

		Request.HttpContext.Response.Headers.Remove("Pragma");
		Request.HttpContext.Response.Headers.Append("Pragma", "no-cache");

		Request.HttpContext.Response.Headers.Remove("Expires");
		Request.HttpContext.Response.Headers.Append("Expires", "0");
	}
}
