using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using starsky.Attributes;
using starsky.feature.import.Interfaces;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.http.Streaming;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.JsonConverter;
using starsky.foundation.platform.Models;
using starsky.foundation.readmeta.Services;
using starsky.foundation.realtime.Interfaces;
using starsky.foundation.storage.Helpers;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starsky.foundation.sync.Helpers;
using starskycore.Models;

namespace starsky.Controllers
{
	[Authorize]
	public class UploadController : Controller
	{
		private readonly AppSettings _appSettings;
		private readonly IImport _import;
		private readonly IStorage _iStorage; 
		private readonly IStorage _iHostStorage;
		private readonly IQuery _query;
		private readonly ISelectorStorage _selectorStorage;
		private readonly IWebSocketConnectionsService _connectionsService;
		private readonly NewItem _newItem;

		public UploadController(IImport import, AppSettings appSettings, 
			ISelectorStorage selectorStorage, IQuery query, 
			IWebSocketConnectionsService connectionsService)
		{
			_appSettings = appSettings;
			_import = import;
			_query = query;
			_selectorStorage = selectorStorage;
			_iStorage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
			_iHostStorage = selectorStorage.Get(SelectorStorage.StorageServices.HostFilesystem);
			_connectionsService = connectionsService;
			_newItem = new NewItem(_iHostStorage, new ReadMeta(_iHostStorage));
		}

		/// <summary>
		/// Upload to specific folder (does not check if already has been imported)
		/// Use the header 'to' to determine the location to where to upload
		/// Add header 'filename' when uploading direct without form
		/// (ActionResult UploadToFolder)
		/// </summary>
		/// <response code="200">done</response>
		/// <response code="404">folder not found</response>
		/// <response code="415">Wrong input (e.g. wrong extenstion type)</response>
		/// <response code="400">missing 'to' header</response>
		/// <returns>the ImportIndexItem of the imported files </returns>
		[HttpPost("/api/upload")]
        [DisableFormValueModelBinding]
		[RequestFormLimits(MultipartBodyLengthLimit = 320_000_000)]
		[RequestSizeLimit(320_000_000)] // in bytes, 305MB
		[ProducesResponseType(typeof(List<ImportIndexItem>),200)] // yes
		[ProducesResponseType(typeof(string),400)]
		[ProducesResponseType(typeof(List<ImportIndexItem>),404)]
		[ProducesResponseType(typeof(List<ImportIndexItem>),415)]  // Wrong input (e.g. wrong extenstion type)
		[Produces("application/json")]	    
        public async Task<IActionResult> UploadToFolder()
		{
			var to = Request.Headers["to"].ToString();
			if ( string.IsNullOrWhiteSpace(to) ) return BadRequest("missing 'to' header");

			var parentDirectory = GetParentDirectoryFromRequestHeader();
			if ( parentDirectory == null )
			{
				return NotFound(new ImportIndexItem());
			}
			
			var tempImportPaths = await Request.StreamFile(_appSettings,_selectorStorage);
			
			var fileIndexResultsList = await _import.Preflight(tempImportPaths, 
				new ImportSettingsModel{IndexMode = false});

			for ( var i = 0; i < fileIndexResultsList.Count; i++ )
			{
				if(fileIndexResultsList[i].Status != ImportStatus.Ok) continue;
			
				var tempFileStream = _iHostStorage.ReadStream(tempImportPaths[i]);
				var fileName = Path.GetFileName(tempImportPaths[i]);

				// subPath is always unix style
				var subPath = PathHelper.AddSlash(parentDirectory) + fileName;
				if ( parentDirectory == "/" ) subPath = parentDirectory + fileName;

				// Add item to db before write to fs
				var item = await _newItem.NewFileItem(tempImportPaths[i], null, parentDirectory, fileName);
				await _query.AddItemAsync(item);
				
				await _iStorage.WriteStreamAsync(tempFileStream, subPath);
				await tempFileStream.DisposeAsync();
				
				 // clear directory cache
				 _query.RemoveCacheParentItem(subPath);

				 // to get the output in the result right
				 fileIndexResultsList[i].FileIndexItem.FileName = fileName;
				 fileIndexResultsList[i].FileIndexItem.ParentDirectory =  parentDirectory;
				 fileIndexResultsList[i].FilePath = subPath;
				 
				_iHostStorage.FileDelete(tempImportPaths[i]);
			}

			// send all uploads as list
			await _connectionsService.SendToAllAsync(
				JsonSerializer.Serialize(
					fileIndexResultsList
						.Where(p => p.Status == ImportStatus.Ok)
						.Select(item => item.FileIndexItem).ToList(), 
					DefaultJsonSerializer.CamelCase), CancellationToken.None);
			
			// Wrong input (extension is not allowed)
            if ( fileIndexResultsList.All(p => p.Status == ImportStatus.FileError) )
            {
	            Response.StatusCode = 415;
            }
            
	        return Json(fileIndexResultsList);
        }
		
