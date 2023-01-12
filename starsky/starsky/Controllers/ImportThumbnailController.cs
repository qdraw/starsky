using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using starsky.Attributes;
using starsky.feature.import.Helpers;
using starsky.foundation.database.Models;
using starsky.foundation.http.Streaming;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;

namespace starsky.Controllers;

public class ImportThumbnailController : Controller
{
	private readonly AppSettings _appSettings;
	private readonly ISelectorStorage _selectorStorage;
	private readonly IStorage _hostFileSystemStorage;
	private readonly IWebLogger _logger;
	private readonly IStorage _thumbnailStorage;

	public ImportThumbnailController(AppSettings appSettings,
		ISelectorStorage selectorStorage, 
		IWebLogger logger)
	{
		_appSettings = appSettings;
		_selectorStorage = selectorStorage; 
		_hostFileSystemStorage = selectorStorage.Get(SelectorStorage.StorageServices.HostFilesystem);
		_thumbnailStorage = selectorStorage.Get(SelectorStorage.StorageServices.Thumbnail);
		_logger = logger;
	}
	
	/// <summary>
	/// Upload thumbnail to ThumbnailTempFolder
	/// Make sure that the filename is correct, a base32 hash of length 26;
	/// Overwrite if the Id is the same
	/// Also known as Thumbnail Upload or Thumbnail Import
	/// </summary>
	/// <returns>json of thumbnail urls</returns>
	/// <response code="200">done</response>
	/// <response code="415">Wrong input (e.g. wrong extenstion type)</response>
	[HttpPost("/api/import/thumbnail")]
	[DisableFormValueModelBinding]
	[Produces("application/json")]
	[RequestFormLimits(MultipartBodyLengthLimit = 100_000_000)]
	[RequestSizeLimit(100_000_000)] // in bytes, 100MB
	[ProducesResponseType(typeof(List<ImportIndexItem>),200)] // yes
	[ProducesResponseType(typeof(List<ImportIndexItem>),415)]  // wrong input
	public async Task<IActionResult> Thumbnail()
	{
		var tempImportPaths = await Request.StreamFile(_appSettings, _selectorStorage);

		var thumbnailNames = new List<string>();
		// ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
		foreach ( var tempImportSinglePath in tempImportPaths )
		{
			var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(tempImportSinglePath);
			    
			var thumbToUpperCase = fileNameWithoutExtension.ToUpperInvariant();
				
			_logger.LogInformation($"[Import/Thumbnail] - {thumbToUpperCase}" );

			if ( ThumbnailNameHelper.GetSize(thumbToUpperCase) == ThumbnailSize.Unknown )
			{
				continue;
			}
			    
			// remove existing thumbnail if exist
			if (_thumbnailStorage.ExistFile(thumbToUpperCase))
			{
				_thumbnailStorage.FileDelete(thumbToUpperCase);
			}
			    
			thumbnailNames.Add(thumbToUpperCase);
		}

		// Status if there is nothing uploaded
		if (tempImportPaths.Count !=  thumbnailNames.Count)
		{
			Response.StatusCode = 415;
			return Json(thumbnailNames);
		}

		for ( var i = 0; i < tempImportPaths.Count; i++ )
		{
			if ( ! _hostFileSystemStorage.ExistFile(tempImportPaths[i]) )
			{
				_logger.LogInformation($"[Import/Thumbnail] ERROR {tempImportPaths[i]} does not exist");
				continue;
			}
				
			await _thumbnailStorage.WriteStreamAsync(
				_hostFileSystemStorage.ReadStream(tempImportPaths[i]), thumbnailNames[i]);
				
			// Remove from temp folder to avoid long list of files
			new RemoveTempAndParentStreamFolderHelper(_hostFileSystemStorage,_appSettings)
				.RemoveTempAndParentStreamFolder(tempImportPaths[i]);
		}

		return Json(thumbnailNames);
	}
}
