using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using starsky.foundation.database.Interfaces;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Thumbnails;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Models;
using starsky.foundation.storage.Storage;
using starsky.foundation.thumbnailgeneration.GenerationFactory.Interfaces;
using starsky.Helpers;
using starsky.project.web.Helpers;

namespace starsky.Controllers;

[Authorize]
public sealed class DownloadPhotoController : Controller
{
	private readonly AppSettings _appSettings;
	private readonly IStorage _iStorage;
	private readonly IWebLogger _logger;
	private readonly IQuery _query;
	private readonly IThumbnailService _thumbnailService;
	private readonly IStorage _thumbnailStorage;

	public DownloadPhotoController(IQuery query, ISelectorStorage selectorStorage,
		IWebLogger logger, IThumbnailService thumbnailService, AppSettings appSettings)
	{
		_query = query;
		_iStorage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
		_thumbnailStorage = selectorStorage.Get(SelectorStorage.StorageServices.Thumbnail);
		_thumbnailService = thumbnailService;
		_logger = logger;
		_appSettings = appSettings;
	}

	/// <summary>
	///     Download sidecar file for example image.xmp
	/// </summary>
	/// <param name="f">string, subPath to find the file</param>
	/// <returns>FileStream with image</returns>
	/// <response code="200">returns content of the file</response>
	/// <response code="404">source image missing</response>
	/// <response code="401">User unauthorized</response>
	[HttpGet("/api/download-sidecar")]
	[ProducesResponseType(200)] // file
	[ProducesResponseType(404)] // not found
	[ProducesResponseType(400)]
	public IActionResult DownloadSidecar([Required] string f)
	{
		if ( !ModelState.IsValid )
		{
			return BadRequest("ModelState is not valid");
		}

		if ( !ExtensionRolesHelper.IsExtensionSidecar(f) )
		{
			return NotFound("FileName is not a sidecar");
		}

		if ( !_iStorage.ExistFile(f) )
		{
			return NotFound($"source image missing {f}");
		}

		var fs = _iStorage.ReadStream(f);
		return File(fs, MimeHelper.GetMimeTypeByFileName(f));
	}

	/// <summary>
	///     Select manually the original or thumbnail
	/// </summary>
	/// <param name="f">string, 'sub path' to find the file</param>
	/// <param name="isThumbnail">true = 1000px thumb (if supported)</param>
	/// <param name="cache">true = send client headers to cache</param>
	/// <returns>FileStream with image</returns>
	/// <response code="200">returns content of the file</response>
	/// <response code="404">source image missing</response>
	/// <response code="500">"Thumbnail generation failed"</response>
	/// <response code="401">User unauthorized</response>
	[HttpGet("/api/download-photo")]
	[ProducesResponseType(200)] // file
	[ProducesResponseType(404)] // not found
	[ProducesResponseType(500)] // "Thumbnail generation failed"
	public async Task<IActionResult> DownloadPhoto(string f, bool isThumbnail = true,
		bool cache = true)
	{
		if ( !ModelState.IsValid )
		{
			return BadRequest("ModelState is not valid");
		}

		if ( f.Contains("?isthumbnail") )
		{
			// f = subpath/filepath
			return NotFound("please use &isthumbnail = instead of ?isthumbnail= ");
		}

		var fileIndexItem = await _query.GetObjectByFilePathAsync(f);
		if ( fileIndexItem == null )
		{
			return NotFound("not in index " + f);
		}

		if ( !_iStorage.ExistFile(fileIndexItem.FilePath!) )
		{
			return NotFound($"source image missing {fileIndexItem.FilePath}");
		}

		// Return full image
		if ( !isThumbnail )
		{
			if ( cache )
			{
				CacheControlOverwrite.SetExpiresResponseHeaders(Request);
			}

			var fileStream = _iStorage.ReadStream(fileIndexItem.FilePath!);

			// Return the right mime type (enableRangeProcessing = needed for safari and mp4)
			return File(fileStream, MimeHelper.GetMimeTypeByFileName(fileIndexItem.FilePath!),
				true);
		}

		if ( !_thumbnailStorage.ExistFolder("/") )
		{
			return NotFound("ThumbnailTempFolder not found");
		}

		var data = new ThumbnailSizesExistStatusModel
		{
			Small = _thumbnailStorage.ExistFile(
				ThumbnailNameHelper.Combine(fileIndexItem.FileHash!, ThumbnailSize.Small,
					_appSettings.ThumbnailImageFormat)),
			Large = _thumbnailStorage.ExistFile(
				ThumbnailNameHelper.Combine(fileIndexItem.FileHash!, ThumbnailSize.Large,
					_appSettings.ThumbnailImageFormat)),
			ExtraLarge = _thumbnailStorage.ExistFile(
				ThumbnailNameHelper.Combine(fileIndexItem.FileHash!, ThumbnailSize.ExtraLarge,
					_appSettings.ThumbnailImageFormat))
		};

		if ( !data.Small || !data.Large || !data.ExtraLarge )
		{
			_logger.LogDebug("Thumbnail generation started");
			await _thumbnailService.GenerateThumbnail(fileIndexItem.FilePath!,
				fileIndexItem.FileHash!);

			var thumbnail = ThumbnailNameHelper.Combine(fileIndexItem.FileHash!,
				ThumbnailSize.Large, _appSettings.ThumbnailImageFormat);
			if ( !_thumbnailStorage.ExistFile(thumbnail) )
			{
				Response.StatusCode = 500;
				return Json("Thumbnail generation failed");
			}
		}

		var thumbnailFileStream = _thumbnailStorage.ReadStream(
			ThumbnailNameHelper.Combine(fileIndexItem.FileHash!, ThumbnailSize.Large,
				_appSettings.ThumbnailImageFormat));
		return File(thumbnailFileStream,
			MimeHelper.GetMimeType(_appSettings.ThumbnailImageFormat.ToString())
		);
	}
}
