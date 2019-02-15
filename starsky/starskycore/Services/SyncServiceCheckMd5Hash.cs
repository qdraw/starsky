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
                    var localHash = FileHash.GetHashCode(_appSettings.DatabasePathToFilePath(itemLocal));
                    if(_appSettings.Verbose) Console.WriteLine("localHash: " + localHash);

                    if (localHash != dbItem.FileHash)
                    {
                        _query.RemoveItem(dbItem);

                        var fullFilePath = _appSettings.DatabasePathToFilePath(itemLocal);
                        
                        var imageFormat = FilesHelper.GetImageFormat(fullFilePath);

                        // Read data from file
                        var updatedDatabaseItem = _readMeta.ReadExifAndXmpFromFile(fullFilePath,imageFormat);
                        
                        updatedDatabaseItem.ImageFormat = imageFormat;
                        updatedDatabaseItem.FileHash = localHash;
                        updatedDatabaseItem.FileName = dbItem.FileName;
                        updatedDatabaseItem.AddToDatabase = DateTime.Now;
                        updatedDatabaseItem.IsDirectory = false;
                        updatedDatabaseItem.ParentDirectory = dbItem.ParentDirectory;
                        _query.AddItem(updatedDatabaseItem);
                    }
                }

            }
        }
    }
}
