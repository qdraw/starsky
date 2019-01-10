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
using starsky.Helpers;
using starsky.Interfaces;
using starsky.Models;
using starsky.Services;

namespace starsky.Controllers
{
	[Authorize]
	public class ExportController : Controller
	{
		private readonly IQuery _query;
		private readonly IExiftool _exiftool;
		private readonly AppSettings _appSettings;
		private readonly IBackgroundTaskQueue _bgTaskQueue;
		private readonly IReadMeta _readMeta;

		public ExportController(
			IQuery query, IExiftool exiftool, 
			AppSettings appSettings, IBackgroundTaskQueue queue,
			IReadMeta readMeta
		)
		{
			_appSettings = appSettings;
			_query = query;
			_exiftool = exiftool;
			_bgTaskQueue = queue;
			_readMeta = readMeta;
		}
		
		/// <summary>
		/// Export source files to an zip archive (alpha feature)
		/// </summary>
		/// <param name="f"></param>
		/// <param name="collections"></param>
		/// <returns></returns>
		[HttpPost("/export/createZip")]
		public async Task<IActionResult> CreateZip(string f, bool collections = true, bool thumbnail = false)
		{
			var inputFilePaths = ConfigRead.SplitInputFilePaths(f);
			// the result list
			var fileIndexResultsList = new List<FileIndexItem>();

			foreach ( var subPath in inputFilePaths )
			{
				var detailView = _query.SingleItem(subPath, null, collections, false);

				// Check if extension is supported for ExtensionExifToolSupportedList
				// Not all files are able to write with exiftool
				if ( detailView != null &&
				     !Files.IsExtensionExifToolSupported(detailView.FileIndexItem.FileName) )
				{
					detailView.FileIndexItem.Status = FileIndexItem.ExifStatus.ReadOnly;
					fileIndexResultsList.Add(detailView.FileIndexItem);
					continue;
				}

				var statusResults =
					new StatusCodesHelper(_appSettings).FileCollectionsCheck(detailView);
				
				var statusModel = new FileIndexItem();
				statusModel.SetFilePath(subPath);
				statusModel.IsDirectory = false;

				if(new StatusCodesHelper(null).ReturnExifStatusError(statusModel, statusResults, fileIndexResultsList)) continue;
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
			if (fileIndexResultsList.All(p => p.Status != FileIndexItem.ExifStatus.Ok))
				return NotFound(fileIndexResultsList);
			
			// Creating a zip is a background task
			_bgTaskQueue.QueueBackgroundWorkItem(async token =>
			{
				
				var filePaths = new List<string>();
				var fileNames = new List<string>();

				foreach ( var item in fileIndexResultsList.Where(p => p.Status == FileIndexItem.ExifStatus.Ok).ToList() )
				{
					var sourceFile = _appSettings.DatabasePathToFilePath(item.FilePath);
					var sourceThumb = Path.Join(_appSettings.ThumbnailTempFolder,
						item.FileHash + ".jpg");
					
					if ( thumbnail )
						new Thumbnail(_appSettings,_exiftool).CreateThumb(item);
					
					filePaths.Add(thumbnail ? sourceThumb : sourceFile ); // has:notHas
					fileNames.Add(item.FileName);
				}

				CreateZip(filePaths,fileNames,zipHash);
				
				// Write a single file to be sure that writing is ready
				var doneFileFullPath = Path.Join(_appSettings.TempFolder,zipHash) + ".done";
				new PlainTextFileHelper().WriteFile(doneFileFullPath,"OK");
				Console.WriteLine("<<<<<<<");

			});
			
			// for the rest api
			return Json(zipHash);
		}

		[HttpGet("/export/zip")]
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
