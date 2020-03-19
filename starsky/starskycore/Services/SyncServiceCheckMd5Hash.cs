using System;
using System.Collections.Generic;
using System.Linq;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;
using starsky.foundation.storage.Services;

namespace starskycore.Services
{
    public partial class SyncService
    {
        // Loop though a local file list and 
        // checks if the fileHash in the database is up to date
        // if the hash is not up to date =
        //      => remove item => add new item

        private void CheckMd5Hash(
            IEnumerable<string> localSubFolderDbStyle,
            IReadOnlyCollection<FileIndexItem> databaseFileList
        )
        {
            foreach (var itemLocal in localSubFolderDbStyle)
            {
                if(_appSettings.Verbose) Console.WriteLine("CheckMd5Hash: (path): " + itemLocal);

                var dbItem = databaseFileList.FirstOrDefault(p => p.FilePath == itemLocal);
                if (dbItem != null)
                {
                    // Check if Hash is changed
	                var localHash = new FileHash(_subPathStorage).GetHashCode(itemLocal);

                    if(_appSettings.Verbose) Console.WriteLine("localHash: " + localHash);

                    if (localHash != dbItem.FileHash)
                    {
                        _query.RemoveItem(dbItem);

                        // Read data from file
	                    var updatedDatabaseItem = _readMeta.ReadExifAndXmpFromFile(itemLocal);
	                    updatedDatabaseItem.ImageFormat = ExtensionRolesHelper.GetImageFormat(_subPathStorage.ReadStream(itemLocal,160));
	                    updatedDatabaseItem.FileHash = localHash;
                        updatedDatabaseItem.SetAddToDatabase();
	                    updatedDatabaseItem.SetLastEdited();
                        updatedDatabaseItem.IsDirectory = false;
                        updatedDatabaseItem.ParentDirectory = dbItem.ParentDirectory;
                        _query.AddItem(updatedDatabaseItem);
                    }
                }

            }
        }
    }
}
