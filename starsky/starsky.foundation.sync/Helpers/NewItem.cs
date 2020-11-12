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
	public class NewItem
	{
		private readonly IStorage _subPathStorage;
		private readonly IReadMeta _readMeta;

		public NewItem(IStorage subPathStorage, IReadMeta readMeta)
		{
			_subPathStorage = subPathStorage;
			_readMeta = readMeta;
		}
		
		/// <summary>
		/// Returns only an object
		/// </summary>
		/// <param name="inputItem"></param>
		/// <returns></returns>
		public async Task<FileIndexItem> NewFileItem(FileIndexItem inputItem)
		{
			var updatedDatabaseItem = _readMeta.ReadExifAndXmpFromFile(inputItem.FilePath);
			updatedDatabaseItem.ImageFormat = ExtensionRolesHelper
				.GetImageFormat(_subPathStorage.ReadStream(inputItem.FilePath,50));

			// future: read json sidecar
			await SetFileHashStatus(inputItem, updatedDatabaseItem);
			updatedDatabaseItem.SetAddToDatabase();
			updatedDatabaseItem.SetLastEdited();
			updatedDatabaseItem.IsDirectory = false;
			updatedDatabaseItem.Size = _subPathStorage.Info(inputItem.FilePath).Size;

			updatedDatabaseItem.ParentDirectory = inputItem.ParentDirectory;
			return updatedDatabaseItem;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="dbItem"></param>
		/// <returns></returns>
		public async Task<FileIndexItem> PrepareUpdateFileItem(FileIndexItem dbItem)
		{
			var metaDataItem = _readMeta.ReadExifAndXmpFromFile(dbItem.FilePath);
			FileIndexCompareHelper.Compare(dbItem, metaDataItem);
			dbItem.Size = _subPathStorage.Info(dbItem.FilePath).Size;
			await SetFileHashStatus(dbItem, dbItem);

			return dbItem;
		}

		/// <summary>
		/// Set file hash when not exist
		/// </summary>
		/// <param name="fileIndexItem">contains filePath</param>
		/// <param name="updatedDatabaseItem">new created object</param>
		/// <returns></returns>
		private async Task SetFileHashStatus(FileIndexItem fileIndexItem, FileIndexItem updatedDatabaseItem)
		{
			updatedDatabaseItem.Status = FileIndexItem.ExifStatus.Ok;
			if ( string.IsNullOrEmpty(updatedDatabaseItem.FileHash) )
			{
				var (localHash, success) = await new FileHash(_subPathStorage).GetHashCodeAsync(fileIndexItem.FilePath);
				updatedDatabaseItem.FileHash = localHash;
				updatedDatabaseItem.Status = success
					? FileIndexItem.ExifStatus.Ok
					: FileIndexItem.ExifStatus.OperationNotSupported;
			}
		}
	}
}
