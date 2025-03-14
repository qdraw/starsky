using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using starsky.Attributes;
using starsky.feature.import.Interfaces;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.http.Streaming;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;

namespace starsky.Controllers;

[SuppressMessage("Usage", "S5693:Make sure the content " +
                          "length limit is safe here", Justification = "Is checked")]
[Authorize]
public class ImportThumbnailController : Controller
{
	private readonly AppSettings _appSettings;
	private readonly IImportThumbnailService _importThumbnailService;
	private readonly ISelectorStorage _selectorStorage;
	private readonly IThumbnailQuery _thumbnailQuery;

	public ImportThumbnailController(AppSettings appSettings,
		ISelectorStorage selectorStorage, IThumbnailQuery thumbnailQuery,
		IImportThumbnailService importThumbnailService)
	{
		_appSettings = appSettings;
		_selectorStorage = selectorStorage;
		_thumbnailQuery = thumbnailQuery;
		_importThumbnailService = importThumbnailService;
	}

	/// <summary>
	///     Upload thumbnail to ThumbnailTempFolder
	///     Make sure that the filename is correct, a base32 hash of length 26;
	///     Overwrite if the 'id' is the same
	///     Also known as Thumbnail Upload or Thumbnail Import
	/// </summary>
	/// <returns>json of thumbnail urls</returns>
	/// <response code="200">done</response>
	/// <response code="415">Wrong input (e.g. wrong extenstion type)</response>
	[HttpPost("/api/import/thumbnail")]
	[DisableFormValueModelBinding]
	[Produces("application/json")]
	[RequestFormLimits(MultipartBodyLengthLimit = 100_000_000)]
	[RequestSizeLimit(100_000_000)] // in bytes, 100MB
	[ProducesResponseType(typeof(List<ImportIndexItem>), 200)] // yes
	[ProducesResponseType(typeof(List<ImportIndexItem>), 415)] // wrong input
	public async Task<IActionResult> Thumbnail()
	{
		var tempImportPaths = await Request.StreamFile(_appSettings, _selectorStorage);

		var thumbnailNamesWithSuffix =
			_importThumbnailService.GetThumbnailNamesWithSuffix(tempImportPaths);

		// Move the files to the correct location
		await _importThumbnailService.WriteThumbnails(tempImportPaths, thumbnailNamesWithSuffix);

		// Status if there is nothing uploaded
		if ( tempImportPaths.Count != thumbnailNamesWithSuffix.Count )
		{
			Response.StatusCode = 415;
			return Json(thumbnailNamesWithSuffix);
		}

		var thumbnailItems = _importThumbnailService.MapToTransferObject(thumbnailNamesWithSuffix)
			.ToList();
		await _thumbnailQuery.AddThumbnailRangeAsync(thumbnailItems);

		return Json(thumbnailNamesWithSuffix);
	}
}
