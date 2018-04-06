using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using starsky.Models;

namespace starsky.Services
{
    public partial class SyncService
    {
        // Very memory using feature to check if folders are not deleted
        public IEnumerable<string>
           OrphanFolder (string subPath) //  RemoveEmptyFolders
        {
            // You will get Out of Memory issues on large folders
            // 1. Index all folders
            // 2. Rename single folder
            // 3. The files are keeped in the index

            // Check if folder does not exist on the fs
            if(Directory.Exists(FileIndexItem.DatabasePathToFilePath(subPath,false))) return null;
                
            var allItemsInDb = _query.GetAllFiles(subPath);

            if (allItemsInDb.Count > 500) throw new ApplicationException("Item in subfolder is to large");

            foreach (var dbItem in allItemsInDb)
            {
                if (dbItem.IsDirectory) continue;
                
                var res = allItemsInDb.Where(
                    p =>
                        p.IsDirectory &&
                        p.FilePath == dbItem.ParentDirectory
                );
                if (!res.Any())
                {
                    _query.RemoveItem(dbItem);
                }

            }

            return null;
        }

    }
}