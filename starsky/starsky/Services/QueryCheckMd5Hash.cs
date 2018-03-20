﻿using System;
using System.Collections.Generic;
using System.Linq;
using starsky.Models;

namespace starsky.Services
{
    public partial class Query
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
                var dbItem = databaseFileList.FirstOrDefault(p => p.FilePath == itemLocal);
                if (dbItem != null)
                {
                    // Check if Hash is changed
                    var localHash = FileHash.CalcHashCode(FileIndexItem.DatabasePathToFilePath(itemLocal));
                    if (localHash != dbItem.FileHash)
                    {
                        RemoveItem(dbItem);
                        var updatedDatabaseItem = ExifRead.ReadExifFromFile(FileIndexItem.DatabasePathToFilePath(itemLocal));
                        updatedDatabaseItem.FilePath = dbItem.FilePath;
                        updatedDatabaseItem.FileHash = localHash;
                        updatedDatabaseItem.FileName = dbItem.FileName;
                        updatedDatabaseItem.AddToDatabase = DateTime.Now;
                        updatedDatabaseItem.IsDirectory = false;
                        updatedDatabaseItem.ParentDirectory = dbItem.ParentDirectory;
                        AddItem(updatedDatabaseItem);
                    }
                }

            }
        }
    }
}
