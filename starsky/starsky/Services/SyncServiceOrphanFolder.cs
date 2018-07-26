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

            // Check if the subfolder it self exist, then search for child folders
            if(!Directory.Exists(_appSettings.DatabasePathToFilePath(subPath,false))) return null;

            var allItemsInDb = _query.GetAllRecursive(subPath);

            // Large items not recruisive
            if (allItemsInDb.Count > 2500)
            {
                Console.WriteLine("Item in subfolder is to large");
                throw new ArgumentOutOfRangeException();
            }

            Console.WriteLine("> running");

            foreach (var dbItem in allItemsInDb)
            {
                Console.WriteLine(dbItem.FilePath);

                if (!dbItem.IsDirectory)
                {
                    // For Checking if File has no parent items
                    var res = allItemsInDb.Where(
                        p =>
                            p.IsDirectory &&
                            p.FilePath == dbItem.ParentDirectory
                    );

                    if (!res.Any() && !File.Exists(_appSettings.DatabasePathToFilePath(dbItem.FilePath)) )
                    {
                        if(AppSettingsProvider.Verbose) Console.WriteLine("o>> " + dbItem.FilePath);
                        _query.RemoveItem(dbItem);
                    }
                }
            }
            return null;
        }

    }
}