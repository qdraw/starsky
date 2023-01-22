using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using starsky.Attributes;
using starsky.feature.import.Helpers;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.http.Streaming;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;

namespace starsky.Controllers;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "S5693:Make sure the content " +
	"length limit is safe here", Justification = "Is checked")]
[Authorize]
public class ImportThumbnailController : Controller
{
	private readonly AppSettings _appSettings;
	private readonly ISelectorStorage _selectorStorage;
	private readonly IStorage _hostFileSystemStorage;
	private readonly IWebLogger _logger;
	private readonly IThumbnailQuery _thumbnailQuery;
	private readonly IStorage _thumbnailStorage;
	private readonly RemoveTempAndParentStreamFolderHelper _removeTempAndParentStreamFolderHelper;

	public ImportThumbnailController(AppSettings appSettings,
		ISelectorStorage selectorStorage, 
		IWebLogger logger, IThumbnailQuery thumbnailQuery)
	{
		_appSettings = appSettings;
		_selectorStorage = selectorStorage; 
		_hostFileSystemStorage = selectorStorage.Get(SelectorStorage.StorageServices.HostFilesystem);
		_thumbnailStorage = selectorStorage.Get(SelectorStorage.StorageServices.Thumbnail);
		_logger = logger;
		_thumbnailQuery = thumbnailQuery;
		_removeTempAndParentStreamFolderHelper =
			new RemoveTempAndParentStreamFolderHelper(_hostFileSystemStorage,
				_appSettings);
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

		var thumbnailNamesWithSuffix = GetThumbnailNamesWithSuffix(tempImportPaths);

		// Move the files to the correct location
		await WriteThumbnails(tempImportPaths, thumbnailNamesWithSuffix);
		
		// Status if there is nothing uploaded
		if (tempImportPaths.Count != thumbnailNamesWithSuffix.Count)
		{
			Response.StatusCode = 415;
			return Json(thumbnailNamesWithSuffix);
		}

		var thumbnailItems = MapToTransferObject(thumbnailNamesWithSuffix).ToList();
		await _thumbnailQuery.AddThumbnailRangeAsync(thumbnailItems);
		
		return Json(thumbnailNamesWithSuffix);
	}

	internal static IEnumerable<ThumbnailResultDataTransferModel> MapToTransferObject(List<string> thumbnailNames)
	{
		var items = new List<ThumbnailResultDataTransferModel>();
		foreach ( var thumbnailNameWithSuffix in thumbnailNames )
		{
			var thumb = ThumbnailNameHelper.GetSize(thumbnailNameWithSuffix);
			var name = ThumbnailNameHelper.RemoveSuffix(thumbnailNameWithSuffix);
			var item = new ThumbnailResultDataTransferModel(name);
			item.Change(thumb,true);
			items.Add(item);
		}
		return items;
	}

	private List<string> GetThumbnailNamesWithSuffix(List<string> tempImportPaths)
	{
		var thumbnailNamesWithSuffix = new List<string>();
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
				_logger.LogInformation($"[Import/Thumbnail] remove already exists - {thumbToUpperCase}" );
				_thumbnailStorage.FileDelete(thumbToUpperCase);
			}
			    
			thumbnailNamesWithSuffix.Add(thumbToUpperCase);
		}
		return thumbnailNamesWithSuffix;
	}

	internal async Task<bool> WriteThumbnails(List<string> tempImportPaths, List<string> thumbnailNames)
	{
		if (tempImportPaths.Count != thumbnailNames.Count)
		{
			_removeTempAndParentStreamFolderHelper.RemoveTempAndParentStreamFolder(tempImportPaths);
			return false;
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
			_removeTempAndParentStreamFolderHelper.RemoveTempAndParentStreamFolder(tempImportPaths[i]);
		}
		return true;
	}
}
