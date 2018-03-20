using System;
using System.Collections.Generic;
using System.IO;
using starsky.Models;

namespace starsky.Services
{
    public partial class Query
    {
        // When input a direct file
        //        => if this file is deleted on the file system 
        //              => delete it from the database

        public void SyncDeleted(string subPath = "")
        {
            if (Files.IsFolderOrFile(subPath) == FolderOrFileModel.FolderOrFileTypeList.Deleted)
            {
                // single file or folder deleting
                var dbListWithOneFile = new List<FileIndexItem>();
                var dbItem = GetObjectByFilePath(subPath);
                if (dbItem != null)
                {
                    RemoveItem(dbItem);
                    Console.WriteLine("File " + subPath +" not found and removed");

                    if (dbItem.IsDirectory == false) throw new FileNotFoundException();
                    var toBeDeleted = GetAllFiles(dbItem.FilePath);

                    foreach (var item in toBeDeleted)
                    {
                        Console.WriteLine("|");
                        RemoveItem(item);
                    }
                }

                throw new FileNotFoundException();
            }
        }
    }
}
