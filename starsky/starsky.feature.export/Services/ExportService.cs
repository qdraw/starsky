using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using starsky.feature.export.Interfaces;
using starsky.foundation.database.Helpers;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.injection;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.ArchiveFormats;
using starsky.foundation.storage.Helpers;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Models;
using starsky.foundation.storage.Storage;
using starsky.foundation.thumbnailgeneration.Services;

[assembly: InternalsVisibleTo("starskytest")]
namespace starsky.feature.export.Services
{
	[Service(typeof(IExport), InjectionLifetime = InjectionLifetime.Scoped)]
	public class ExportService: IExport
	{
		private readonly IQuery _query;
		private readonly AppSettings _appSettings;
		private readonly IStorage _iStorage;
		private readonly IStorage _thumbnailStorage;
		private readonly IStorage _hostFileSystemStorage;
		private readonly StatusCodesHelper _statusCodeHelper;
		private readonly IConsole _console;

		public ExportService(IQuery query, AppSettings appSettings, 
			ISelectorStorage selectorStorage, IConsole console)
		{
			_appSettings = appSettings;
			_query = query;
			_iStorage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
			_thumbnailStorage = selectorStorage.Get(SelectorStorage.StorageServices.Thumbnail);
			_hostFileSystemStorage = selectorStorage.Get(SelectorStorage.StorageServices.HostFilesystem);
			_statusCodeHelper = new StatusCodesHelper();
			_console = console;
		}

		public Tuple<string, List<FileIndexItem>> Preflight(string[] inputFilePaths, 
			bool collections = true, 
			bool thumbnail = false )
		{
			// the result list
			var fileIndexResultsList = new List<FileIndexItem>();

			foreach ( var subPath in inputFilePaths )
			{
				var detailView = _query.SingleItem(subPath, null, collections, false);
				if ( detailView?.FileIndexItem == null )
				{
					_statusCodeHelper.ReturnExifStatusError(new FileIndexItem(subPath), 
						FileIndexItem.ExifStatus.NotFoundNotInIndex,
						fileIndexResultsList);
					continue;
				}
				
				if ( _iStorage.IsFolderOrFile(detailView.FileIndexItem.FilePath) == 
				     FolderOrFileModel.FolderOrFileTypeList.Deleted )
				{
					_statusCodeHelper.ReturnExifStatusError(detailView.FileIndexItem, 
						FileIndexItem.ExifStatus.NotFoundSourceMissing,
						fileIndexResultsList);
					continue; 
				}
				
				if ( detailView.FileIndexItem.IsDirectory == true )
				{
					// Collection is only relevant for when selecting one file
					foreach ( var item in 
						_query.GetAllRecursive(detailView.FileIndexItem.FilePath).
							Where(item => _iStorage.ExistFile(item.FilePath)) )
					{
						item.Status = FileIndexItem.ExifStatus.Ok;
						fileIndexResultsList.Add(item);
					}
					continue;
				}
				
				// Now Add Collection based images
				var collectionSubPathList = detailView.GetCollectionSubPathList(detailView, collections, subPath);
				foreach ( var item in collectionSubPathList )
				{
					var itemDetailView = _query.SingleItem(item, null, 
						false, false).FileIndexItem;
					itemDetailView.Status = FileIndexItem.ExifStatus.Ok;
					fileIndexResultsList.Add(itemDetailView);
				}
			}

			var isThumbnail = thumbnail ? "TN" : "SR"; // has:notHas
			var zipHash = isThumbnail + GetName(fileIndexResultsList);
			
			return new Tuple<string, List<FileIndexItem>>(zipHash, fileIndexResultsList);
		}

		/// <summary>
		/// Based on the preflight create a Zip Export
		/// </summary>
		/// <param name="fileIndexResultsList">Result of Preflight</param>
		/// <param name="thumbnail">isThumbnail?</param>
		/// <param name="zipOutputFileName">filename of zip file (no extension)</param>
		/// <returns>nothing</returns>
		public async Task CreateZip(List<FileIndexItem> fileIndexResultsList, bool thumbnail, 
			string zipOutputFileName)
		{
			var filePaths = CreateListToExport(fileIndexResultsList, thumbnail);
			var fileNames = FilePathToFileName(filePaths, thumbnail);

			new Zipper().CreateZip(_appSettings.TempFolder,filePaths,fileNames,zipOutputFileName);
				
			// Write a single file to be sure that writing is ready
			var doneFileFullPath = Path.Combine(_appSettings.TempFolder,zipOutputFileName) + ".done";
			await _hostFileSystemStorage.
				WriteStreamAsync(new PlainTextFileHelper().StringToStream("OK"), doneFileFullPath);
			if(_appSettings.Verbose) _console.WriteLine("Zip done: " + doneFileFullPath);
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

			foreach ( var item in fileIndexResultsList.Where(p => 
				p.Status == FileIndexItem.ExifStatus.Ok).ToList() )
			{
				var sourceFile = _appSettings.DatabasePathToFilePath(item.FilePath);
				var sourceThumb = Path.Combine(_appSettings.ThumbnailTempFolder,
					item.FileHash + ".jpg");

				if ( thumbnail )
					new Thumbnail(_iStorage, _thumbnailStorage).CreateThumb(item.FilePath, item.FileHash);

				filePaths.Add(thumbnail ? sourceThumb : sourceFile); // has:notHas
				
				
				// when there is .xmp sidecar file
				if ( !thumbnail && ExtensionRolesHelper.IsExtensionForceXmp(item.FilePath) 
				                && _iStorage.ExistFile(
					                ExtensionRolesHelper.ReplaceExtensionWithXmp(item.FilePath)))
				{
					filePaths.Add(
						_appSettings.DatabasePathToFilePath(
							ExtensionRolesHelper.ReplaceExtensionWithXmp(item.FilePath))
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
		internal List<string> FilePathToFileName(IEnumerable<string> filePaths, bool thumbnail)
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
					var filename = subPath.Split('/').LastOrDefault(); // first a string
					fileNames.Add(filename);
					continue;
				}
				fileNames.Add(Path.GetFileName(filePath));
			}
			return fileNames;
		}

		/// <summary>
		/// to create a unique name of the zip using c# get hashcode
		/// </summary>
		/// <param name="fileIndexResultsList">list of objects with fileHashes</param>
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

		/// <summary>
		/// Is Zip Ready?
		/// </summary>
		/// <param name="zipOutputFileName">fileName without extension</param>
		/// <returns>null if status file is not found, true if done file exist</returns>
		public Tuple<bool?,string> StatusIsReady(string zipOutputFileName)
		{
			var sourceFullPath = Path.Combine(_appSettings.TempFolder,zipOutputFileName) + ".zip";
			var doneFileFullPath = Path.Combine(_appSettings.TempFolder,zipOutputFileName) + ".done";

			if ( !_hostFileSystemStorage.ExistFile(sourceFullPath)  ) return new Tuple<bool?, string>(null,null);

			// Read a single file to be sure that writing is ready
			return new Tuple<bool?, string>(_hostFileSystemStorage.ExistFile(doneFileFullPath), sourceFullPath);
		}
	}
}
