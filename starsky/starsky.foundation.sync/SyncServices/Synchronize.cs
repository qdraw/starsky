using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
	public class Synchronize : ISynchronize
	{
		private readonly IStorage _subPathStorage;
		private readonly SyncSingleFile _syncSingleFile;
		private readonly SyncRemove _syncRemove;
		private readonly IConsole _console;
		private readonly SyncFolder _syncFolder;
		private readonly SyncIgnoreCheck _syncIgnoreCheck;

		public Synchronize(AppSettings appSettings, IQuery query, ISelectorStorage selectorStorage, IWebLogger logger)
		{
			_console = new ConsoleWrapper();
			_subPathStorage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
			_syncSingleFile = new SyncSingleFile(appSettings, query, _subPathStorage, logger);
			_syncRemove = new SyncRemove(appSettings, query);
			_syncFolder = new SyncFolder(appSettings, query, selectorStorage, _console,logger);
			_syncIgnoreCheck = new SyncIgnoreCheck(appSettings, _console);
		}
		
		public async Task<List<FileIndexItem>> Sync(string subPath, bool recursive = true)
		{
			// Prefix / for database
			subPath = PathHelper.PrefixDbSlash(subPath);
			if ( subPath != "/" ) subPath = PathHelper.RemoveLatestSlash(subPath);
			
			if ( FilterCommonTempFiles.Filter(subPath)  || _syncIgnoreCheck.Filter(subPath)  ) 
				return FilterCommonTempFiles.DefaultOperationNotSupported(subPath);

			_console.WriteLine($"[Synchronize] Sync {subPath}");
			
			// ReSharper disable once ConvertSwitchStatementToSwitchExpression
			switch ( _subPathStorage.IsFolderOrFile(subPath) )
			{
				case FolderOrFileModel.FolderOrFileTypeList.Folder:
					return await _syncFolder.Folder(subPath);
				case FolderOrFileModel.FolderOrFileTypeList.File:
					var item = await _syncSingleFile.SingleFile(subPath);
					return new List<FileIndexItem>{item};
				case FolderOrFileModel.FolderOrFileTypeList.Deleted:
					return await _syncRemove.Remove(subPath);
				default:
					throw new AggregateException("enum is not valid");
			}
		}

		public async Task<List<FileIndexItem>> Sync(List<string> subPaths, bool recursive = true)
		{
			var results = new List<FileIndexItem>();
			foreach ( var subPath in subPaths )
			{
				results.AddRange(await Sync(subPath, recursive));
			}
			return results;
		}
	}
}
