using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using starsky.Attributes;
using starsky.Helpers;
using starskycore.Helpers;
using starskycore.Interfaces;
using starskycore.Models;
using starskycore.Services;

namespace starsky.Controllers
{
	public class UploadController : Controller
	{
		private readonly AppSettings _appSettings;
		private readonly IStorage _iStorage; 
		private readonly IImport _import;
		private readonly StorageHostFullPathFilesystem _iHostStorage;
		private readonly ISync _iSync;
		private readonly IQuery _query;

		public UploadController(IImport import, AppSettings appSettings, 
			ISync sync, IStorage iStorage, IQuery query)
		{
			_appSettings = appSettings;
			_import = import;
			_iSync = sync;
			_query = query;
			_iStorage = iStorage; 
			_iHostStorage = new StorageHostFullPathFilesystem();
		}
		
		
		/// <summary>
		/// Upload to specific folder (does not check if already has been imported)
		/// Use the header 'to' to determine the location to where to upload
		/// </summary>
		/// <response code="200">done</response>
		/// <response code="404">folder not found</response>
		/// <response code="415">Wrong input (e.g. wrong extenstion type)</response>
		/// <response code="400">missing 'to' header</response>
		/// <returns>the ImportIndexItem of the imported files</returns>
		[HttpPost("/api/upload")]
        [DisableFormValueModelBinding]
		[RequestFormLimits(MultipartBodyLengthLimit = 320_000_000)]
		[RequestSizeLimit(320_000_000)] // in bytes, 305MB
		[ProducesResponseType(typeof(List<ImportIndexItem>),200)] // yes
		[ProducesResponseType(typeof(string),400)]
		[ProducesResponseType(typeof(List<ImportIndexItem>),404)]
		[ProducesResponseType(typeof(List<ImportIndexItem>),415)]  // Wrong input (e.g. wrong extenstion type)
        public async Task<IActionResult> UploadToFolder()
		{
			var to = Request.Headers["to"].ToString();
			if ( string.IsNullOrWhiteSpace(to) ) return BadRequest("missing 'to' header");
			
			var parentDirectory = _iStorage.ExistFolder(to) ? PathHelper.AddSlash(to) :  PathHelper.RemoveLatestSlash(to);

			// only used for direct import
			if ( _iStorage.ExistFolder(FilenamesHelper.GetParentPath(to)) && 
			     FilenamesHelper.IsValidFileName(FilenamesHelper.GetFileName(to)) )
			{
				Request.Headers["filename"] = FilenamesHelper.GetFileName(to);
				parentDirectory = FilenamesHelper.GetParentPath(to);
			}
			else if (!_iStorage.ExistFolder(to))
			{
				return NotFound(new ImportIndexItem());
			}
			
			var tempImportPaths = await Request.StreamFile(_appSettings);
			
			
			var fileIndexResultsList = _import.Preflight(tempImportPaths, new ImportSettingsModel{IndexMode = false});

			for ( var i = 0; i < fileIndexResultsList.Count; i++ )
			{
				if(fileIndexResultsList[i].Status != ImportStatus.Ok) continue;

				await using var tempFileStream = _iHostStorage.ReadStream(tempImportPaths[i]);
				
				var fileName = Path.GetFileName(tempImportPaths[i]);

				_iStorage.WriteStream(tempFileStream, parentDirectory + fileName);
				tempFileStream.Dispose();
				
				_iSync.SyncFiles(parentDirectory + fileName,false);
				
				 // clear directory cache
				 _query.RemoveCacheParentItem(parentDirectory);

				_iHostStorage.FileDelete(tempImportPaths[i]);
			}
			
			// Wrong input (extension is not allowed)
            if ( fileIndexResultsList.All(p => p.Status == ImportStatus.FileError) )
            {
	            Response.StatusCode = 415;
            }
            
	        return Json(fileIndexResultsList);
        }
		
	}
}
