using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.injection;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Services;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Models;
using starsky.foundation.storage.Storage;
using starsky.foundation.sync.Helpers;
using starsky.foundation.sync.SyncInterfaces;

namespace starsky.foundation.sync.SyncServices
{
	[Service(typeof(ISynchronize), InjectionLifetime = InjectionLifetime.Scoped)]
	public sealed class Synchronize : ISynchronize
	{
		private readonly ISyncAddThumbnailTable _syncAddThumbnail;
		private readonly IStorage _subPathStorage;
		private readonly SyncSingleFile _syncSingleFile;
		private readonly SyncRemove _syncRemove;
		private readonly ConsoleWrapper _console;
		private readonly SyncFolder _syncFolder;
		private readonly SyncIgnoreCheck _syncIgnoreCheck;
		private readonly SyncMultiFile _syncMultiFile;

		public Synchronize(AppSettings appSettings, IQuery query, ISelectorStorage selectorStorage, IWebLogger logger,
			ISyncAddThumbnailTable syncAddThumbnail, IServiceScopeFactory? serviceScopeFactory = null,
			IMemoryCache? memoryCache = null)
		{
			_syncAddThumbnail = syncAddThumbnail;
			_console = new ConsoleWrapper();
			_subPathStorage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
			_syncSingleFile = new SyncSingleFile(appSettings, query, _subPathStorage, memoryCache, logger);
			_syncRemove = new SyncRemove(appSettings, query, memoryCache, logger, serviceScopeFactory);
			_syncFolder = new SyncFolder(appSettings, query, selectorStorage, _console, logger, memoryCache, serviceScopeFactory);
			_syncIgnoreCheck = new SyncIgnoreCheck(appSettings, _console);
			_syncMultiFile = new SyncMultiFile(appSettings, query, _subPathStorage, memoryCache, logger);
		}

		public async Task<List<FileIndexItem>> Sync(string subPath,
			ISynchronize.SocketUpdateDelegate? updateDelegate = null,
			DateTime? childDirectoriesAfter = null)
		{
			return await _syncAddThumbnail.SyncThumbnailTableAsync(
				await SyncWithoutThumbnail(subPath, updateDelegate,
					childDirectoriesAfter));
		}

		/// <summary>
		/// Sync list by subPaths
		/// </summary>
		/// <param name="subPaths"></param>
		/// <param name="updateDelegate"></param>
		/// <returns></returns>
		public async Task<List<FileIndexItem>> Sync(List<string> subPaths,
			ISynchronize.SocketUpdateDelegate? updateDelegate = null)
		{
			var results = await _syncMultiFile.MultiFile(subPaths, updateDelegate);
			return await _syncAddThumbnail.SyncThumbnailTableAsync(results);
		}

		private async Task<List<FileIndexItem>> SyncWithoutThumbnail(string subPath,
			ISynchronize.SocketUpdateDelegate? updateDelegate = null,
			DateTime? childDirectoriesAfter = null)
		{
			// Prefix / for database
			subPath = PathHelper.PrefixDbSlash(subPath);
			if ( subPath != "/" )
			{
				subPath = PathHelper.RemoveLatestSlash(subPath);
			}

			if ( FilterCommonTempFiles.Filter(subPath) || _syncIgnoreCheck.Filter(subPath) )
			{
				return FilterCommonTempFiles.DefaultOperationNotSupported(subPath);
			}

			_console.WriteLine($"[Synchronize] Sync {subPath} {DateTimeDebug()}");

			// ReSharper disable once ConvertSwitchStatementToSwitchExpression
			switch ( _subPathStorage.IsFolderOrFile(subPath) )
			{
				case FolderOrFileModel.FolderOrFileTypeList.Folder:
					var syncFolder = await _syncFolder.Folder(subPath,
						updateDelegate, childDirectoriesAfter);
					return syncFolder;
				case FolderOrFileModel.FolderOrFileTypeList.File:
					return await _syncSingleFile.SingleFile(subPath, updateDelegate);
				case FolderOrFileModel.FolderOrFileTypeList.Deleted:
					return await _syncRemove.RemoveAsync(subPath, updateDelegate);
				default:
					throw new AggregateException("enum is not valid");
			}
		}

		internal static string DateTimeDebug()
		{
			return ": " + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss",
				CultureInfo.InvariantCulture);
		}
	}
}
