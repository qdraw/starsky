using System;
using System.Collections.Generic;
using System.IO;
using starsky.Models;

namespace starsky.Services
{
    public partial class SyncService
    {
        // When input a direct file
        //        => if this file is deleted on the file system 
        //              => delete it from the database

        private void Deleted(string subPath = "")
        {
            if (Files.IsFolderOrFile(subPath) == FolderOrFileModel.FolderOrFileTypeList.Deleted)
            {
                // single file or folder deleting
                //var dbListWithOneFile = new List<FileIndexItem>();
                var dbItem = _query.GetObjectByFilePath(subPath);
                if (dbItem != null)
                {
                    _query.RemoveItem(dbItem);
                    Console.WriteLine("File " + subPath +" not found and removed");

                    if (dbItem.IsDirectory == false) throw new FileNotFoundException();
                    // Remove subitems in directory
                    var toBeDeleted = _query.GetAllFiles(dbItem.FilePath);

                    foreach (var item in toBeDeleted)
                    {
                        Console.WriteLine("|");
                        _query.RemoveItem(item);
                    }
                }
                throw new FileNotFoundException();
            }
        }
    }
}
