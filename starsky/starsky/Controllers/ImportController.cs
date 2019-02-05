using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

	    public ImportController(IImport import, AppSettings appSettings, 
            IServiceScopeFactory scopeFactory, IBackgroundTaskQueue queue, HttpClientHelper httpClientHelper)
        {
            _appSettings = appSettings;
            _import = import;
            _bgTaskQueue = queue;
	        _httpClientHelper = httpClientHelper;
        }

        [HttpGet]
        [ActionName("Index")]
        public IActionResult Index()
        {
            return View("Index");
        }

        
		/// <summary>
		/// Import a file using the structure format
		/// </summary>
		/// <returns></returns>
        [HttpPost("/import")]
        [DisableFormValueModelBinding]
        [RequestSizeLimit(160000000)] // in bytes, 160mb
        [ProducesResponseType(typeof(List<ImportIndexItem>),200)] // yes
        [ProducesResponseType(typeof(List<ImportIndexItem>),206)]  // When all items are already imported
        public async Task<IActionResult> IndexPost()
        {
            var tempImportPaths = await Request.StreamFile(_appSettings);
            var importSettings = new ImportSettingsModel(Request);

            // Do some import checks before sending it to the background service
            var fileIndexResultsList = new List<ImportIndexItem>();
            var hashList = FileHash.GetHashCode(tempImportPaths.ToArray());

            for (int i = 0; i < hashList.Count; i++)
            {
                var hash = hashList[i];

                var fileIndexItem = _import.ReadExifAndXmpFromFile(tempImportPaths[i]);
                var importIndexItem = _import.ObjectCreateIndexItem(
                    tempImportPaths[i], hash, fileIndexItem, importSettings.Structure);
                
                // do some filename reading to get dates, based on 'structure config' 
                importIndexItem.ParseDateTimeFromFileName();

                var item = _import.GetItemByHash(hash);
                if (item != null)
                {
                    fileIndexResultsList.Add(item);
                    continue;
                }

                if (!_import.IsAgeFileFilter(importSettings, importIndexItem.DateTime))
                {
                    fileIndexResultsList.Add(importIndexItem);
                }
            }


            // Import files >
            _bgTaskQueue.QueueBackgroundWorkItem(async token =>
            {    
                var importedFiles = _import.Import(tempImportPaths, importSettings);
                Files.DeleteFile(tempImportPaths);
                foreach (var file in importedFiles)
                {
                    Console.WriteLine(file);
                }
            });
            
            // When all items are already imported
            if (importSettings.IndexMode && fileIndexResultsList.All(p => p.Id != 0)) Response.StatusCode = 206;

            return Json(fileIndexResultsList);
        }

	    
	    /// <summary>
	    /// Upload thumbnail to ThumbnailTempFolder
	    /// Make sure that the filename is correct, a base32 hash of length 26;
	    /// </summary>
	    /// <returns>json of thumbnail urls</returns>
	    [HttpPost]
	    [ActionName("Thumbnail")]
	    [DisableFormValueModelBinding]
	    [RequestSizeLimit(160000000)] // in bytes, 160mb
	    public async Task<IActionResult> Thumbnail()
	    {
		    var tempImportPaths = await Request.StreamFile(_appSettings);

		    var thumbnailPaths = new List<string>();
		    for ( int i = 0; i < tempImportPaths.Count; i++ )
		    {
			    var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(tempImportPaths[i]);
			    var thumbToUpperCase = Path.Combine(_appSettings.ThumbnailTempFolder, fileNameWithoutExtension.ToUpperInvariant() + ".jpg");
			    if ( fileNameWithoutExtension.Length != 26 || 
			         Files.IsFolderOrFile(thumbToUpperCase) == FolderOrFileModel.FolderOrFileTypeList.File)
			    {
				    Files.DeleteFile(tempImportPaths[i]);
				    tempImportPaths.Remove(tempImportPaths[i]);
				    continue;
			    }
			    thumbnailPaths.Add(thumbToUpperCase);
		    }

		    for ( int i = 0; i < tempImportPaths.Count; i++ )
		    {
			    System.IO.File.Move(tempImportPaths[i],thumbnailPaths[i]);
		    }

		    return Json(thumbnailPaths);
	    }


	    /// <summary>
	    /// Import file from weburl (only whitelisted domains) and import this file into the application
	    /// </summary>
	    /// <param name="fileUrl">the url</param>
	    /// <param name="filename">the filename (optional, random used if empty)</param>
	    /// <param name="structure">use structure (optional)</param>
	    /// <returns></returns>
	    [HttpPost("/import/fromUrl")]
	    [ProducesResponseType(typeof(List<ImportIndexItem>),200)] // yes
	    [ProducesResponseType(404)] // url 404
        public async Task<IActionResult> FromUrl(string fileUrl, string filename, string structure)
        {
	        if (filename == null) filename = Base32.Encode(FileHash.GenerateRandomBytes(8)) + ".unknown";
	        var tempImportFullPath = Path.Combine(_appSettings.TempFolder, filename);
	        var importSettings = new ImportSettingsModel(Request);
            importSettings.Structure = structure;
            var isDownloaded = await _httpClientHelper.Download(fileUrl,tempImportFullPath);
            if (!isDownloaded) return NotFound("'file url' not found or domain not allowed " + fileUrl);

	        var importedFiles = _import.Import(new List<string>{tempImportFullPath}, importSettings);
            Files.DeleteFile(tempImportFullPath);
            if(importedFiles.Count == 0) Response.StatusCode = 206;
            return Json(importedFiles);
        }

    }
}