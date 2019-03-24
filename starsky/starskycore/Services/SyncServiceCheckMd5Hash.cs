using System;
using System.Collections.Generic;
using System.Linq;
using starskycore.Helpers;
using starskycore.Models;

namespace starskycore.Services
{
    public partial class SyncService
    {
        // Loop thoug a local file list and 
        // checks if the filehash in the database is up to date
        // if the hash is not up to date =
        //      => remove item => add new item

        public void CheckMd5Hash(
            List<string> localSubFolderDbStyle,
            List<FileIndexItem> databaseFileList
        )
        {
            foreach (var itemLocal in localSubFolderDbStyle)
            {
                if(_appSettings.Verbose) Console.WriteLine("CheckMd5Hash: (path): " + itemLocal);

                var dbItem = databaseFileList.FirstOrDefault(p => p.FilePath == itemLocal);
                if (dbItem != null)
                {
                    // Check if Hash is changed
                    // how should i unittest this?
	                var localHash = new FileHash(_iStorage).GetHashCode(itemLocal);

                    if(_appSettings.Verbose) Console.WriteLine("localHash: " + localHash);

                    if (localHash != dbItem.FileHash)
                    {
                        _query.RemoveItem(dbItem);

                        // Read data from file
	                    var updatedDatabaseItem = _readMeta.ReadExifAndXmpFromFile(itemLocal);
	                    updatedDatabaseItem.ImageFormat = ExtensionRolesHelper.GetImageFormat(_iStorage.ReadStream(itemLocal,160));
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
