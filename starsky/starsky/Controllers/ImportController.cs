using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using starsky.Attributes;
using starsky.foundation.database.Models;
using starsky.foundation.http.Interfaces;
using starsky.foundation.http.Streaming;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Services;
using starsky.foundation.storage.Storage;
using starskycore.Interfaces;
using starskycore.Models;
using starskycore.Services;

namespace starsky.Controllers
{
    [Authorize]
    public class ImportController : Controller
    {
        private readonly IImport _import;
        private readonly AppSettings _appSettings;
        private readonly IBackgroundTaskQueue _bgTaskQueue;
	    private readonly IHttpClientHelper _httpClientHelper;
	    private readonly ISelectorStorage _selectorStorage;
	    private readonly IStorage _hostFileSystemStorage;
	    private readonly IStorage _thumbnailStorage;

	    public ImportController(IImport import, AppSettings appSettings,
		    IBackgroundTaskQueue queue, 
            IHttpClientHelper httpClientHelper, ISelectorStorage selectorStorage)
        {
            _appSettings = appSettings;
            _import = import;
            _bgTaskQueue = queue;
	        _httpClientHelper = httpClientHelper;
	        _selectorStorage = selectorStorage; 
	        _hostFileSystemStorage = selectorStorage.Get(SelectorStorage.StorageServices.HostFilesystem);
	        _thumbnailStorage = selectorStorage.Get(SelectorStorage.StorageServices.Thumbnail);
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
        public async Task<IActionResult> IndexPost()
        {
            var tempImportPaths = await Request.StreamFile(_appSettings,_selectorStorage);
            var importSettings = new ImportSettingsModel(Request);

	        var fileIndexResultsList = _import.Preflight(tempImportPaths, importSettings);

            // Import files >
            _bgTaskQueue.QueueBackgroundWorkItem(async token =>
            {    
                var importedFiles = _import.Import(tempImportPaths, importSettings);
                
                if ( _appSettings.Verbose )
                {
	                foreach (var file in importedFiles)
	                {
		                Console.WriteLine($">> import => {file}");
	                }
                }

                foreach ( var toDelPath in tempImportPaths )
                {
	                _hostFileSystemStorage.FileDelete(toDelPath);
                }
            });
            
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
		
	    /// <summary>
	    /// Upload thumbnail to ThumbnailTempFolder
	    /// Make sure that the filename is correct, a base32 hash of length 26;
	    /// Overwrite if the Id is the same
	    /// </summary>
	    /// <returns>json of thumbnail urls</returns>
	    [HttpPost("/api/import/thumbnail")]
	    [DisableFormValueModelBinding]
	    [Produces("application/json")]
	    [RequestFormLimits(MultipartBodyLengthLimit = 100_000_000)]
	    [RequestSizeLimit(100_000_000)] // in bytes, 100MB
	    public async Task<IActionResult> Thumbnail()
	    {
		    var tempImportPaths = await Request.StreamFile(_appSettings, _selectorStorage);

		    var thumbnailNames = new List<string>();
		    foreach ( var t in tempImportPaths )
		    {
			    var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(t);
			    
			    var thumbToUpperCase = fileNameWithoutExtension.ToUpperInvariant();

			    if ( fileNameWithoutExtension.Length != 26 )
			    {
				    continue;
			    }
			    
			    if (_thumbnailStorage.ExistFile(thumbToUpperCase))
			    {
				    _thumbnailStorage.FileDelete(thumbToUpperCase);
			    }
			    
			    thumbnailNames.Add(thumbToUpperCase);
		    }

		    // Status if there is nothing uploaded
		    if ( !thumbnailNames.Any() )
		    {
			    Response.StatusCode = 206;
		    }

		    for ( var i = 0; i < tempImportPaths.Count; i++ )
		    {
			    await _thumbnailStorage.WriteStreamAsync(
				    _hostFileSystemStorage.ReadStream(tempImportPaths[i]), thumbnailNames[i]);
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
	        if (!Regex.IsMatch(filename, "^[a-zA-Z0-9_\\s\\.]+$") || !FilenamesHelper.IsValidFileName(filename))
	        {
		        return BadRequest();
	        }
	        
	        var tempImportFullPath = Path.Combine(_appSettings.TempFolder, filename);
	        var importSettings = new ImportSettingsModel(Request) {Structure = structure};
	        var isDownloaded = await _httpClientHelper.Download(fileUrl,tempImportFullPath);
            if (!isDownloaded) return NotFound("'file url' not found or domain not allowed " + fileUrl);

	        var importedFiles = _import.Import(new List<string>{tempImportFullPath}, importSettings);
	        _hostFileSystemStorage.FileDelete(tempImportFullPath);
            if(importedFiles.Count == 0) Response.StatusCode = 206;
            return Json(importedFiles);
        }

	    /// <summary>
	    /// Today's imported files
	    /// </summary>
	    /// <returns>list of files</returns>
	    /// <response code="200">done</response>
	    [HttpGet("/api/import/history")]
	    [ProducesResponseType(typeof(List<ImportIndexItem>),200)] // yes
	    [Produces("application/json")]
	    public IActionResult History()
	    {
		    return Json(_import.History());
	    }

    }
}
