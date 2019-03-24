using System.Linq;
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
			var subFoldersSubPath = _iStorage.GetDirectoryRecursive(subPath);
	        
			foreach (var singleFolderSubPath in subFoldersSubPath)
			{
				var dbItem = _query.GetObjectByFilePath(singleFolderSubPath);
				
				// Check if folder item exist in database
				if (dbItem == null) continue;
				
				var firstFileSubPath = _iStorage.GetAllFilesInDirectory(singleFolderSubPath)
					.FirstOrDefault(ExtensionRolesHelper.IsExtensionThumbnailSupported);

				if ( string.IsNullOrEmpty(firstFileSubPath) || ! _iStorage.ExistFile(firstFileSubPath) ) continue;
				
				// get hash from file
				var singleFileHash =  new FileHash(_iStorage).GetHashCode(firstFileSubPath);
				
				// compare both
				if ( dbItem.FileHash == singleFileHash ) continue;
				
				dbItem.FileHash = singleFileHash;
				_query.UpdateItem(dbItem);
			}
        }
    }
}