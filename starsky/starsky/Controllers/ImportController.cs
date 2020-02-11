using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using starsky.Attributes;
using starsky.Helpers;
using starskycore.Helpers;
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
	    private readonly HttpClientHelper _httpClientHelper;
	    private readonly IStorage _iStorage; //<= not yet implemented

	    public ImportController(IImport import, AppSettings appSettings, 
            IServiceScopeFactory scopeFactory, IBackgroundTaskQueue queue, 
            HttpClientHelper httpClientHelper, IStorage iStorage)
        {
            _appSettings = appSettings;
            _import = import;
            _bgTaskQueue = queue;
	        _httpClientHelper = httpClientHelper;
	        _iStorage = iStorage; //<= not yet implemented
        }
	    
        
		/// <summary>
		/// Import a file using the structure format
		/// </summary>
		/// <returns>the ImportIndexItem of the imported files</returns>
		/// <response code="200">done</response>
		/// <response code="206">file already imported</response>
		/// <response code="415">Wrong input (e.g. wrong extenstion type)</response>
		[HttpPost("/import")]
        [DisableFormValueModelBinding]
		[RequestFormLimits(MultipartBodyLengthLimit = 320_000_000)]
		[RequestSizeLimit(320_000_000)] // in bytes, 305MB
		[ProducesResponseType(typeof(List<ImportIndexItem>),200)] // yes
        [ProducesResponseType(typeof(List<ImportIndexItem>),206)]  // When all items are already imported
		[ProducesResponseType(typeof(List<ImportIndexItem>),415)]  // Wrong input (e.g. wrong extenstion type)
        public async Task<IActionResult> IndexPost()
        {
            var tempImportPaths = await Request.StreamFile(_appSettings);
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
                
                FilesHelper.DeleteFile(tempImportPaths);
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
	    /// </summary>
	    /// <returns>json of thumbnail urls</returns>
	    [HttpPost("/import/thumbnail")]
	    [DisableFormValueModelBinding]
	    [RequestFormLimits(MultipartBodyLengthLimit = 100_000_000)]
	    [RequestSizeLimit(100_000_000)] // in bytes, 100MB
	    public async Task<IActionResult> Thumbnail()
	    {
		    var tempImportPaths = await Request.StreamFile(_appSettings);

		    var thumbnailPaths = new List<string>();
		    for ( int i = 0; i < tempImportPaths.Count; i++ )
		    {
			    var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(tempImportPaths[i]);
			    var thumbToUpperCase = Path.Combine(_appSettings.ThumbnailTempFolder, fileNameWithoutExtension.ToUpperInvariant() + ".jpg");
			    if ( fileNameWithoutExtension.Length != 26 || 
			         FilesHelper.IsFolderOrFile(thumbToUpperCase) == FolderOrFileModel.FolderOrFileTypeList.File)
			    {
				    FilesHelper.DeleteFile(tempImportPaths[i]);
				    tempImportPaths.Remove(tempImportPaths[i]);
				    continue;
			    }
			    thumbnailPaths.Add(thumbToUpperCase);
		    }

		    // Status if there is nothing uploaded
		    if ( !thumbnailPaths.Any() )
		    {
			    Response.StatusCode = 206;
		    }

		    for ( int i = 0; i < tempImportPaths.Count; i++ )
		    {
			    System.IO.File.Move(tempImportPaths[i],thumbnailPaths[i]);
		    }

		    return Json(thumbnailPaths);
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
            FilesHelper.DeleteFile(tempImportFullPath);
            if(importedFiles.Count == 0) Response.StatusCode = 206;
            return Json(importedFiles);
        }

	    [HttpGet("/import/history")]
	    public IActionResult History()
	    {
		    return Json(_import.History());
	    }

    }
}
