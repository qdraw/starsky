using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

		public Synchronize(AppSettings appSettings, IQuery query, ISelectorStorage selectorStorage, 
			IServiceScopeFactory serviceScopeFactory)
		{
			_console = new ConsoleWrapper();
			_subPathStorage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
			_syncSingleFile = new SyncSingleFile(appSettings, query, selectorStorage, _console);
			_syncRemove = new SyncRemove(appSettings, serviceScopeFactory, query);
			_syncFolder = new SyncFolder(query, selectorStorage);
		}
		
		public async Task<List<FileIndexItem>> Sync(string subPath, bool recursive = true)
		{
			// Prefix / for database
			subPath = PathHelper.PrefixDbSlash(subPath);
			if ( subPath != "/" ) subPath = PathHelper.RemoveLatestSlash(subPath);

			_console.WriteLine(subPath);
			
			// ReSharper disable once ConvertSwitchStatementToSwitchExpression
			switch ( _subPathStorage.IsFolderOrFile(subPath) )
			{
				case FolderOrFileModel.FolderOrFileTypeList.Folder:
					return await _syncFolder.Folder(subPath);
				case FolderOrFileModel.FolderOrFileTypeList.File:
					_console.WriteLine("file");
					return await _syncSingleFile.SingleFile(subPath);
				case FolderOrFileModel.FolderOrFileTypeList.Deleted:
					_console.WriteLine("Remove");
					return await _syncRemove.Remove(subPath);
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
