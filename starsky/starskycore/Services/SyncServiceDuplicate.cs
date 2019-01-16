using System;
using System.Collections.Generic;
using System.Linq;
using starskycore.Models;

namespace starskycore.Services
{
    public partial class SyncService
    {
        // Add duplicate check
        public List<FileIndexItem> RemoveDuplicate(List<FileIndexItem> databaseSubFolderList)
        {
            
            // Get a list of duplicate items
            var duplicateItemsByFilePath = databaseSubFolderList.GroupBy(item => item.FilePath)
                .SelectMany(grp => grp.Skip(1).Take(1)).ToList();
            
            // duplicateItemsByFilePath > 
            // If you have 3 item with the same name it will include 1 name
            // So we do a linq query to search simalar items
            // We keep the first item
            // And Delete duplicate items
            
            foreach (var duplicateItemByName in duplicateItemsByFilePath)
            {
                var duplicateItems = databaseSubFolderList.Where(p => p.FilePath == duplicateItemByName.FilePath).ToList();
                for (int i = 1; i < duplicateItems.Count; i++)
                {
                    databaseSubFolderList.Remove(duplicateItems[i]);
                    if (_appSettings.Verbose) Console.WriteLine("> RemoveDuplicate - " + duplicateItems[i].FilePath);
                    _query.RemoveItem(duplicateItems[i]);
                }
            }
            return databaseSubFolderList;
        }

    }
}