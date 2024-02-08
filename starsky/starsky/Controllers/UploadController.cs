using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using starsky.Attributes;
using starsky.feature.import.Interfaces;
using starsky.feature.import.Models;
using starsky.feature.realtime.Interface;
using starsky.foundation.database.Helpers;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.http.Streaming;
using starsky.foundation.thumbnailmeta.Interfaces;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Helpers;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starsky.foundation.sync.SyncServices;

namespace starsky.Controllers
{
	[Authorize] // <- should be logged in!
	[SuppressMessage("Usage", "S5693:Make sure the content " +
							  "length limit is safe here", Justification = "Is checked")]
	public sealed class UploadController : Controller
	{
		private readonly AppSettings _appSettings;
		private readonly IImport _import;
		private readonly IStorage _iStorage;
		private readonly IStorage _iHostStorage;
		private readonly IQuery _query;
		private readonly ISelectorStorage _selectorStorage;
		private readonly IRealtimeConnectionsService _realtimeService;
		private readonly IWebLogger _logger;
		private readonly IMetaExifThumbnailService _metaExifThumbnailService;
		private readonly IMetaUpdateStatusThumbnailService _metaUpdateStatusThumbnailService;

		[SuppressMessage("Usage",
			"S107: Constructor has 8 parameters, which is greater than the 7 authorized")]
		public UploadController(IImport import, AppSettings appSettings,
			ISelectorStorage selectorStorage, IQuery query,
			IRealtimeConnectionsService realtimeService, IWebLogger logger,
			IMetaExifThumbnailService metaExifThumbnailService,
			IMetaUpdateStatusThumbnailService metaUpdateStatusThumbnailService)
		{
			_appSettings = appSettings;
			_import = import;
			_query = query;
			_selectorStorage = selectorStorage;
			_iStorage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
			_iHostStorage = selectorStorage.Get(SelectorStorage.StorageServices.HostFilesystem);
			_realtimeService = realtimeService;
			_logger = logger;
			_metaExifThumbnailService = metaExifThumbnailService;
			_metaUpdateStatusThumbnailService =
				metaUpdateStatusThumbnailService;
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
		[ProducesResponseType(typeof(List<ImportIndexItem>), 200)] // yes
		[ProducesResponseType(typeof(string), 400)]
		[ProducesResponseType(typeof(List<ImportIndexItem>), 404)]
		[ProducesResponseType(typeof(List<ImportIndexItem>),
			415)] // Wrong input (e.g. wrong extenstion type)
		[Produces("application/json")]
		public async Task<IActionResult> UploadToFolder()
		{
			var to = Request.Headers["to"].ToString();
			if ( string.IsNullOrWhiteSpace(to) ) return BadRequest("missing 'to' header");

			var parentDirectory = GetParentDirectoryFromRequestHeader();
			if ( parentDirectory == null )
			{
				return NotFound(new ImportIndexItem
				{
					Status = ImportStatus.ParentDirectoryNotFound
				});
			}

			var tempImportPaths = await Request.StreamFile(_appSettings, _selectorStorage);

			var fileIndexResultsList = await _import.Preflight(tempImportPaths,
				new ImportSettingsModel { IndexMode = false });
			// fail/pass, right type, string=subPath, string?2= error reason
			var metaResultsList = new List<(bool, bool, string, string?)>();

			for ( var i = 0; i < fileIndexResultsList.Count; i++ )
			{
				if ( fileIndexResultsList[i].Status != ImportStatus.Ok )
				{
					continue;
				}

				var tempFileStream = _iHostStorage.ReadStream(tempImportPaths[i]);

				var fileName = Path.GetFileName(tempImportPaths[i]);

				// subPath is always unix style
				var subPath = PathHelper.AddSlash(parentDirectory) + fileName;
				if ( parentDirectory == "/" ) subPath = parentDirectory + fileName;

				// to get the output in the result right
				fileIndexResultsList[i].FileIndexItem!.FileName = fileName;
				fileIndexResultsList[i].FileIndexItem!.ParentDirectory = parentDirectory;
				fileIndexResultsList[i].FilePath = subPath;
				// Do sync action before writing it down
				fileIndexResultsList[i].FileIndexItem =
					await SyncItem(fileIndexResultsList[i].FileIndexItem!);

				var writeStatus =
					await _iStorage.WriteStreamAsync(tempFileStream, subPath + ".tmp");
				await tempFileStream.DisposeAsync();

				// to avoid partly written stream to be read by an other application
				_iStorage.FileDelete(subPath);
				_iStorage.FileMove(subPath + ".tmp", subPath);
				_logger.LogInformation($"[UploadController] write {subPath} is {writeStatus}");

				// clear directory cache
				_query.RemoveCacheParentItem(subPath);

				var deleteStatus = _iHostStorage.FileDelete(tempImportPaths[i]);
				_logger.LogInformation(
					$"[UploadController] delete {tempImportPaths[i]} is {deleteStatus}");

				var parentPath = Directory.GetParent(tempImportPaths[i])?.FullName;
				if ( !string.IsNullOrEmpty(parentPath) && parentPath != _appSettings.TempFolder )
				{
					_iHostStorage.FolderDelete(parentPath);
				}

				metaResultsList.Add(( await _metaExifThumbnailService.AddMetaThumbnail(subPath,
					fileIndexResultsList[i].FileIndexItem!.FileHash!) ));
			}

			// send all uploads as list
			var socketResult = fileIndexResultsList
				.Where(p => p.Status == ImportStatus.Ok)
				.Select(item => item.FileIndexItem).Cast<FileIndexItem>().ToList();

			var webSocketResponse = new ApiNotificationResponseModel<List<FileIndexItem>>(
				socketResult, ApiNotificationType.UploadFile);
			await _realtimeService.NotificationToAllAsync(webSocketResponse,
				CancellationToken.None);

			await _metaUpdateStatusThumbnailService.UpdateStatusThumbnail(metaResultsList);

			// Wrong input (extension is not allowed)
			if ( fileIndexResultsList.TrueForAll(p => p.Status == ImportStatus.FileError) )
			{
				_logger.LogInformation($"Wrong input extension is not allowed" +
									   $" {string.Join(",", fileIndexResultsList.Select(p => p.FilePath))}");
				Response.StatusCode = 415;
			}

			return Json(fileIndexResultsList);
		}

		/// <summary>
		/// Perform database updates
		/// </summary>
		/// <param name="metaDataItem">to update to</param>
		/// <returns>updated item</returns>
		private async Task<FileIndexItem> SyncItem(FileIndexItem metaDataItem)
		{
			var itemFromDatabase = await _query.GetObjectByFilePathAsync(metaDataItem.FilePath!);
			if ( itemFromDatabase == null )
			{
				AddOrRemoveXmpSidecarFileToDatabase(metaDataItem);
				await _query.AddItemAsync(metaDataItem);
				return metaDataItem;
			}

			FileIndexCompareHelper.Compare(itemFromDatabase, metaDataItem);
			AddOrRemoveXmpSidecarFileToDatabase(metaDataItem);

			await _query.UpdateItemAsync(itemFromDatabase);
			return itemFromDatabase;
		}

		private void AddOrRemoveXmpSidecarFileToDatabase(FileIndexItem metaDataItem)
		{
			if ( _iStorage.ExistFile(ExtensionRolesHelper.ReplaceExtensionWithXmp(metaDataItem
					.FilePath)) )
			{
				metaDataItem.AddSidecarExtension("xmp");
				return;
			}

			metaDataItem.RemoveSidecarExtension("xmp");
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
				_logger.LogInformation("[IsValidXml] non valid xml");
				return false;
			}
		}

