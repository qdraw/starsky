using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using starsky.Models;

namespace starsky.Services
{
    public partial class SyncService
    {
        // Sync the database
        // Based on subpath, do a cached database query
        // Check differences in the database and local version

        // If this class does not exist, this problem:
        // if directory remove parent elements
        // 1. Index all folders
        // 2. Rename single folder
        // 3. The files are keeped in the index
        // RemoveOldFilePathItemsFromDatabase => remove this items from database

        public List<FileIndexItem> RemoveOldFilePathItemsFromDatabase(
            List<string> localSubFolderListDatabaseStyle,
            List<FileIndexItem> databaseSubFolderList,
            string subpath
        )
        {

            //Check fileName Difference
            var databaseFileListFileName =
                databaseSubFolderList.Where
                        (p => p.FilePath.Contains(subpath))
                    .OrderBy(r => r.FileName)
                    .Select(item => item.FilePath)
                    .ToList();

            IEnumerable<string> differenceFileNames = databaseFileListFileName.Except(localSubFolderListDatabaseStyle);

            Console.Write(differenceFileNames.Count() + " " + databaseSubFolderList.Count);

            // Delete removed items
            foreach (var item in differenceFileNames)
            {
                Console.Write("*");

                var ditem = databaseSubFolderList.FirstOrDefault(p => p.FilePath == item);
                databaseSubFolderList.Remove(ditem);
                _update.RemoveItem(ditem);

                if (ditem?.IsDirectory == null) continue;
                if (!ditem.IsDirectory) continue;

                var orphanPictures = _context.FileIndex.Where(p => !p.IsDirectory && p.ParentDirectory == ditem.FilePath);
                foreach (var orphanItem in orphanPictures)
                {
                    Console.Write("$");
                    _update.RemoveItem(orphanItem);
                }


            }

            return databaseSubFolderList;
        }
    }
}
