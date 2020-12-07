using System;
using System.Linq;
using System.Threading.Tasks;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.readmeta.Services;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Services;
using starsky.foundation.sync.Helpers;

namespace starsky.foundation.sync.SyncServices
{
	public class SyncSingleFile
	{
		private readonly IStorage _subPathStorage;
		private readonly IQuery _query;
		private readonly NewItem _newItem;
		private readonly IConsole _console;
		private readonly AppSettings _appSettings;

		public SyncSingleFile(AppSettings appSettings, IQuery query, IStorage subPathStorage, IConsole console)
		{
			_appSettings = appSettings;
			_subPathStorage = subPathStorage;
			_query = query;
			_newItem = new NewItem(_subPathStorage, new ReadMeta(_subPathStorage, appSettings));
			_console = console;
		}

		/// <summary>
		/// For Checking single items without querying the database
		/// </summary>
		/// <param name="subPath">path</param>
		/// <param name="dbItem">current item, can be null</param>
		/// <returns>updated item with status</returns>
		internal async Task<FileIndexItem> SingleFile(string subPath, FileIndexItem dbItem)
		{
			// when item does not exist in db
			if ( dbItem == null )
			{
				return await SingleFile(subPath);
			}

			// Route without database check
			if (_appSettings.Verbose ) _console?.WriteLine($"sync file {subPath}" );
			
			// Sidecar files are updated but ignored by the process
			await UpdateSidecarFile(subPath);

			var statusItem = CheckForStatusNotOk(subPath);
			if ( statusItem.Status != FileIndexItem.ExifStatus.Ok )
			{
				return statusItem;
			}
			
			var (isSame, updatedDbItem) = await SizeFileHashIsTheSame(dbItem);
			if ( isSame ) return updatedDbItem;

			return await UpdateItem(dbItem, dbItem.Size, subPath);
		}

		/// <summary>
		/// Query the database and check if an single item has changed
		/// </summary>
		/// <param name="subPath">path</param>
		/// <returns>updated item with status</returns>
		internal async Task<FileIndexItem> SingleFile(string subPath)
		{
			// route with database check
			if (_appSettings.Verbose ) _console?.WriteLine($"sync file {subPath}" );

			// Sidecar files are updated but ignored by the process
			await UpdateSidecarFile(subPath);
			
			// ignore all the 'wrong' files
			var statusItem = CheckForStatusNotOk(subPath);

			if ( statusItem.Status != FileIndexItem.ExifStatus.Ok )
			{
				return statusItem;
			}

			var dbItem =  await _query.GetObjectByFilePathAsync(subPath);
			// // // when item does not exist in Database
			if ( dbItem == null )
			{
				return await NewItem(statusItem, subPath);
			}

			var (isSame, updatedDbItem) = await SizeFileHashIsTheSame(dbItem);
			if ( isSame ) return updatedDbItem;

			return await UpdateItem(dbItem, updatedDbItem.Size, subPath);
		}

		/// <summary>
		/// When the same stop checking and return value
		/// </summary>
		/// <param name="dbItem">item that contain size and fileHash</param>
		/// <returns>database item</returns>
		private async Task<Tuple<bool,FileIndexItem>> SizeFileHashIsTheSame(FileIndexItem dbItem)
		{
			// when size is the same dont update
			var (isByteSizeTheSame, size) = CompareByteSizeIsTheSame(dbItem);
			dbItem.Size = size;
			if (isByteSizeTheSame) return new Tuple<bool, FileIndexItem>(true ,dbItem);

			// when byte hash is different update
			var (fileHashTheSame,_ ) = await CompareFileHashIsTheSame(dbItem);

			return new Tuple<bool, FileIndexItem>(fileHashTheSame,dbItem);
		}

		internal FileIndexItem AddDeleteStatus(FileIndexItem dbItem)
		{
			if ( dbItem == null ) return null;
			if ( dbItem.Tags.Contains("!delete!") )
			{
				dbItem.Status = FileIndexItem.ExifStatus.Deleted;
			}
			return dbItem;
		}

