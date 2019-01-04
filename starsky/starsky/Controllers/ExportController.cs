using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
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
		[HttpPost("/export/zip")]
		public IActionResult Zip(string f, bool collections = true)
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
				var collectionFullPaths = _appSettings.DatabasePathToFilePath(collectionSubPathList);

				var fileCompontentList = _readMeta.ReadExifAndXmpFromFileAddFilePathHash(collectionFullPaths.ToArray());
				fileIndexResultsList.AddRange(fileCompontentList);
			}
			var sourceFullPath =  CreateZip(fileIndexResultsList.Where(p => p.Status == FileIndexItem.ExifStatus.Ok).ToList());
			
			FileStream fs = System.IO.File.OpenRead(sourceFullPath);
			// Return the right mime type
			return File(fs, MimeHelper.GetMimeTypeByFileName(sourceFullPath));
		}

		public string CreateZip(List<FileIndexItem> fileIndexResultsList)
		{
			var tempFileNameStringBuilder = new StringBuilder();
			foreach ( var item in fileIndexResultsList )
			{
				tempFileNameStringBuilder.Append(item.FileHash);
			}
			var tempFileFullPath = Path.Join(_appSettings.TempFolder,tempFileNameStringBuilder.ToString()) + ".zip";

			if(System.IO.File.Exists(tempFileFullPath))
			{
				return tempFileFullPath;
			}
			ZipArchive zip = ZipFile.Open(tempFileFullPath, ZipArchiveMode.Create);
			foreach (var item in fileIndexResultsList)
			{
				zip.CreateEntryFromFile(_appSettings.DatabasePathToFilePath(item.FilePath), item.FileName);
			}
			zip.Dispose();
			return tempFileFullPath;
		}
	}
}
