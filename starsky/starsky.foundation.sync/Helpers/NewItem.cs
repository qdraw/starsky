using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using starsky.foundation.database.Helpers;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;
using starsky.foundation.readmeta.Interfaces;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Services;

namespace starsky.foundation.sync.Helpers
{
	/// <summary>
	/// Scope is only a object 
	/// </summary>
	public sealed class NewItem
	{
		private readonly IStorage _subPathStorage;
		private readonly IReadMeta _readMeta;

		public NewItem(IStorage subPathStorage, IReadMeta readMeta)
		{
			_subPathStorage = subPathStorage;
			_readMeta = readMeta;
		}
		
		public async Task<List<FileIndexItem>> NewFileItemAsync(List<FileIndexItem> inputItems)
		{
			var result = new List<FileIndexItem>();
			foreach ( var inputItem in inputItems )
			{
				result.Add(await NewFileItemAsync(inputItem));
			}
			return result;
		}
		
		/// <summary>
		/// Returns only an object (no db update)
		/// </summary>
		/// <param name="inputItem">at least FilePath and ParentDirectory, fileHash is optional</param>
		/// <returns></returns>
		public async Task<FileIndexItem> NewFileItemAsync(FileIndexItem inputItem)
		{
			return await NewFileItemAsync(inputItem.FilePath, inputItem.FileHash,
				inputItem.ParentDirectory, inputItem.FileName);
		}

		/// <summary>
		/// Prepare an new item (no update in db)
		/// </summary>
		/// <param name="filePath">path of file</param>
		/// <param name="fileHash">optional could be null</param>
		/// <param name="parentDirectory">parent directory name</param>
		/// <param name="fileName">name without path</param>
		/// <returns></returns>
		private async Task<FileIndexItem> NewFileItemAsync(string filePath, string fileHash, string parentDirectory, string fileName)
		{
			var updatedDatabaseItem =  await _readMeta.ReadExifAndXmpFromFileAsync(filePath);
			updatedDatabaseItem!.ImageFormat = ExtensionRolesHelper
				.GetImageFormat(_subPathStorage.ReadStream(filePath,50));

			// future: read json sidecar
			await SetFileHashStatus(filePath, fileHash, updatedDatabaseItem);
			updatedDatabaseItem.SetAddToDatabase();
			var info = _subPathStorage.Info(filePath);
			
			updatedDatabaseItem.LastEdited = info.LastWriteTime;
			updatedDatabaseItem.IsDirectory = false;
			updatedDatabaseItem.Size = info.Size;
			updatedDatabaseItem.ParentDirectory = parentDirectory;
			updatedDatabaseItem.FileName = fileName;
			
			return updatedDatabaseItem;
		}

		/// <summary>
		/// Only update an item with updated content form disk
		/// </summary>
		/// <param name="dbItem">database item</param>
		/// <param name="size">byte size</param>
		/// <returns>the updated item</returns>
		public async Task<FileIndexItem> PrepareUpdateFileItemAsync(FileIndexItem dbItem, long size)
		{
			var metaDataItem = await _readMeta.ReadExifAndXmpFromFileAsync(dbItem.FilePath!);
			var compare = FileIndexCompareHelper.Compare(dbItem, metaDataItem);
			dbItem.Size = size;
			await SetFileHashStatus(dbItem.FilePath, dbItem.FileHash, dbItem);
			dbItem.LastChanged.AddRange(compare);
			if ( compare.Count == 0 )
			{
				dbItem.Status = FileIndexItem.ExifStatus.OkAndSame;
			}
		
			return dbItem;
		}

		/// <summary>
		/// Set file hash when not exist
		/// </summary>
		/// <param name="filePath">filePath</param>
		/// <param name="fileHash"></param>
		/// <param name="updatedDatabaseItem">new created object</param>
		/// <returns></returns>
		private async Task SetFileHashStatus(string filePath, string fileHash,  FileIndexItem updatedDatabaseItem)
		{
			updatedDatabaseItem.Status = FileIndexItem.ExifStatus.Ok;
			if ( string.IsNullOrEmpty(fileHash) )
			{
				var (localHash, success) = await new FileHash(_subPathStorage).GetHashCodeAsync(filePath);
				updatedDatabaseItem.FileHash = localHash;
				updatedDatabaseItem.Status = success
					? FileIndexItem.ExifStatus.Ok
					: FileIndexItem.ExifStatus.OperationNotSupported;
			}
		}
	}
}