		/// <summary>
		/// When the file is not supported or does not exist return status
		/// </summary>
		/// <param name="subPath">relative path</param>
		/// <returns>item with status</returns>
		private FileIndexItem CheckForStatusNotOk(string subPath)
		{
			var statusItem = new FileIndexItem(subPath){Status = FileIndexItem.ExifStatus.Ok};

			// File extension is not supported
			if ( !ExtensionRolesHelper.IsExtensionSyncSupported(subPath) )
			{
				statusItem.Status = FileIndexItem.ExifStatus.OperationNotSupported;
				return statusItem;
			}

			// File check if jpg #not corrupt
			var imageFormat = ExtensionRolesHelper.GetImageFormat(_subPathStorage.ReadStream(subPath,160));
			if ( imageFormat == ExtensionRolesHelper.ImageFormat.notfound )
			{
				statusItem.Status = FileIndexItem.ExifStatus.NotFoundSourceMissing;
				return statusItem;
			}
			
			// ReSharper disable once InvertIf
			if ( !ExtensionRolesHelper.ExtensionSyncSupportedList.Contains(imageFormat.ToString()) )
			{
				statusItem.Status = FileIndexItem.ExifStatus.OperationNotSupported;
				return statusItem;
			}
			return statusItem;
		}

		/// <summary>
		/// Create an new item in the database
		/// </summary>
		/// <param name="statusItem">contains the status</param>
		/// <param name="subPath">relative path</param>
		/// <returns>database item</returns>
		private async Task<FileIndexItem> NewItem(FileIndexItem statusItem, string subPath)
		{
			// Add a new Item
			var dbItem = await _newItem.NewFileItem(statusItem);

			// When not OK do not Add (fileHash issues)
			if ( dbItem.Status != FileIndexItem.ExifStatus.Ok ) return dbItem;
				
			await _query.AddItemAsync(dbItem);
			await _query.AddParentItemsAsync(subPath);
			AddDeleteStatus(dbItem);
			return dbItem;
		}

		/// <summary>
		/// Update item to database
		/// </summary>
		/// <param name="dbItem">item to update</param>
		/// <param name="size">byte size</param>
		/// <param name="subPath">relative path</param>
		/// <returns>same item</returns>
		private async Task<FileIndexItem> UpdateItem(FileIndexItem dbItem, long size, string subPath)
		{
			var updateItem = await _newItem.PrepareUpdateFileItem(dbItem, size);
			await _query.UpdateItemAsync(updateItem);
			await _query.AddParentItemsAsync(subPath);
			AddDeleteStatus(dbItem);
			return updateItem;
		}
		
		/// <summary>
		/// Compare the file hash en return 
		/// </summary>
		/// <param name="dbItem">database item</param>
		/// <returns>tuple that has value: is the same; and the fileHash</returns>
		private async Task<Tuple<bool,string>> CompareFileHashIsTheSame(FileIndexItem dbItem)
		{
			var (localHash,_) = await new 
				FileHash(_subPathStorage).GetHashCodeAsync(dbItem.FilePath);
			var isTheSame = dbItem.FileHash == localHash;
			dbItem.FileHash = localHash;
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
			var isTheSame = dbItem.Size == storageByteSize;
			dbItem.Size = storageByteSize;
			return new Tuple<bool, long>(isTheSame, storageByteSize);
		}

		/// <summary>
		/// Sidecar files don't have an own item, but there referenced by file items
		/// in the method xmp files are added to the AddSidecarExtension list.
		/// </summary>
		/// <param name="subPath">sidecar item</param>
		/// <returns>completed task</returns>
		private async Task UpdateSidecarFile(string subPath)
		{
			if ( !ExtensionRolesHelper.IsExtensionSidecar(subPath) )
			{
				return;
			}

			var parentPath = FilenamesHelper.GetParentPath(subPath);
			var fileNameWithoutExtension = FilenamesHelper.GetFileNameWithoutExtension(subPath);

			var itemsInDirectories = (await 
				_query.GetAllFilesAsync(parentPath)).Where(
				p => p.ParentDirectory == parentPath &&
				     p.FileCollectionName == fileNameWithoutExtension).ToList();
			
			var sidecarExt =
				FilenamesHelper.GetFileExtensionWithoutDot(subPath);
			
			foreach ( var item in 
				itemsInDirectories.Where(item => !item.SidecarExtensionsList.Contains(sidecarExt)) )
			{
				item.AddSidecarExtension(sidecarExt);
				await _query.UpdateItemAsync(item);
			}
		}
	}
}
