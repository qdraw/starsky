using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
			var f = Request.Headers["to"].ToString();
			if ( string.IsNullOrWhiteSpace(f) ) return BadRequest("missing 'to' header");
			
			var subPath = PathHelper.AddSlash(f);
			if (  !_iStorage.ExistFolder(subPath) ) return NotFound(new ImportIndexItem());
			
			var tempImportPaths = await Request.StreamFile(_appSettings);
			
			var fileIndexResultsList = _import.Preflight(tempImportPaths, new ImportSettingsModel{IndexMode = false});

			for ( var i = 0; i < fileIndexResultsList.Count; i++ )
			{
				if(fileIndexResultsList[i].Status != ImportStatus.Ok) continue;

				await using var tempFile = _iHostStorage.ReadStream(tempImportPaths[i]);
				var fileName = Path.GetFileName(tempImportPaths[i]);
				
				var copyToSubPath = subPath + fileName;
				_iStorage.WriteStream(tempFile, copyToSubPath);

				 // to show the correct output
				 fileIndexResultsList[i].FilePath = copyToSubPath;
				 fileIndexResultsList[i].FileIndexItem.ParentDirectory = subPath;
				 fileIndexResultsList[i].FileIndexItem.FileName = fileName;
				 fileIndexResultsList[i].FileIndexItem.SetAddToDatabase();

				 // Add or run sync file
				 var queryItem = _query.SingleItem(copyToSubPath);
				 if (queryItem  == null )
				 {
					 _query.AddItem(fileIndexResultsList[i].FileIndexItem);
				 }
				 else
				 {
					 _iSync.SyncFiles(copyToSubPath,false);
				 }
				 
				 // clear directory cache
				 _query.RemoveCacheParentItem(subPath);

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