		/// <summary>
		/// Upload sidecar file to specific folder (does not check if already has been imported)
		/// Use the header 'to' to determine the location to where to upload
		/// Add header 'filename' when uploading direct without form
		/// (ActionResult UploadToFolderSidecarFile)
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
			_logger.LogInformation($"[UploadToFolderSidecarFile] to:{to}");

			var parentDirectory = GetParentDirectoryFromRequestHeader();
			if ( parentDirectory == null )
			{
				return NotFound(new ImportIndexItem());
			}

			var tempImportPaths = await Request.StreamFile(_appSettings, _selectorStorage);

			var importedList = new List<string>();
			foreach ( var tempImportSinglePath in tempImportPaths )
			{
				var data = await StreamToStringHelper.StreamToStringAsync(
					_iHostStorage.ReadStream(tempImportSinglePath));
				if ( !IsValidXml(data) ) continue;

				var tempFileStream = _iHostStorage.ReadStream(tempImportSinglePath);
				var fileName = Path.GetFileName(tempImportSinglePath);

				var subPath = PathHelper.AddSlash(parentDirectory) + fileName;
				if ( parentDirectory == "/" ) subPath = parentDirectory + fileName;

				if ( _appSettings.UseDiskWatcher == false )
				{
					await new SyncSingleFile(_appSettings, _query,
						_iStorage, null!, _logger).UpdateSidecarFile(subPath);
				}

				await _iStorage.WriteStreamAsync(tempFileStream, subPath);
				await tempFileStream.DisposeAsync();
				importedList.Add(subPath);

				var deleteStatus = _iHostStorage.FileDelete(tempImportSinglePath);
				_logger.LogInformation($"delete {tempImportSinglePath} is {deleteStatus}");
			}

			if ( importedList.Count == 0 )
			{
				Response.StatusCode = 415;
			}

			return Json(importedList);
		}

		internal string? GetParentDirectoryFromRequestHeader()
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
			if ( !_iStorage.ExistFolder(PathHelper.RemoveLatestSlash(to)) ) return null;
			return PathHelper.RemoveLatestSlash(to);
		}
	}
}
