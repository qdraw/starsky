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
		
		public async Task<FileIndexItem> Item(FileIndexItem fileIndexItem, string localHash = null)
		{
			var updatedDatabaseItem = _readMeta.ReadExifAndXmpFromFile(fileIndexItem.FilePath);
			updatedDatabaseItem.ImageFormat = ExtensionRolesHelper.GetImageFormat(_subPathStorage.ReadStream(fileIndexItem.FilePath,50));
			
			var success = true;
			if ( localHash == null )
			{
				(localHash, success) = await new FileHash(_subPathStorage).GetHashCodeAsync(fileIndexItem.FilePath);
			}
			updatedDatabaseItem.FileHash = localHash;
			updatedDatabaseItem.SetAddToDatabase();
			updatedDatabaseItem.SetLastEdited();
			updatedDatabaseItem.IsDirectory = false;
			updatedDatabaseItem.Size = 0;
			updatedDatabaseItem.Status = success
				? FileIndexItem.ExifStatus.Ok
				: FileIndexItem.ExifStatus.OperationNotSupported;
			updatedDatabaseItem.ParentDirectory = fileIndexItem.ParentDirectory;
			return updatedDatabaseItem;
		}
	}
}
