using System;
using System.Collections.Generic;
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

            // Check if folder exist
            if(Files.IsFolderOrFile(subPath) != FolderOrFileModel.FolderOrFileTypeList.Folder) return null;
                
            var allItemsInDb = _query.GetAllFiles(subPath);
            //                _context.FileIndex.Where
            //                        (p => p.ParentDirectory.Contains(subPath))
            //                    .OrderBy(r => r.FileName).ToList();

            if (allItemsInDb.Count > 500) throw new ApplicationException("Item in subfolder is to large");
            

            foreach (var dbItem in allItemsInDb)
            {
                if (!dbItem.IsDirectory)
                {
                    var res = allItemsInDb.Where(
                        p =>
                            p.IsDirectory &&
                            p.FilePath == dbItem.ParentDirectory
                    );
                    if (!res.Any())
                    {
                        var c = res.Count();
                        var q = dbItem.FilePath;
                        var w = dbItem.IsDirectory;
                        _query.RemoveItem(dbItem);
                    }
                }

            }

            return null;
        }

    }
}