using System;
using System.Collections.Generic;
using System.Linq;
using starsky.Models;

namespace starsky.Services
{
    public partial class SyncService
    {
        // Sync the database
        // Based on subpath, do a cached database query
        // Check differences in the database and local version
        // No Duplicate file support in the class

        public List<FileIndexItem> RemoveOldFilePathItemsFromDatabase(
            List<string> localSubFolderListDatabaseStyle,
            List<FileIndexItem> databaseSubFolderList,
            string subpath)
        {

            //Check fileName Difference
            var databaseFileListFileName =
                databaseSubFolderList.Where
                        (p => p.FilePath.Contains(subpath))
                    .OrderBy(r => r.FileName)
                    .Select(item => item.FilePath)
                    .ToList();

            var differenceFileNames = databaseFileListFileName.Except(localSubFolderListDatabaseStyle).ToList();

            if (_appSettings.Verbose)
            {
                Console.Write("diff: " + differenceFileNames.Count + "  | in db folder" + databaseSubFolderList.Count);
			}

            // Delete removed items
            foreach (var item in differenceFileNames)
            {

                var ditem = databaseSubFolderList.FirstOrDefault(p => p.FilePath == item);
                if (ditem?.FilePath == null) continue;

                // If the item is the subpath directory don't delete this one
                // SyncServiceAddSubPathFolder => is adding the complete list of subpaths
                if (GetListOfSubpaths(subpath).LastOrDefault() == ditem?.FilePath)
                {
                    continue;
                }

                Console.Write("*");

                // Remove different item from list
                databaseSubFolderList.Remove(ditem);
                _query.RemoveItem(ditem);

                // If Directory check if it has orphan items
                // If this feature does not exist, this problem exist:
                // if directory remove parent elements
                // 1. Index all folders
                // 2. Rename single folder
                // 3. The files are keeped in the index
                // RemoveOldFilePathItemsFromDatabase => remove this items from database
                
                // todo: not shure if this function is any good
                
                if (!ditem.IsDirectory) continue;

	            var orphanPictures = _query.GetAllFiles(ditem.FilePath);
                foreach (var orphanItem in orphanPictures)
                {
                    Console.Write("$");
                    _query.RemoveItem(orphanItem);
                }
            }
            return databaseSubFolderList;
        }
    }
}
