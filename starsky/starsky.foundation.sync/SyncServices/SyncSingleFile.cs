using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.database.Query;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starsky.foundation.readmeta.Services;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starsky.foundation.sync.Helpers;

namespace starsky.foundation.sync.SyncServices
{
	public class SyncSingleFile
	{
		private readonly IStorage _subPathStorage;
		private readonly AppSettings _appSettings;
		private readonly IQuery _query;
		private readonly NewItem _newItem;

		public SyncSingleFile(AppSettings appSettings, IQuery query, ISelectorStorage selectorStorage)
		{
			_appSettings = appSettings;
			_subPathStorage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
			_query = query;
			_newItem = new NewItem(_subPathStorage, new ReadMeta(_subPathStorage, _appSettings));
		}
		
		internal async Task<List<FileIndexItem>> SingleFile(string subPath)
		{
			Console.WriteLine($"sync file {subPath}" );
			var statusItem = new FileIndexItem(subPath);

			// File extension is not supported
			if ( !ExtensionRolesHelper.IsExtensionSyncSupported(subPath) )
			{
				statusItem.Status = FileIndexItem.ExifStatus.OperationNotSupported;
				return new List<FileIndexItem>{statusItem};
			}

			// File check if jpg #not corrupt
			var imageFormat = ExtensionRolesHelper.GetImageFormat(_subPathStorage.ReadStream(subPath,160));
			// ReSharper disable once InvertIf
			if ( !ExtensionRolesHelper.ExtensionSyncSupportedList.Contains(imageFormat.ToString()) )
			{
				statusItem.Status = FileIndexItem.ExifStatus.OperationNotSupported;
				return new List<FileIndexItem>{statusItem};
			}

			var dbItem =  await _query.GetObjectByFilePathAsync(subPath);
			// // // when item does not exist in Database
			if ( dbItem == null )
			{
				dbItem = await _newItem.FileItem(statusItem);
			}
			
			
			
			// await _query.AddParentItemsAsync(subPath);
			return new List<FileIndexItem>();
		}
	}
}
