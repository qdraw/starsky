using System;
using System.Collections.Generic;
using System.Linq;
using starsky.Models;

namespace starsky.Services
{
    public partial class SyncService
    {
        // Add duplicate check
        public List<FileIndexItem> RemoveDuplicate(List<FileIndexItem> databaseSubFolderList)
        {
            
            // Get a list of duplicate items
            var duplicateItems = databaseSubFolderList.GroupBy(item => item.FilePath)
                .SelectMany(grp => grp.Skip(1).Take(1)).ToList();
            
            // Delete duplicate items
            foreach (var item in duplicateItems)
            {
                var ditem = databaseSubFolderList.FirstOrDefault(p => p == item);
                databaseSubFolderList.Remove(ditem);
                if (AppSettingsProvider.Verbose) Console.WriteLine("> RemoveDuplicate - " + item.FilePath);
                _query.RemoveItem(ditem);
            }

            return databaseSubFolderList;
        }

    }
}