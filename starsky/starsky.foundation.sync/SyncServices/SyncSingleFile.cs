using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.sync.Helpers;
using starsky.foundation.sync.SyncInterfaces;

namespace starsky.foundation.sync.SyncServices
{
	public sealed class SyncSingleFile
	{
		private readonly IQuery _query;
		private readonly IWebLogger _logger;
		private readonly AppSettings _appSettings;
		private readonly SyncMultiFile _syncMultiFile;
		private readonly CheckForStatusNotOkHelper _checkForStatusNotOkHelper;

		public SyncSingleFile(AppSettings appSettings, IQuery query, IStorage subPathStorage,
			IMemoryCache? memoryCache, IWebLogger logger)
		{
			_appSettings = appSettings;
			_query = query;
			_checkForStatusNotOkHelper = new CheckForStatusNotOkHelper(subPathStorage);
			_logger = logger;
			_syncMultiFile = new SyncMultiFile(appSettings, query, subPathStorage, memoryCache, logger);
		}

		/// <summary>
		/// For Checking single items without querying the database
		/// </summary>
		/// <param name="subPath">path</param>
		/// <param name="dbItems">current item, can be null</param>
		/// <param name="updateDelegate">push updates realtime to the user and avoid waiting</param>
		/// <returns>updated item with status</returns>
		internal async Task<List<FileIndexItem>> SingleFile(string subPath,
			List<FileIndexItem>? dbItems,
			ISynchronize.SocketUpdateDelegate? updateDelegate = null)
		{
			// when item does not exist in db
			if ( dbItems?.Find(p => p.FilePath == subPath) == null )
			{
				return await SingleFile(subPath);
			}

			return await _syncMultiFile.MultiFile(dbItems.ToList(), updateDelegate);
		}

		/// <summary>
		/// Query the database and check if an single item has changed
		/// </summary>
		/// <param name="subPath">path</param>
		/// <param name="updateDelegate">realtime updates, can be null to ignore</param>
		/// <returns>updated item with status</returns>
		internal async Task<List<FileIndexItem>> SingleFile(string subPath,
			ISynchronize.SocketUpdateDelegate? updateDelegate = null)
		{
			// route with database check
			if ( _appSettings.ApplicationType == AppSettings.StarskyAppType.WebController )
			{
				_logger.LogInformation($"[SingleFile/db] info {subPath} " + Synchronize.DateTimeDebug());
			}

			// ignore all the 'wrong' files
			var statusItems = _checkForStatusNotOkHelper.CheckForStatusNotOk(subPath);

			if ( statusItems.FirstOrDefault()!.Status != FileIndexItem.ExifStatus.Ok )
			{
				_logger.LogDebug("[SingleFile/db] status " +
								 "{Status} for {subPath} {Time}",
					statusItems.FirstOrDefault()!.Status, Synchronize.DateTimeDebug());
				return statusItems;
			}

			var scanItems = new List<FileIndexItem>();
			var dbItems = await _query.GetObjectsByFilePathAsync(subPath, true);
			foreach ( var item in statusItems )
			{
				var dbItem = dbItems.Find(p => item.FilePath == p.FilePath);
				if ( dbItem != null )
				{
					scanItems.Add(dbItem);
					continue;
				}
				item.Status = FileIndexItem.ExifStatus.NotFoundNotInIndex;
				scanItems.Add(item);
			}

			foreach ( var item in dbItems.Where(item => scanItems.TrueForAll(p => p.FilePath != item.FilePath)) )
			{
				scanItems.Add(item);
			}

			return await _syncMultiFile.MultiFile(scanItems, updateDelegate);
		}

		/// <summary>
		/// Sidecar files don't have an own item, but there referenced by file items
		/// in the method xmp files are added to the AddSidecarExtension list.
		/// </summary>
		/// <param name="xmpSubPath">sidecar item</param>
		/// <returns>completed task</returns>
		public async Task UpdateSidecarFile(string xmpSubPath)
		{
			if ( !ExtensionRolesHelper.IsExtensionSidecar(xmpSubPath) )
			{
				return;
			}

			var parentPath = FilenamesHelper.GetParentPath(xmpSubPath);
			var fileNameWithoutExtension = FilenamesHelper.GetFileNameWithoutExtension(xmpSubPath);

			var directoryWithFileIndexItems = ( await
				_query.GetAllFilesAsync(parentPath) ).Where(
				p => p.ParentDirectory == parentPath &&
					 p.FileCollectionName == fileNameWithoutExtension).ToList();

			await UpdateSidecarFile(xmpSubPath, directoryWithFileIndexItems);
		}

		/// <summary>
		/// this updates the main database item for a sidecar file
		/// </summary>
		/// <param name="xmpSubPath">sidecar file</param>
		/// <param name="directoryWithFileIndexItems">directory where the sidecar is located</param>
		/// <returns>completed task</returns>
		internal async Task<bool> UpdateSidecarFile(string xmpSubPath, List<FileIndexItem> directoryWithFileIndexItems)
		{
			if ( !ExtensionRolesHelper.IsExtensionSidecar(xmpSubPath) )
			{
				return false;
			}
			var sidecarExt =
				FilenamesHelper.GetFileExtensionWithoutDot(xmpSubPath);

			foreach ( var item in
				directoryWithFileIndexItems.Where(item => !item.SidecarExtensionsList.Contains(sidecarExt)) )
			{
				item.AddSidecarExtension(sidecarExt);
				await _query.UpdateItemAsync(item);
			}
			return true;
		}

	}
}
