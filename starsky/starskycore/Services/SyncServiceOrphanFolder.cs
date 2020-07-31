using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace starskycore.Services
{
    public partial class SyncService
    {
        /// <summary>
        /// The folder is deleted, but there are fileindexitems that has no parrent
        /// Output is to delete this child items
        /// Very memory using feature to check if folders are not deleted
        /// </summary>
        /// <param name="subPath">internal/subpath</param>
        /// <param name="maxNumberOfItems">(int) the max child items</param>
        /// <returns>Output is to delete this child items</returns>
        /// <exception cref="ConstraintException">more than 3000</exception>
        public IEnumerable<string>
           OrphanFolder (string subPath, int maxNumberOfItems = 3000) 
        {
            // You will get Out of Memory issues on large folders
            // 1. Index all folders
            // 2. Rename single folder
            // 3. The files are keeped in the index

            // Check if the subfolder it self exist, then search for child folders
            if(!Directory.Exists(_appSettings.DatabasePathToFilePath(subPath,false))) return null;

            var allItemsInDb = _query.GetAllRecursive(subPath);

            // Large items not recruisive
            if (allItemsInDb.Count > maxNumberOfItems)
            {
                // item name is overwritten
                throw new ConstraintException(
                    "Item in subfolder is to large - now: " +
                  allItemsInDb.Count + " vs max:" + maxNumberOfItems);
            }

            Console.WriteLine("> running");

            foreach (var dbItem in allItemsInDb)
            {
                Console.WriteLine(dbItem.FilePath);

                if (dbItem.IsDirectory == false)
                {
                    // For Checking if File has no parent items
                    var res = allItemsInDb.Where(
                        p =>
                            p.IsDirectory == true &&
                            p.FilePath == dbItem.ParentDirectory
                    );

                    if (!res.Any() && !File.Exists(_appSettings.DatabasePathToFilePath(dbItem.FilePath)) )
                    {
                        if(_appSettings.Verbose) Console.WriteLine("o>> " + dbItem.FilePath);
                        _query.RemoveItem(dbItem);
                    }
                }
            }
            return null;
        }

    }
}
