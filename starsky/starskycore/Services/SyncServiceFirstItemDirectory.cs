using System.Linq;
using starsky.foundation.storage.Services;
using starskycore.Helpers;

namespace starskycore.Services
{
	// Gives folder an thumbnail image (only if contains direct files)
	// SyncServiceFirstItemDirectory
	public partial class SyncService
	{
		// SubPath is the base folder, it scans subfolders
		public void FirstItemDirectory(string subPath = "/")
		{
			subPath = _query.SubPathSlashRemove(subPath);
			
			// Loop though all folders
			var subFoldersSubPath = _subPathStorage.GetDirectoryRecursive(subPath);
	        
			foreach (var singleFolderSubPath in subFoldersSubPath)
			{
				var dbItem = _query.GetObjectByFilePath(singleFolderSubPath);
				
				// Check if folder item exist in database
				if (dbItem == null) continue;
				
				var firstFileSubPath = _subPathStorage.GetAllFilesInDirectory(singleFolderSubPath)
					.FirstOrDefault(ExtensionRolesHelper.IsExtensionThumbnailSupported);

				if ( string.IsNullOrEmpty(firstFileSubPath) || ! _subPathStorage.ExistFile(firstFileSubPath) ) continue;
				
				// get hash from file
				var singleFileHash =  new FileHash(_subPathStorage).GetHashCode(firstFileSubPath);
				
				// compare both
				if ( dbItem.FileHash == singleFileHash ) continue;
				
				dbItem.FileHash = singleFileHash;
				_query.UpdateItem(dbItem);
			}
        }
    }
}