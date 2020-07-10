using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.ArchiveFormats;
using starsky.foundation.storage.Helpers;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starsky.foundation.thumbnailgeneration.Services;
using starskycore.Helpers;
using starskycore.Services;

namespace starsky.Controllers
{
	[Authorize]
	public class ExportController : Controller
	{
		private readonly IQuery _query;
		private readonly AppSettings _appSettings;
		private readonly IBackgroundTaskQueue _bgTaskQueue;
		private readonly IStorage _iStorage;
		private readonly IStorage _thumbnailStorage;
		private readonly IStorage _hostFileSystemStorage;

		public ExportController(
			IQuery query, AppSettings appSettings, IBackgroundTaskQueue queue,
			ISelectorStorage selectorStorage)
		{
			_appSettings = appSettings;
			_query = query;
			_bgTaskQueue = queue;
			_iStorage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
			_thumbnailStorage = selectorStorage.Get(SelectorStorage.StorageServices.Thumbnail);
			_hostFileSystemStorage = selectorStorage.Get(SelectorStorage.StorageServices.HostFilesystem);
		}

		/// <summary>
		/// Export source files to an zip archive
		/// </summary>
		/// <param name="f">subpath to files</param>
		/// <param name="collections">enable files with the same name (before the extension)</param>
		/// <param name="thumbnail">export thumbnails</param>
		/// <returns>name of a to generate zip file</returns>
		/// <response code="200">the name of the to generated zip file</response>
		/// <response code="404">files not found</response>
		[HttpPost("/export/createZip")]
		[ProducesResponseType(typeof(string),200)] // "zipHash"
		[ProducesResponseType(typeof(List<FileIndexItem>),404)] // "Not found"
		[Produces("application/json")]
		public IActionResult CreateZip(string f, bool collections = true, bool thumbnail = false)
		{
			var inputFilePaths = PathHelper.SplitInputFilePaths(f);
			// the result list
			var fileIndexResultsList = new List<FileIndexItem>();

			foreach ( var subPath in inputFilePaths )
			{
				var detailView = _query.SingleItem(subPath, null, collections, false);
				
				// all filetypes that are exist > should be added 
				
				// todo: add filesystem check
				
				
				
				// var statusResults =
				// 	new StatusCodesHelper(_appSettings).FileCollectionsCheck(detailView);
				//
				// // ignore readonly status
				// if ( statusResults == FileIndexItem.ExifStatus.ReadOnly )
				// 	statusResults = FileIndexItem.ExifStatus.Ok;
				//
				//
				// var statusModel = new FileIndexItem();
				// statusModel.SetFilePath(subPath);
				// statusModel.IsDirectory = false;
				//
				// if(new StatusCodesHelper().ReturnExifStatusError(statusModel, statusResults, fileIndexResultsList)) continue;
				//
				//
				

				if ( detailView == null ) throw new InvalidDataException("DetailView is null ~ " + nameof(detailView));

				// Now Add Collection based images
				var collectionSubPathList = detailView.GetCollectionSubPathList(detailView, collections, subPath);
				foreach ( var item in collectionSubPathList )
				{
					var itemDetailView = _query.SingleItem(item, null, false, false).FileIndexItem;
					itemDetailView.Status = FileIndexItem.ExifStatus.Ok;
					fileIndexResultsList.Add(itemDetailView);
				}
			}

			var isThumbnail = thumbnail ? "TN" : "SR"; // has:notHas
			var zipHash = isThumbnail + GetName(fileIndexResultsList);
			
			// When all items are not found
			// allow read only
			if (fileIndexResultsList.All(p => p.Status != FileIndexItem.ExifStatus.Ok) )
				return NotFound(fileIndexResultsList);
			
			// NOT covered: when try to export for example image thumbnails of xml file
				
			// Creating a zip is a background task
			_bgTaskQueue.QueueBackgroundWorkItem(async token =>
			{

				var filePaths = CreateListToExport(fileIndexResultsList, thumbnail);
				var fileNames = FilePathToFileName(filePaths, thumbnail);

				new Zipper().CreateZip(_appSettings.TempFolder,filePaths,fileNames,zipHash);
				
				// Write a single file to be sure that writing is ready
				var doneFileFullPath = Path.Join(_appSettings.TempFolder,zipHash) + ".done";
				await _hostFileSystemStorage.WriteStreamAsync(new PlainTextFileHelper().StringToStream("OK"), doneFileFullPath);
				if(_appSettings.Verbose) Console.WriteLine("Zip done: " + doneFileFullPath);
				
			});
			
			// for the rest api
			return Json(zipHash);
		}