		/// <summary>
		/// Check if xml can be parsed
		/// Used by sidecar upload
		/// </summary>
		/// <param name="xml">string with xml</param>
		/// <returns>true when parsed</returns>
		private bool IsValidXml(string xml)
		{
			try
			{
				// ReSharper disable once ReturnValueOfPureMethodIsNotUsed
				XDocument.Parse(xml);
				return true;
			}
			catch
			{
				return false;
			}
		}
		
		/// <summary>
		/// Upload sidecar file to specific folder (does not check if already has been imported)
		/// Use the header 'to' to determine the location to where to upload
		/// Add header 'filename' when uploading direct without form
		/// (ActionResult UploadToFolder)
		/// </summary>
		/// <response code="200">done</response>
		/// <response code="404">parent folder not found</response>
		/// <response code="415">Wrong input (e.g. wrong extenstion type)</response>
		/// <response code="400">missing 'to' header</response>
		/// <returns>the ImportIndexItem of the imported files </returns>
		[HttpPost("/api/upload-sidecar")]
		[DisableFormValueModelBinding]
		[RequestFormLimits(MultipartBodyLengthLimit = 3_000_000)]
		[RequestSizeLimit(3_000_000)] // in bytes, 3 MB
		[ProducesResponseType(typeof(List<ImportIndexItem>), 200)] // yes
		[ProducesResponseType(typeof(string), 400)]
		[ProducesResponseType(typeof(List<ImportIndexItem>), 404)] // parent dir not found
		[ProducesResponseType(typeof(List<ImportIndexItem>),
			415)] // Wrong input (e.g. wrong extenstion type)
		[Produces("application/json")]
		public async Task<IActionResult> UploadToFolderSidecarFile()
		{
			var to = Request.Headers["to"].ToString();
			if ( string.IsNullOrWhiteSpace(to) ) return BadRequest("missing 'to' header");

			var parentDirectory = GetParentDirectoryFromRequestHeader();
			if ( parentDirectory == null )
			{
				return NotFound(new ImportIndexItem());
			}

			var tempImportPaths = await Request.StreamFile(_appSettings, _selectorStorage);

			var importedList = new List<string>();
			foreach ( var tempImportSinglePath in tempImportPaths )
			{
				var data = await new PlainTextFileHelper().StreamToStringAsync(
					_iHostStorage.ReadStream(tempImportSinglePath));
				if ( !IsValidXml(data) ) continue;
				
				var tempFileStream = _iHostStorage.ReadStream(tempImportSinglePath);
				var fileName = Path.GetFileName(tempImportSinglePath);

				var subPath = PathHelper.AddSlash(parentDirectory) + fileName;
				if ( parentDirectory == "/" ) subPath = parentDirectory + fileName;

				await _iStorage.WriteStreamAsync(tempFileStream, subPath);
				await tempFileStream.DisposeAsync();
				importedList.Add(subPath);
			}
			
			if ( !importedList.Any() )
			{
				Response.StatusCode = 415;
			}
			return Json(importedList);
		}

		internal string GetParentDirectoryFromRequestHeader()
		{
			var to = Request.Headers["to"].ToString();
			if ( to == "/" ) return "/";

			// only used for direct import
			if ( _iStorage.ExistFolder(FilenamesHelper.GetParentPath(to)) && 
			     FilenamesHelper.IsValidFileName(FilenamesHelper.GetFileName(to)) )
			{
				Request.Headers["filename"] = FilenamesHelper.GetFileName(to);
				return FilenamesHelper.GetParentPath(PathHelper.RemoveLatestSlash(to));
			}
			// ReSharper disable once ConvertIfStatementToReturnStatement
			if (!_iStorage.ExistFolder(PathHelper.RemoveLatestSlash(to))) return null;
			return PathHelper.RemoveLatestSlash(to);
		}
		
	}
}
