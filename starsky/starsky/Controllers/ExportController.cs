using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using starskycore.Helpers;
using starskycore.Interfaces;
using starskycore.Models;
using starskycore.Services;

namespace starsky.Controllers
{
	[Authorize]
	public class ExportController : Controller
	{
		private readonly IQuery _query;
		private readonly IExiftool _exiftool;
		private readonly AppSettings _appSettings;
		private readonly IBackgroundTaskQueue _bgTaskQueue;

		public ExportController(
			IQuery query, IExiftool exiftool, 
			AppSettings appSettings, IBackgroundTaskQueue queue
		)
		{
			_appSettings = appSettings;
			_query = query;
			_exiftool = exiftool;
			_bgTaskQueue = queue;
		}
		
		/// <summary>
		/// Export source files to an zip archive
		/// </summary>
		/// <param name="f">subpath to files</param>
		/// <param name="collections">enable files with the same name (before the extension)</param>
		/// <returns>name of a to generate zip file</returns>
		/// <response code="200">the name of the to generated zip file</response>
		/// <response code="404">files not found</response>
		[HttpPost("/export/createZip")]
		[ProducesResponseType(200)] // "zipHash"
		[ProducesResponseType(404)] // "Not found"
		public async Task<IActionResult> CreateZip(string f, bool collections = true, bool thumbnail = false)
		{
			var inputFilePaths = PathHelper.SplitInputFilePaths(f);
			// the result list
			var fileIndexResultsList = new List<FileIndexItem>();

			foreach ( var subPath in inputFilePaths )
			{
				var detailView = _query.SingleItem(subPath, null, collections, false);
				
				// all filetypes that are exist > should be added 
				
				var statusResults =
					new StatusCodesHelper(_appSettings).FileCollectionsCheck(detailView);
				
				// ignore readonly status
				if ( statusResults == FileIndexItem.ExifStatus.ReadOnly )
					statusResults = FileIndexItem.ExifStatus.Ok;

				
				var statusModel = new FileIndexItem();
				statusModel.SetFilePath(subPath);
				statusModel.IsDirectory = false;
				
				if(new StatusCodesHelper(null).ReturnExifStatusError(statusModel, statusResults, fileIndexResultsList)) continue;

				if ( detailView == null ) throw new ArgumentNullException(nameof(detailView));

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

				CreateZip(filePaths,fileNames,zipHash);
				
				// Write a single file to be sure that writing is ready
				var doneFileFullPath = Path.Join(_appSettings.TempFolder,zipHash) + ".done";
				new PlainTextFileHelper().WriteFile(doneFileFullPath,"OK");

				Console.WriteLine("<<<<<<<");

			});
			
			// for the rest api
			return Json(zipHash);
		}


		/// <summary>
		/// This list will be included in the zip
		/// </summary>
		/// <param name="fileIndexResultsList">the items</param>
		/// <param name="thumbnail">add the thumbnail or the source image</param>
		/// <returns>list of filepaths</returns>
		public List<string> CreateListToExport(List<FileIndexItem> fileIndexResultsList, bool thumbnail)
		{
			var filePaths = new List<string>();

			foreach ( var item in fileIndexResultsList.Where(p => p.Status == FileIndexItem.ExifStatus.Ok).ToList() )
			{
				var sourceFile = _appSettings.DatabasePathToFilePath(item.FilePath);
				var sourceThumb = Path.Join(_appSettings.ThumbnailTempFolder,
					item.FileHash + ".jpg");

				if ( thumbnail )
					new Thumbnail(_appSettings, _exiftool).CreateThumb(item);

				filePaths.Add(thumbnail ? sourceThumb : sourceFile); // has:notHas

				// when there is .xmp sidecar file
				if ( !thumbnail && Files.IsXmpSidecarRequired(sourceFile) && Files.ExistFile(Files.GetXmpSidecarFile(sourceFile)))
				{
					filePaths.Add(Files.GetXmpSidecarFile(sourceFile));
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
					// We use base32 filehashes but export 
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
		/// </summary>
		/// <param name="f">zip hash</param>
		/// <param name="json">true to get OK instead of a zip file</param>
		/// <returns>Not ready or the zipfile</returns>
		/// <response code="200">if json is true return 'OK', else the zip file</response>
		/// <response code="206">Not ready generating the zip, please wait</response>
		[HttpGet("/export/zip/{f}.zip")]
		[ProducesResponseType(200)] // "zip file"
		[ProducesResponseType(206)] // "Not Ready"
		public async Task<IActionResult> Zip(string f, bool json = false)
		{
			var sourceFullPath = Path.Join(_appSettings.TempFolder,f) + ".zip";
			var doneFileFullPath = Path.Join(_appSettings.TempFolder,f) + ".done";

			if ( Files.IsFolderOrFile(sourceFullPath) ==
			     FolderOrFileModel.FolderOrFileTypeList.Deleted ) return NotFound("Path is not found");

			// Read a single file to be sure that writing is ready
			if ( Files.IsFolderOrFile(doneFileFullPath) ==
			     FolderOrFileModel.FolderOrFileTypeList.Deleted )
			{
				Response.StatusCode = 206;
				return Json("Not Ready");
			}
			
			if ( json ) return Json("OK");
			FileStream fs = System.IO.File.OpenRead(sourceFullPath);
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
			var shortName = tempFileNameStringBuilder.ToString().GetHashCode().ToString(CultureInfo.InvariantCulture).ToLower().Replace("-","A");
			return shortName;
		}
	
		/// <summary>
		/// To Create the zip file in the temp folder
		/// </summary>
		/// <param name="filePaths">list of full file paths</param>
		/// <param name="fileNames">list of filenames</param>
		/// <param name="zipHash">to name of the zip file</param>
		/// <returns>a zip in the temp folder</returns>
		public string CreateZip(List<string> filePaths, List<string> fileNames, string zipHash)
		{

			var tempFileFullPath = Path.Join(_appSettings.TempFolder,zipHash) + ".zip";

			if(System.IO.File.Exists(tempFileFullPath))
			{
				return tempFileFullPath;
			}
			ZipArchive zip = ZipFile.Open(tempFileFullPath, ZipArchiveMode.Create);

			for ( int i = 0; i < filePaths.Count; i++ )
			{
				if ( System.IO.File.Exists(filePaths[i]) )
				{
					zip.CreateEntryFromFile(filePaths[i], fileNames[i]);
				}
			}
			zip.Dispose();
			return tempFileFullPath;
		}
	}
}
