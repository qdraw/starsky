using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.readmeta.Services;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Services;
using starsky.foundation.storage.Storage;
using starsky.foundation.sync.Helpers;

namespace starsky.foundation.sync.SyncServices
{
	public class SyncSingleFile
	{
		private readonly IStorage _subPathStorage;
		private readonly IQuery _query;
		private readonly NewItem _newItem;
		private readonly IConsole _console;

		public SyncSingleFile(AppSettings appSettings, IQuery query, ISelectorStorage selectorStorage, IConsole console)
		{
			_subPathStorage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
			_query = query;
			_newItem = new NewItem(_subPathStorage, new ReadMeta(_subPathStorage, appSettings));
			_console = console;
		}

		internal async Task<List<FileIndexItem>> SingleFile(string subPath)
		{
			_console.WriteLine($"sync file {subPath}" );
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
				// Add a new Item
				dbItem = await _newItem.NewFileItem(statusItem);

				await _query.AddItemAsync(dbItem);
				await _query.AddParentItemsAsync(subPath);
				return new List<FileIndexItem>{dbItem};
			}

			// when size is the same dont update
			var (isByteSizeTheSame, size) = CompareByteSizeIsTheSame(dbItem);
			if (isByteSizeTheSame) return new List<FileIndexItem> {dbItem};
			dbItem.Size = size;

			// when byte hash is different update
			var (fileHashTheSame, newFileHash ) = await CompareFileHashIsTheSame(dbItem);
			if ( fileHashTheSame ) return new List<FileIndexItem>{dbItem};
			dbItem.FileHash = newFileHash;
			
			var updateItem = await _newItem.PrepareUpdateFileItem(dbItem, size);
			await _query.UpdateItemAsync(updateItem);
			await _query.AddParentItemsAsync(subPath);
			return new List<FileIndexItem>{updateItem};

		}
		
		
		private async Task<Tuple<bool,string>> CompareFileHashIsTheSame(FileIndexItem dbItem)
		{
			var (localHash,_) = await new 
				FileHash(_subPathStorage).GetHashCodeAsync(dbItem.FilePath);
			var isTheSame = dbItem.FileHash == localHash;
			return new Tuple<bool, string>(isTheSame, localHash);
		}

		/// <summary>
		/// True when result is the same
		/// </summary>
		/// <param name="dbItem"></param>
		/// <returns></returns>
		private Tuple<bool,long> CompareByteSizeIsTheSame(FileIndexItem dbItem)
		{
			var storageByteSize = _subPathStorage.Info(dbItem.FilePath).Size;
			return new Tuple<bool, long>(dbItem.Size == storageByteSize, storageByteSize);
		}

	}
}