		/// <summary>
		/// This list will be included in the zip
		/// </summary>
		/// <param name="fileIndexResultsList">the items</param>
		/// <param name="thumbnail">add the thumbnail or the source image</param>
		/// <returns>list of file paths</returns>
		public List<string> CreateListToExport(List<FileIndexItem> fileIndexResultsList, bool thumbnail)
		{
			var filePaths = new List<string>();

			foreach ( var item in fileIndexResultsList.Where(p => p.Status == FileIndexItem.ExifStatus.Ok).ToList() )
			{
				var sourceFile = _appSettings.DatabasePathToFilePath(item.FilePath);
				var sourceThumb = Path.Join(_appSettings.ThumbnailTempFolder,
					item.FileHash + ".jpg");

				if ( thumbnail )
					new Thumbnail(_iStorage, _thumbnailStorage).CreateThumb(item.FilePath, item.FileHash);

				filePaths.Add(thumbnail ? sourceThumb : sourceFile); // has:notHas
				
				
				// when there is .xmp sidecar file
				if ( !thumbnail && ExtensionRolesHelper.IsExtensionForceXmp(item.FilePath) 
				                && _iStorage.ExistFile(ExtensionRolesHelper.ReplaceExtensionWithXmp(item.FilePath)))
				{
					filePaths.Add(
						_appSettings.DatabasePathToFilePath(ExtensionRolesHelper.ReplaceExtensionWithXmp(item.FilePath))
						);
				}
				
			}

			return filePaths;
		}

		/// <summary>
		/// Get the filename (in case of thumbnail the source image name)
		/// </summary>
		/// <param name="filePaths">the full file paths </param>
		/// <param name="thumbnail">copy the thumbnail (true) or the source image (false)</param>
		/// <returns></returns>
		public List<string> FilePathToFileName(List<string> filePaths, bool thumbnail)
		{
			var fileNames = new List<string>();
			foreach ( var filePath in filePaths )
			{
				if ( thumbnail )
				{
					// We use base32 fileHashes but export 
					// the file with the original name
					
					var thumbFilename = Path.GetFileNameWithoutExtension(filePath);
					var subPath = _query.GetSubPathByHash(thumbFilename);
					var filename = subPath.Split("/").LastOrDefault();
					fileNames.Add(filename);
					continue;
				}
				fileNames.Add(Path.GetFileName(filePath));
			}
			return fileNames;
		}


		/// <summary>
		/// Get the exported zip, but first call 'createZip'
		/// use for example this url: /export/zip/TNA995920129.zip
		/// TNA995920129 is from 'createZip'
		/// </summary>
		/// <param name="f">zip hash e.g. TNA995920129</param>
		/// <param name="json">true to get OK instead of a zip file</param>
		/// <returns>Not ready or the zip-file</returns>
		/// <response code="200">if json is true return 'OK', else the zip file</response>
		/// <response code="206">Not ready generating the zip, please wait</response>
		[HttpGet("/export/zip/{f}.zip")]
		[ProducesResponseType(200)] // "zip file"
		[ProducesResponseType(206)] // "Not Ready"
		public async Task<IActionResult> Zip(string f, bool json = false)
		{
			var sourceFullPath = Path.Join(_appSettings.TempFolder,f) + ".zip";
			var doneFileFullPath = Path.Join(_appSettings.TempFolder,f) + ".done";

			if ( !_hostFileSystemStorage.ExistFile(sourceFullPath)  ) return NotFound("Path is not found");

			// Read a single file to be sure that writing is ready
			if ( !_hostFileSystemStorage.ExistFile(doneFileFullPath)  )
			{
				Response.StatusCode = 206;
				return Json("Not Ready");
			}
			
			if ( json ) return Json("OK");
			var fs = _hostFileSystemStorage.ReadStream(sourceFullPath);
			// Return the right mime type
			return File(fs, MimeHelper.GetMimeTypeByFileName(sourceFullPath));
		}

		/// <summary>
		/// to create a unique name of the zip using c# get hashcode
		/// </summary>
		/// <param name="fileIndexResultsList">list of objects with filehashes</param>
		/// <returns>unique 'get hashcode' string</returns>
		private string GetName(List<FileIndexItem> fileIndexResultsList)
		{
			var tempFileNameStringBuilder = new StringBuilder();
			foreach ( var item in fileIndexResultsList )
			{
				tempFileNameStringBuilder.Append(item.FileHash);
			}
			// to be sure that the max string limit
			var shortName = tempFileNameStringBuilder.ToString().GetHashCode()
				.ToString(CultureInfo.InvariantCulture).ToLower().Replace("-","A");
			
			return shortName;
		}
	}
}
