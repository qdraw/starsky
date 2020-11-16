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

		public Synchronize(AppSettings appSettings, IQuery query, ISelectorStorage selectorStorage)
		{
			_console = new ConsoleWrapper();
			_subPathStorage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
			_syncSingleFile = new SyncSingleFile(appSettings, query, selectorStorage, _console);
			_syncRemove = new SyncRemove(query, _console);
		}
		
		public async Task<List<FileIndexItem>> Sync(string subPath, bool recursive = true)
		{
			// Prefix / for database
			subPath = PathHelper.PrefixDbSlash(subPath);
			subPath = PathHelper.RemoveLatestSlash(subPath);

			_console.WriteLine(subPath);
			
			// ReSharper disable once ConvertSwitchStatementToSwitchExpression
			switch ( _subPathStorage.IsFolderOrFile(subPath) )
			{
				case FolderOrFileModel.FolderOrFileTypeList.Folder:
					throw new NotImplementedException();
				case FolderOrFileModel.FolderOrFileTypeList.File:
					_console.WriteLine("file");
					return await _syncSingleFile.SingleFile(subPath);
				case FolderOrFileModel.FolderOrFileTypeList.Deleted:
					_console.WriteLine("Remove");
					return await _syncRemove.Remove(new []{subPath});
				default:
					throw new AggregateException("enum is not valid");
			}
		}

		public Task<List<FileIndexItem>> SingleFile(string subPath)
		{
			return _syncSingleFile.SingleFile(subPath);
		}

		
	}
}
