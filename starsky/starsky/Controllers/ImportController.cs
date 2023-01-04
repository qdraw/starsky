﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualBasic;
using starsky.Attributes;
using starsky.feature.import.Interfaces;
using starsky.feature.import.Models;
using starsky.feature.import.Services;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.http.Interfaces;
using starsky.foundation.http.Streaming;
using starsky.foundation.thumbnailmeta.Interfaces;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Services;
using starsky.foundation.storage.Storage;
using starsky.foundation.worker.Interfaces;
using starsky.foundation.writemeta.Interfaces;

namespace starsky.Controllers
{
	[Authorize] // <- should be logged in!
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "S5693:Make sure the content " +
		"length limit is safe here", Justification = "Is checked")]
	public sealed class ImportController : Controller
	{
		private readonly IImport _import;
		private readonly AppSettings _appSettings;
		private readonly IUpdateBackgroundTaskQueue _bgTaskQueue;
		private readonly IHttpClientHelper _httpClientHelper;
		private readonly ISelectorStorage _selectorStorage;
		private readonly IStorage _hostFileSystemStorage;
		private readonly IStorage _thumbnailStorage;
		private readonly IServiceScopeFactory _scopeFactory;
		private readonly IWebLogger _logger;

		public ImportController(IImport import, AppSettings appSettings,
			IUpdateBackgroundTaskQueue queue, 
			IHttpClientHelper httpClientHelper, ISelectorStorage selectorStorage, 
			IServiceScopeFactory scopeFactory, IWebLogger logger)
		{
			_appSettings = appSettings;
			_import = import;
			_bgTaskQueue = queue;
			_httpClientHelper = httpClientHelper;
			_selectorStorage = selectorStorage; 
			_hostFileSystemStorage = selectorStorage.Get(SelectorStorage.StorageServices.HostFilesystem);
			_thumbnailStorage = selectorStorage.Get(SelectorStorage.StorageServices.Thumbnail);
			_scopeFactory = scopeFactory;
			_logger = logger;
		}
        
		/// <summary>
		/// Import a file using the structure format
		/// </summary>
		/// <returns>the ImportIndexItem of the imported files</returns>
		/// <response code="200">done</response>
		/// <response code="206">file already imported</response>
		/// <response code="415">Wrong input (e.g. wrong extenstion type)</response>
		[HttpPost("/api/import")]
		[DisableFormValueModelBinding]
		[Produces("application/json")]
		[RequestFormLimits(MultipartBodyLengthLimit = 320_000_000)]
		[RequestSizeLimit(320_000_000)] // in bytes, 305MB
		[ProducesResponseType(typeof(List<ImportIndexItem>),200)] // yes
		[ProducesResponseType(typeof(List<ImportIndexItem>),206)]  // When all items are already imported
		[ProducesResponseType(typeof(List<ImportIndexItem>),415)]  // Wrong input (e.g. wrong extenstion type)
		public async Task<IActionResult> IndexPost() // aka ActionResult Import
		{
			var tempImportPaths = await Request.StreamFile(_appSettings,_selectorStorage);
			var importSettings = new ImportSettingsModel(Request);

			var fileIndexResultsList = await _import.Preflight(tempImportPaths, importSettings);

			// Import files >
			await _bgTaskQueue.QueueBackgroundWorkItemAsync(async _ =>
			{
				await ImportPostBackgroundTask(tempImportPaths, importSettings, _appSettings.IsVerbose());
			}, string.Join(",", tempImportPaths));
            
			// When all items are already imported
			if ( importSettings.IndexMode &&
			     fileIndexResultsList.All(p => p.Status != ImportStatus.Ok) )
			{
				Response.StatusCode = 206;
			}
            
			// Wrong input (extension is not allowed)
			if ( fileIndexResultsList.All(p => p.Status == ImportStatus.FileError) )
			{
				Response.StatusCode = 415;
			}

			return Json(fileIndexResultsList);
		}

		internal async Task<List<ImportIndexItem>> ImportPostBackgroundTask(List<string> tempImportPaths,
			ImportSettingsModel importSettings, bool isVerbose = false)
		{
			List<ImportIndexItem> importedFiles;
	            
			using ( var scope = _scopeFactory.CreateScope() )
			{
				var selectorStorage = scope.ServiceProvider.GetRequiredService<ISelectorStorage>();
				var importQuery = scope.ServiceProvider.GetRequiredService<IImportQuery>();
				var exifTool = scope.ServiceProvider.GetRequiredService<IExifTool>();
				var query = scope.ServiceProvider.GetRequiredService<IQuery>();
				var console = scope.ServiceProvider.GetRequiredService<IConsole>();
				var metaExifThumbnailService = scope.ServiceProvider.GetRequiredService<IMetaExifThumbnailService>();
				var memoryCache = scope.ServiceProvider.GetRequiredService<IMemoryCache>();
				var thumbnailQuery = scope.ServiceProvider.GetRequiredService<IThumbnailQuery>();

				// use of IImport direct does not work
				importedFiles = await new Import(selectorStorage,_appSettings,
					importQuery, exifTool, query,console, 
					metaExifThumbnailService, _logger, thumbnailQuery, memoryCache).Importer(tempImportPaths, importSettings);
			}
	            
			if (isVerbose)
			{
				foreach (var file in importedFiles)
				{
					_logger.LogInformation(
							$"[ImportPostBackgroundTask] import {file.Status} " +
							$"=> {file.FilePath} ~ {file.FileIndexItem?.FilePath}");
				}
			}
                
			// Remove source files
			foreach ( var toDelPath in tempImportPaths )
			{
				RemoveTempAndParentStreamFolder(toDelPath);
			}

			return importedFiles;
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
				RemoveTempAndParentStreamFolder(tempImportPaths[i]);
			}

			return Json(thumbnailNames);
		}

		/// <summary>
		/// Import file from web-url (only whitelisted domains) and import this file into the application
		/// </summary>
		/// <param name="fileUrl">the url</param>
		/// <param name="filename">the filename (optional, random used if empty)</param>
		/// <param name="structure">use structure (optional)</param>
		/// <returns></returns>
		/// <response code="200">done</response>
		/// <response code="206">file already imported</response>
		/// <response code="404">the file url is not found or the domain is not whitelisted</response>
		[HttpPost("/api/import/fromUrl")]
		[ProducesResponseType(typeof(List<ImportIndexItem>),200)] // yes
		[ProducesResponseType(typeof(List<ImportIndexItem>),206)] // file already imported
		[ProducesResponseType(404)] // url 404
		[Produces("application/json")]
		public async Task<IActionResult> FromUrl(string fileUrl, string filename, string structure)
		{
			if (filename == null) filename = Base32.Encode(FileHash.GenerateRandomBytes(8)) + ".unknown";
	        
			// I/O function calls should not be vulnerable to path injection attacks
			if (!Regex.IsMatch(filename, "^[a-zA-Z0-9_\\s\\.]+$", 
				RegexOptions.None, TimeSpan.FromMilliseconds(100)) || !FilenamesHelper.IsValidFileName(filename))
			{
				return BadRequest();
			}
	        
			var tempImportFullPath = Path.Combine(_appSettings.TempFolder, filename);
			var importSettings = new ImportSettingsModel(Request) {Structure = structure};
			var isDownloaded = await _httpClientHelper.Download(fileUrl,tempImportFullPath);
			if (!isDownloaded) return NotFound("'file url' not found or domain not allowed " + fileUrl);

			var importedFiles = await _import.Importer(new List<string>{tempImportFullPath}, importSettings);
			RemoveTempAndParentStreamFolder(tempImportFullPath);
			
			if(importedFiles.Count == 0) Response.StatusCode = 206;
			return Json(importedFiles);
		}

		private void RemoveTempAndParentStreamFolder(string tempImportFullPath)
		{
			_hostFileSystemStorage.FileDelete(tempImportFullPath);
			// remove parent folder of tempFile
			var parentPath = Directory.GetParent(tempImportFullPath)?.FullName;
			if ( !string.IsNullOrEmpty(parentPath) && parentPath != _appSettings.TempFolder )
			{
				_hostFileSystemStorage.FolderDelete(parentPath);
			}
		}
	}
}
