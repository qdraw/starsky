using System.Threading.Tasks;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;
using starsky.foundation.readmeta.Interfaces;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Services;

namespace starsky.foundation.sync.Helpers
{
	public class NewItem
	{
		private readonly IStorage _subPathStorage;
		private readonly IReadMeta _readMeta;

		public NewItem(IStorage subPathStorage, IReadMeta readMeta)
		{
			_subPathStorage = subPathStorage;
			_readMeta = readMeta;
		}


		
		public async Task<FileIndexItem> FileItem(FileIndexItem fileIndexItem)
		{
			var updatedDatabaseItem = _readMeta.ReadExifAndXmpFromFile(fileIndexItem.FilePath);
			updatedDatabaseItem.ImageFormat = ExtensionRolesHelper
				.GetImageFormat(_subPathStorage.ReadStream(fileIndexItem.FilePath,50));

			await SetFileHashStatus(fileIndexItem, updatedDatabaseItem);
			updatedDatabaseItem.SetAddToDatabase();
			updatedDatabaseItem.SetLastEdited();
			updatedDatabaseItem.IsDirectory = false;
			updatedDatabaseItem.Size = _subPathStorage.Info(fileIndexItem.FilePath).Size;

			updatedDatabaseItem.ParentDirectory = fileIndexItem.ParentDirectory;
			return updatedDatabaseItem;
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
